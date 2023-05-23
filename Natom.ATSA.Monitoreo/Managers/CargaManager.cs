using Natom.ATSA.Monitoreo.Models;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Natom.ATSA.Monitoreo.WorkbooksManagers;

namespace Natom.ATSA.Monitoreo.Managers
{
    public class CargaManager
    {
        private DbMonitoreoContext db = new DbMonitoreoContext();

        //public ResultadoImportacion ImportarRendicion(string importData)
        //{
        //    var result = new ResultadoImportacion();
        //    RendicionCabecera rendicion = null;

        //    try
        //    {
        //        int numLine = 0;
        //        string[] lines = importData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var line in lines)
        //        {
        //            numLine++;
        //            if (numLine == 1)
        //            {
        //                rendicion = MapCabecera(line);
        //                rendicion.Detalle = new List<RendicionDetalle>();
        //                rendicion.UploadedBy = "admin";
        //                rendicion.UploadedDateTime = DateTime.Now;
        //                ValidarSiLaRendicionYaFueCargada(rendicion);
        //            }
        //            else
        //            {
        //                rendicion.Detalle.Add(MapDetalle(line));
        //            }
        //        }

        //        db.Rendiciones.Add(rendicion);
        //        db.SaveChanges();

        //        result.SinErrores = true;
        //        result.Mensaje = "Se han procesado " + rendicion.Detalle.Count + " registros correctamente.";
        //    }
        //    catch (Exception ex)
        //    {
        //        result.SinErrores = false;
        //        result.Mensaje = ex.Message;
        //    }

        //    return result;
        //}

        public IEnumerable<ListarCargasResult> ObtenerCargasConFiltros(string search, int? clinicaId = null)
        {
            IEnumerable<ListarCargasResult> query = this.db.Database.SqlQuery<ListarCargasResult>("CALL ListarCargas({0})", clinicaId);
            if (!string.IsNullOrEmpty(search))
            {
                int n;
                if (int.TryParse(search, out n))
                {
                    query = query.Where(q => q.Numero == n);
                }
                else
                {
                    search = search.ToLower();
                    query = query.Where(q => (q.Mes + "/" + q.Anio).Equals(search)
                                                || q.Clinica.ToLower().Contains(search)
                                                || q.Estado.ToLower().Contains(search));
                }
            }
            return query;
        }

        public int ObtenerCantidadCargas()
        {
            try
            {
                return this.db.Cargas.Count();
            }
            catch (Exception e)
            {

                throw e;
            }
            
        }

        private DateTime ObtenerFechaDeFormatoAAAAMMDD(string date)
        {
            int año = Convert.ToInt32(date.Substring(0, 4));
            int mes = Convert.ToInt32(date.Substring(4, 2));
            int dia = Convert.ToInt32(date.Substring(6, 2));
            return new DateTime(año, mes, dia);
        }

        public Carga ObtenerCarga(int cargaId)
        {
            return this.db.Cargas.FirstOrDefault(c => c.CargaId == cargaId);
        }

        private decimal ObtenerDecimalDeFormato8E2D(string dato)
        {
            decimal retorno = Convert.ToDecimal(dato);
            retorno /= 100;
            return retorno;
        }

        public Carga ObtenerCargaFull(int cargaId)
        {
            return this.db.Cargas
                            .Include(c => c.Clinica)
                            .Include(c => c.Facturas)
                            .Include(c => c.Facturas.Select(f => f.PrestacionObjeto))
                            .Where(c => c.CargaId == cargaId)
                            .FirstOrDefault();
        }

