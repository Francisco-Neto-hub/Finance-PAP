using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class Transacao
    {
        public int IdTransacao { get; set; }
        public int IdConta { get; set; }
        public string NomeTransacao { get; set; } = string.Empty;
        public DateTime DataTransacao { get; set; } = DateTime.Now;
        public decimal ValorTransacao { get; set; }
        public int IdCategoria { get; set; }
        public int IdTipo { get; set; } // 1 para Receita, 2 para Despesa
    }
}
