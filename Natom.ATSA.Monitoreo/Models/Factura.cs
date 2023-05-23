using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class Factura
    {
        public int FacturaId { get; set; }

        public int CargaId { get; set; }
        public Carga Carga { get; set; }

        public string NInterno { get; set; }    //ESTE ES EL NRO QUE SALE EN EL EXCEL AUDITORIA

        public string NOrden { get; set; }
        public string XX { get; set; }
        public string IT { get; set; }
        public string Afiliado { get; set; }
        public string ApellidoYNombre { get; set; }

        public int Edad { get; set; } //ESTE DATO SALDRÁ CALCULADO AL MOMENTO DE HACER LA CARGA BUSCANDO EL AFILIADO EN LA DB DE PRODUCCIÓN DE ATSA
    
        public string Practica { get; set; }
        public string Can { get; set; }

        public string Prestacion { get; set; }
        public int? PrestacionId { get; set; }
        [ForeignKey("PrestacionId")]
        public Prestacion PrestacionObjeto { get; set; }

        public decimal? Honor { get; set; }
        public decimal? Gastos { get; set; }
        public DateTime? Fecha { get; set; }
        public string Filial { get; set; }
        public string Cabecera { get; set; }
        public string Detalle { get; set; }
        public string ErrorCab { get; set; }
        public string ErrorDet { get; set; }

        public bool? AuditoriaAprobado { get; set; }
        public DateTime? AuditoriaFechaHora { get; set; }
        public string AuditoriaAuditorFundamento { get; set; }
        public decimal? AuditoriaAuditorMonto { get; set; }
        public int? AuditoriaUsuarioId { get; set; }
        [ForeignKey("AuditoriaUsuarioId")]
        public Usuario AuditoriaUsuario { get; set; }

        public decimal? ConsiliacionPrestadorOferta { get; set; }
        public string ConsiliacionPrestadorFundamento { get; set; }
        public bool? ConsiliacionAuditorAprueba { get; set; }
        public DateTime? ConsiliacionFechaHora { get; set; }
        public string ConsiliacionAuditorFundamento { get; set; }
        public int? ConsiliacionUsuarioId { get; set; }
        [ForeignKey("ConsiliacionUsuarioId")]
        public Usuario ConsiliacionUsuario { get; set; }

        public string AuditoriaNumero { get; set; }
        public string AuditoriaIngreso { get; set; }
        public string AuditoriaEgreso { get; set; }
        public string AuditoriaDebito { get; set; }
    }
}