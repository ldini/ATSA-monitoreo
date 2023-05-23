using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class ComprobanteRecibidoPeriodo
    {
        public int ComprobanteRecibidoPeriodoId { get; set; }

        public int Mes { get; set; }
        public int Anio { get; set; }

        public int CreaUsuarioId { get; set; }
        [ForeignKey("CreaUsuarioId")]
        public Usuario CreaUsuario { get; set; }
        public DateTime CreaFechaHora { get; set; }

        public int? AnulaUsuarioId { get; set; }
        [ForeignKey("AnulaUsuarioId")]
        public Usuario AnulaUsuario { get; set; }

        public DateTime? AnulaFechaHora { get; set; }
        public string AnulaMotivo { get; set; }

        public List<ComprobanteRecibido> ComprobantesRecibidos { get; set; }
    }
}