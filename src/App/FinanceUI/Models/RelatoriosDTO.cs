namespace FinanceUI.Models
{
    public class RelatoriosDTO
    {
        public class RelatorioTransacaoDTO
        {
            public int IdTransacao { get; set; }
            public string NomeTransacao { get; set; }
            public decimal ValorTransacao { get; set; }
            public DateTime DataTransacao { get; set; }
            public string Categoria { get; set; }
            public string ContaDestino { get; set; }
            public string TipoMovimento { get; set; } // Receita ou Despesa
        }
    }
}
