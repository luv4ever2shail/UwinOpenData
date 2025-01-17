using ClosedXML.Excel;
using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace UwinOpenDMT
{
    public static class ImpExpUtilities
    {
        public static DataSet GetXmlData(Stream xmlDoc)
        {
            var xmlData = new DataSet();
            xmlData.ReadXml(xmlDoc);
            return xmlData;
        }

        public static DataSet GetSpreadSheetData(FileStream excelSheet)
        {
            DataSet spreadSheet;
            using (IExcelDataReader excelReader = (Path.GetExtension(excelSheet.Name)?.ToLower() != ".csv")
                ? ExcelReaderFactory.CreateReader(excelSheet)
                : ExcelReaderFactory.CreateCsvReader(excelSheet))
            {
                spreadSheet = excelReader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });
                excelReader.Close();
            }
            return spreadSheet;
        }

        public static bool ExportXML(DataTable xmlData, String xmlDocPath)
        {
            bool flag;
            try
            {
                xmlData.WriteXml(xmlDocPath);
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

        public static bool ExportCSV(DataGridView dgv, string filename)
        {
            var flag = false;
            var sb = new StringBuilder();
            var headers = dgv.Columns.Cast<DataGridViewColumn>();
            sb.AppendLine(string.Join(",", headers.Select(column => "\"" + SecurityElement.Escape(column.HeaderText) + "\"").ToArray()));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                var cells = row.Cells.Cast<DataGridViewCell>();
                sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + SecurityElement.Escape(cell?.Value + String.Empty) + "\"").ToArray()));
            }

            try
            {
                File.WriteAllText(filename, sb.ToString());
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }

            return flag;
        }

        public static bool ExportXLS(DataTable xlsData, string filename)
        {
            var xlsWorkbook = new XLWorkbook();
            xlsWorkbook.Worksheets.Add(xlsData, xlsData.TableName);
            bool flag;
            try
            {
                xlsWorkbook.SaveAs(filename);
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
    }
}