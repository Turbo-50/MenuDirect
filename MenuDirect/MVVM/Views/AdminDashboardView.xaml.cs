using MenuDirect.Data.Services;

namespace MenuDirect.MVVM.Views;

public partial class AdminDashboardView : ContentPage
{
    private readonly UsuarioSessionService _sessionService;

    public AdminDashboardView(UsuarioSessionService sessionService)
    {
        InitializeComponent();
        _sessionService = sessionService;
    }

    private async void OnEmpleadosClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminEmpleadosView));
    }

    private async void OnProductosClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminProductosView));
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        _sessionService.CerrarSesion();
        await Shell.Current.GoToAsync("//login");
    }
}