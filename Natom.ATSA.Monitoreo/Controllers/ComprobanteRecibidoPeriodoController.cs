using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.DataTable;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class ComprobanteRecibidoPeriodoController : BaseController
    {
        private ComprobanteRecibidoPeriodoManager manager = new ComprobanteRecibidoPeriodoManager();

        // GET: ComprobanteRecibidoPeriodo
        public ActionResult Index()
        {
            var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            ViewBag.Usuario = usuario;
            ViewBag.EsAdmin = (usuario.UsuarioId == 0);
            return View();
        }

        public ActionResult Editar(int id, string sv = null)
        {
            var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            ViewBag.Usuario = usuario;
            ViewBag.PuedeAnularEnPeriodo = usuario.PuedeAnularEnPeriodo;
            ViewBag.TiposComprobantes = manager.ObtenerTipoComprobantes();
            ViewBag.SoloLectura = !string.IsNullOrEmpty(sv);
            ViewBag.PuedeAuditar = usuario.PuedeAuditarFactura || usuario.UsuarioId == 0;
            ViewBag.PuedeEditarPMIF = usuario.UsuarioId == 0 && string.IsNullOrEmpty(sv); //SI ES ADMIN Y NO ES VISTA SOLO LECTURA

            var comprobantes = manager.ObtenerPeriodoFull(id);

            //QUITO VALIDACIÓN PORQUE AL FINAL NO APLICA
            //if (usuario.UsuarioId > 0 && comprobantes.CreaUsuarioId != usuario.UsuarioId)
            //{
            //    return View("~/Views/Shared/SinPermisos.cshtml");
            //}

            int diaLimite = Convert.ToInt32(ConfigurationManager.AppSettings["ATSA.Comprobantes.DiaDelMesLimite"]);
            DateTime fechaLimite = new DateTime(comprobantes.Anio, comprobantes.Mes, diaLimite);
            ViewBag.MostrarAlertaFecha = DateTime.Now.Date > fechaLimite;

            return View(comprobantes);
        }

        [HttpPost]
        public ActionResult Grabar(int comprobanteRecibidoPeriodoId, List<ComprobanteRecibido> comprobantes)
        {
            try
            {
                var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
                ViewBag.Usuario = usuario;
                if (!usuario.PuedeCargarEnPeriodo)
                {
                    throw new Exception("No tienes permisos para grabar.");
                }
                this.manager.Grabar(this.SesionUsuarioId.Value, comprobanteRecibidoPeriodoId, comprobantes);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AnularPeriodo(int id, string motivo)
        {
            try
            {
                var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
                ViewBag.Usuario = usuario;
                if (!usuario.PuedeDarDeBajaPeriodo) /* SI NO PUEDE ELIMINAR */
                {
                    throw new Exception("No tienes permisos para anular el período.");
                }
                this.manager.AnularPeriodo(this.SesionUsuarioId.Value, id, motivo);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult NuevoPeriodo(int mes, int anio)
        {
            try
            {
                var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
                ViewBag.Usuario = usuario;
                if (!usuario.PuedeDarDeAltaPeriodo) /* SI NO PUEDE DAR DE ALTA */
                {
                    throw new Exception("No tienes permisos para crear nuevo período.");
                }
                this.manager.NuevoPeriodo(this.SesionUsuarioId.Value, mes, anio);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult ObtenerListadoIndex(JQueryDataTableParamModel param)
        {
            try
            {
                Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

                DataTableParams dtParams = new DataTableParams(Request);
                int cantidadRegistros = this.manager.ObtenerCantidadPeriodos(0/*this.SesionUsuarioId.Value*/);

                IEnumerable<ListarComprobantePeriodoResult> cargasFiltradas = this.manager.ObtenerPeriodosConFiltros(0/*this.SesionUsuarioId.Value*/, dtParams.Search);
                cargasFiltradas = cargasFiltradas.Where(c => c.Id.HasValue);

                Func<ListarComprobantePeriodoResult, string> orderingFunction =
                    (c => dtParams.SortByColumnIndex == 0 ? (c.Anio * 12 + c.Mes).ToString().PadLeft(10, '0') :
                        dtParams.SortByColumnIndex == 1 ? c.TotalComprobantes.ToString().PadLeft(8, '0') :
                        dtParams.SortByColumnIndex == 2 ? c.SumaTotal.ToString().PadLeft(8, '0') :
                    "");

                if (dtParams.SortingDirection == eSortingDirection.ASC)
                {
                    cargasFiltradas = cargasFiltradas.OrderBy(orderingFunction);
                }
                else
                {
                    cargasFiltradas = cargasFiltradas.OrderByDescending(orderingFunction);
                }

                List<ListarComprobantePeriodoResult> displayedCargas = cargasFiltradas
                    .Skip(param.start).Take(param.length).ToList();


                var result = from c in displayedCargas
                             select new object[] {
                                c.Periodo,
                                c.TotalComprobantes,
                                "$ " + (c.SumaTotal ?? 0).ToString("F"),
                                c.ResponsableNombreYApellido,
                                c.Id,
                                c.Anulado,
                                usuario.PuedeCargarEnPeriodo, /* Puede cargar */
                                usuario.PuedeDarDeBajaPeriodo, /* Puede eliminar */
                                (this.SesionUsuarioId.Value == 0) || (c.ResponsableUsuarioId == this.SesionUsuarioId.Value), /* Puede editarlo */
                                (this.SesionUsuarioId.Value == 0) /* Es admin */
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
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public ActionResult ObtenerInfoAnulacion(int id)
        {
            ComprobanteRecibidoPeriodo periodo = this.manager.ObtenerPeriodo(id);
            Usuario anulo = new UsuarioManager().ObtenerUsuario(periodo.AnulaUsuarioId.Value);
            return Json(new
            {
                anulo = anulo.Nombre + " " + anulo.Apellido,
                fechaHora = periodo.AnulaFechaHora.Value.ToString("dd/MM/yyyy HH:mm") + "hs",
                motivo = periodo.AnulaMotivo
            });
        }

        [HttpPost]
        public JsonResult ObtenerPrestadores(string prestadores)
        {
            try
            {
                return Json(new
                {
                    success = true,
                    datos = from l in this.manager.ObtenerPrestadores(prestadores).Take(20)
                            select new
                            {
                                id = l.PrestadorId,
                                label = l.RazonSocial + " /// " + " CUIT: " + l.CUIT
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
    }
}