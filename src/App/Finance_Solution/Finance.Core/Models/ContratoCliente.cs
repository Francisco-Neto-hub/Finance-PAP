using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models;

public class ContratoCliente
{
    public int IdContrato { get; set; }
    public int IdCliente { get; set; }
    public bool IsTitular { get; set; } // Define se é o dono ou apenas beneficiário
}
