using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class CarritoView : ContentPage
{
    public CarritoView(CarritoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}