using Natom.ATSA.Monitoreo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo
{
    public class DbMonitoreoContext : DbContext
    {
        public DbSet<Carga> Cargas { get; set; }
        public DbSet<Clinica> Clinicas { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Prestacion> Prestaciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ComprobanteRecibido> ComprobantesRecibidos { get; set; }
        public DbSet<ComprobanteRecibidoPeriodo> ComprobantesRecibidosPeriodos { get; set; }
        public DbSet<Prestador> Prestadores { get; set; }
        public DbSet<TipoComprobante> TiposComprobantes { get; set; }

        public DbMonitoreoContext()
            : base("name=DbMonitoreoContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}