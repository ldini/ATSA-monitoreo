using Natom.ATSA.Monitoreo.Controllers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class AuditoriaManager : BaseController
    {
        private DbMonitoreoContext db = new DbMonitoreoContext();

        public IEnumerable<ListarDeudasAlPeriodoResult> ListarDeudasAlPeriodo(int mes, int anio)
        {
            long periodo = Convert.ToInt64(anio.ToString() + mes.ToString().PadLeft(2, '0'));
            IEnumerable<ListarDeudasAlPeriodoResult> query = this.db.Database.SqlQuery<ListarDeudasAlPeriodoResult>("call ListarDeudasAlPeriodo({0})", periodo);
            return query;
        }

        public IEnumerable<ListarAuditoriasResult> ObtenerCargasConFiltros(string search, int? clinicaId = null, string status = "")
        {
            IEnumerable<ListarAuditoriasResult> query = this.db.Database.SqlQuery<ListarAuditoriasResult>("CALL ListarAuditorias({0})", clinicaId);
            if (!string.IsNullOrEmpty(search))
            {
                int n;
                if (int.TryParse(search, out n))
                {
                    query = query.Where(q => q.Numero == n);
                }
                else
                {
                    search = search.ToLower();
                    query = query.Where(q => (q.Mes + "/" + q.Anio).Equals(search)
                                                || q.Clinica.ToLower().Contains(search)
                                                || q.Estado.ToLower().Contains(search));
                }
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Estado.Equals(status));
            }
            return query;
        }

        public int ObtenerCantidadCargas()
        {
            return this.db.Cargas.Count();
        }

        private DateTime ObtenerFechaDeFormatoAAAAMMDD(string date)
        {
            int año = Convert.ToInt32(date.Substring(0, 4));
            int mes = Convert.ToInt32(date.Substring(4, 2));
            int dia = Convert.ToInt32(date.Substring(6, 2));
            return new DateTime(año, mes, dia);
        }

        private decimal ObtenerDecimalDeFormato8E2D(string dato)
        {
            decimal retorno = Convert.ToDecimal(dato);
            retorno /= 100;
            return retorno;
        }

        public IEnumerable<Clinica> ObtenerClinicas(string clinica)
        {
            return db.Clinicas.Where(x => x.Descripcion.ToLower().Contains(clinica.ToLower()));
        }

        public IEnumerable<Factura> ObtenerFacturas(string search, int cargaid)
        {
            try
            {
                var f = db.Facturas.Where(x => x.CargaId == cargaid);

                if(!string.IsNullOrEmpty(search))
                {
                    int n;
                    if(int.TryParse(search, out n))
                    {
                        f = f.Where(x => x.NOrden.Equals(search) 
                        || x.Afiliado.Equals(search)
                        || x.Practica.Equals(search)
                        || x.Filial.Equals(search));
                    }
                    else
                    {
                        search = search.ToLower();
                        f = f.Where(x => x.ApellidoYNombre.ToLower().Contains(search)
                                            || x.Prestacion.ToLower().Contains(search));
                    }
                }

                return f;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<Factura> ObtenerFacturasAuditoria(string search, int cargaid)
        {
            try
            {
                var f = db.Facturas.Where(x => x.CargaId == cargaid && x.AuditoriaAprobado == false);

                if (!string.IsNullOrEmpty(search))
                {
                    int n;
                    if (int.TryParse(search, out n))
                    {
                        f = f.Where(x => x.NOrden.Equals(search)
                        || x.Afiliado.Equals(search)
                        || x.Practica.Equals(search)
                        || x.Filial.Equals(search));
                    }
                    else
                    {
                        search = search.ToLower();
                        f = f.Where(x => x.ApellidoYNombre.ToLower().Contains(search)
                                            || x.Prestacion.ToLower().Contains(search));
                    }
                }

                return f;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Factura GetFactura(int facturaid)
        {
            try
            {
                return db.Facturas
                    .Include("Carga")
                    .Include("Carga.Clinica")
                    .Include("PrestacionObjeto")
                    .FirstOrDefault(x => x.FacturaId == facturaid);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void PersistirOfertasConsiliacion(int cargaId, List<Factura> facturas)
        {
            List<int> ids = facturas.Select(f => f.FacturaId).ToList();
            List<Factura> facturasDb = this.db.Facturas.Where(f => ids.Contains(f.FacturaId)).ToList();
            foreach (Factura fdb in facturasDb)
            {
                Factura f = facturas.First(fa => fa.FacturaId == fdb.FacturaId);
                this.db.Entry(fdb).State = System.Data.Entity.EntityState.Modified;
                fdb.ConsiliacionPrestadorOferta = f.ConsiliacionPrestadorOferta;
                fdb.ConsiliacionPrestadorFundamento = f.ConsiliacionPrestadorFundamento;
            }
            this.db.SaveChanges();
        }

        public List<Prestacion> ObtenerPrestaciones()
        {
            try
            {
                return db.Prestaciones.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void MarcarComoDescargado(int cargaId, int usuarioId)
        {
            Carga carga = this.db.Cargas.Find(cargaId);
            this.db.Entry(carga).State = System.Data.Entity.EntityState.Modified;
            carga.DescargaExcelConsiliacionFechaHora = DateTime.Now;
            carga.DescargaExcelUsuarioId = usuarioId;
            this.db.SaveChanges();
        }

        public void AnularAuditoria(int facturaId)
        {
            Factura factura = this.db.Facturas.Find(facturaId);
            this.db.Entry(factura).State = System.Data.Entity.EntityState.Modified;
            factura.Edad = 0;
            factura.PrestacionId = null;
            factura.AuditoriaNumero = null;
            factura.AuditoriaIngreso = null;
            factura.AuditoriaEgreso = null;
            factura.AuditoriaDebito = null;
            factura.AuditoriaAprobado = null;
            factura.AuditoriaFechaHora = null;
            factura.AuditoriaAuditorMonto = null;
            factura.AuditoriaAuditorFundamento = null;

            this.db.SaveChanges();
        }

        public void AnularConsiliacion(int facturaId)
        {
            Factura factura = this.db.Facturas.Find(facturaId);
            this.db.Entry(factura).State = System.Data.Entity.EntityState.Modified;
            factura.ConsiliacionAuditorAprueba = null;
            factura.ConsiliacionAuditorFundamento = null;
            factura.ConsiliacionFechaHora = null;
            factura.ConsiliacionUsuarioId = null;

            this.db.SaveChanges();
        }

        public void MarcarComoSubido(int cargaId, int usuarioId)
        {
            Carga carga = this.db.Cargas.Find(cargaId);
            this.db.Entry(carga).State = System.Data.Entity.EntityState.Modified;
            carga.CargaExcelConsiliacionFechaHora = DateTime.Now;
            carga.CargaExcelUsuarioId = usuarioId;
            this.db.SaveChanges();
        }

        public void GrabarAuditoria(int facturaId, int usuarioId, int edad, int prestacionId, string auditoriaNumero,
                                        string auditoriaIngreso, string auditoriaEgreso,
                                        string auditoriaDebito, bool auditoriaRechaza,
                                        decimal? auditoriaRechazaMonto, string auditoriaRechazaFundamento,
                                        out int? facturaSiguienteId)
        {
            facturaSiguienteId = null;
            Factura factura = this.db.Facturas.Find(facturaId);

            if (this.db.Facturas.Any(f => f.CargaId == factura.CargaId && f.AuditoriaNumero.Trim().Equals(auditoriaNumero.Trim())))
            {
                throw new Exception("Ya existe el número de auditoría " + auditoriaNumero);
            }

            this.db.Entry(factura).State = System.Data.Entity.EntityState.Modified;
            factura.Edad = edad;
            factura.PrestacionId = prestacionId;
            factura.AuditoriaNumero = auditoriaNumero;
            factura.AuditoriaIngreso = auditoriaIngreso;
            factura.AuditoriaEgreso = auditoriaEgreso;
            factura.AuditoriaDebito = auditoriaDebito;
            factura.AuditoriaAprobado = !auditoriaRechaza;
            factura.AuditoriaFechaHora = DateTime.Now;
            factura.AuditoriaUsuarioId = usuarioId;
            if (auditoriaRechaza)
            {
                factura.AuditoriaAuditorMonto = auditoriaRechazaMonto;
                factura.AuditoriaAuditorFundamento = auditoriaRechazaFundamento;
            }
            this.db.SaveChanges();

            facturaSiguienteId = this.db.Facturas
                                            .Where(f => f.CargaId == factura.CargaId
                                                        && !f.AuditoriaFechaHora.HasValue)
                                            .OrderByDescending(f => f.Filial.Equals("1200") ? "999999999999" : f.Filial)
                                            .Select(f => f.FacturaId)
                                            .FirstOrDefault();

        }

        public void GrabarConsiliacion(int facturaId, int usuarioId, bool rechazada, string rechazadaMotivo, out int? facturaSiguienteId)
        {
            Factura factura = this.db.Facturas.Find(facturaId);
            this.db.Entry(factura).State = System.Data.Entity.EntityState.Modified;

            factura.ConsiliacionAuditorAprueba = !rechazada;
            factura.ConsiliacionAuditorFundamento = rechazada ? rechazadaMotivo : null;
            factura.ConsiliacionUsuarioId = usuarioId;
            factura.ConsiliacionFechaHora = DateTime.Now;

            this.db.SaveChanges();

            facturaSiguienteId = this.db.Facturas
                                            .Where(f => f.CargaId == factura.CargaId
                                                        && f.AuditoriaAprobado == false
                                                        && !f.ConsiliacionFechaHora.HasValue)
                                            .OrderByDescending(f => f.Filial.Equals("1200") ? "999999999999" : f.Filial)
                                            .Select(f => f.FacturaId)
                                            .FirstOrDefault();
        }
    }
}