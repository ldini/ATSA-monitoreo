using Natom.ATSA.Monitoreo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class UsuarioManager
    {
        private DbMonitoreoContext db = new DbMonitoreoContext();

        public Usuario ObtenerUsuario(long usuarioId)
        {
            if (usuarioId == 0)
            {
                Usuario e = new Usuario();
                e.UsuarioId = 0;
                e.Nombre = "ATSA";
                e.Email = "admin";
                e.PuedeAuditarConsiliacion = true;
                e.PuedeAuditarFactura = true;
                e.PuedeCargarExcelAuditoria = true;
                e.PuedeCargarExcelFacturas = true;
                e.PuedeDescargarExcelAuditoria = true;
                e.PuedeEliminarExcelAuditoria = true;
                e.PuedeEliminarExcelFacturas = true;
                e.PuedeVerAuditorias = true;
                e.PuedeDarDeAltaPeriodo = true;
                e.PuedeDarDeBajaPeriodo = true;
                e.PuedeCargarEnPeriodo = true;
                e.PuedeAnularEnPeriodo = true;
                e.PuedeDescargarExcelDeudaActualizada = true;   //SOLO ADMIN PUEDE DESCARGAR ESTE EXCEL!!
                return e;
            }

            return this.db.Usuarios.Find(usuarioId);
        }

        public Usuario ObtenerUsuarioPorToken(string token)
        {
            return this.db.Usuarios.First(e => e.Token.Equals(token));
        }

        public void GrabarPassword(long usuarioId, string clave)
        {
            Usuario e = this.db.Usuarios.Find(usuarioId);
            this.db.Entry(e).State = System.Data.Entity.EntityState.Modified;
            e.Clave = this.GenerarHashMD5(clave);
            e.Token = null;
            this.db.SaveChanges();
        }

        public List<Usuario> ObtenerUsuarios()
        {
            return this.db.Usuarios.Where(e => !e.FechaHoraBaja.HasValue).ToList();
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

        public int ObtenerCantidadUsuarios()
        {
            return this.db.Usuarios.Where(e => !e.FechaHoraBaja.HasValue).Count();
        }
        
        public IEnumerable<Usuario> ObtenerUsuariosConFiltros(string search)
        {
            search = search.ToLower();
            return this.db.Usuarios
                            .Where(u => !u.FechaHoraBaja.HasValue
                                        && (
                                                (u.Nombre + " " + u.Apellido).Contains(search)
                                                || u.Email.Contains(search)
                                            )
                                        );
        }

        public void Eliminar(int id)
        {
            Usuario e = this.db.Usuarios.Find(id);
            this.db.Entry(e).State = System.Data.Entity.EntityState.Modified;
            e.FechaHoraBaja = DateTime.Now;
            this.db.SaveChanges();
        }

        public void Grabar(Usuario pUsuario)
        {
            pUsuario.Email = pUsuario.Email.Trim().ToLower();
            if (this.db.Usuarios.Any(e => !e.FechaHoraBaja.HasValue && e.Email.Equals(pUsuario.Email) && e.UsuarioId != pUsuario.UsuarioId))
            {
                throw new Exception("Ya hay un usuario dado de alta con el mismo Email.");
            }

            Usuario usuario;
            if (pUsuario.UsuarioId == 0)
            {
                usuario = new Usuario();
                usuario.Token = Guid.NewGuid().ToString().Replace("-", "");
                usuario.FechaHoraAlta = DateTime.Now;
                usuario.Clave = null;
                this.db.Usuarios.Add(usuario);
            }
            else
            {
                usuario = this.db.Usuarios.Find(pUsuario.UsuarioId);
                this.db.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
            }
            
            usuario.Email = pUsuario.Email;
            usuario.Nombre = pUsuario.Nombre;
            usuario.Apellido = pUsuario.Apellido;
            usuario.PuedeAuditarConsiliacion = pUsuario.PuedeAuditarConsiliacion;
            usuario.PuedeAuditarFactura = pUsuario.PuedeAuditarFactura;
            usuario.PuedeCargarExcelAuditoria = pUsuario.PuedeCargarExcelAuditoria;
            usuario.PuedeCargarExcelFacturas = pUsuario.PuedeCargarExcelFacturas;
            usuario.PuedeDescargarExcelAuditoria = pUsuario.PuedeDescargarExcelAuditoria;
            usuario.PuedeEliminarExcelAuditoria = pUsuario.PuedeEliminarExcelAuditoria;
            usuario.PuedeEliminarExcelFacturas = pUsuario.PuedeEliminarExcelFacturas;
            usuario.PuedeVerAuditorias = pUsuario.PuedeVerAuditorias;
            usuario.PuedeDarDeAltaPeriodo = pUsuario.PuedeDarDeAltaPeriodo;
            usuario.PuedeDarDeBajaPeriodo = pUsuario.PuedeDarDeBajaPeriodo;
            usuario.PuedeCargarEnPeriodo = pUsuario.PuedeCargarEnPeriodo;
            usuario.PuedeAnularEnPeriodo = pUsuario.PuedeAnularEnPeriodo;

            EmailManager.EnviarMailSetearClave(usuario);

            this.db.SaveChanges();
        }

        public void EnviarMailRecuperoDeClave(string email)
        {
            if (!this.db.Usuarios.Any(e => !e.FechaHoraBaja.HasValue && e.Email.Equals(email.ToLower())))
            {
                throw new Exception("El Email ingresado no se encuentra registrado como Usuario.");
            }

            Usuario usuario = this.db.Usuarios.First(e => !e.FechaHoraBaja.HasValue && e.Email.Equals(email.ToLower()));
            this.db.Entry(usuario).State = System.Data.Entity.EntityState.Modified;

            usuario.Clave = null;

            usuario.Token = Guid.NewGuid().ToString().Replace("-", "");

            EmailManager.EnviarMailSetearClave(usuario);

            this.db.SaveChanges();
        }
    }
}