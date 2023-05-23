using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.DataTable;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using Natom.ATSA.Monitoreo.WorkbooksManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class AuditoriasController : BaseController
    {
        AuditoriaManager manager = new AuditoriaManager();

        public ActionResult Index(string estado = "")
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            ViewBag.FiltroEstado = estado;
            return View();
        }

        public ActionResult VerAuditoria(int cargaid)
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!(ViewBag.Usuario as Usuario).PuedeVerAuditorias && !(ViewBag.Usuario as Usuario).PuedeAuditarFactura)
            {
                return View("~/Views/Shared/SinPermisos.cshtml");
            }
            Carga carga = new CargaManager().ObtenerCarga(cargaid);
            return View(carga);
        }

        public ActionResult VerConsiliacion(int cargaid)
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!(ViewBag.Usuario as Usuario).PuedeVerAuditorias && !(ViewBag.Usuario as Usuario).PuedeAuditarConsiliacion)
            {
                return View("~/Views/Shared/SinPermisos.cshtml");
            }
            Carga carga = new CargaManager().ObtenerCarga(cargaid);
            return View(carga);
        }

        public ActionResult ObtenerListadoIndex(JQueryDataTableParamModel param, int? clinicaId = null, string filtro = "", string status = "")
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = this.manager.ObtenerCantidadCargas();

            IEnumerable<ListarAuditoriasResult> cargasFiltradas = this.manager.ObtenerCargasConFiltros(dtParams.Search, clinicaId, status);

            if (!string.IsNullOrEmpty(filtro))
            {
                cargasFiltradas = cargasFiltradas.Where(c => c.Estado.ToLower().Contains(filtro));
            }

            Func<ListarAuditoriasResult, string> orderingFunction =
                (c => dtParams.SortByColumnIndex == 0 ? c.CargadoFechaHora.ToOADate().ToString().PadLeft(20, '0') :
                    dtParams.SortByColumnIndex == 1 ? c.Numero.ToString().PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 2 ? c.Periodo :
                    dtParams.SortByColumnIndex == 3 ? c.Clinica :
                    dtParams.SortByColumnIndex == 4 ? c.Facturas.ToString().PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 5 ? c.Estado.ToString().PadLeft(8, '0') :
                "");

            if (dtParams.SortingDirection == eSortingDirection.ASC)
            {
                cargasFiltradas = cargasFiltradas.OrderBy(orderingFunction);
            }
            else
            {
                cargasFiltradas = cargasFiltradas.OrderByDescending(orderingFunction);
            }

            List<ListarAuditoriasResult> displayedCargas = cargasFiltradas
                .Skip(param.start).Take(param.length).ToList();


            var result = from c in displayedCargas
                         select new object[] {
                                c.CargadoFechaHora.ToString("dd/MM/yyyy HH:mm") + "hs",
                                c.Numero.ToString().PadLeft(8, '0'),
                                c.Periodo,
                                c.Clinica,
                                c.Facturas,
                                c.EstadoConInfo,
                                c.CargaId,
                                c.PendienteAuditar == c.Facturas, //PUEDE ELIMINAR
                                c.EstadoEsAuditoria,
                                c.EstadoEsConsiliacion,
                                c.ExcelDescargado,
                                c.ExcelCargado,
                                c.EstadoEsPendienteDeAuditoria,
                                c.EstadoEsPendienteDeConsiliacion,
                                usuario.PuedeCargarExcelAuditoria,
                                usuario.PuedeDescargarExcelAuditoria,
                                usuario.PuedeAuditarFactura,
                                usuario.PuedeAuditarConsiliacion
                            };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = cantidadRegistros,
                iTotalDisplayRecords = cargasFiltradas.Count(),
                aaData = result
            },
                        JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult UploadExcelAuditoria(HttpPostedFileBase files, int CargaIdAuditoria)
        {
            try
            {
                Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
                if (!usuario.PuedeCargarExcelAuditoria)
                {
                    throw new Exception("No tienes permisos para subir el Excel.");
                }

                Carga carga = new CargaManager().ObtenerCargaFull(CargaIdAuditoria);
                AuditoriaWorkbook wb = new AuditoriaWorkbook(carga);
                var facturas = wb.ReadFacturasFromExcel(files.InputStream);
                this.manager.PersistirOfertasConsiliacion(CargaIdAuditoria, facturas);

                this.manager.MarcarComoSubido(CargaIdAuditoria, this.SesionUsuarioId.Value);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { @msg = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Error(string msg)
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            ViewBag.Error = msg;
            return View();
        }

        [HttpPost]
        public JsonResult ObtenerClinicas(string clinicas)
        {
            try
            {
                return Json(new
                {
                    success = true,
                    datos = from l in this.manager.ObtenerClinicas(clinicas).Take(20)
                            select new
                            {
                                id = l.ClinicaId,
                                label = l.Descripcion
                            }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ObtenerListadoFacturas(JQueryDataTableParamModel param, int cargaid, bool auditada)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = this.manager.ObtenerCantidadCargas();

            IEnumerable<Factura> facturasFiltradas = this.manager.ObtenerFacturas(dtParams.Search, cargaid);

            if (auditada)
            {
                facturasFiltradas = facturasFiltradas.Where(x => x.AuditoriaFechaHora.HasValue);
            }
            else
            {
                facturasFiltradas = facturasFiltradas.Where(x => !x.AuditoriaFechaHora.HasValue);
            }

            Func<Factura, string> orderingFunction =
                (c => dtParams.SortByColumnIndex == 0 ? c.NOrden :
                    dtParams.SortByColumnIndex == 1 ? c.XX :
                    dtParams.SortByColumnIndex == 2 ? c.IT.PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 3 ? c.Afiliado :
                    dtParams.SortByColumnIndex == 4 ? c.ApellidoYNombre :
                    dtParams.SortByColumnIndex == 5 ? c.Practica :
                    dtParams.SortByColumnIndex == 6 ? c.Can.PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 7 ? c.Prestacion :
                    dtParams.SortByColumnIndex == 8 ? c.Fecha.ToString() :
                    dtParams.SortByColumnIndex == 9 ? c.Filial.ToString().PadLeft(8, '0') :
                "");

            if (dtParams.SortingDirection == eSortingDirection.ASC)
            {
                facturasFiltradas = facturasFiltradas.OrderBy(orderingFunction);
            }
            else
            {
                facturasFiltradas = facturasFiltradas.OrderByDescending(orderingFunction);
            }

            List<Factura> displayedCargas = facturasFiltradas
                .Skip(param.start).Take(param.length).ToList();


            var result = from c in displayedCargas
                         select new object[] {
                             c.NOrden,
                             c.XX,
                             c.IT,
                             c.Afiliado,
                             c.ApellidoYNombre,
                             c.Practica,
                             c.Can,
                             c.Prestacion,
                             c.Fecha ? .ToString("dd/MM/yyyy"),
                             c.Filial.ToString().PadLeft(4,'0'),
                             c.FacturaId,
                             usuario.PuedeAuditarFactura
                            };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = cantidadRegistros,
                iTotalDisplayRecords = facturasFiltradas.Count(),
                aaData = result
            },
                        JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerListadoFacturasConsiliacion(JQueryDataTableParamModel param, int cargaid, bool consiliada)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = this.manager.ObtenerCantidadCargas();

            IEnumerable<Factura> facturasFiltradas = this.manager.ObtenerFacturasAuditoria(dtParams.Search, cargaid);

            if (consiliada)
            {
                facturasFiltradas = facturasFiltradas.Where(x => x.ConsiliacionFechaHora.HasValue);
            }
            else
            {
                facturasFiltradas = facturasFiltradas.Where(x => !x.ConsiliacionFechaHora.HasValue);
            }

            Func<Factura, string> orderingFunction =
                (c => dtParams.SortByColumnIndex == 0 ? c.NOrden :
                    dtParams.SortByColumnIndex == 1 ? c.XX :
                    dtParams.SortByColumnIndex == 2 ? c.IT.PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 3 ? c.Afiliado :
                    dtParams.SortByColumnIndex == 4 ? c.ApellidoYNombre :
                    dtParams.SortByColumnIndex == 5 ? c.Practica :
                    dtParams.SortByColumnIndex == 6 ? c.Can.PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 7 ? c.Prestacion :
                    dtParams.SortByColumnIndex == 8 ? c.Fecha.ToString() :
                    dtParams.SortByColumnIndex == 9 ? c.Filial.ToString().PadLeft(8, '0') :
                "");

            if (dtParams.SortingDirection == eSortingDirection.ASC)
            {
                facturasFiltradas = facturasFiltradas.OrderBy(orderingFunction);
            }
            else
            {
                facturasFiltradas = facturasFiltradas.OrderByDescending(orderingFunction);
            }

            List<Factura> displayedCargas = facturasFiltradas
                .Skip(param.start).Take(param.length).ToList();


            var result = from c in displayedCargas
                         select new object[] {
                             c.NOrden,
                             c.XX,
                             c.IT,
                             c.Afiliado,
                             c.ApellidoYNombre,
                             c.Practica,
                             c.Can,
                             c.Prestacion,
                             c.Fecha ? .ToString("dd/MM/yyyy"),
                             c.Filial.ToString().PadLeft(4,'0'),
                             c.FacturaId,
                             usuario.PuedeAuditarConsiliacion
                            };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = cantidadRegistros,
                iTotalDisplayRecords = facturasFiltradas.Count(),
                aaData = result
            },
                        JsonRequestBehavior.AllowGet);
        }

        public ActionResult ModalCrearAuditoria(int facturaid)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!usuario.PuedeAuditarFactura)
            {
                return PartialView("~/Views/Shared/SinPermisos.cshtml");
            }
            var f = this.manager.GetFactura(facturaid);

            var p = this.manager.ObtenerPrestaciones();

            ViewBag.Prestaciones = p  != null && p.Count() > 0 ? p.Select(x => new SelectListItem() { Value = x.PrestacionId.ToString(), Text = x.Codigo + " - " + x.Descripcion }).ToList() : new List<SelectListItem>();

            return PartialView("_ModalAuditoria", f);
        }

        
        [HttpPost]
        public ActionResult AnularAuditoria(int facturaId)
        {
            try
            {
                this.manager.AnularAuditoria(facturaId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AnularConsiliacion(int facturaId)
        {
            try
            {
                this.manager.AnularConsiliacion(facturaId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GrabarAuditoria(int facturaId, int edad, int prestacionId,
                                                string auditoriaNumero, string auditoriaIngreso, string auditoriaEgreso,
                                                string auditoriaDebito, bool auditoriaRechaza, string auditoriaRechazaMonto,
                                                string auditoriaRechazaFundamento)
        {
            try
            {
                decimal? dAuditoriaRechazaMonto = null;
                decimal d;
                auditoriaRechazaMonto = auditoriaRechazaMonto.Replace(".", ",");
                if (decimal.TryParse(auditoriaRechazaMonto, out d ))
                {
                    dAuditoriaRechazaMonto = decimal.Parse(auditoriaRechazaMonto);
                }
                

                int? facturaSiguienteId = null;
                this.manager.GrabarAuditoria(facturaId, this.SesionUsuarioId.Value, edad, prestacionId, auditoriaNumero, auditoriaIngreso,
                                                auditoriaEgreso, auditoriaDebito, auditoriaRechaza, dAuditoriaRechazaMonto,
                                                auditoriaRechazaFundamento, out facturaSiguienteId);

                return Json(new { success = true, facturaSiguienteId = facturaSiguienteId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GrabarConsiliacion(int facturaId, bool consiliacionRechaza,
                                                string consiliacionRechazaFundamento)
        {
            try
            {
                int? facturaSiguienteId = null;
                this.manager.GrabarConsiliacion(facturaId, SesionUsuarioId.Value, consiliacionRechaza, consiliacionRechazaFundamento, out facturaSiguienteId);

                return Json(new { success = true, facturaSiguienteId = facturaSiguienteId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult ModalCrearConsiliacion(int facturaid)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!usuario.PuedeAuditarFactura)
            {
                return PartialView("~/Views/Shared/SinPermisos.cshtml");
            }

            var c = this.manager.GetFactura(facturaid);

            return PartialView("_ModalConsiliacion", c);
        }

        [HttpGet]
        public ActionResult DownloadExcelAuditoria(int cargaId)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!usuario.PuedeDescargarExcelAuditoria)
            {
                throw new Exception("No tienes permisos para descargar el Excel.");
            }

            Carga carga = new CargaManager().ObtenerCargaFull(cargaId);
            AuditoriaWorkbook wb = new AuditoriaWorkbook(carga);
            var ms = wb.BuildExcel();

            string periodo = String.Format("{0}-{1}", carga.Mes.ToString().PadLeft(2, '0'), carga.Anio);
            this.StreamXLSFile(ms.GetBuffer(), "auditoria " + carga.Clinica.Descripcion.ToLower() + " " + periodo);

            this.manager.MarcarComoDescargado(cargaId, this.SesionUsuarioId.Value);

            return null;
        }

        [HttpGet]
        public ActionResult DownloadExcelDeudas(int mes, int anio)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            if (!usuario.PuedeDescargarExcelDeudaActualizada)
            {
                throw new Exception("No tienes permisos para descargar el Excel.");
            }

            List<ListarDeudasAlPeriodoResult> deudas = this.manager.ListarDeudasAlPeriodo(mes, anio).ToList();
            DeudaWorkbook wb = new DeudaWorkbook(mes, anio, deudas);
            var ms = wb.BuildExcel();

            string periodo = String.Format("{0}-{1}", mes.ToString().PadLeft(2, '0'), anio);
            this.StreamXLSFile(ms.GetBuffer(), "deudas al " + periodo);
            
            return null;
        }
    }
}