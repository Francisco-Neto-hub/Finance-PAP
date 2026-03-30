namespace Finance.API.DTOs
{
    public class AuditoriaSaldoDTO
    {
        public int IdLog { get; set; }
        public int IdConta { get; set; }
        public string NomeConta { get; set; } // Extraído com JOIN
        public decimal SaldoAntigo { get; set; }
        public decimal SaldoNovo { get; set; }
        public DateTime DataAlteracao { get; set; }
        public string Usuario { get; set; }
    }
}
