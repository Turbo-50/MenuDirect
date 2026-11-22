using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class AdminProductosView : ContentPage
{
    private readonly AdminProductosViewModel _viewModel;

    public AdminProductosView(AdminProductosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}