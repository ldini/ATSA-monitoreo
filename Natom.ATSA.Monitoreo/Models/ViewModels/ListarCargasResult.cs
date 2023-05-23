using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class ListarCargasResult
    {
        public int CargaId { get; set; }
        public DateTime CargadoFechaHora { get; set; }
        public int Numero { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int ClinicaId { get; set; }
        public string Clinica { get; set; }
        public int Facturas { get; set; }
        public int FacturasRechazadasEnAuditoria { get; set; }
        public int PendienteAuditar { get; set; }
        public int PendienteConsiliar { get; set; }
        public bool Anulado { get; set; }

        public string Estado { get; set; }

        [NotMapped]
        public string Periodo
        {
            get
            {
                return String.Format("{0}/{1}", this.Mes.ToString().PadLeft(2, '0'), this.Anio);
            }
        }
    }
}