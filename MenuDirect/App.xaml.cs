using MenuDirect.MVVM.Views;

namespace MenuDirect;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            MainPage = CrearPaginaError(ex);
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var shell = IPlatformApplication.Current!.Services.GetRequiredService<AppShell>();
            return new Window(shell);
        }
        catch (Exception ex)
        {
            return new Window(CrearPaginaError(ex));
        }
    }

    private static Page CrearPaginaError(Exception ex)
    {
        return new ContentPage
        {
            Content = new ScrollView
            {
                Content = new Label
                {
                    Text = $"Error:\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                    Padding = 20
                }
            }
        };
    }
}