
namespace Finance.Core.Models
{
    public partial class Perfil
    {
        public int IdPerfil { get; set; }
        public string NomePerfil { get; set; } = null!;
        public string? Descricao { get; set; }

        // Esta coleção é necessária para o .WithMany(p => p.Clientes) no DbContext
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
