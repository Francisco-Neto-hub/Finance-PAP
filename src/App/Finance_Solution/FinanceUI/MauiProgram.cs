using Finance.Core.Models;
using Finance.Core.Services;
using FinanceUI.ViewModel;
using Microsoft.Extensions.Logging;

namespace FinanceUI
{
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
                });
            
            // 1. Configurar o DbContext
            // Nota: Vê o ponto abaixo sobre a Connection String para Android!
            builder.Services.AddDbContext<FinanceDbContext>(); // Contexto

            // 2. Registar os Serviços (Abstração e Implementação)
            builder.Services.AddSingleton<IAuthService, AuthService>(); // Serviço de Login
            builder.Services.AddSingleton<IFinanceService, FinanceService>(); // Lógica Financeira

            // 3. Registar as ViewModels e Pages
            // Transient garante que uma nova instância é criada ao navegar
            builder.Services.AddTransient<DashboardViewModel>();
            //builder.Services.AddTransient<DashboardPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
