namespace Finance.Core.Models;

public class AuditoriaSaldo
{
    public int IdLog { get; set; }
    public int IdConta { get; set; }
    public decimal SaldoAntigo { get; set; }
    public decimal SaldoNovo { get; set; }
    public DateTime DataAlteracao { get; set; }
}
