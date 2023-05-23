using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.DataTable;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class CargasController : BaseController
    {
        private CargaManager manager = new CargaManager();


        public ActionResult Index()
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            return View();
        }

        public ActionResult ObtenerListadoIndex(JQueryDataTableParamModel param, int? clinicaId = null)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = this.manager.ObtenerCantidadCargas();

            IEnumerable<ListarCargasResult> cargasFiltradas = this.manager.ObtenerCargasConFiltros(dtParams.Search, clinicaId);

            Func<ListarCargasResult, string> orderingFunction =
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

            List<ListarCargasResult> displayedCargas = cargasFiltradas
                .Skip(param.start).Take(param.length).ToList();


            var result = from c in displayedCargas
                         select new object[] {
                                c.CargadoFechaHora.ToString("dd/MM/yyyy HH:mm") + "hs",
                                c.Numero.ToString().PadLeft(8, '0'),
                                c.Periodo,
                                c.Clinica,
                                c.Facturas,
                                c.Estado,
                                c.CargaId,
                                c.PendienteAuditar == c.Facturas, //PUEDE ELIMINAR
                                c.Anulado,
                                usuario.PuedeEliminarExcelFacturas
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
        public ActionResult ObtenerInfoAnulacion(int cargaId)
        {
            Carga carga = this.manager.ObtenerCarga(cargaId);
            Usuario anulo = new UsuarioManager().ObtenerUsuario(carga.AnuladoPorUsuarioId);
            return Json(new
            {
                anulo = anulo.Nombre + " " + anulo.Apellido,
                fechaHora = carga.AnuladoFechaHora.Value.ToString("dd/MM/yyyy HH:mm") + "hs",
                motivo = carga.AnuladoMotivo
            });
        }

        [HttpPost]
        public ActionResult UploadExcelFacturas(HttpPostedFileBase files, int Mes, int Anio, string Clinica, int ClinicaId)
        {            
            this.manager.ImportarExcel(files.InputStream, this.SesionUsuarioId.Value, Mes, Anio, Clinica, ClinicaId);

            //string result = System.Text.Encoding.UTF8.GetString(binData);
            //ResultadoImportacion resultado = manager.ImportarRendicion(result);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UploadComparacionComprobantes(HttpPostedFileBase filesComprobantes)
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            var result = this.manager.CompararTxtContraComprobantes(filesComprobantes.InputStream);

            return View("ComparacionComprobantes", result);
        }

        [HttpPost]
        public ActionResult ValidarPeriodo(int Mes, int Anio, string Clinica, int ClinicaId)
        {
            try
            {
                this.manager.ValidarPeriodo(Mes, Anio, Clinica, ClinicaId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarCarga(int CargaId, string motivo)
        {
            try
            {
                int usuarioId = this.SesionUsuarioId.Value;
                Usuario usuario = new UsuarioManager().ObtenerUsuario(usuarioId);

                if (!usuario.PuedeEliminarExcelFacturas)
                {
                    throw new Exception("No tienes permisos para eliminar las cargas de factura.");
                }

                this.manager.EliminarCarga(CargaId, usuarioId, motivo);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
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
    }
}