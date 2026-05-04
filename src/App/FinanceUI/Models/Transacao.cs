using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class Transacao
    {
        [JsonPropertyName("nomeTransacao")]
        public string NomeTransacao { get; set; }

        [JsonPropertyName("valorTransacao")]
        public decimal ValorTransacao { get; set; }

        [JsonPropertyName("dataTransacao")]
        public DateTime DataTransacao { get; set; }

        [JsonPropertyName("tipoMovimento")]
        public string TipoMovimento { get; set; }

        // Lógica para definir a cor na lista
        [JsonIgnore] // Opcional: evita enviar de volta para a API
        public Color CorValor => TipoMovimento == "Receita" ? Colors.Green : Colors.Red;
    }
}
