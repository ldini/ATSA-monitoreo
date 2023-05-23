using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.ViewModels
{
    public class LoginView
    {
        public long UsuarioId { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
    }
}