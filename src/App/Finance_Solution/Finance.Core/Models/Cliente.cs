namespace Finance.Core.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }
    public string Nome { get; set; } = null!;
    public string? Telemovel { get; set; } // Faltava esta propriedade
    public string Email { get; set; } = null!;
    public DateTime? DataNasc { get; set; }
    public int? IdEstadoCliente { get; set; } // Faltava esta propriedade
    public int IdPerfil { get; set; }
    public string ByPass { get; set; } = null!;
    public DateTime? DataCriacao { get; set; }

    // Propriedades de Navegação (Essenciais para o HasOne/WithMany do DbContext)
    public virtual EstadoCliente? IdEstadoClienteNavigation { get; set; }
    public virtual Perfil IdPerfilNavigation { get; set; } = null!;

    // Relação com Contratos (Para evitar o erro de ContratoClientes)
    public virtual ICollection<ContratoCliente> ContratoClientes { get; set; } = new List<ContratoCliente>();
}
