using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class ListarDeudasAlPeriodoResult
    {
        public string NroFactura { get; set; }
        public int ClinicaId { get; set; }
		public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Periodo { get; set; }
        public decimal ImporteFactura { get; set; }
        public decimal SaldoFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public bool CargadoPorAdmin { get; set; }
    }
}