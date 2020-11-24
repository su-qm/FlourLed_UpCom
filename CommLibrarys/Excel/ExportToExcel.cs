using System;
using System.Data;
using System.Text;
using System.Web;

namespace CommLibrarys.Excel
{
    public class ExportToExcel
    {
        public static void CreateExcel(DataSet ds, string typeid, string FileName)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.ContentEncoding = Encoding.GetEncoding("GB2312");
            response.AppendHeader("Content-Disposition", "attachment;filename=" + FileName);
            string text = "";
            string text2 = "";
            int i = 0;
            DataTable dataTable = ds.Tables[0];
            DataRow[] array = dataTable.Select("");
            if (typeid == "1")
            {
                for (i = 0; i < dataTable.Columns.Count; i++)
                {
                    text = text + dataTable.Columns[i].Caption.ToString() + "\t";
                }
                text += "\n";
                response.Write(text);
                DataRow[] array2 = array;
                for (int j = 0; j < array2.Length; j++)
                {
                    DataRow dataRow = array2[j];
                    for (i = 0; i < dataTable.Columns.Count; i++)
                    {
                        text2 = text2 + dataRow[i].ToString() + "\t";
                    }
                    text2 += "\n";
                    response.Write(text2);
                    text2 = "";
                }
            }
            else
            {
                if (typeid == "2")
                {
                    response.Write(ds.GetXml());
                }
            }
            response.End();
        }
    }
}
