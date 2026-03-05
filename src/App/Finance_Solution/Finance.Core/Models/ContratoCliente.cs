using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Finance.Core.Models
{
    public class ContratoCliente
    {
        public int IdContrato { get; set; }
        public virtual Contrato? Contrato { get; set; }

        public int IdCliente { get; set; }
        public virtual Cliente? Cliente { get; set; }

        // Estado da ligação (Titular, Beneficiário, etc.)
        public int IdEstadoContratoCliente { get; set; }
        public virtual EstadoContratoCliente? Estado { get; set; }
    }
}
