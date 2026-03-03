using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    public class Conta
    {
        public int IdConta { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
        public int IdUtilizador { get; set; }
        public bool Ativo { get; set; } = true; // Reflete o campo BIT do SQL
    }
}
