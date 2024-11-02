using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LidomNet.Domain
{
    public class Jugador : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string Posicion { get; set; } = null!;

        // Relacion obligatoria con equipo
        [Required]
        public Guid EquipoId { get; set; }
        public virtual Equipo? Equipo { get; set; }
    }
}
