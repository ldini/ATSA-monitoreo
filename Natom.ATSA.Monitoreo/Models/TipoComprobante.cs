using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class TipoComprobante
    {
        public int TipoComprobanteId { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int Signo { get; set; }
    }
}