using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models;

public class Contrato
{
    public int IdContrato { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool IsVigente { get; set; } // Antigo Estado_Contrato
}