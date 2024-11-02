namespace LidomNet.Domain
{
    public class Equipo : BaseEntity<Guid>
    {
        public string Nombre { get; set; } = null!;
        public DateTime AnioFundacion { get; set; }
        public int Campeonatos { get; set; }
        public string Ciudad { get; set; } = null!;

        public ICollection<Jugador>? Jugadores { get; set; }
        public ICollection<Partido>? PartidosLocal { get; set; }
        public ICollection<Partido>? PartidosVisitante { get; set; }
    }
}
