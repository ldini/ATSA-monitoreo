using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Natom.ATSA.Monitoreo.Models.DataTable
{
    public enum eSortingDirection
    {
        ASC, DESC
    }

    public class DataTableParams
    {
        public DataTableParams(HttpRequestBase Request)
        {
            this.Search = Request["search[value]"];
            this.SortByColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            this.SortingDirection = Request["order[0][dir]"] == "asc" ? eSortingDirection.ASC : eSortingDirection.DESC;
            this.Filter0 = Request["columns[0][search][value]"];
            this.Filter1 = Request["columns[1][search][value]"];
            this.Filter2 = Request["columns[2][search][value]"];
            this.Filter3 = Request["columns[3][search][value]"];
            this.Filter4 = Request["columns[4][search][value]"];
            this.Filter5 = Request["columns[5][search][value]"];
            this.Filter6 = Request["columns[6][search][value]"];
            this.Filter7 = Request["columns[7][search][value]"];
            this.Filter8 = Request["columns[8][search][value]"];
            this.Filter9 = Request["columns[9][search][value]"];
        }
        public string Search { get; set; }
        public eSortingDirection SortingDirection { get; set; }
        public int SortByColumnIndex { get; set; }
        public string Filter0 { get; set; }
        public string Filter1 { get; set; }
        public string Filter2 { get; set; }
        public string Filter3 { get; set; }
        public string Filter4 { get; set; }
        public string Filter5 { get; set; }
        public string Filter6 { get; set; }
        public string Filter7 { get; set; }
        public string Filter8 { get; set; }
        public string Filter9 { get; set; }


    }
}