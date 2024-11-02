using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidomNet.Domain
{
    public class Estadio : BaseEntity<Guid>
    {
        public string? Nombre { get; set; }
        public string? Ciudad { get; set; }
        public int Capacidad { get; set; }

        // Relación obligatoria con un equipo
        public Guid EquipoId { get; set; }
        public Equipo? Equipo { get; set; } = null!;
    }


}
