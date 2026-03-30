namespace Finance.API.DTOs
{
    public class DashboardResponseDTO
    {
        public decimal SaldoTotal { get; set; }
        public decimal TotalReceitasMes { get; set; } // NOVO: Para saber o que entrou este mês
        public decimal TotalDespesasMes { get; set; } // NOVO: Para saber o que saiu este mês
        public List<ContaResumoDTO> Contas { get; set; } = new();
        public List<GastoCategoriaDTO> GastosPorCategoria { get; set; } = new();
        public List<TransacaoRecenteDTO> UltimasTransacoes { get; set; } = new();
    }

    public class ContaResumoDTO
    {
        public string NomeConta { get; set; }
        public decimal Montante { get; set; }
    }

    public class GastoCategoriaDTO
    {
        public string Categoria { get; set; }
        public decimal TotalGasto { get; set; }
    }

    public class TransacaoRecenteDTO
    {
        public string NomeTransacao { get; set; }
        public decimal ValorTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public string TipoMovimento { get; set; }
    }
}