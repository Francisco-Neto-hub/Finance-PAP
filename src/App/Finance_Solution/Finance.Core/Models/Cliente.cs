using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telemovel { get; set; }
        public DateTime? DataNasc { get; set; }
        public bool IsAtivo { get; set; }
        public bool IsExcluido { get; set; }
        public int IdPerfil { get; set; }
        public string ByPass { get; set; } = "12345";
    }
}
