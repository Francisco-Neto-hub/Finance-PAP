using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class DashboardResumo
    {
        [JsonPropertyName("saldoTotal")] // Nome exato que vem na API
        public decimal SaldoTotal { get; set; }

        [JsonPropertyName("totalReceitasMes")]
        public decimal TotalReceitasMes { get; set; }

        [JsonPropertyName("totalDespesasMes")]
        public decimal TotalDespesasMes { get; set; }

        [JsonPropertyName("Contas")]
        public List<ContaDTO> Contas { get; set; }

        [JsonPropertyName("ultimasTransacoes")]
        public List<Transacao> UltimasTransacoes { get; set; }
    }
}
