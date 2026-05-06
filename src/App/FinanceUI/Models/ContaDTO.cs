using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinanceUI.Models
{
    public class ContaDTO
    {
        public int Id { get; set; }

        public string NomeConta { get; set; }

        public decimal Montante { get; set; }

        // Propriedade extra apenas para a Interface (XAML) não vai para a API
        [JsonIgnore]
        public string MontanteFormatado => Montante.ToString("C");
    }
}
