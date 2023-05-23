using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Natom.ATSA.Monitoreo.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace Natom.ATSA.Monitoreo.WorkbooksManagers
{
    public class DeudaWorkbook
    {
        private int mes;
        private int anio;
        private List<ListarDeudasAlPeriodoResult> deudas;

        public DeudaWorkbook(int mes, int anio, List<ListarDeudasAlPeriodoResult> deudas)
        {
            this.mes = mes;
            this.anio = anio;
            this.deudas = deudas;
        }

        public MemoryStream BuildExcel()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            ISheet sheet = wb.CreateSheet("Hoja1");

            int iRow = 0;
            this.CreateHeader(ref wb, ref sheet, ref iRow);

            var fontBolRazonSocial = wb.CreateFont();
            fontBolRazonSocial.IsBold = true;
            fontBolRazonSocial.FontHeightInPoints = 12;

            var wrapedTextAndVerticalAlignCellStyle = wb.CreateCellStyle();
            wrapedTextAndVerticalAlignCellStyle.WrapText = true;
            wrapedTextAndVerticalAlignCellStyle.VerticalAlignment = VerticalAlignment.Center;
            wrapedTextAndVerticalAlignCellStyle.Alignment = HorizontalAlignment.Center;

            var TotalCellStyle = wb.CreateCellStyle();
            TotalCellStyle.VerticalAlignment = VerticalAlignment.Bottom;
            TotalCellStyle.Alignment = HorizontalAlignment.Right;
            TotalCellStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index;

            sheet.SetColumnWidth(0, 300 * 30);
            sheet.SetColumnWidth(1, 150 * 30);
            sheet.SetColumnWidth(2, 150 * 30);
            sheet.SetColumnWidth(3, 130 * 30);
            sheet.SetColumnWidth(4, 130 * 30);
            sheet.SetColumnWidth(5, 130 * 30);
            sheet.SetColumnWidth(6, 150 * 30);
            sheet.SetColumnWidth(7, 150 * 30);
            sheet.SetColumnWidth(8, 250 * 30);

            var deudasPorClinicaNoADMIN = this.deudas
                                        .Where(k => !k.CargadoPorAdmin)
                                        //.GroupBy(k => new { k.RazonSocial, k.CUIT },
                                        .GroupBy(k => new { CUIT = new String(k.CUIT.Where(char.IsDigit).ToArray()) },
                                                 (k, v) => new
                                                 {
                                                     //RazonSocial = k.RazonSocial,
                                                     RazonSocial = v.FirstOrDefault()?.RazonSocial,
                                                     CUIT = k.CUIT,
                                                     Facturas = (from f in v
                                                                 select new
                                                                 {
                                                                     cuit = f.CUIT,
                                                                     nro = f.NroFactura,
                                                                     periodo = f.Periodo,
                                                                     importe = f.ImporteFactura,
                                                                     saldo = f.SaldoFactura,
                                                                     fecha = f.FechaFactura
                                                                 }).OrderBy(f => f.fecha)
                                                 })
                                        .OrderBy(k => k.RazonSocial);
            var deudasPorClinicaADMIN = this.deudas
                                        .Where(k => k.CargadoPorAdmin)
                                        //.GroupBy(k => new { k.RazonSocial, k.CUIT },
                                        .GroupBy(k => new { CUIT = new String(k.CUIT.Where(char.IsDigit).ToArray()) },
                                                 (k, v) => new
                                                 {
                                                     //RazonSocial = k.RazonSocial,
                                                     RazonSocial = v.FirstOrDefault()?.RazonSocial,
                                                     CUIT = k.CUIT,
                                                     Facturas = (from f in v
                                                                 select new
                                                                 {
                                                                     cuit = f.CUIT,
                                                                     nro = f.NroFactura,
                                                                     periodo = f.Periodo,
                                                                     importe = f.ImporteFactura,
                                                                     saldo = f.SaldoFactura,
                                                                     fecha = f.FechaFactura
                                                                 }).OrderBy(f => f.fecha)
                                                 })
                                        .OrderBy(k => k.RazonSocial);

            int clinica_iRowStart = iRow;
            int clinica_iRowEnd = iRow;
            bool hayNoAdmin = deudasPorClinicaNoADMIN.Where(c => c.Facturas.Count() > 0).Any();
            //NO ADMIN (MAS ABAJO HAY OTRO FOREACH IDENTICO PARA ADMIN!!)
            foreach (var clinica in deudasPorClinicaNoADMIN.Where(c => c.Facturas.Count() > 0))
            {
                clinica_iRowStart = iRow + 1;

                foreach (var factura in clinica.Facturas)
                {
                    clinica_iRowEnd = ++iRow;
                    var rowFAC = sheet.CreateRow(iRow);
                    var cellRazonSocial = rowFAC.CreateCell(0);
                    cellRazonSocial.SetCellValue(""); // RAZON SOCIAL
                    cellRazonSocial.RichStringCellValue.ApplyFont(fontBolRazonSocial);

                    var cellCUIT = rowFAC.CreateCell(1);
                    cellCUIT.SetCellValue(factura.cuit); // CUIT
                    cellCUIT.RichStringCellValue.ApplyFont(fontBolRazonSocial);

                    rowFAC.CreateCell(2).SetCellValue(factura.nro);
                    rowFAC.CreateCell(3).SetCellValue(factura.periodo);
                    rowFAC.CreateCell(4).SetCellValue((double)factura.importe);
                    rowFAC.CreateCell(5).SetCellValue((double)factura.saldo);
                    rowFAC.CreateCell(6).SetCellValue("");
                    rowFAC.CreateCell(7).SetCellValue("");
                    rowFAC.CreateCell(8).SetCellValue("");
                }
                sheet.GetRow(clinica_iRowStart).GetCell(0).SetCellValue(clinica.RazonSocial);
                sheet.GetRow(clinica_iRowStart).GetCell(1).SetCellValue(clinica.CUIT);    //CUIT

                decimal sumaFacturas = clinica.Facturas.Sum(f => f.importe);
                sheet.GetRow(clinica_iRowStart).GetCell(6).SetCellValue((double)sumaFacturas);

                decimal sumaPendiente = clinica.Facturas.Sum(f => f.saldo);
                sheet.GetRow(clinica_iRowStart).GetCell(7).SetCellValue((double)sumaPendiente);

                var mergeRazonSocial = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 0, 0);
                sheet.AddMergedRegion(mergeRazonSocial);

                var mergeCUIT = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 1, 1);
                sheet.AddMergedRegion(mergeCUIT);

                var mergeTotal = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 6, 6);
                sheet.AddMergedRegion(mergeTotal);

                var mergePendiente = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 7, 7);
                sheet.AddMergedRegion(mergePendiente);

                var mergedCellRS = sheet.GetRow(clinica_iRowStart).GetCell(0);
                mergedCellRS.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellRS.CellStyle = wrapedTextAndVerticalAlignCellStyle;

                var mergedCellCUIT = sheet.GetRow(clinica_iRowStart).GetCell(1);
                mergedCellCUIT.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellCUIT.CellStyle = wrapedTextAndVerticalAlignCellStyle;

                var mergedCellTotal = sheet.GetRow(clinica_iRowStart).GetCell(6);
                //mergedCellTotal.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellTotal.CellStyle = TotalCellStyle;

                var mergedCellPendiente = sheet.GetRow(clinica_iRowStart).GetCell(8);
                //mergedCellPendiente.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellPendiente.CellStyle = TotalCellStyle;

                SetBorderToRegion(clinica_iRowStart, clinica_iRowEnd, 0, 8, ref wb, ref sheet);
            }

            //SI GRABO
            bool hayAdmin = deudasPorClinicaADMIN.Where(c => c.Facturas.Count() > 0).Any();
            if (hayAdmin && hayNoAdmin)
            {
                iRow++;
                var row = sheet.CreateRow(iRow);
                row.CreateCell(0).SetCellValue("");
                row.CreateCell(1).SetCellValue("");
                row.CreateCell(2).SetCellValue("");
                row.CreateCell(3).SetCellValue("");
                row.CreateCell(4).SetCellValue("");
                row.CreateCell(5).SetCellValue("");
                row.CreateCell(6).SetCellValue("");
                row.CreateCell(7).SetCellValue("");
                row.CreateCell(8).SetCellValue("");

                var mergeCellTitle = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 8);
                sheet.AddMergedRegion(mergeCellTitle);
            }

            //ADMIN (MAS ARRIBA HAY OTRO FOREACH IDENTICO PARA NO ADMIN!!)
            foreach (var clinica in deudasPorClinicaADMIN.Where(c => c.Facturas.Count() > 0))
            {
                clinica_iRowStart = iRow + 1;

                foreach (var factura in clinica.Facturas)
                {
                    clinica_iRowEnd = ++iRow;
                    var rowFAC = sheet.CreateRow(iRow);
                    var cellRazonSocial = rowFAC.CreateCell(0);
                    cellRazonSocial.SetCellValue(""); // RAZON SOCIAL
                    cellRazonSocial.RichStringCellValue.ApplyFont(fontBolRazonSocial);

                    var cellCUIT = rowFAC.CreateCell(1);
                    cellCUIT.SetCellValue(""); // CUIT
                    cellCUIT.RichStringCellValue.ApplyFont(fontBolRazonSocial);

                    rowFAC.CreateCell(2).SetCellValue(factura.nro);
                    rowFAC.CreateCell(3).SetCellValue(factura.periodo);
                    rowFAC.CreateCell(4).SetCellValue((double)factura.importe);
                    rowFAC.CreateCell(5).SetCellValue((double)factura.saldo);
                    rowFAC.CreateCell(6).SetCellValue("");
                    rowFAC.CreateCell(7).SetCellValue("");
                    rowFAC.CreateCell(8).SetCellValue("");
                }
                sheet.GetRow(clinica_iRowStart).GetCell(0).SetCellValue(clinica.RazonSocial);
                sheet.GetRow(clinica_iRowStart).GetCell(1).SetCellValue(clinica.CUIT);    //CUIT

                decimal sumaFacturas = clinica.Facturas.Sum(f => f.importe);
                sheet.GetRow(clinica_iRowStart).GetCell(6).SetCellValue((double)sumaFacturas);

                decimal sumaPendiente = clinica.Facturas.Sum(f => f.saldo);
                sheet.GetRow(clinica_iRowStart).GetCell(7).SetCellValue((double)sumaPendiente);

                var mergeRazonSocial = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 0, 0);
                sheet.AddMergedRegion(mergeRazonSocial);

                var mergeCUIT = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 1, 1);
                sheet.AddMergedRegion(mergeCUIT);

                var mergeTotal = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 6, 6);
                sheet.AddMergedRegion(mergeTotal);

                var mergePendiente = new NPOI.SS.Util.CellRangeAddress(clinica_iRowStart, clinica_iRowEnd, 7, 7);
                sheet.AddMergedRegion(mergePendiente);

                var mergedCellRS = sheet.GetRow(clinica_iRowStart).GetCell(0);
                mergedCellRS.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellRS.CellStyle = wrapedTextAndVerticalAlignCellStyle;

                var mergedCellCUIT = sheet.GetRow(clinica_iRowStart).GetCell(1);
                mergedCellCUIT.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellCUIT.CellStyle = wrapedTextAndVerticalAlignCellStyle;

                var mergedCellTotal = sheet.GetRow(clinica_iRowStart).GetCell(6);
                //mergedCellTotal.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellTotal.CellStyle = TotalCellStyle;

                var mergedCellPendiente = sheet.GetRow(clinica_iRowStart).GetCell(8);
                //mergedCellPendiente.RichStringCellValue.ApplyFont(fontBolRazonSocial);
                mergedCellPendiente.CellStyle = TotalCellStyle;

                SetBorderToRegion(clinica_iRowStart, clinica_iRowEnd, 0, 8, ref wb, ref sheet);
            }

            SetBorderToRegion(2, 2, 0, 8, ref wb, ref sheet);

            SetBorderToRegion(0, clinica_iRowEnd, 0, 0, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 1, 1, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 2, 2, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 3, 3, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 4, 4, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 5, 5, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 6, 6, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 7, 7, ref wb, ref sheet);
            SetBorderToRegion(0, clinica_iRowEnd, 8, 8, ref wb, ref sheet);

            MemoryStream ms = new MemoryStream();
            using (MemoryStream tempStream = new MemoryStream())
            {
                wb.Write(tempStream);
                var byteArray = tempStream.ToArray();
                ms.Write(byteArray, 0, byteArray.Length);
            }

            return ms;
        }

        private void SetBorderToRegion(int rowStart, int rowEnd, int colStart, int colEnd, ref HSSFWorkbook wb, ref ISheet sheet)
        {
            var range = new NPOI.SS.Util.CellRangeAddress(rowStart, rowEnd, colStart, colEnd);

            RegionUtil.SetBorderBottom(1, range, sheet, wb);
            RegionUtil.SetBorderLeft(1, range, sheet, wb);
            RegionUtil.SetBorderRight(1, range, sheet, wb);
            RegionUtil.SetBorderTop(1, range, sheet, wb);
        }

        private void CreateHeader(ref HSSFWorkbook wb, ref ISheet sheet, ref int iRow)
        {
            var centeredCellStyle = wb.CreateCellStyle();
            centeredCellStyle.Alignment = HorizontalAlignment.Center;

            var wrapedTextAndVerticalAlignCellStyle = wb.CreateCellStyle();
            wrapedTextAndVerticalAlignCellStyle.WrapText = true;
            wrapedTextAndVerticalAlignCellStyle.VerticalAlignment = VerticalAlignment.Center;

            var meses = new string[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            string periodo = meses[this.mes - 1] + " - " + this.anio.ToString().Substring(2, 2);

            var fontBold = wb.CreateFont();
            fontBold.IsBold = true;

            var fontBoldTitle = wb.CreateFont();
            fontBoldTitle.IsBold = true;
            fontBoldTitle.FontHeightInPoints = 16;


            var fontBoldSubTitle = wb.CreateFont();
            fontBoldSubTitle.IsBold = true;
            fontBoldSubTitle.FontHeightInPoints = 14;

            var fontBoldColHeader = wb.CreateFont();
            fontBoldColHeader.IsBold = true;
            fontBoldColHeader.FontHeightInPoints = 10;
            
            //PRIMER ROW
            var row = sheet.CreateRow(iRow);
            var cellTitle = row.CreateCell(0);
            cellTitle.SetCellValue(periodo);
            cellTitle.RichStringCellValue.ApplyFont(fontBoldTitle);
            cellTitle.CellStyle = centeredCellStyle;

            row.CreateCell(1).SetCellValue("");
            row.CreateCell(2).SetCellValue("");
            row.CreateCell(3).SetCellValue("");
            row.CreateCell(4).SetCellValue("");
            row.CreateCell(5).SetCellValue("");
            row.CreateCell(6).SetCellValue("");
            row.CreateCell(7).SetCellValue("");
            row.CreateCell(8).SetCellValue("");

            var mergeCellTitle = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 8);
            sheet.AddMergedRegion(mergeCellTitle);


            //SEGUNDO ROW
            iRow++;
            row = sheet.CreateRow(iRow);
            row.CreateCell(0).SetCellValue("");
            row.CreateCell(1).SetCellValue("");
            row.CreateCell(2).SetCellValue("");
            row.CreateCell(3).SetCellValue("");
            row.CreateCell(4).SetCellValue("");
            row.CreateCell(5).SetCellValue("");
            row.CreateCell(6).SetCellValue("");
            row.CreateCell(7).SetCellValue("");
            row.CreateCell(8).SetCellValue("");

            var mergeCellTitle2 = new NPOI.SS.Util.CellRangeAddress(1, 1, 0, 8);
            sheet.AddMergedRegion(mergeCellTitle2);


            //TERCER ROW
            iRow++;
            row = sheet.CreateRow(iRow);

            var cellRazonSocial = row.CreateCell(0);
            cellRazonSocial.SetCellValue("RAZON SOCIAL");
            cellRazonSocial.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellCUIT = row.CreateCell(1);
            cellCUIT.SetCellValue("CUIT");
            cellCUIT.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellFactura = row.CreateCell(2);
            cellFactura.SetCellValue("FACTURA N°");
            cellFactura.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellPeriodoFactura = row.CreateCell(3);
            cellPeriodoFactura.SetCellValue("PERIODO\nFACTURA");
            cellPeriodoFactura.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            cellPeriodoFactura.CellStyle = wrapedTextAndVerticalAlignCellStyle;

            var cellImporteFactura = row.CreateCell(4);
            cellImporteFactura.SetCellValue("IMPORTE\nFACTURA");
            cellImporteFactura.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            cellImporteFactura.CellStyle = wrapedTextAndVerticalAlignCellStyle;

            var cellSaldoFactura = row.CreateCell(5);
            cellSaldoFactura.SetCellValue("SALDO\nFACTURA");
            cellSaldoFactura.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            cellSaldoFactura.CellStyle = wrapedTextAndVerticalAlignCellStyle;

            var cellTotales = row.CreateCell(6);
            cellTotales.SetCellValue("TOTALES");
            cellTotales.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellPendiente = row.CreateCell(7);
            cellPendiente.SetCellValue("PENDIENTE");
            cellPendiente.RichStringCellValue.ApplyFont(fontBoldSubTitle);

            var cellCompensacion = row.CreateCell(8);
            cellCompensacion.SetCellValue("COMPENSACIÓN");
            cellCompensacion.RichStringCellValue.ApplyFont(fontBoldSubTitle);
            
        }
    }
}