using Natom.ATSA.Monitoreo.Managers;
using Natom.ATSA.Monitoreo.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.WorkbooksManagers
{
    public class AuditoriaWorkbook
    {
        private Carga _carga = null;

        public AuditoriaWorkbook(int cargaId)
        {
            _carga = new CargaManager().ObtenerCargaFull(cargaId);
        }

        public AuditoriaWorkbook(Carga carga)
        {
            _carga = carga;
        }

        public MemoryStream BuildExcel()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            ISheet sheet = wb.CreateSheet("Hoja1");

            int iRow = 0;
            this.CreateHeader(_carga, ref wb, ref sheet, ref iRow);

            sheet.SetColumnWidth(0, 130 * 30);
            sheet.SetColumnWidth(1, 310 * 30);
            sheet.SetColumnWidth(2, 100 * 30);
            sheet.SetColumnWidth(3, 150 * 30);
            sheet.SetColumnWidth(4, 90 * 30);
            sheet.SetColumnWidth(5, 90 * 30);
            sheet.SetColumnWidth(6, 200 * 30);
            sheet.SetColumnWidth(7, 141 * 30);
            sheet.SetColumnWidth(8, 100 * 30);
            sheet.SetColumnWidth(9, 420 * 30);
            sheet.SetColumnWidth(10, 80 * 30);
            sheet.SetColumnWidth(11, 120 * 30);
            sheet.SetColumnWidth(12, 480 * 30);

            foreach (var factura in _carga.Facturas.Where(f => f.AuditoriaAprobado == false))
            {
                iRow++;
                var row = sheet.CreateRow(iRow);
                row.CreateCell(0).SetCellValue(factura.AuditoriaNumero);
                row.CreateCell(1).SetCellValue(factura.ApellidoYNombre);
                row.CreateCell(2).SetCellValue(factura.Edad);
                row.CreateCell(3).SetCellValue(factura.Afiliado);
                row.CreateCell(4).SetCellValue(factura.AuditoriaIngreso);
                row.CreateCell(5).SetCellValue(factura.AuditoriaEgreso);
                row.CreateCell(6).SetCellValue(factura.Prestacion);
                row.CreateCell(7).SetCellValue(factura.PrestacionObjeto.Codigo);
                row.CreateCell(8).SetCellValue(factura.AuditoriaDebito);
                row.CreateCell(9).SetCellValue(factura.AuditoriaAuditorFundamento);
                row.CreateCell(10).SetCellValue(Convert.ToDouble(factura.AuditoriaAuditorMonto ?? 0));
                row.CreateCell(11).SetCellValue(0);
                row.CreateCell(12).SetCellValue("");
            }

            MemoryStream ms = new MemoryStream();
            using (MemoryStream tempStream = new MemoryStream())
            {
                wb.Write(tempStream);
                var byteArray = tempStream.ToArray();
                ms.Write(byteArray, 0, byteArray.Length);
            }

            return ms;
        }

        private void CreateHeader(Carga carga, ref HSSFWorkbook wb, ref ISheet sheet, ref int iRow)
        {
            var fontBold = wb.CreateFont();
            fontBold.IsBold = true;

            var fontBoldTitle = wb.CreateFont();
            fontBoldTitle.IsBold = true;
            fontBoldTitle.FontHeightInPoints = 14;


            var fontBoldSubTitle = wb.CreateFont();
            fontBoldSubTitle.IsBold = true;
            fontBoldSubTitle.FontHeightInPoints = 14;

            var fontBoldColHeader = wb.CreateFont();
            fontBoldColHeader.IsBold = true;
            fontBoldColHeader.FontHeightInPoints = 10;

            //PRIMER ROW
            var row = sheet.CreateRow(iRow);
            row.CreateCell(0).SetCellValue("");
            row.CreateCell(1).SetCellValue("");
            row.CreateCell(2).SetCellValue("");
            row.CreateCell(3).SetCellValue("");
            var cellTitle = row.CreateCell(4);
            cellTitle.SetCellValue("AUDITORIA OSPSA ZONA OESTE");
            cellTitle.RichStringCellValue.ApplyFont(fontBoldTitle);

            row.CreateCell(5).SetCellValue("");
            row.CreateCell(6).SetCellValue("");
            row.CreateCell(7).SetCellValue("");
            row.CreateCell(8).SetCellValue("");
            row.CreateCell(9).SetCellValue("");
            row.CreateCell(10).SetCellValue("");
            row.CreateCell(11).SetCellValue("");
            row.CreateCell(12).SetCellValue("");
            var mergeCellTitle = new NPOI.SS.Util.CellRangeAddress(0, 0, 4, 8);
            sheet.AddMergedRegion(mergeCellTitle);


            //SEGUNDO ROW
            iRow++;
            row = sheet.CreateRow(iRow);
            row.CreateCell(0).SetCellValue("");
            var cellMes = row.CreateCell(1);
            cellMes.SetCellValue("Mes:");
            cellMes.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellMes2 = row.CreateCell(2);
            cellMes2.SetCellValue(String.Format("{0}/{1}", carga.Mes.ToString().PadLeft(2, '0'), carga.Anio));
            cellMes2.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            row.CreateCell(3).SetCellValue("");
            row.CreateCell(4).SetCellValue("");
            row.CreateCell(5).SetCellValue("");
            row.CreateCell(6).SetCellValue("");
            row.CreateCell(7).SetCellValue("");
            row.CreateCell(8).SetCellValue("");
            var cellExtra = row.CreateCell(9);
            cellExtra.SetCellValue("QX--cirugía  // CL--clínica // P---pediat. // NEO--neonat. // OB---obstetricia.\n        00       //       00        //         00     //       00           //          00 ");
            cellExtra.RichStringCellValue.ApplyFont(fontBold);
            row.CreateCell(10).SetCellValue("TOTAL INTERN\n        00");
            row.CreateCell(11).SetCellValue("");
            row.CreateCell(12).SetCellValue("");

            //TERCER ROW
            iRow++;
            row = sheet.CreateRow(iRow);
            row.CreateCell(0).SetCellValue("");
            var cellPrestador = row.CreateCell(1);
            cellPrestador.SetCellValue("Prestador:");
            cellPrestador.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            var cellPrestador2 = row.CreateCell(2);
            cellPrestador2.SetCellValue(carga.Clinica.Descripcion);
            cellPrestador2.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            row.CreateCell(3).SetCellValue("");
            row.CreateCell(4).SetCellValue("");
            row.CreateCell(5).SetCellValue("");
            row.CreateCell(6).SetCellValue("");
            row.CreateCell(7).SetCellValue("");
            row.CreateCell(8).SetCellValue("");
            row.CreateCell(9).SetCellValue("");
            row.CreateCell(10).SetCellValue("");
            row.CreateCell(11).SetCellValue("");
            row.CreateCell(12).SetCellValue("");
            var mergeCellPrestador = new NPOI.SS.Util.CellRangeAddress(2, 2, 2, 5);
            sheet.AddMergedRegion(mergeCellPrestador);

            //CUARTO ROW: HEADER COLUMNS
            iRow++;
            row = sheet.CreateRow(iRow);
            var cellHdr0 = row.CreateCell(0);
            cellHdr0.SetCellValue("NRO.");
            cellHdr0.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr1 = row.CreateCell(1);
            cellHdr1.SetCellValue("PACIENTE");
            cellHdr1.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr2 = row.CreateCell(2);
            cellHdr2.SetCellValue("EDAD");
            cellHdr2.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr3 = row.CreateCell(3);
            cellHdr3.SetCellValue("N° AFILIADO");
            cellHdr3.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr4 = row.CreateCell(4);
            cellHdr4.SetCellValue("INGRESO");
            cellHdr4.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr5 = row.CreateCell(5);
            cellHdr5.SetCellValue("EGRESO");
            cellHdr5.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr6 = row.CreateCell(6);
            cellHdr6.SetCellValue("DIAGNÓSTICO");
            cellHdr6.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr7 = row.CreateCell(7);
            cellHdr7.SetCellValue("TIPO");
            cellHdr7.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr8 = row.CreateCell(8);
            cellHdr8.SetCellValue("DEBITO");
            cellHdr8.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr9 = row.CreateCell(9);
            cellHdr9.SetCellValue("FUNDAMENTO");
            cellHdr9.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr10 = row.CreateCell(10);
            cellHdr10.SetCellValue("MONTO");
            cellHdr10.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr11 = row.CreateCell(11);
            cellHdr11.SetCellValue("OFERTA PRESTADOR");
            cellHdr11.RichStringCellValue.ApplyFont(fontBoldColHeader);

            var cellHdr12 = row.CreateCell(12);
            cellHdr12.SetCellValue("FUNDAMENTO PRESTADOR");
            cellHdr12.RichStringCellValue.ApplyFont(fontBoldColHeader);
        }

        public List<Factura> ReadFacturasFromExcel(Stream stream)
        {
            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);

            ISheet sheet = hssfwb.GetSheetAt(0); //hssfwb.GetSheet("Arkusz1");
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null) //null is when the row only contains empty cells 
                {
                    
                    var f = new Factura();

                    try
                    {
                        try { f.AuditoriaNumero = row.GetCell(0).StringCellValue; }
                        catch (Exception ex) { f.AuditoriaNumero = Convert.ToString((long)row.GetCell(0).NumericCellValue); }

                        f.Afiliado = row.GetCell(3).StringCellValue;
                        f.ApellidoYNombre = row.GetCell(1).StringCellValue;
                        f.Prestacion = row.GetCell(6).StringCellValue;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(f.AuditoriaNumero) && string.IsNullOrEmpty(f.Afiliado))
                        continue;

                    long l;
                    if (!long.TryParse(f.Afiliado, out l))
                        continue;

                    if (string.IsNullOrEmpty(f.AuditoriaNumero))
                    {
                        throw new Exception("Falta el 'Nro.' de auditoría en el Excel. Registro #" + (rowIndex + 1));
                    }
                    if (string.IsNullOrEmpty(f.Afiliado))
                    {
                        throw new Exception("Falta el Nro. de afiliado en el Excel. Auditoria Nro. " + f.AuditoriaNumero + " // Registro #" + (rowIndex + 1));
                    }
                    var fdb = this._carga.Facturas.FirstOrDefault(fa => fa.AuditoriaNumero.Equals(f.AuditoriaNumero));
                    if (fdb == null)
                    {
                        throw new Exception("No se encontró la auditoria Nro. " + f.AuditoriaNumero + " en la carga auditada.");
                    }
                    if (!fdb.Afiliado.Trim().Equals(f.Afiliado.Trim()))
                    {
                        throw new Exception("El numero de afiliado " + f.Afiliado + " no corresponde con el auditado. Auditoria nro. " + f.AuditoriaNumero);
                    }
                    if (!fdb.ApellidoYNombre.ToUpper().Trim().Equals(f.ApellidoYNombre.ToUpper().Trim()))
                    {
                        throw new Exception("El nombre y apellido de afiliado " + f.ApellidoYNombre + " no corresponde con el auditado. Auditoria nro. " + f.AuditoriaNumero);
                    }
                    if (!fdb.Prestacion.ToUpper().Trim().Equals(f.Prestacion.ToUpper().Trim()))
                    {
                        throw new Exception("La prestación " + f.Prestacion + " no corresponde con la auditada. Auditoria nro. " + f.AuditoriaNumero);
                    }

                    try
                    {
                        fdb.ConsiliacionPrestadorOferta = (decimal)row.GetCell(11).NumericCellValue;
                    }
                    catch (Exception ex)
                    {
                        fdb.ConsiliacionPrestadorOferta = Convert.ToDecimal(row.GetCell(11).StringCellValue.Replace(".", ","));
                    }

                    if (fdb.ConsiliacionPrestadorOferta <= 0)
                    {
                        throw new Exception("El prestador no puede ofertar por monto menor o igual a CERO. Auditoria nro. " + f.AuditoriaNumero);
                    }
                    
                    fdb.ConsiliacionPrestadorFundamento = row.GetCell(12).StringCellValue;

                    if (string.IsNullOrEmpty(fdb.ConsiliacionPrestadorFundamento.Trim()))
                    {
                        throw new Exception("El prestador debe fundamentar la factura. Auditoria nro. " + f.AuditoriaNumero);
                    }
                }
            }

            return this._carga.Facturas.Where(f => f.ConsiliacionPrestadorOferta.HasValue).ToList();

        }
    }
}