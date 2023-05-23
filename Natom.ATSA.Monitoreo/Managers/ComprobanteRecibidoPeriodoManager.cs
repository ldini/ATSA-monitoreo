using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System.Data.Entity;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class ComprobanteRecibidoPeriodoManager
    {
        private DbMonitoreoContext db = new DbMonitoreoContext();

        public int ObtenerCantidadPeriodos(int usuarioId)
        {
            if (usuarioId == 0)
                return this.db.ComprobantesRecibidosPeriodos.Count();
            else
                return this.db.ComprobantesRecibidosPeriodos.Where(r => r.CreaUsuarioId == usuarioId).Count();
        }

        public IEnumerable<ListarComprobantePeriodoResult> ObtenerPeriodosConFiltros(int usuarioId, string search)
        {
            IEnumerable<ListarComprobantePeriodoResult> query = this.db.Database.SqlQuery<ListarComprobantePeriodoResult>("CALL ListarComprobantePeriodo({0})", usuarioId);
            int n;
            if (int.TryParse(search, out n))
            {
                query = query.Where(q => q.Mes == n || q.Anio == n);
            }
            else
            {
                search = search.ToLower();
                query = query.Where(q => q.ResponsableNombreYApellido.ToLower().Contains(search));
            }

            return query;
        }

        public ComprobanteRecibidoPeriodo ObtenerPeriodo(int id)
        {
            return this.db.ComprobantesRecibidosPeriodos.Find(id);
        }

        public ComprobanteRecibidoPeriodo ObtenerPeriodoFull(int id)
        {
            return this.db.ComprobantesRecibidosPeriodos
                                .Include(c => c.ComprobantesRecibidos)
                                .Include(c => c.ComprobantesRecibidos.Select(ce => ce.Prestador))
                                .First(c => c.ComprobanteRecibidoPeriodoId == id);
        }

        public void AnularPeriodo(int usuarioId, int id, string motivo)
        {
            var periodo = this.db.ComprobantesRecibidosPeriodos.Find(id);
            this.db.Entry(periodo).State = System.Data.Entity.EntityState.Modified;
            periodo.AnulaFechaHora = DateTime.Now;
            periodo.AnulaMotivo = motivo;
            periodo.AnulaUsuarioId = usuarioId;
            this.db.SaveChanges();
        }

        public void Grabar(int usuarioId, int comprobanteRecibidoPeriodoId, List<ComprobanteRecibido> comprobantes)
        {
            this.db.Database.ExecuteSqlCommand("DELETE FROM ComprobanteRecibido WHERE ComprobanteRecibidoPeriodoId = " + comprobanteRecibidoPeriodoId + " AND (Pago1 IS NULL AND Pago2 IS NULL AND Pago3 IS NULL)");
            comprobantes.ForEach(c => {
                c.ComprobanteRecibidoPeriodoId = comprobanteRecibidoPeriodoId;
                if (c.PrestadorId == 0)
                {
                    c.Prestador = new Prestador()
                    {
                        CUIT = c.PrestadorCUIT,
                        RazonSocial = c.PrestadorRazonSocial
                    };
                }
            });
            this.db.ComprobantesRecibidos.AddRange(comprobantes);
            this.db.SaveChanges();
        }

        public void NuevoPeriodo(int usuarioId, int mes, int anio)
        {
            if (this.db.ComprobantesRecibidosPeriodos.Any(p => !p.AnulaUsuarioId.HasValue && p.Mes == mes && p.Anio == anio && p.CreaUsuarioId == usuarioId))
            {
                throw new Exception("El período ya existe para tu usuario.");
            }
            this.db.ComprobantesRecibidosPeriodos.Add(new ComprobanteRecibidoPeriodo()
            {
                Anio = anio,
                CreaFechaHora = DateTime.Now,
                CreaUsuarioId = usuarioId,
                Mes = mes
            });
            this.db.SaveChanges();
        }

        public List<Prestador> ObtenerPrestadores(string search)
        {
            IEnumerable<Prestador> query = this.db.Prestadores;
            int n;
            if (int.TryParse(search, out n))
            {
                query = query.Where(q => q.CUIT.Equals(search));
            }
            else
            {
                search = search.ToUpper();
                query = query.Where(q => q.RazonSocial.ToUpper().Contains(search));
            }
            return query.ToList();
        }

        public List<TipoComprobante> ObtenerTipoComprobantes()
        {
            return this.db.TiposComprobantes.ToList();
        }
    }
}