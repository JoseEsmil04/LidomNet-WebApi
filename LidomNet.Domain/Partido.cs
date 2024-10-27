using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidomNet.Domain
{
    public class Partido
    {
        public DateTime? Fecha { get; set; }
        public Guid? EquipoLocalId { get; set; }
        public Equipo? EquipoLocal { get; set; }

        public Guid? EquipoVisitanteId { get; set; }
        public Equipo? EquipoVisitante { get; set; }

        public Guid? EstadioId { get; set; }
        public Estadio? Estadio { get; set; }

        public int MarcadorLocal { get; set; }
        public int MarcadorVisitante { get; set; }
    }

}
