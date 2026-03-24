using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.DTOs;

public class ResumoContaDTO
{
    public int IdConta { get; set; }
    public string NomeConta { get; set; } = string.Empty;
    public decimal SaldoAtual { get; set; }
    public string Estado { get; set; } = string.Empty; // "Ativa" ou "Fechada"
    public string Titular { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
}