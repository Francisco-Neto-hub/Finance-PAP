using Finance.Core.Interfaces;
using Finance.Infrastructure.Repositories;
using Finance.Infrastructure.Services;
using FinanceUI;
using Microsoft.Extensions.Logging;

namespace Finance.UI;

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

        // 1. CONFIGURAÇÃO DA STRING DE CONEXÃO
        // Dica: Em produção, isto viria de um ficheiro de config, para a PAP podes pôr aqui:
        string connectionString = "Server=DESKTOP-76S1NRV\\SQLEXPRESS;Database=BD_Finance;TrustServerCertificate=True;";

        // 2. REGISTAR OS REPOSITÓRIOS (Infrastructure)
        builder.Services.AddSingleton<IUsuarioRepository>(new UsuarioRepository(connectionString));
        builder.Services.AddSingleton<IContaRepository>(new ContaRepository(connectionString));
        builder.Services.AddSingleton<ITransacaoRepository>(new TransacaoRepository(connectionString));

        // 3. REGISTAR O SERVIÇO PRINCIPAL (Core <-> Infrastructure)
        builder.Services.AddSingleton<IFinanceService, FinanceDataService>();

        // 4. REGISTAR AS VIEWMODELS E PÁGINAS (Para a UI conseguir pedir o serviço no construtor)
        // builder.Services.AddTransient<LoginViewModel>();
        // builder.Services.AddTransient<LoginPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}