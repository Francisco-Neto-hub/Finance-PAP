using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class TransacaoRequestDTO
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
        public int? IdContaDestino { get; set; }
    }
}
