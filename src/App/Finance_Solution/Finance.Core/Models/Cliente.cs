using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Finance.Core.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string by_pass { get; set; } = string.Empty;

        [Column("IdEstadoCliente")] // Nome real na BD
        public int IdEstadoCliente { get; set; }

        [ForeignKey("IdEstadoCliente")] // Diz ao EF que esta é a ligação
        public virtual EstadoCliente? Estado { get; set; }
        public virtual ICollection<ContratoCliente> Contratos { get; set; } = new List<ContratoCliente>();
    }
}
