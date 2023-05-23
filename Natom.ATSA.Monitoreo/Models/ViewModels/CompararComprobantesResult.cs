using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class CompararComprobantesResult
    {
        public List<ComprobanteRecibido> ComprobantesFaltantesEnATSA { get; set; }
        public List<ComprobanteRecibido> ComprobantesEnATSA { get; set; }
    }
}