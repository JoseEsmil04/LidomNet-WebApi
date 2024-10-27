namespace LidomNet.Domain
{
    public class Equipo : BaseEntity<Guid>
    {
        public string? Nombre { get; set; }
        public DateTime AnioFuncadion { get; set; }
        public int Campeonatos { get; set; }
        public string? Ciudad { get; set; }
        public ICollection<Jugador>? Jugadores { get; set; } // Relacion con jugadores
        public ICollection<Partido>? PartidosLocal { get; set; } // Relacion con partidos
        public ICollection<Partido>? PartidosVisitante { get; set; }
    }
}
