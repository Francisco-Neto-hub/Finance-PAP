using System;
using System.Text.Json.Serialization; // Não te esqueças de adicionar este using

namespace FinanceUI.Models
{
    public class TransacaoRequest
    {
        [JsonPropertyName("nomeTransacao")]
        public string NomeTransacao { get; set; }

        [JsonPropertyName("valorTransacao")]
        public decimal ValorTransacao { get; set; }

        [JsonPropertyName("idCategoria")]
        public int IdCategoria { get; set; }

        [JsonPropertyName("dataTransacao")]
        public DateTime DataTransacao { get; set; }

        [JsonPropertyName("idContaOrigem")]
        public int IdContaOrigem { get; set; }

        [JsonPropertyName("idTipo")]
        public int IdTipo { get; set; }

        [JsonPropertyName("idContaDestino")]
        public int? IdContaDestino { get; set; } // O '?' permite que seja nulo em Receitas/Despesas
    }
}