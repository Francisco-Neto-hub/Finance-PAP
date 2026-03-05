using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class Contrato
    {
        public int IdContrato { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        // Relacionamento com o Estado do Contrato
        public int IdEstado_Contrato { get; set; }
        public virtual EstadoContrato? Estado { get; set; }

        // Relacionamentos: Um contrato pode ter vários clientes e várias contas
        public virtual ICollection<ContratoCliente> Clientes { get; set; } = new List<ContratoCliente>();
        public virtual ICollection<Conta> Contas { get; set; } = new List<Conta>();
    }
}
