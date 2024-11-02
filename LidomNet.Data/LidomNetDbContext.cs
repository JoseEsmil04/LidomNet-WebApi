using LidomNet.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LidomNet.Data
{
    public class LidomNetDbContext : IdentityDbContext
    {
        public LidomNetDbContext(DbContextOptions<LidomNetDbContext> options) : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Estadio> Estadios { get; set; }
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Partido> Partidos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.Jugadores)
                .WithOne(e => e.Equipo)
                .HasForeignKey(j => j.EquipoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);   

            // Estadio <-> Equipo
            modelBuilder.Entity<Estadio>()
                .HasOne(e => e.Equipo)
                .WithMany()
                .HasForeignKey(e => e.EquipoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de relaciones para el Partido
            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoLocal)
                .WithMany(e => e.PartidosLocal)
                .HasForeignKey(p => p.EquipoLocalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoVisitante)
                .WithMany(e => e.PartidosVisitante)
                .HasForeignKey(p => p.EquipoVisitanteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
