using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Natom.ATSA.Monitoreo.Controllers
{
    public class BaseController : Controller
    {
        public int? SesionUsuarioId
        {
            get
            {
                HttpCookie cookie = Request.Cookies["ATSAMntWebApp"];
                if (cookie == null)
                {
                    return null;
                }
                else
                {
                    return Convert.ToInt32(cookie.Value);
                }
            }
        }

        public void StreamXLSFile(byte[] bytes, string name)
        {
            Response.Clear();
            Response.ContentType = "application/force-download";
            Response.AddHeader("content-disposition", "attachment; filename=" + name.Trim().Replace(" ", "_") + ".xls");
            Response.BinaryWrite(bytes);
            Response.End();
        }
    }
}