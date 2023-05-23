using Natom.ATSA.Monitoreo.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class PagosManager
    {
        private DbMonitoreoContext db = new DbMonitoreoContext();

        public IEnumerable<ListarFacturasConSaldoParaPagarResult> ListarFacturasConSaldoParaPagar(string filtro, int? tipoComprobanteId, int? prestadorId, int? mesPeriodo, int? anioPeriodo, int estado = 1)
        {
            IEnumerable<ListarFacturasConSaldoParaPagarResult> query = this.db.Database.SqlQuery<ListarFacturasConSaldoParaPagarResult>("CALL ListarFacturasConSaldoParaPagar({0}, {1}, {2}, {3})", tipoComprobanteId, prestadorId, mesPeriodo, anioPeriodo);
            switch (estado)
            {
                case 1: //CON SALDO POSITIVO
                    query = query.Where(q => q.Saldo > 0);
                    break;
                case 2: //CON SALDO NEGATIVO (a favor)
                    query = query.Where(q => q.Saldo < 0);
                    break;
                case 3: //SIN SALDO
                    query = query.Where(q => q.Saldo == 0);
                    break;
                case 4: // TODOS
                    //NADA
                    break;
            }

            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.ToLower();
                query = query.Where(q => q.Prestador.ToLower().Contains(filtro)
                                        || q.PrestadorCUIT.ToLower().Contains(filtro)
                                        || q.Comprobante.ToLower().Contains(filtro)
                                        || q.Periodo.Contains(filtro)
                                        || q.Por.ToLower().Contains(filtro));
            }
            

            return query;
        }

        public void UpdateObs(int comprobanteId, string observaciones)
        {
            var comprobante = this.db.ComprobantesRecibidos.Where(a => a.ComprobanteRecibidoId == comprobanteId).First();
            this.db.Entry(comprobante).State = System.Data.Entity.EntityState.Modified;
            comprobante.PagoObservaciones = observaciones;
            this.db.SaveChanges();
        }

        public decimal GetSaldoCuentaPrestador(int prestadorId)
        {
            var saldo = this.db.ComprobantesRecibidos.Where(p => p.PrestadorId == prestadorId)
                                                        .Sum(c => (decimal?)(((c.Monto ?? 0) - (c.Debito ?? 0)) - ((c.Pago1 ?? 0) + (c.Pago2 ?? 0) + (c.Pago3 ?? 0))));

            return saldo ?? 0;
        }

        public decimal GetSaldoCuentaPrestadorSumaFacturas(int prestadorId)
        {
            var saldo = this.db.ComprobantesRecibidos.Where(c => c.PrestadorId == prestadorId
                                                                    && (((decimal?)(((c.Monto ?? 0) - (c.Debito ?? 0)) - ((c.Pago1 ?? 0) + (c.Pago2 ?? 0) + (c.Pago3 ?? 0)))) ?? 0) < 0)
                                                        .Sum(c => ((decimal?)(((c.Monto ?? 0) - (c.Debito ?? 0)) - ((c.Pago1 ?? 0) + (c.Pago2 ?? 0) + (c.Pago3 ?? 0)))) ?? 0 * -1);

            return saldo;
        }

        public void UpdatePago(int comprobanteId, int numPago, decimal? monto)
        {
            var comprobante = this.db.ComprobantesRecibidos.Where(a => a.ComprobanteRecibidoId == comprobanteId).First();

            if (monto.HasValue)
            {
                var total = comprobante.Monto - comprobante.Debito;
                var saldo = total - (comprobante.Pago1 ?? 0) - (comprobante.Pago2 ?? 0) - (comprobante.Pago3 ?? 0);
                //if (saldo < monto)
                //{
                //    throw new Exception("El monto no puede ser superior al saldo ($ " + saldo.Value.ToString("F") + ")");
                //}
            }

            if (numPago == 1)
                comprobante.Pago1 = monto;
            else if (numPago == 2)
                comprobante.Pago2 = monto;
            else if (numPago == 3)
                comprobante.Pago3 = monto;

            this.db.Entry(comprobante).State = System.Data.Entity.EntityState.Modified;
            this.db.SaveChanges();
        }

        public int ObtenerCantidadFacturas()
        {
            return this.db.ComprobantesRecibidos.Where(c => !c.ComprobanteRecibidoPeriodo.AnulaUsuarioId.HasValue).Count();
        }
    }
}