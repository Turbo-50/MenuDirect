using MenuDirect.MVVM.ViewModel;

namespace MenuDirect.MVVM.Views;

public partial class LoginView : ContentPage
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}