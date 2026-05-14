using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceUI.Models
{
    public class RegistoRequestDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telemovel { get; set; }
        public DateTime? DataNasc { get; set; }
        public string Password { get; set; }
    }
}
