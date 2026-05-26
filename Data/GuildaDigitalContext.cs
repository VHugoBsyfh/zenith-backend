using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class GuildaDigitalContext : DbContext
    {
        public GuildaDigitalContext(DbContextOptions<GuildaDigitalContext> options) : base(options) { }
        public DbSet<MissaoAceita> MissoesAceitas => Set<MissaoAceita>();
        public DbSet<Grupo> Grupos => Set<Grupo>();
        public DbSet<GrupoUsuario> GrupoUsuarios => Set<GrupoUsuario>();
        public DbSet<Mensagem> Mensagens => Set<Mensagem>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Missao> Missoes => Set<Missao>();
        public DbSet<HistoricoMissao> HistoricoMissoes => Set<HistoricoMissao>();
        public DbSet<Penalidade> Penalidades => Set<Penalidade>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
        public DbSet<Contrato> Contratos => Set<Contrato>();
        public DbSet<Notificacao> Notificacoes => Set<Notificacao>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Usuarios
            modelBuilder.Entity<Usuario>()
                .ToTable("Usuarios")
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Reputacao)
                .HasColumnType("decimal(5,2)");

            // Missoes
            modelBuilder.Entity<Missao>()
                .ToTable("Missoes");

            modelBuilder.Entity<Missao>()
                .Property(m => m.Recompensa)
                .HasColumnType("decimal(10,2)");

            // FK: Missoes.IdCriador -> Usuarios.Id  (já existe no banco)
            modelBuilder.Entity<Missao>()
                .HasOne<Usuario>()              // sem navegação por enquanto
                .WithMany()                     // idem
                .HasForeignKey(m => m.IdCriador)
                .OnDelete(DeleteBehavior.Restrict);
                
            // ADICIONE ESTE BLOCO ABAIXO:
            // FK: Missoes.IdAventureiro -> Usuarios.Id
            modelBuilder.Entity<Missao>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.IdAventureiro)
                // Usamos Restrict ou NoAction para evitar erro de "Múltiplos caminhos de cascata" no SQL Server
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrupoUsuario>()
       .HasKey(gu => new { gu.IdGrupo, gu.IdUsuario });

            modelBuilder.Entity<GrupoUsuario>()
                .HasOne<Grupo>()
                .WithMany()
                .HasForeignKey(gu => gu.IdGrupo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrupoUsuario>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(gu => gu.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Mensagem>()
       .Property(m => m.MensagemTexto)
       .HasColumnName("Mensagem"); // mapeia para a coluna existente

            modelBuilder.Entity<Mensagem>()
                .HasOne<Grupo>()
                .WithMany()
                .HasForeignKey(m => m.IdGrupo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Mensagem>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MissaoAceita>()
    .HasOne<Missao>()
    .WithMany()
    .HasForeignKey(ma => ma.IdMissao)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MissaoAceita>()
                .HasOne<Grupo>()
                .WithMany()
                .HasForeignKey(ma => ma.IdGrupo)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MissaoAceita>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(ma => ma.IdUsuario)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Avaliacao>()
        .HasIndex(a => new { a.IdAvaliador, a.IdAvaliado, a.IdMissaoAceita })
        .IsUnique();

        }
    }
}
