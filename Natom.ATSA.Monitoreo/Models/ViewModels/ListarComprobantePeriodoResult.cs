using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class ListarComprobantePeriodoResult
    {
        public int? Id { get; set; }
        public int? Mes { get; set; }
        public int? Anio { get; set; }
        public int? TotalComprobantes { get; set; }
        public decimal? SumaTotal { get; set; }
        public bool Anulado { get; set; }

        public int ResponsableUsuarioId { get; set; }
        public string ResponsableNombreYApellido { get; set; }

        [NotMapped]
        public string Periodo
        {
            get { return Mes == null ? string.Empty : String.Format("{0}-{1}", Mes.ToString().PadLeft(2, '0'), Anio.ToString()); }
        }
    }
}