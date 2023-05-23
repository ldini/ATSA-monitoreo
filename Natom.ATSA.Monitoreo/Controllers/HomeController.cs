using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Usuario = new UsuarioManager().ObtenerUsuario(this.SesionUsuarioId.Value);
            return View();
        }

        public ActionResult Login(string error = "")
        {
            ViewBag.ErrorMessage = error;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginView data)
        {
            try
            {
                SesionManager mgr = new SesionManager();
                int usuarioId;
                mgr.ValidarLogin(data.Email, data.Clave, out usuarioId);
                HttpCookie myCookie = new HttpCookie("ATSAMntWebApp");
                myCookie.Value = usuarioId.ToString();
                Response.Cookies.Add(myCookie);
                Response.Redirect("/monitoreotest/home/Index"); //Response.Redirect("/liquidacionestest/monitoreotest/home/Index");
                Response.End();
                return null;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Home", new { @error = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpCookie cookie = Request.Cookies["ATSAMntWebApp"];
            Request.Cookies.Remove("ATSAMntWebApp");
            Response.Redirect("/monitoreotest/home/Login"); //Response.Redirect("/liquidacionestest/monitoreotest/home/Login");
            Response.End();
            return RedirectToAction("Login", "Home", new { @error = "" });
        }
    }
}