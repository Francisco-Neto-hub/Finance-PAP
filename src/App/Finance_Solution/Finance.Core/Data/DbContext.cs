using Finance.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Data
{
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
        {
        }

        // Tabelas Principais
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }

        // Tabelas de Ligação e Auxiliares
        public DbSet<ContratoCliente> ContratosClientes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<TipoMovimento> TiposMovimento { get; set; }

        // Tabelas de Estado
        public DbSet<EstadoCliente> EstadosCliente { get; set; }
        public DbSet<EstadoContrato> EstadosContrato { get; set; }
        public DbSet<EstadoContratoCliente> EstadosContratoCliente { get; set; }
        public DbSet<EstadoConta> EstadosConta { get; set; }
        public DbSet<EstadoTransacao> EstadosTransacao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente"); // Garante o nome da tabela no singular ou plural conforme o SQL
                entity.HasKey(e => e.IdCliente);

                // Resolve o erro do 'by_pass'
                entity.Property(e => e.by_pass).HasColumnName("by_pass");

                // Resolve o erro do 'EstadoIdEstado'
                entity.HasOne(d => d.Estado)
                      .WithMany(p => p.Clientes)
                      .HasForeignKey(d => d.IdEstadoCliente);
            });

            // 1. Definir as Chaves Primárias explicitamente (PKs)
            modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Cliente>().HasKey(c => c.IdCliente);
            modelBuilder.Entity<Conta>().HasKey(c => c.IdConta);
            modelBuilder.Entity<Transacao>().HasKey(t => t.IdTransacao);
            modelBuilder.Entity<Contrato>().HasKey(c => c.IdContrato);

            // Tabelas de Estado (Lookup)
            modelBuilder.Entity<EstadoCliente>().HasKey(e => e.IdEstado);
            modelBuilder.Entity<EstadoContrato>().HasKey(e => e.IdEstado);
            modelBuilder.Entity<EstadoContratoCliente>().HasKey(e => e.IdEstado);
            modelBuilder.Entity<EstadoConta>().HasKey(e => e.IdEstado);
            modelBuilder.Entity<EstadoTransacao>().HasKey(e => e.IdEstado);
            modelBuilder.Entity<TipoMovimento>().HasKey(t => t.IdTipo);

            // 2. Configuração da Chave Composta (Contrato_Cliente)
            modelBuilder.Entity<ContratoCliente>()
                .HasKey(cc => new { cc.IdContrato, cc.IdCliente });

            // 3. Mapeamento de Nomes (SQL em Português/Snake_Case vs C# PascalCase)
            modelBuilder.Entity<EstadoCliente>().ToTable("Estado_Cliente");
            modelBuilder.Entity<EstadoContrato>().ToTable("Estado_Contrato");
            modelBuilder.Entity<EstadoContratoCliente>().ToTable("Estado_Contrato_Cliente");
            modelBuilder.Entity<EstadoConta>().ToTable("Estado_Conta");
            modelBuilder.Entity<EstadoTransacao>().ToTable("Estado_Transacao");
            modelBuilder.Entity<ContratoCliente>().ToTable("Contrato_Cliente");
            modelBuilder.Entity<TipoMovimento>().ToTable("Tipo_Movimento");
            modelBuilder.Entity<Transacao>().ToTable("Transacao");

            // 4. Configuração de Precisão para Valores Monetários
            modelBuilder.Entity<Conta>()
                .Property(c => c.Montante)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transacao>()
                .Property(t => t.ValorTransacao)
                .HasPrecision(18, 2);
        }
    }
}
