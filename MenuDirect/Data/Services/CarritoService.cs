using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Mappers;
using MenuDirect.MVVM.Models;

namespace MenuDirect.Data.Services;

public partial class CarritoService : ObservableObject
{
    private readonly ICarritoRepository _carritoRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly UsuarioSessionService _sessionService;

    [ObservableProperty]
    private int? pedidoEnEdicion;

    public int? IdCarritoActivo { get; private set; }

    public ObservableCollection<Producto> ProductosSeleccionados { get; } = new();

    public decimal TotalPedido => ProductosSeleccionados.Sum(x => x.Subtotal);
    public int CantidadProductosCarrito => ProductosSeleccionados.Sum(x => x.Cantidad);
    public bool EditandoPedido => PedidoEnEdicion.HasValue;

    public CarritoService(
        ICarritoRepository carritoRepository,
        IPedidoRepository pedidoRepository,
        IProductoRepository productoRepository,
        UsuarioSessionService sessionService)
    {
        _carritoRepository = carritoRepository;
        _pedidoRepository = pedidoRepository;
        _productoRepository = productoRepository;
        _sessionService = sessionService;

        ProductosSeleccionados.CollectionChanged += (_, _) => NotificarCambios();
    }

    partial void OnPedidoEnEdicionChanged(int? value)
    {
        OnPropertyChanged(nameof(EditandoPedido));
    }

    private void SuscribirProducto(Producto producto)
    {
        producto.PropertyChanged -= Producto_PropertyChanged;
        producto.PropertyChanged += Producto_PropertyChanged;
    }

    private void Producto_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Producto.Cantidad) or nameof(Producto.Subtotal))
        {
            NotificarCambios();
        }
    }

    private void NotificarCambios()
    {
        OnPropertyChanged(nameof(TotalPedido));
        OnPropertyChanged(nameof(CantidadProductosCarrito));
    }

    private Producto? BuscarEnCarrito(int productoId)
    {
        return ProductosSeleccionados.FirstOrDefault(x => x.Id == productoId);
    }

    public async Task CargarCarritoMeseroAsync(IEnumerable<Producto> catalogoProductos)
    {
        if (EditandoPedido || !_sessionService.IdUsuario.HasValue)
            return;

        var carrito = await _carritoRepository.ObtenerOCrearCarritoPorMeseroAsync(_sessionService.IdUsuario.Value);
        IdCarritoActivo = carrito.IdCarrito;

        var detalles = await _carritoRepository.ObtenerDetallesCarritoAsync(carrito.IdCarrito);
        var listaCatalogo = catalogoProductos.ToList();

        foreach (var prod in listaCatalogo)
            prod.Cantidad = 0;

        ProductosSeleccionados.Clear();

        foreach (var det in detalles)
        {
            var prodBase = listaCatalogo.FirstOrDefault(p => p.Id == det.IdProducto);
            if (prodBase != null)
            {
                prodBase.Cantidad = det.Cantidad;
                SuscribirProducto(prodBase);
                ProductosSeleccionados.Add(prodBase);
                prodBase.Refrescar();
            }
        }

        NotificarCambios();
    }

    public async Task AgregarProductoAsync(Producto producto)
    {
        var existente = BuscarEnCarrito(producto.Id);
        int nuevaCantidad;

        if (existente == null)
        {
            producto.Cantidad = 1;
            nuevaCantidad = 1;
            SuscribirProducto(producto);
            ProductosSeleccionados.Add(producto);
        }
        else
        {
            existente.Cantidad++;
            nuevaCantidad = existente.Cantidad;
            if (!ReferenceEquals(producto, existente))
                producto.Cantidad = nuevaCantidad;
        }

        producto.Refrescar();
        NotificarCambios();

        if (!EditandoPedido && IdCarritoActivo.HasValue)
        {
            try
            {
                await _carritoRepository.GuardarOActualizarDetalleAsync(
                    IdCarritoActivo.Value,
                    producto.Id,
                    nuevaCantidad,
                    producto.PrecioUnitario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando detalle en Neon: {ex.Message}");
            }
        }
    }

    public async Task QuitarProductoAsync(Producto producto)
    {
        var existente = BuscarEnCarrito(producto.Id);
        if (existente == null)
            return;

        existente.Cantidad--;
        int nuevaCantidad = existente.Cantidad;

        if (!ReferenceEquals(producto, existente))
            producto.Cantidad = Math.Max(0, nuevaCantidad);

        if (nuevaCantidad <= 0)
        {
            existente.Cantidad = 0;
            producto.Cantidad = 0;
            ProductosSeleccionados.Remove(existente);
            producto.Refrescar();
            NotificarCambios();

            if (!EditandoPedido && IdCarritoActivo.HasValue)
            {
                try
                {
                    await _carritoRepository.EliminarDetalleProductoAsync(IdCarritoActivo.Value, producto.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error eliminando detalle en Neon: {ex.Message}");
                }
            }
        }
        else
        {
            producto.Refrescar();
            NotificarCambios();

            if (!EditandoPedido && IdCarritoActivo.HasValue)
            {
                try
                {
                    await _carritoRepository.GuardarOActualizarDetalleAsync(
                        IdCarritoActivo.Value,
                        producto.Id,
                        nuevaCantidad,
                        producto.PrecioUnitario);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error actualizando detalle en Neon: {ex.Message}");
                }
            }
        }
    }

    public async Task CancelarPedidoAsync()
    {
        bool estabaEditando = EditandoPedido;
        LimpiarColeccionLocal();
        PedidoEnEdicion = null;
        NotificarCambios();

        if (!estabaEditando && IdCarritoActivo.HasValue)
        {
            try
            {
                await _carritoRepository.VaciarCarritoAsync(IdCarritoActivo.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error vaciando carrito en Neon: {ex.Message}");
            }
        }
    }

    public void LimpiarDespuesDeOrdenar()
    {
        LimpiarColeccionLocal();
        PedidoEnEdicion = null;
        NotificarCambios();
    }

    private void LimpiarColeccionLocal()
    {
        foreach (var item in ProductosSeleccionados)
        {
            item.Cantidad = 0;
            item.Refrescar();
        }
        ProductosSeleccionados.Clear();
    }

    public void SincronizarProductos(IEnumerable<Producto> productosCatalogo)
    {
        foreach (var producto in productosCatalogo)
        {
            var itemEnCarrito = BuscarEnCarrito(producto.Id);
            producto.Cantidad = itemEnCarrito?.Cantidad ?? 0;
        }
    }

    public async Task CargarPedidoParaEdicionAsync(int pedidoId)
    {
        LimpiarColeccionLocal();
        PedidoEnEdicion = pedidoId;

        var detalles = await _pedidoRepository.ObtenerDetallesPorPedidoAsync(pedidoId);
        var catalogo = (await _productoRepository.ObtenerTodosDisponiblesAsync())
            .Select(p => p.ToModel())
            .ToList();

        foreach (var detalle in detalles)
        {
            var producto = catalogo.FirstOrDefault(p => p.Id == detalle.IdProducto)
                           ?? new Producto
                           {
                               Id = detalle.IdProducto,
                               Nombre = detalle.NombreProducto,
                               PrecioUnitario = detalle.PrecioUnitario
                           };

            producto.Cantidad = detalle.Cantidad;
            SuscribirProducto(producto);
            ProductosSeleccionados.Add(producto);
        }

        NotificarCambios();
    }
}