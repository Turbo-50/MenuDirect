using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.Models;

namespace MenuDirect.MVVM.ViewModel;

public partial class CatalogoViewModel : ObservableObject
{
    private readonly CatalogoService _catalogoService;
    private readonly CarritoService _carritoService;
    private readonly UsuarioSessionService _sessionService;

    private Categoria? _categoriaSeleccionada;
    private Subcategoria? _subcategoriaSeleccionada;
    private bool _estaInicializado;

    [ObservableProperty]
    private bool estaCargando;

    public ObservableCollection<Categoria> Categorias { get; } = new();
    public ObservableCollection<Subcategoria> Subcategorias { get; } = new();
    public ObservableCollection<Producto> Productos { get; } = new();

    public CatalogoViewModel(
        CatalogoService catalogoService,
        CarritoService carritoService,
        UsuarioSessionService sessionService)
    {
        _catalogoService = catalogoService;
        _carritoService = carritoService;
        _sessionService = sessionService;
    }

    public async Task InicializarAsync(bool forzarRecarga = false)
    {
        if (EstaCargando)
            return;

        // Si ya está inicializado y no se forzó recarga, solo sincroniza cantidades en memoria
        if (_estaInicializado && !forzarRecarga)
        {
            RefrescarCantidades();
            return;
        }

        try
        {
            EstaCargando = true;

            await _catalogoService.CargarAsync(forzarRecarga);

            Categorias.Clear();
            foreach (var cat in _catalogoService.ObtenerCategorias())
            {
                cat.IsSelected = false;
                Categorias.Add(cat);
            }

            Subcategorias.Clear();
            _categoriaSeleccionada = null;
            _subcategoriaSeleccionada = null;

            await _carritoService.CargarCarritoMeseroAsync(_catalogoService.Productos);
            MostrarTodosLosProductos();
            RefrescarCantidades();
            _estaInicializado = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar el menú: {ex.Message}", "Aceptar");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private void MostrarTodosLosProductos()
    {
        Productos.Clear();
        foreach (var prod in _catalogoService.ObtenerTodosLosProductos())
            Productos.Add(prod);
    }

    private void CargarSubcategorias(int categoriaId)
    {
        Subcategorias.Clear();
        foreach (var sub in _catalogoService.ObtenerSubcategoriasPorCategoria(categoriaId))
        {
            sub.IsSelected = false;
            Subcategorias.Add(sub);
        }
    }

    private void FiltrarPorCategoria(int categoriaId)
    {
        Productos.Clear();
        foreach (var prod in _catalogoService.ObtenerProductosPorCategoria(categoriaId))
            Productos.Add(prod);
    }

    private void FiltrarPorSubcategoria(int subcategoriaId)
    {
        Productos.Clear();
        foreach (var prod in _catalogoService.ObtenerProductosPorSubcategoria(subcategoriaId))
            Productos.Add(prod);
    }

    [RelayCommand]
    private void SeleccionarCategoria(Categoria categoria)
    {
        if (_categoriaSeleccionada == categoria)
        {
            categoria.IsSelected = false;
            _categoriaSeleccionada = null;
            _subcategoriaSeleccionada = null;
            Subcategorias.Clear();
            MostrarTodosLosProductos();
            return;
        }

        foreach (var item in Categorias)
            item.IsSelected = false;

        categoria.IsSelected = true;
        _categoriaSeleccionada = categoria;
        _subcategoriaSeleccionada = null;

        CargarSubcategorias(categoria.Id);
        FiltrarPorCategoria(categoria.Id);
    }

    [RelayCommand]
    private void SeleccionarSubcategoria(Subcategoria subcategoria)
    {
        if (_subcategoriaSeleccionada == subcategoria)
        {
            subcategoria.IsSelected = false;
            _subcategoriaSeleccionada = null;
            if (_categoriaSeleccionada != null)
                FiltrarPorCategoria(_categoriaSeleccionada.Id);
            return;
        }

        foreach (var item in Subcategorias)
            item.IsSelected = false;

        subcategoria.IsSelected = true;
        _subcategoriaSeleccionada = subcategoria;
        FiltrarPorSubcategoria(subcategoria.Id);
    }

    public void RefrescarCantidades()
    {
        _carritoService.SincronizarProductos(_catalogoService.Productos);
        foreach (var prod in _catalogoService.Productos)
            prod.Refrescar();
    }

    [RelayCommand]
    private void AgregarProducto(Producto? producto)
    {
        if (producto == null)
            return;

        _ = _carritoService.AgregarProductoAsync(producto);
    }

    [RelayCommand]
    private void QuitarProducto(Producto? producto)
    {
        if (producto == null)
            return;

        _ = _carritoService.QuitarProductoAsync(producto);
    }

    [RelayCommand]
    private async Task CerrarSesionAsync()
    {
        _estaInicializado = false;
        await _carritoService.CancelarPedidoAsync();
        _sessionService.CerrarSesion();
        await Shell.Current.GoToAsync("//login");
    }
}