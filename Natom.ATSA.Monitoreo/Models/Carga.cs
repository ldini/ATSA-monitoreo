using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models
{
    public class Carga
    {
        public int CargaId { get; set; }
        public int Numero { get; set; }

        public int Mes { get; set; }
        public int Anio { get; set; }

        public int ClinicaId { get; set; }
        public Clinica Clinica { get; set; }

        public string NombreArchivoExcel { get; set; }

        public DateTime CargadoFechaHora { get; set; }
        public int? CargadoPorUsuarioId { get; set; }
        [ForeignKey("CargadoPorUsuarioId")]
        public Usuario CargadoPorUsuario { get; set; }

        public DateTime? AnuladoFechaHora { get; set; }
        public string AnuladoMotivo { get; set; }
        public int AnuladoPorUsuarioId { get; set; }
        [ForeignKey("AnuladoPorUsuarioId")]
        public Usuario AnuladoPorUsuario { get; set; }

        public DateTime? DescargaExcelConsiliacionFechaHora { get; set; }
        public int? DescargaExcelUsuarioId { get; set; }
        [ForeignKey("DescargaExcelUsuarioId")]
        public Usuario DescargaExcelUsuario { get; set; }

        public DateTime? CargaExcelConsiliacionFechaHora { get; set; }
        public int? CargaExcelUsuarioId { get; set; }
        [ForeignKey("CargaExcelUsuarioId")]
        public Usuario CargaExcelUsuario { get; set; }

        public List<Factura> Facturas { get; set; }
    }
}