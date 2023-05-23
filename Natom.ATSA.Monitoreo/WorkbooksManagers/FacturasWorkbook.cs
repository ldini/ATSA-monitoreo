using Natom.ATSA.Monitoreo.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.WorkbooksManagers
{
    public class FacturasWorkbook
    {
        public List<Factura> ObtenerFacturasDesdeExcel(Stream stream)
        {
            List<Factura> facturas = new List<Factura>();
            XSSFWorkbook hssfwb = new XSSFWorkbook(stream);

            ISheet sheet = hssfwb.GetSheetAt(0); //hssfwb.GetSheet("Arkusz1");
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null) //null is when the row only contains empty cells 
                {

                    try
                    {
                        var f = new Factura();
                        f.NOrden = row.GetCell(0).NumericCellValue.ToString();
                        if (f.NOrden.Equals("0"))
                            continue;

                        f.XX = row.GetCell(1).StringCellValue;

                        try { f.IT = Convert.ToInt32(row.GetCell(2).NumericCellValue).ToString(); }
                        catch (Exception ex) { f.IT = row.GetCell(2).StringCellValue; }

                        try { f.Afiliado = Convert.ToInt64(row.GetCell(3).NumericCellValue).ToString(); }
                        catch (Exception ex) { f.Afiliado = row.GetCell(3).StringCellValue; }

                        f.ApellidoYNombre = row.GetCell(4).StringCellValue;
                        f.Practica = row.GetCell(5).StringCellValue;

                        try { f.Can = Convert.ToInt64(row.GetCell(6).NumericCellValue).ToString(); }
                        catch (Exception ex) { f.Can = row.GetCell(6).StringCellValue; }

                        try { f.Prestacion = row.GetCell(7).StringCellValue; }
                        catch (Exception ex) { f.Prestacion = Convert.ToInt64(row.GetCell(7).NumericCellValue).ToString(); }

                        f.Honor = Convert.ToDecimal(row.GetCell(8).NumericCellValue);
                        f.Gastos = Convert.ToDecimal(row.GetCell(9).NumericCellValue);
                        f.Fecha = row.GetCell(10).DateCellValue;
                        f.Filial = Convert.ToInt64(row.GetCell(11).NumericCellValue).ToString();
                        f.Cabecera = Convert.ToInt64(row.GetCell(12).NumericCellValue).ToString();
                        f.Detalle = Convert.ToInt64(row.GetCell(13).NumericCellValue).ToString();
                        //f.ErrorCab = row.GetCell(14).StringCellValue;
                        //f.ErrorDet = row.GetCell(15).StringCellValue;
                        facturas.Add(f);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return facturas;
        }
    }
}