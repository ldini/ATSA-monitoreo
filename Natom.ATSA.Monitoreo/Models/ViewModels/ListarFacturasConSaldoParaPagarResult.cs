using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class ListarFacturasConSaldoParaPagarResult
    {
        public int PrestadorId { get; set; }
        public int ComprobanteRecibidoId { get; set; }
        public int ComprobanteRecibidoPeriodoId { get; set; }
        public string Prestador { get; set; }
        public string PrestadorCUIT { get; set; }
        public string Comprobante { get; set; }
        public DateTime Fecha { get; set; }
        public string Periodo { get; set; }
        public string Por { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public decimal? Pago1 { get; set; }
        public decimal? Pago2 { get; set; }
        public decimal? Pago3 { get; set; }
        public string PagoObservaciones { get; set; }
    }
}