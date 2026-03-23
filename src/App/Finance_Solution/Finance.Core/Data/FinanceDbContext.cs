using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Finance.Core.Models;

public partial class FinanceDbContext : DbContext
{
    public FinanceDbContext()
    {
    }

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Perfil> Perfis { get; set; } // O nome da tabela é Perfil
    
    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Contrato> Contratos { get; set; }

    public virtual DbSet<ContratoCliente> ContratoClientes { get; set; }

    public virtual DbSet<Contum> Conta { get; set; }

    public virtual DbSet<EstadoCliente> EstadoClientes { get; set; }

    public virtual DbSet<EstadoContrato> EstadoContratos { get; set; }

    public virtual DbSet<EstadoContratoCliente> EstadoContratoClientes { get; set; }

    public virtual DbSet<EstadoContum> EstadoConta { get; set; }

    public virtual DbSet<EstadoTransacao> EstadoTransacaos { get; set; }

    public virtual DbSet<TipoMovimento> TipoMovimentos { get; set; }

    public virtual DbSet<Transacao> Transacaos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Usa o pacote Microsoft.EntityFrameworkCore.SqlServer
            // Se estiveres a testar no Windows Machine, o nome do server funciona.            
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-76S1NRV\SQLEXPRESS;Database=BD_Finance_v2;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");

            // Se for no Android, troca o nome do Server pelo IP 10.0.2.2

            //string connectionString = @"Server=10.0.2.2\SQLEXPRESS;Database=BD_Finance_v2;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
            //optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__8A3D240CAB117B20");

            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Cliente__885457EECCE8A431");

            entity.ToTable("Cliente");

            entity.HasIndex(e => e.Email, "UQ__Cliente__AB6E61646F93A472").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.ByPass)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("12345")
                .HasColumnName("by_pass");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IdEstadoCliente).HasColumnName("idEstadoCliente");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nome");
            entity.Property(e => e.Telemovel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telemovel");

            entity.HasOne(d => d.IdEstadoClienteNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdEstadoCliente)
                .HasConstraintName("FK__Cliente__idEstad__5535A963");

            entity.HasOne(d => d.IdPerfilNavigation)
                  .WithMany(p => p.Clientes)
                  .HasForeignKey(d => d.IdPerfil)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasKey(e => e.IdPerfil);
            entity.Property(e => e.NomePerfil).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.HasKey(e => e.IdContrato).HasName("PK__Contrato__91431FE178D41582");

            entity.ToTable("Contrato");

            entity.Property(e => e.IdContrato).HasColumnName("idContrato");
            entity.Property(e => e.IdEstadoContrato).HasColumnName("idEstado_Contrato");

            entity.HasOne(d => d.IdEstadoContratoNavigation).WithMany(p => p.Contratos)
                .HasForeignKey(d => d.IdEstadoContrato)
                .HasConstraintName("FK__Contrato__idEsta__5812160E");
        });

        modelBuilder.Entity<ContratoCliente>(entity =>
        {
            entity.HasKey(e => new { e.IdContrato, e.IdCliente }).HasName("PK__Contrato__69C65A9F06AB7CBF");

            entity.ToTable("Contrato_Cliente");

            entity.Property(e => e.IdContrato).HasColumnName("idContrato");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdEstadoContratoCliente).HasColumnName("idEstadoContratoCliente");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ContratoClientes)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contrato___idCli__5BE2A6F2");

            entity.HasOne(d => d.IdContratoNavigation).WithMany(p => p.ContratoClientes)
                .HasForeignKey(d => d.IdContrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contrato___idCon__5AEE82B9");

            entity.HasOne(d => d.IdEstadoContratoClienteNavigation).WithMany(p => p.ContratoClientes)
                .HasForeignKey(d => d.IdEstadoContratoCliente)
                .HasConstraintName("FK__Contrato___idEst__5CD6CB2B");
        });

        modelBuilder.Entity<Contum>(entity =>
        {
            entity.HasKey(e => e.IdConta).HasName("PK__Conta__51CAE0C1B149A160");

            entity.Property(e => e.IdConta).HasColumnName("idConta");
            entity.Property(e => e.IdContrato).HasColumnName("idContrato");
            entity.Property(e => e.IdEstadoConta).HasColumnName("idEstadoConta");
            entity.Property(e => e.Montante)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NomeConta)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdContratoNavigation).WithMany(p => p.Conta)
                .HasForeignKey(d => d.IdContrato)
                .HasConstraintName("FK__Conta__idContrat__619B8048");

            entity.HasOne(d => d.IdEstadoContaNavigation).WithMany(p => p.Conta)
                .HasForeignKey(d => d.IdEstadoConta)
                .HasConstraintName("FK__Conta__idEstadoC__60A75C0F");
        });

        modelBuilder.Entity<EstadoCliente>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado_C__62EA894AFBB6D242");

            entity.ToTable("Estado_Cliente");

            entity.Property(e => e.IdEstado).HasColumnName("idEstado");
            entity.Property(e => e.Designacao)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("designacao");
        });

        modelBuilder.Entity<EstadoContrato>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado_C__62EA894A3709C9C2");

            entity.ToTable("Estado_Contrato");

            entity.Property(e => e.IdEstado).HasColumnName("idEstado");
            entity.Property(e => e.Designacao)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("designacao");
        });

        modelBuilder.Entity<EstadoContratoCliente>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado_C__62EA894AE2BCA1E2");

            entity.ToTable("Estado_Contrato_Cliente");

            entity.Property(e => e.IdEstado).HasColumnName("idEstado");
            entity.Property(e => e.Designacao)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("designacao");
        });

        modelBuilder.Entity<EstadoContum>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado_C__62EA894AB861EBDF");

            entity.ToTable("Estado_Conta");

            entity.Property(e => e.IdEstado).HasColumnName("idEstado");
            entity.Property(e => e.Designacao)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("designacao");
        });

        modelBuilder.Entity<EstadoTransacao>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado_T__62EA894A48D0DF77");

            entity.ToTable("Estado_Transacao");

            entity.Property(e => e.IdEstado).HasColumnName("idEstado");
            entity.Property(e => e.Designacao)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("designacao");
        });

        modelBuilder.Entity<TipoMovimento>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("PK__Tipo_Mov__BDD0DFE1A61D64C3");

            entity.ToTable("Tipo_Movimento");

            entity.Property(e => e.IdTipo).HasColumnName("idTipo");
            entity.Property(e => e.Descricao)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("descricao");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.HasKey(e => e.IdTransacao).HasName("PK__Transaca__455E3CA03CAD7909");

            entity.ToTable("Transacao");

            entity.Property(e => e.IdTransacao).HasColumnName("idTransacao");
            entity.Property(e => e.DataTransacao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.IdConta).HasColumnName("idConta");
            entity.Property(e => e.IdEstadoTransacao).HasColumnName("idEstadoTransacao");
            entity.Property(e => e.IdTipo).HasColumnName("idTipo");
            entity.Property(e => e.NomeTransacao)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ValorTransacao).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Transacaos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__Transacao__idCat__6B24EA82");

            entity.HasOne(d => d.IdContaNavigation).WithMany(p => p.Transacaos)
                .HasForeignKey(d => d.IdConta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacao__idCon__693CA210");

            entity.HasOne(d => d.IdEstadoTransacaoNavigation).WithMany(p => p.Transacaos)
                .HasForeignKey(d => d.IdEstadoTransacao)
                .HasConstraintName("FK__Transacao__idEst__6A30C649");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Transacaos)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK__Transacao__idTip__6C190EBB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
