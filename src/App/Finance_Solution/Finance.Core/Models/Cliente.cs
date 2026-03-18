namespace Finance.Core.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nome { get; set; } = null!;

    public string? Telemovel { get; set; }

    public string Email { get; set; } = null!;

    public DateOnly? DataNasc { get; set; }

    public int? IdEstadoCliente { get; set; }

    public string ByPass { get; set; } = null!;

    public DateTime? DataCriacao { get; set; }
    public virtual ICollection<ContratoCliente> ContratoClientes { get; set; } = new List<ContratoCliente>();

    public virtual EstadoCliente? IdEstadoClienteNavigation { get; set; }
}
