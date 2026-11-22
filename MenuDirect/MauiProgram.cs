using MenuDirect.Data.Database;
using MenuDirect.Data.Interfaces;
using MenuDirect.Data.Repositories;
using MenuDirect.Data.Services;
using MenuDirect.MVVM.ViewModel;
using MenuDirect.MVVM.Views;
using Microsoft.Extensions.Logging;

namespace MenuDirect;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fontello.ttf", "Icons");
            });

        // Conexión a PostgreSQL en Neon
        builder.Services.AddSingleton<PostgresConnectionFactory>();

        // Repositorios
        builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddSingleton<ICatalogoRepository, CatalogoRepository>();
        builder.Services.AddSingleton<IProductoRepository, ProductoRepository>();
        builder.Services.AddSingleton<ICarritoRepository, CarritoRepository>();
        builder.Services.AddSingleton<IPedidoRepository, PedidoRepository>();

        // Servicios de aplicación
        builder.Services.AddSingleton<UsuarioSessionService>();
        builder.Services.AddSingleton<CatalogoService>();
        builder.Services.AddSingleton<CarritoService>();
        builder.Services.AddSingleton<PedidoService>();
        builder.Services.AddSingleton<AdminService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddSingleton<CatalogoViewModel>();
        builder.Services.AddSingleton<CarritoViewModel>();
        builder.Services.AddTransient<PedidoListViewModel>();
        builder.Services.AddTransient<CocinaViewModel>();
        builder.Services.AddTransient<AdminEmpleadosViewModel>();
        builder.Services.AddTransient<AdminProductosViewModel>();

        // Views
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginView>();
        builder.Services.AddSingleton<CatalogoView>();
        builder.Services.AddSingleton<CarritoView>();
        builder.Services.AddTransient<PedidoListView>();
        builder.Services.AddTransient<CocinaView>();
        builder.Services.AddTransient<AdminDashboardView>();
        builder.Services.AddTransient<AdminEmpleadosView>();
        builder.Services.AddTransient<AdminProductosView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}