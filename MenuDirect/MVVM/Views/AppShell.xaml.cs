using System.ComponentModel;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class AppShell : Shell
{
    private readonly CarritoService _carritoService;

    public AppShell(CatalogoViewModel catalogoViewModel, CarritoService carritoService)
    {
        InitializeComponent();
        BindingContext = catalogoViewModel;
        _carritoService = carritoService;

        Routing.RegisterRoute(nameof(PedidoListView), typeof(PedidoListView));
        Routing.RegisterRoute(nameof(AdminEmpleadosView), typeof(AdminEmpleadosView));
        Routing.RegisterRoute(nameof(AdminProductosView), typeof(AdminProductosView));

        _carritoService.PropertyChanged += CarritoService_PropertyChanged;
        ActualizarTituloCarrito();
    }

    private void CarritoService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CarritoService.CantidadProductosCarrito))
        {
            ActualizarTituloCarrito();
        }
    }

    private void ActualizarTituloCarrito()
    {
        int cantidad = _carritoService.CantidadProductosCarrito;
        CarritoTab.Title = cantidad == 0 ? "Carrito" : $"Carrito ({cantidad})";
    }
}