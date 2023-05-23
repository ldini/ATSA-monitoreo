using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Reporting
{
    public static class ReportHelper
    {
        public static string ExportToPDF(ReportViewer viewer, string filePath)
        {
            string ack = "";
            try
            {
                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string extension;

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);

                using (FileStream stream = File.OpenWrite(filePath))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                return ack;
            }
            catch (Exception ex)
            {
                ack = ex.InnerException.Message;
                return ack;
            }
        }
    }
}