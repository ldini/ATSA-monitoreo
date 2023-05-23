using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Natom.ATSA.Monitoreo.Models
{
    public class ComprobanteRecibido
    {
        public int ComprobanteRecibidoId { get; set; }

        public int ComprobanteRecibidoPeriodoId { get; set; }
        public ComprobanteRecibidoPeriodo ComprobanteRecibidoPeriodo { get; set; }

        public DateTime FechaRecibido { get; set; }

        public int PrestadorId { get; set; }
        public Prestador Prestador { get; set; }

        public int TipoComprobanteId { get; set; }
        public TipoComprobante TipoComprobante { get; set; }

        public DateTime Fecha { get; set; }
        public int? PuntoDeVenta { get; set; }
        public string Numero { get; set; }
        public string NInterno { get; set; }
        public int? MesPeriodo { get; set; }
        public int? AnioPeriodo { get; set; }
        public bool? Refact { get; set; }
        public decimal? Monto { get; set; }
        public decimal? Debito { get; set; }
        public decimal? APagar { get; set; } /* MONTO - DEBITO */
        public bool? TipoSoporteMag { get; set; }
        public bool? Ambul { get; set; }
        public bool? Int { get; set; }
        public decimal? MontoInt { get; set; }
        public string Tipo { get; set; }
        public string Observaciones { get; set; }
        public bool? Consiliado { get; set; }

        public bool InformadoEnPMIF { get; set; }

        public DateTime? CargaFechaHora { get; set; }
        public int? CargaUsuarioId { get; set; }
        [ForeignKey("CargaUsuarioId")]
        public Usuario CargaUsuario { get; set; }

        public DateTime? FechaInformada { get; set; }

        public bool? Liquidado { get; set; }
        public string LiquidadoPor { get; set; }
        //public int? LiquidadoUsuarioId { get; set; }
        //[ForeignKey("LiquidadoUsuarioId")]
        //public Usuario LiquidadoUsuario { get; set; }

        public bool? Babich { get; set; }
        public bool? GT { get; set; }
        public bool? MT { get; set; }

        [NotMapped]
        public string PrestadorRazonSocial { get; set; }

        [NotMapped]
        public string PrestadorCUIT { get; set; }

        public decimal? Pago1 { get; set; }
        public decimal? Pago2 { get; set; }
        public decimal? Pago3 { get; set; }
        public string PagoObservaciones { get; set; }

        public bool Auditado { get; set; }
    }
}