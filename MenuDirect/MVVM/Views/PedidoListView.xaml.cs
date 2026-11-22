using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class PedidoListView : ContentPage
{
    private readonly PedidoListViewModel _viewModel;

    public PedidoListView(PedidoListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarPedidosAsync();
    }
}