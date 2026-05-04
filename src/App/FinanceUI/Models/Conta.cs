using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class Conta
    {
        [JsonPropertyName("nomeConta")]
        public string NomeConta { get; set; }

        [JsonPropertyName("montante")]
        public decimal Montante { get; set; }

        // Propriedade para exibir o valor formatado
        public string MontanteFormatado => Montante.ToString("C");
    }
}
