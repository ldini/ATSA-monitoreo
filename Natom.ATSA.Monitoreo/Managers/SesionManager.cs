using Natom.ATSA.Monitoreo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class SesionManager
    {
        DbMonitoreoContext db = new DbMonitoreoContext();

        public bool ValidarLogin(string usuario, string clave, out int usuarioId)
        {
            usuarioId = 0;

            string userAdmin = ConfigurationManager.AppSettings["ATSA.Admin.Usuario"].ToLower();
            string userClave = ConfigurationManager.AppSettings["ATSA.Admin.Clave"];

            if (usuario.ToLower().Equals(userAdmin) && clave.Equals(userClave))
            {
                //ADMIN ATSA
                usuarioId = 0;
                return true;
            }
            else
            {
                List<Usuario> usuarios = this.db.Usuarios
                                                .Where(e => e.Email.Equals(usuario.Trim().ToLower()) && !e.FechaHoraBaja.HasValue)
                                                .ToList();

                if (usuarios.Count == 0)
                {
                    throw new Exception("Usuario inexistente");
                }
                else if (usuarios.Any(e => string.IsNullOrEmpty(e.Clave)))
                {
                    throw new Exception("El usuario debe primero confirmar su cuenta ingresando a su Email.");
                }
                else
                {
                    string claveMD5 = this.GenerarHashMD5(clave);
                    Usuario usuarioEncontrado = usuarios.FirstOrDefault(e => !string.IsNullOrEmpty(e.Clave) && e.Clave.Equals(claveMD5));
                    if (usuarioEncontrado == null)
                    {
                        throw new Exception("Clave incorrecta");
                    }
                    else
                    {
                        usuarioId = usuarioEncontrado.UsuarioId;
                        return true;
                    }
                }
            }

            return false;
        }

        private string GenerarHashMD5(string dato)
        {
            dato = dato ?? string.Empty;
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(dato);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}