using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceUI.Models
{
    public class DashboardDto
    {
        public decimal SaldoTotal { get; set; }
        public List<ContaResumoDto> Contas { get; set; }
    }
}
