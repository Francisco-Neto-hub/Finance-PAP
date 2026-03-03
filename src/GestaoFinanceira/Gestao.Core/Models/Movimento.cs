using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    public class Movimento
    {
        public int IdMovimento { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int IdConta { get; set; }
        public int IdCategoria { get; set; }
        public int IdTipoMovimento { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
    }
}
