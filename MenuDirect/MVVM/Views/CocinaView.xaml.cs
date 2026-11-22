using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class CocinaView : ContentPage
{
    private readonly CocinaViewModel _viewModel;

    public CocinaView(CocinaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarComandasAsync();
    }
}