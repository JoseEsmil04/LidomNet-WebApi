using LidomNet.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LidomNet.Data
{
    public class LidomNetDbContext : IdentityDbContext
    {
        public LidomNetDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Estadio> Estadios { get; set; }
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Partido> Partidos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
            base.OnConfiguring(optionsBuilder);


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación 1 a muchos: Equipo -> Jugadores
            modelBuilder.Entity<Jugador>()
                .HasOne(j => j.Equipo)
                .WithMany(e => e.Jugadores)
                .HasForeignKey(j => j.EquipoId)
                .OnDelete(DeleteBehavior.Cascade); // Si un equipo es eliminado, los jugadores también.

            // Relación 1 a muchos: Estadio -> Equipos (si aplicara en tu caso)
            modelBuilder.Entity<Estadio>()
                .HasOne(e => e.Equipo)
                .WithMany() // Un equipo tiene un estadio, pero no hay colección de estadios.
                .HasForeignKey(e => e.EquipoId)
                .OnDelete(DeleteBehavior.SetNull); // Eliminar un equipo no elimina el estadio.

            modelBuilder.Entity<Partido>()
                .HasKey(p => new { p.EquipoLocalId, p.EquipoVisitanteId, p.EstadioId });

            // Relación 1 a muchos: Partido -> Equipo (Local y Visitante)
            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoLocal)
                .WithMany(e => e.PartidosLocal) // Equipo puede tener varios partidos como local
                .HasForeignKey(p => p.EquipoLocalId)
                .OnDelete(DeleteBehavior.Restrict); // No permite eliminar si tiene partidos.

            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoVisitante)
                .WithMany(e => e.PartidosVisitante) // Equipo puede tener varios partidos como visitante
                .HasForeignKey(p => p.EquipoVisitanteId)
                .OnDelete(DeleteBehavior.Restrict); // No permite eliminar si tiene partidos.
        }

    }
}
