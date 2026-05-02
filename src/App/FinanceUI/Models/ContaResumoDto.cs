using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceUI.Models
{
    public class ContaResumoDto
    {
        public string Nome { get; set; }
        public decimal Saldo { get; set; }
        public string Tipo { get; set; }
    }
}