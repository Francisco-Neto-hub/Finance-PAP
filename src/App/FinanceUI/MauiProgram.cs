using FinanceUI.Views;
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

            // Singleton: Existe apenas uma instância do serviço para toda a App
            builder.Services.AddSingleton<ApiService>();

            // Transient: Cria uma nova instância da página sempre que navegamos para ela
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // No método CreateMauiApp, antes do return builder.Build();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<LoginPage>(); // Importante!
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
