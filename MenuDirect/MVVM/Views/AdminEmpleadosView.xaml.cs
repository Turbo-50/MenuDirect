using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class AdminEmpleadosView : ContentPage
{
    private readonly AdminEmpleadosViewModel _viewModel;

    public AdminEmpleadosView(AdminEmpleadosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarEmpleadosAsync();
    }
}