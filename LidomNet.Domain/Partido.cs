using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidomNet.Domain
{
    public class Partido : BaseEntity<Guid>
    {
        public DateTime Fecha { get; set; }
        public Guid EquipoLocalId { get; set; }
        public Equipo? EquipoLocal { get; set; } = null!;

        public Guid EquipoVisitanteId { get; set; }
        public Equipo? EquipoVisitante { get; set; } = null!;

        public Guid EstadioId { get; set; }
        public Estadio? Estadio { get; set; } = null!;

        public int MarcadorLocal { get; set; }
        public int MarcadorVisitante { get; set; }
    }


}
