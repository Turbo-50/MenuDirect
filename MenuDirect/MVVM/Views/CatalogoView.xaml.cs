using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class CatalogoView : ContentPage
{
    private readonly CatalogoViewModel _viewModel;

    public CatalogoView(CatalogoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
        _viewModel.RefrescarCantidades();
    }
}