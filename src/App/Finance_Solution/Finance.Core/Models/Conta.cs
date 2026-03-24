namespace Finance.Core.Models;

public class Conta
{
    public int IdConta { get; set; }
    public string NomeConta { get; set; } = string.Empty;
    public decimal Montante { get; set; }
    public bool IsAberta { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int IdContrato { get; set; }
}
