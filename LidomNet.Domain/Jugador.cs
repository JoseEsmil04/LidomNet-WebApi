namespace LidomNet.Domain
{
    public class Jugador : BaseEntity<Guid>
    {
        public string? Name { get; set; }
        public string? Posicion { get; set; }
        public Guid EquipoId { get; set; } // Relacion con Equipo
        public Equipo? Equipo { get; set; } // Navegacion hasta equipo
    }
}
