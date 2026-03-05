using Finance.Core.Data;
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

            // 1. Carregar a configuração (podes usar uma variável direta por agora para facilitar no MAUI)
            string connectionString = "Server=10.18.249.136;Database=Finance_BD_v2;User Id=admin;Password=812876;TrustServerCertificate=True;MultipleActiveResultSets=true";

            // 2. Registar o DbContext do projeto Finance.Core
            builder.Services.AddDbContext<FinanceDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Registar os Serviços para que possam ser usados em qualquer página (DI)
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<FinanceService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
