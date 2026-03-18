using Finance.App.ViewModels;
using Finance.App.Views;
using Finance.Core.Models;
using Finance.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Finance.App
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

            // 1. REGISTAR A BASE DE DADOS (Substitui pela tua Connection String de ontem)
            builder.Services.AddDbContext<FinanceDbContext>(options => 
            options.UseSqlServer("Server=DESKTOP-76S1NRV\\SQLEXPRESS;Database=Finance_BD_v2;Trusted_Connection=True;TrustServerCertificate=True;"));

            // 2. REGISTAR SERVIÇOS E UI
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