        public IEnumerable<Clinica> ObtenerClinicas(string clinica)
        {
            return db.Clinicas.Where(x => x.Descripcion.ToLower().Contains(clinica.ToLower()));
        }

        
        public CompararComprobantesResult CompararTxtContraComprobantes(Stream stream)
        {
            var result = new CompararComprobantesResult();
            var comprobantesEnTxt = MapComprobantesFromTxt(stream);
            var comprobantesEnAtsa = db.ComprobantesRecibidos
                                            .Include(c => c.Prestador)
                                            .Include(c => c.TipoComprobante)
                                            .ToList();


            //PASO 1: BUSCAMOS LOS COMPROBANTES DEL TXT QUE FALTAN CARGAR EN ATSA
            result.ComprobantesFaltantesEnATSA = comprobantesEnTxt
                                                    .Where(cTxt =>  !comprobantesEnAtsa.Any(c =>
                                                                        (
                                                                            cTxt.TipoComprobanteId.Equals(10) //RBO
                                                                                ? (new List<int>() { 10, 11, 12 }).Contains(c.TipoComprobanteId)
                                                                                : c.TipoComprobanteId.Equals(cTxt.TipoComprobanteId)
                                                                        )
                                                                        && c.Prestador.CUIT.Equals(cTxt.PrestadorCUIT)
                                                                        && c.Numero.Equals(cTxt.Numero)
                                                                    )
                                                    ).ToList();

            //PASO 2: BUSCAMOS LOS COMPROBANTES CARGADOS EN ATSA QUE TAMBIÉN ESTAN EN EL TXT
            result.ComprobantesEnATSA = comprobantesEnAtsa
                                                    .Where(cAtsa => comprobantesEnTxt.Any(c =>
                                                                        (
                                                                            c.TipoComprobanteId.Equals(10) //RBO
                                                                                ? (new List<int>() { 10, 11, 12 }).Contains(cAtsa.TipoComprobanteId)
                                                                                : cAtsa.TipoComprobanteId.Equals(c.TipoComprobanteId)
                                                                        )
                                                                        && c.PrestadorCUIT.Equals(cAtsa.Prestador.CUIT)
                                                                        && c.Numero.Equals(cAtsa.Numero)
                                                                    )
                                                    ).ToList();

            //PASO 3: A LOS COMPROBANTES ENCONTRADOS EN ATSA, LES PONEMOS EL FLAG InformadoEnPMIF EN 'TRUE'
            if (result.ComprobantesEnATSA.Count > 0)
            {
                var ids = result.ComprobantesEnATSA.Select(c => c.ComprobanteRecibidoId).ToList();
                while (ids.Count() > 0)
                {
                    var tandaLength = ids.Count() > 50 ? 50 : ids.Count();
                    var tanda = ids.Take(tandaLength);
                    db.Database.ExecuteSqlCommand("UPDATE ComprobanteRecibido SET InformadoEnPMIF = 1 WHERE ComprobanteRecibidoId IN (" + String.Join(",", tanda) + ")");
                    ids = ids.Skip(tandaLength).Take(ids.Count() - tandaLength).ToList();
                }                
            }

            return result;
        }

        private List<ComprobanteRecibido> MapComprobantesFromTxt(Stream stream)
        {
            var prestadores = db.Prestadores.ToList();
            var tiposComprobante = db.TiposComprobantes.ToList();
            var result = new List<ComprobanteRecibido>();
            using (StreamReader sr = new StreamReader(stream))
            {
                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    var data = line.Split('|');

                    var codigoTipoComprobante = "RBO";

                    if (data[9].StartsWith("FACTURA"))
                        codigoTipoComprobante = $"FC-{data[10]}";
                    else if (data[9].StartsWith("NOTA DE DEBITO"))
                        codigoTipoComprobante = $"ND-{data[10]}";
                    else if (data[9].StartsWith("NOTA DE CREDITO"))
                        codigoTipoComprobante = $"NC-{data[10]}";

                    //SI ES 'PERIODO' NO LO MAPEO
                    else if (data[9].StartsWith("PERIODO"))
                        continue;

                    var tipoComprobante = tiposComprobante.First(t => t.Codigo.Equals(codigoTipoComprobante));

                    var comprobante = new ComprobanteRecibido
                    {
                        Fecha = Convert.ToDateTime(data[14]),
                        FechaInformada = Convert.ToDateTime(data[1]),
                        Monto = Convert.ToDecimal(data[3].Replace('.', ',')),
                        PrestadorCUIT = data[6],
                        PrestadorRazonSocial = prestadores.FirstOrDefault(p => p.CUIT.Equals(data[6]))?.RazonSocial ?? "",
                        Numero = data[11],
                        TipoComprobanteId = tipoComprobante.TipoComprobanteId,
                        TipoComprobante = tipoComprobante
                    };

                    if (comprobante.Monto < 0)
                        comprobante.Monto *= -1;

                    result.Add(comprobante);
                }
            }
            return result;
        }

