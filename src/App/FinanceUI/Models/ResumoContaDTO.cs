using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceUI.Models
{
    public class ResumoContaDTO
    {
        public int IdConta { get; set; }
        public string NomeConta { get; set; }
        public decimal SaldoAtual { get; set; }
        public string Estado { get; set; }
        public string Titular { get; set; }
    }
}