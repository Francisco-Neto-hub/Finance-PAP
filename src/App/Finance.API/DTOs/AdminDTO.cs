namespace Finance.API.DTOs
{
    public class AdminDTO
    {
        public class DashboardStatsDTO
        {
            public int TotalClientes { get; set; }
            public int ClientesAtivos { get; set; }
            public decimal VolumeTransacoesMes { get; set; }
            public decimal CapitalTotalCustodia { get; set; }
        }

        public class TicketSuporteDTO
        {
            public int IdTicket { get; set; }
            public string NomeCliente { get; set; }
            public string Assunto { get; set; }
            public string Mensagem { get; set; }
            public DateTime DataCriacao { get; set; }
            public bool IsResolvido { get; set; }
        }
    }
}