        public Carga ImportarExcel(Stream stream, int usuarioId, int mes, int anio, string clinica, int? clinicaId)
        {
            Carga carga = new Carga();
            carga.Facturas = new FacturasWorkbook().ObtenerFacturasDesdeExcel(stream);
            carga.Numero = (this.db.Cargas.Max(c => (int?)c.Numero) ?? 0) + 1;
            carga.CargadoFechaHora = DateTime.Now;
            carga.CargadoPorUsuarioId = usuarioId;
            carga.Mes = mes;
            carga.Anio = anio;

            Clinica dbClinica = this.db.Clinicas.FirstOrDefault(c => c.ClinicaId == clinicaId);
            if (dbClinica == null)
            {
                dbClinica = this.db.Clinicas.FirstOrDefault(c => c.Descripcion.ToLower().Trim().Equals(clinica.ToLower().Trim()));
            }
            if (dbClinica == null)
            {
                carga.Clinica = new Clinica();
                carga.Clinica.Descripcion = clinica;
            }
            else
            {
                carga.ClinicaId = dbClinica.ClinicaId;
            }
            this.db.Cargas.Add(carga);
            this.db.SaveChanges();

            return carga;
        }

        public void EliminarCarga(int cargaId, int usuarioId, string motivo)
        {
            Carga c = this.db.Cargas.Find(cargaId);
            this.db.Entry(c).State = System.Data.Entity.EntityState.Modified;
            c.AnuladoFechaHora = DateTime.Now;
            c.AnuladoMotivo = motivo;
            c.AnuladoPorUsuarioId = usuarioId;
            this.db.SaveChanges();
        }

        public void ValidarPeriodo(int mes, int anio, string clinica, int clinicaId)
        {
            if (clinicaId == 0) return;
            
            Clinica dbClinica = this.db.Clinicas.FirstOrDefault(c => c.ClinicaId == clinicaId);
            if (dbClinica == null)
            {
                dbClinica = this.db.Clinicas.FirstOrDefault(c => c.Descripcion.ToLower().Trim().Equals(clinica.ToLower().Trim()));
            }
            if (dbClinica == null)
            {
                throw new Exception("Debe seleccionar una clinica.");
            }
            else
            {
                if (this.db.Cargas.Any(c => !c.AnuladoFechaHora.HasValue
                                                && c.Mes == mes && c.Anio == anio
                                                && c.ClinicaId == dbClinica.ClinicaId))
                {
                    throw new Exception("Ya existe una carga para la clínica y período seleccionado.");
                }
            }
        }

        private string GetValue(IRow row, int index)
        {
            string value = null;
            try
            {
                value = row.GetCell(index).StringCellValue;
            }
            catch (Exception ex)
            {
                try
                {
                    value = row.GetCell(index).NumericCellValue.ToString();
                }
                catch (Exception ex2)
                {
                    value = row.GetCell(index).DateCellValue.ToString();
                }
            }

            return value;
        }

        private bool IsEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            else
            {
                string[] values = { "0", "1/1/0001 12:00:00 a. m." };
                return values.Contains(value);
            }
        }

        //private RendicionDetalle MapDetalle(string line)
        //{
        //    string strCodigoBarra = line.Substring(0, 60).Trim();
        //    string strFechaCobro = line.Substring(60, 8);
        //    string strSucursal = line.Substring(68, 4);
        //    string strImporteTotalPagado = line.Substring(72, 10);
        //    string strImporteParcial = line.Substring(82, 10);

        //    return new RendicionDetalle()
        //    {
        //        CodigoBarra = strCodigoBarra,
        //        FechaCobro = ObtenerFechaDeFormatoAAAAMMDD(strFechaCobro),
        //        ImporteParcial = ObtenerDecimalDeFormato8E2D(strImporteParcial),
        //        ImporteTotalPagado = ObtenerDecimalDeFormato8E2D(strImporteTotalPagado),
        //        Sucursal = strSucursal
        //    };
        //}

        //private RendicionCabecera MapCabecera(string line)
        //{
        //    string strFechaProceso = line.Substring(0, 8);
        //    string strTitulo = line.Substring(8, 13);
        //    string strNroEnte = line.Substring(21, 4);
        //    string strCantidadComprobantesCobrados = line.Substring(25, 5);
        //    string strImporteGralCobrado = line.Substring(72, 10);
        //    string strImporteTotal = line.Substring(82, 10);

        //    return new RendicionCabecera()
        //    {
        //        CantidadComprobantesCobrados = Convert.ToInt32(strCantidadComprobantesCobrados),
        //        FechaProceso = ObtenerFechaDeFormatoAAAAMMDD(strFechaProceso),
        //        ImporteGralCobrado = ObtenerDecimalDeFormato8E2D(strImporteGralCobrado),
        //        ImporteTotal = ObtenerDecimalDeFormato8E2D(strImporteTotal),
        //        NroEnte = Convert.ToInt32(strNroEnte),
        //        Titulo = strTitulo
        //    };
        //}
    }
}