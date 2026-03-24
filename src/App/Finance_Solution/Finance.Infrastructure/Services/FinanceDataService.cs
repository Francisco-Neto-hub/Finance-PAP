using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Logging;
using Finance.Core.Interfaces;
using Finance.Core.Models;
using Finance.Core.DTOs;

namespace Finance.Infrastructure.Services;

public class FinanceDataService : IFinanceService
{
    private readonly IUsuarioRepository _userRepo;
    private readonly IContaRepository _contaRepo;
    private readonly ITransacaoRepository _transRepo;
    private readonly ILogger<FinanceDataService> _logger;

    public FinanceDataService(
        IUsuarioRepository userRepo,
        IContaRepository contaRepo,
        ITransacaoRepository transRepo,
        ILogger<FinanceDataService> logger)
    {
        _userRepo = userRepo;
        _contaRepo = contaRepo;
        _transRepo = transRepo;
        _logger = logger;
    }

    public async Task<int?> LoginAsync(string email, string password) => await _userRepo.ValidarLoginAsync(email, password);
    public async Task<IEnumerable<ResumoContaDTO>> ObterDashboardAsync() => await _contaRepo.ObterResumoContasAsync();
    public async Task<bool> RegistarTransacaoAsync(int c, string d, decimal v, int cat, int t) => await _transRepo.AdicionarAsync(c, d, v, cat, t);
    public async Task<bool> RealizarTransferenciaAsync(int o, int d, decimal v) => await _transRepo.TransferirAsync(o, d, v);
    public async Task<bool> CriarContaAsync(Conta c) => await _contaRepo.CriarAsync(c);
    public async Task<IEnumerable<Categoria>> ObterCategoriasAsync() => await _transRepo.ListarCategoriasAsync();
}
