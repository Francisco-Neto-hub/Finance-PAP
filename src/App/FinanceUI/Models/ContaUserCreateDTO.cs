using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class ContaUserCreateDTO
    {
        [JsonPropertyName("nomeConta")]
        public string NomeConta { get; set; }

        [JsonPropertyName("montante")]
        public decimal Montante { get; set; }
    }
}
