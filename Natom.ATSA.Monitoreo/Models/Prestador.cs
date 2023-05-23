using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class Prestador
    {
        public int PrestadorId { get; set; }
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
    }
}