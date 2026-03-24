using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models;

public class TipoMovimento
{
    public int IdTipo { get; set; }
    public string Descricao { get; set; } = string.Empty; // 'Receita' ou 'Despesa'
}
