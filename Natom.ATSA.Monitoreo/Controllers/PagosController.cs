using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models.DataTable;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class PagosController : BaseController
    {
        private PagosManager manager = new PagosManager();

        // GET: Pagos
        public ActionResult Index()
        {
            var usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            ViewBag.Usuario = usuario;
            if (usuario.UsuarioId != 0)
            {
                return PartialView("~/Views/Shared/SinPermisos.cshtml");
            }

            var periodoMgr = new ComprobanteRecibidoPeriodoManager();
            ViewBag.Tipos = periodoMgr.ObtenerTipoComprobantes();
            ViewBag.Prestadores = periodoMgr.ObtenerPrestadores(string.Empty);

            return View();
        }

        [HttpPost]
        public ActionResult UpdatePago(int comprobanteId, int numPago, decimal? monto)
        {
            try
            {
                this.manager.UpdatePago(comprobanteId, numPago, monto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GetSaldoCuentaPrestador(int prestadorId)
        {
            try
            {
                var saldo = this.manager.GetSaldoCuentaPrestador(prestadorId);
                return Json(new { success = true, saldo = saldo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GetSaldoCuentaPrestadorSumaFacturas(int prestadorId)
        {
            try
            {
                var saldo = this.manager.GetSaldoCuentaPrestadorSumaFacturas(prestadorId);
                return Json(new { success = true, saldo = saldo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateObs(int comprobanteId, string observaciones)
        {
            try
            {
                this.manager.UpdateObs(comprobanteId, observaciones);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult ObtenerListadoIndex(JQueryDataTableParamModel param, int? tipoComprobanteId = null, int? prestadorId = null, int? mesPeriodo = null, int? anioPeriodo = null, int estado = 1)
        {
            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = this.manager.ObtenerCantidadFacturas();

            IEnumerable<ListarFacturasConSaldoParaPagarResult> cargasFiltradas = this.manager.ListarFacturasConSaldoParaPagar(dtParams.Search, tipoComprobanteId, prestadorId, mesPeriodo, anioPeriodo, estado);

            Func<ListarFacturasConSaldoParaPagarResult, string> orderingFunction =
                (c => dtParams.SortByColumnIndex == 0 ? c.Prestador :
                    dtParams.SortByColumnIndex == 1 ? c.PrestadorCUIT :
                    dtParams.SortByColumnIndex == 2 ? c.Comprobante :
                    dtParams.SortByColumnIndex == 3 ? c.Fecha.ToOADate().ToString().PadLeft(20, '0') :
                    dtParams.SortByColumnIndex == 4 ? c.Periodo :
                    dtParams.SortByColumnIndex == 5 ? c.Por :
                    dtParams.SortByColumnIndex == 6 ? c.Total.ToString().PadLeft(8, '0') :
                    dtParams.SortByColumnIndex == 7 ? c.Saldo.ToString().PadLeft(8, '0') :
                "");

            if (dtParams.SortingDirection == eSortingDirection.ASC)
            {
                cargasFiltradas = cargasFiltradas.OrderBy(orderingFunction);
            }
            else
            {
                cargasFiltradas = cargasFiltradas.OrderByDescending(orderingFunction);
            }

            List<ListarFacturasConSaldoParaPagarResult> displayedCargas = cargasFiltradas
                .Skip(param.start).Take(param.length).ToList();


            var result = from c in displayedCargas
                         select new object[] {
                                c.Prestador,
                                c.PrestadorCUIT,
                                c.Comprobante,
                                c.Fecha.ToString("dd/MM/yyyy"),
                                c.Periodo,
                                c.Por,
                                "$ " + c.Total.ToString("F"),
                                "$ " + c.Saldo.ToString("F"),
                                c.Pago1,
                                c.Pago2,
                                c.Pago3,
                                c.PagoObservaciones,
                                c.ComprobanteRecibidoId,
                                c.PrestadorId
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
    }
}