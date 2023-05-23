using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
	    public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }

        public DateTime FechaHoraAlta { get; set; }
        public DateTime? FechaHoraBaja { get; set; }
        public string Token { get; set; }

        [Display(Name = "Facturas: Puede cargar Excel")]
        public bool PuedeCargarExcelFacturas { get; set; }
        [Display(Name = "Facturas: Puede eliminar Excel")]
        public bool PuedeEliminarExcelFacturas { get; set; }

        [Display(Name = "Auditoria: Puede cargar Excel de consiliación")]
        public bool PuedeCargarExcelAuditoria { get; set; }
        [Display(Name = "Auditoria: Puede eliminar Excel de consiliación")]
        public bool PuedeEliminarExcelAuditoria { get; set; }

        [Display(Name = "Auditoria: Puede auditar facturas")]
        public bool PuedeAuditarFactura { get; set; }
        [Display(Name = "Auditoria: Puede auditar consiliación")]
        public bool PuedeAuditarConsiliacion { get; set; }

        [Display(Name = "Auditoria: Puede ver")]
        public bool PuedeVerAuditorias { get; set; }
        [Display(Name = "Auditoria: Puede descargar Excel de consiliación")]
        public bool PuedeDescargarExcelAuditoria { get; set; }

        [Display(Name = "Período de Comprobantes: Puede dar de alta")]
        public bool PuedeDarDeAltaPeriodo { get; set; }
        [Display(Name = "Período de Comprobantes: Puede anular")]
        public bool PuedeDarDeBajaPeriodo { get; set; }
        [Display(Name = "Comprobantes: Puede cargar")]
        public bool PuedeCargarEnPeriodo { get; set; }
        [Display(Name = "Comprobantes: Puede eliminar")]
        public bool PuedeAnularEnPeriodo { get; set; }


        [NotMapped] //SOLO ADMIN PUEDE DESCARGAR ESTE EXCEL!!
        public bool PuedeDescargarExcelDeudaActualizada { get; set; }
    }
}