using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models;

public class Perfil
{
    public int IdPerfil { get; set; }
    public string NomePerfil { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
