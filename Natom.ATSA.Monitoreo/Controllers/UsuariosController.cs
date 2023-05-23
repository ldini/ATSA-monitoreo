using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.DataTable;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class UsuariosController : BaseController
    {
        public ActionResult RecuperoDeClave(string u1du_22m2dl)
        {
            Usuario e = new UsuarioManager().ObtenerUsuarioPorToken(u1du_22m2dl);
            LoginView data = new LoginView();
            data.UsuarioId = e.UsuarioId;
            data.Email = e.Email;
            data.Clave = "";
            return View(data);
        }

        [HttpPost]
        public ActionResult RecuperoDeClave(LoginView loginData)
        {
            new UsuarioManager().GrabarPassword(loginData.UsuarioId, loginData.Clave);
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public ActionResult EnviarMailRecupero(string email)
        {
            try
            {
                new UsuarioManager().EnviarMailRecuperoDeClave(email);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

 

        public ActionResult Index()
        {
            if (this.SesionUsuarioId.Value != 0)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);

            return View();
        }

        public ActionResult Create()
        {
            if (this.SesionUsuarioId.Value != 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var manager = new UsuarioManager();
            ViewBag.Usuario = manager.ObtenerUsuario(this.SesionUsuarioId.Value);
            return View();
        }

        public ActionResult Edit(long id)
        {
            if (this.SesionUsuarioId.Value != 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var manager = new UsuarioManager();
            ViewBag.Usuario = manager.ObtenerUsuario(this.SesionUsuarioId.Value);
            return View(manager.ObtenerUsuario(id));
        }

        public ActionResult Eliminar(int id)
        {
            try
            {
                var manager = new UsuarioManager();
                manager.Eliminar(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult Grabar(Usuario usuario)
        {
            try
            {
                var manager = new UsuarioManager();
                manager.Grabar(usuario);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult ObtenerListadoIndex(JQueryDataTableParamModel param)
        {
            var manager = new UsuarioManager();
            DataTableParams dtParams = new DataTableParams(Request);
            int cantidadRegistros = manager.ObtenerCantidadUsuarios();

            IEnumerable<Usuario> cargosFiltrados = manager.ObtenerUsuariosConFiltros(dtParams.Search);

            Func<Usuario, string> orderingFunction =
                (c => dtParams.SortByColumnIndex == 0 ? (c.Nombre + " " + c.Apellido) :
                    dtParams.SortByColumnIndex == 1 ? c.Email :
                    dtParams.SortByColumnIndex == 2 ? c.FechaHoraAlta.ToOADate().ToString().PadLeft(15, '0') :
                "");

            if (dtParams.SortingDirection == eSortingDirection.ASC)
            {
                cargosFiltrados = cargosFiltrados.OrderBy(orderingFunction);
            }
            else
            {
                cargosFiltrados = cargosFiltrados.OrderByDescending(orderingFunction);
            }

            List<Usuario> displayedCargos = cargosFiltrados
                .Skip(param.start).Take(param.length).ToList();

            var result = from c in displayedCargos
                         select new object[] {
                             c.Nombre + " " + c.Apellido,
                             c.Email,
                             String.Concat(c.FechaHoraAlta.ToString("dd/MM/yyyy HH:mm"), " hs"),
                             c.UsuarioId
                            };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = cantidadRegistros,
                iTotalDisplayRecords = cargosFiltrados.Count(),
                aaData = result
            },
                        JsonRequestBehavior.AllowGet);

        }
    }
}