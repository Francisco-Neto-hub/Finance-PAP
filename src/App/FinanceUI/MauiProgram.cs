using FinanceUI.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting; // ADICIONAR ISTO

namespace FinanceUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp() // ADICIONAR ESTA LINHA AQUUUIII! É ela que impede o crash!
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Singleton: Existe apenas uma instância do serviço para toda a App
            builder.Services.AddSingleton<ApiService>();

            // Transient: Cria uma nova instância da página sempre que navegamos para ela
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<RecuperarPasswordPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<TransactionPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ContasPage>();
            builder.Services.AddTransient<HistoricoPage>();
            builder.Services.AddTransient<RelatoriosPage>();
            builder.Services.AddTransient<GraficosPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
