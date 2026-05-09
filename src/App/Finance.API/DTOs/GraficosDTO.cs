namespace Finance.API.DTOs
{
    public class GraficosDTO
    {
        // Já tens este do Dashboard, perfeito para o Gráfico de Donut (Despesas)
        public class GastoCategoriaDTO
        {
            public string Categoria { get; set; }
            public decimal TotalGasto { get; set; }
        }

        // NOVO: Para um Gráfico de Barras ou Linhas (Evolução ao longo dos meses)
        public class FluxoCaixaDTO
        {
            public int Mes { get; set; }
            public string NomeMes { get; set; }
            public decimal TotalReceitas { get; set; }
            public decimal TotalDespesas { get; set; }
        }
    }
}
