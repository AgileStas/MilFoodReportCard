using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;


namespace MilFoodReportCard
{
    internal class XlsReportWaybill
    {
        /*
                    // TODO: use MaltReport for ODS/XLS https://github.com/oldrev/maltreport
                    var xlsTplFilename = System.IO.Path.Combine(ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                        "Templates",
                        "waybill.ods");
                    var data = new Dictionary<string, object>()
                    {
                        { "WbItems", entries },
                    };
                    var xmlContext = new Sandwych.Reporting.TemplateContext(data);
                    using (var stream = System.IO.File.OpenRead(xlsTplFilename))
                    {
                        var odt = OdfDocument.LoadFrom(stream);
                        var odtTemplate = new OdtTemplate(odt);
                        //var xls = Sandwych.Reporting.OfficeML.ExcelMLDocument.LoadFrom(xlsTplFilename);
                        //var xlsTemplate = new Sandwych.Reporting.OfficeML.ExcelMLTemplate(xls);

                        var result = odtTemplate.Render(xmlContext);
                        //var result = xlsTemplate.Render(xmlContext);

                        var outputFile = System.IO.Path.Combine(
                            ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                            "Output",
                            "waybill_" + DateTime.Now.ToString("yyyy-MM-dd") + ".ods");
                        //"waybill_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");

                        result.Save(outputFile);
                    }
        */

        //struct WbItem
        //{
        //    public string name;
        //    public string id;
        //    public decimal count;
        //    public int factor;
        //    public string measure;
        //}

        public bool TryGenerate(string templatePath, string outputPath, MainWindow.WaybillOutputData data, out string error)
        {
            error = string.Empty;

            var xlApp = new Excel.Application();

            if (xlApp == null)
            {
                error = "Excel is not properly installed!!!";
                return false;
            }

            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            var wbNumber = data.docNumber;
            var wbConsumer = data.customer;
            var wbDate = data.docDate;

            if (!Utils.UpperName(data.customerPersonName, out var wbCInitials, out var wbCUpper)) { }

            if (System.IO.File.Exists(outputPath))
            {
                System.IO.File.Delete(outputPath);
            }
            System.IO.File.Copy(templatePath, outputPath);

            //xlWorkBook = xlApp.Workbooks.Add(misValue);
            //xlWorkBook = xlApp.Workbooks.Open(tplFileName);
            xlWorkBook = xlApp.Workbooks.Open(outputPath);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlWorkSheet.Cells[7, 8] = wbNumber;
            xlWorkSheet.Cells[11, 2] = wbNumber;
            xlWorkSheet.Cells[11, 5] = wbNumber;
            xlWorkSheet.Cells[14, 8] = wbConsumer;
            xlWorkSheet.Cells[14, 11] = data.customerPersonRank + " " + wbCInitials;
            xlWorkSheet.Cells[29, 3] = data.customerPersonRank;
            xlWorkSheet.Cells[29, 8] = wbCUpper;
            xlWorkSheet.Cells[6, 3] = wbDate; // Дійсна до
            xlWorkSheet.Cells[11, 7] = wbDate; // Дата документа
            xlWorkSheet.Cells[14, 2] = wbDate; // Дата операції
            xlWorkSheet.Cells[31, 2] = wbDate; // Дата підписання

            //xlWorkSheet.Cells[19, 6] = this.items.Count.ToString() + " (" + num2Text(this.items.Count) + ") найменувань";
            int entriesCount = data.entries.Count();
            xlWorkSheet.Cells[19, 6] = entriesCount.ToString() + " (" + Utils.Num2Text(entriesCount) + ") найменувань";

            //xlWorkSheet.Rows.Insert(18);
            /*
            xlWorkSheet.Rows[19].EntireRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, false);
            xlWorkSheet.Cells[19, 1] = "Test";
            xlWorkSheet.Cells[19, 2] = "New";
            xlWorkSheet.Cells[19, 3] = "Row";
            */
            int wCount = data.customerKits;
            int idx = entriesCount;
            foreach (var entry in data.entries.Reverse())
            {
                xlWorkSheet.Rows[19].EntireRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, false);
                xlWorkSheet.Range[xlWorkSheet.Cells[19, 3], xlWorkSheet.Cells[19, 5]].Merge();
                //xlWorkSheet.Cells[19, 3].HorizontalAlignment = HorizontalAlignment.Left;
                xlWorkSheet.Cells[19, 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                xlWorkSheet.Range[xlWorkSheet.Cells[19, 9], xlWorkSheet.Cells[19, 10]].Merge();
                xlWorkSheet.Cells[19, 9].NumberFormat = "0,000";
                xlWorkSheet.Range[xlWorkSheet.Cells[19, 11], xlWorkSheet.Cells[19, 12]].Merge();
                xlWorkSheet.Cells[19, 11].NumberFormat = "0,000";
                var item = entry;
                xlWorkSheet.Cells[19, 2] = idx--;
                xlWorkSheet.Cells[19, 3] = item.ProductName;
                xlWorkSheet.Cells[19, 6] = item.ProductId;
                xlWorkSheet.Cells[19, 7] = item.AccUoM;
                xlWorkSheet.Cells[19, 8] = "б/к";
                xlWorkSheet.Cells[19, 9] = (item.Quantity * wCount) / item.ScaleUoM;
                xlWorkSheet.Cells[19, 11] = (item.Quantity * wCount) / item.ScaleUoM;
            }
            xlWorkSheet.Cells[19, 13] = wCount;

            //xlWorkBook.SaveAs(fileName, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Save();
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

            return true;
        }
    }
}
