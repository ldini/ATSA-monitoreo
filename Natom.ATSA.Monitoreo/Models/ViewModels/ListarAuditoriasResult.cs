using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class ListarAuditoriasResult
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

        public bool ExcelDescargado { get; set; }
        public bool ExcelCargado { get; set; }

        public string Estado { get; set; }

        [NotMapped]
        public bool EstadoEsAuditoria
        {
            get { return this.Estado.Equals("Auditoría"); }
        }

        [NotMapped]
        public bool EstadoEsPendienteDeAuditoria
        {
            get { return this.Estado.Equals("Pendiente de auditar"); }
        }

        [NotMapped]
        public bool EstadoEsConsiliacion
        {
            get { return this.Estado.Equals("Consiliación"); }
        }

        [NotMapped]
        public bool EstadoEsPendienteDeConsiliacion
        {
            get { return this.Estado.Equals("Pendiente de consiliación"); }
        }

        [NotMapped]
        public string EstadoConInfo
        {
            get
            {
                string estado = this.Estado;
                if (EstadoEsAuditoria)
                {
                    int porcentaje = Convert.ToInt32((Facturas - PendienteAuditar) * 100 / Facturas);
                    estado += String.Format(" ({0}%)", porcentaje);
                }
                else if (EstadoEsConsiliacion)
                {
                    int porcentaje = Convert.ToInt32((FacturasRechazadasEnAuditoria - PendienteConsiliar) * 100 / FacturasRechazadasEnAuditoria);
                    estado += String.Format(" ({0}%)", porcentaje);
                }
                return estado;
            }
        }

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