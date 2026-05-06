using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceUI.Models
{
    public class TransacaoReadDTO
    {
        public int IdTransacao { get; set; }
        public string TipoMovimento { get; set; } // "Receita" ou "Despesa"
        public string Categoria { get; set; }
        public decimal ValorTransacao { get; set; }
        public string NomeTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public bool IsConcluida { get; set; }

        // --- Helpers Visuais para a UI ---
        public Color CorSinal => TipoMovimento == "Receita" ? Colors.Green : Colors.Red;
        public string Sinal => TipoMovimento == "Receita" ? "+" : "-";
    }
}
