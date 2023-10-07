using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.IdentityModel.Tokens;
using Fluid;
using System.Diagnostics;
using System.Windows.Markup;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.VisualBasic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Controls.Primitives;
using System.DirectoryServices.ActiveDirectory;

namespace MilFoodReportCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // See also https://github.com/oldrev/maltreport/wiki
        // Fluid: https://github.com/sebastienros/fluid     https://deanebarker.net/tech/fluid/member-access/
        private MfrcDbContext dbContext;
        private static readonly FluidParser _parser = new FluidParser();

        public MainWindow()
        {
            InitializeComponent();
            dbContext = new MfrcDbContext();
            TemplateOptions.Default.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //db.Divisions.Load();
            cbDivisions.Items.Clear();
            cbDivisions.ItemsSource = dbContext.Divisions.Select(d => d.DivisionName).ToList();
            //cbDivisions.ItemsSource = db.Divisions.Local.ToObservableCollection();
        }

        private void ReloadLayouts()
        {
            void DisableBlock()
            {
                lbLayouts.ItemsSource = null;
                btnRemains.IsEnabled = false;
                btnYielding.IsEnabled = false;
                btnWritingOff.IsEnabled = false;
                btnFedCnt.IsEnabled = false;
                lblPeriod.Content = null;
            }

            if (dpDate.SelectedDate == null || cbDivisions.SelectedItem == null)
            {
                DisableBlock();
                return;
            }

            var dPeriod = new ZsuProdPeriod(dpDate.SelectedDate.Value);
            var divisionName = cbDivisions.SelectedItem as string;
            if (dPeriod == null || divisionName == null || divisionName.Trim() == "")
            {
                DisableBlock();
                return;
            }

            lblPeriod.Content = "Період: " + dPeriod.Year + " рік, " + dPeriod.Period + " період, " + dPeriod.Week + " тиждень";
            btnRemains.IsEnabled = true;
            btnWritingOff.IsEnabled = true;

            var layouts = dbContext.Layouts.Where(l => l.Division == divisionName && l.Year == dPeriod.Year && l.Period == dPeriod.Period && l.Week == dPeriod.Week).ToList();
            //lbMenus.Items.Clear();
            lbLayouts.ItemsSource = layouts;
        }

        private void cbDivisions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadLayouts();
        }

        private void dpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadLayouts();
        }

        private void lbLayouts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbLayouts.SelectedItems.Count == 0 || lbLayouts.SelectedItem == null)
            {
                btnYielding.IsEnabled = false;
                btnFedCnt.IsEnabled = false;
            }
            else
            {
                btnYielding.IsEnabled = true;
                btnFedCnt.IsEnabled = true;
            }
        }

        public class DayLine
        {
            public DateTime Date { get; set; }
            public float TotalFed { get; set; }
            public int[] MealsCount { get; set; }
            public int[] MealsPercent { get; set; }
        }

        private void btnFedCnt_Click(object sender, RoutedEventArgs e)
        {
            var layout = lbLayouts.SelectedItem as Layout;
            var dPeriod = new ZsuProdPeriod(dpDate.SelectedDate.Value);
            var startDate = dPeriod.GetStartDate();
            var endDate = dPeriod.GetEndDate();
            var layouts = dbContext.FedLayouts.Where(fl => fl.LayoutId == layout.LayoutId && (fl.IsSetProgrammatically == null || fl.IsSetProgrammatically == false));
            //var layouts = dbContext.FedLayouts.Where(fl => fl.LayoutId == layout.LayoutId && fl.Date >= startDate && fl.Date <= endDate);

            var dictMeals = new Dictionary<int, int>();
            int idx = 0;
            foreach (var meal in dbContext.Meals)
            {
                //dictMeals[meal.Name] = meal.MealId;
                dictMeals[meal.MealId] = idx++;
            }
            var dayLines = new DayLine[7];
            for (int i = 0; i < 7; i++)
            {
                dayLines[i] = new DayLine();
                dayLines[i].Date = startDate.AddDays(i);
                dayLines[i].TotalFed = float.NaN;
                dayLines[i].MealsCount = new int[dictMeals.Count];
                dayLines[i].MealsPercent = new int[dictMeals.Count];
            }
            //StringBuilder stringBuilder = new StringBuilder();
            foreach (var fl in layouts)
            {
                var dayIdx = fl.Date.Subtract(startDate).Days;
                dayLines[dayIdx].MealsCount[dictMeals[fl.MealId]] = Convert.ToInt32(fl.FedNumber);
                dayLines[dayIdx].MealsPercent[dictMeals[fl.MealId]] = Convert.ToInt32(fl.Weight);
                //stringBuilder.Append(dayIdx.ToString())
                //    .Append('\t')
                //    .Append(dictMeals[fl.MealId].ToString())
                //    .Append('\t')
                //    .Append(fl.FedNumber.ToString())
                //    .Append('\t')
                //    .AppendLine(fl.Weight.ToString());
            }
            //MessageBox.Show(stringBuilder.ToString());
            for (int i = 0; i < 7; i++)
            {
                dayLines[i].TotalFed = 0;
                for (var mealIdx = 0; mealIdx < dictMeals.Count; mealIdx++)
                {
                    dayLines[i].TotalFed += dayLines[i].MealsCount[mealIdx] * dayLines[i].MealsPercent[mealIdx];
                }
                dayLines[i].TotalFed /= 100;
            }
            var dlg = new FedNumberWindow();
            dlg.dgFedNumber.ItemsSource = dayLines.ToList();
            dlg.ShowDialog();
        }

        public class ProductRemain
        {
            public Product Product { get; set; }
            public double Quantity { get; set; }
            public double Cost { get; set; }
        }

        private void btnRemains_Click(object sender, RoutedEventArgs e)
        {
            var qDivision = cbDivisions.SelectedItem as string;
            var qDate = dpDate.SelectedDate.Value;

            //var test0 = dbContext.Outgoings1.IsNullOrEmpty();
            //var test1 = dbContext.Outgoings1.Find(1);
            //var test2 = test1.OutgoingsDoc;
            //var oDocs = dbContext.Outgoings;
            var outgoings = dbContext.Outgoings1
                .Where(o1 => o1.OutgoingsDoc.Division == qDivision && o1.OutgoingsDoc.DocDate < qDate)
                .Select(o1n => new
                {
                    ProductId = o1n.Product.ProductId,
                    ProductName = o1n.Product.Name,
                    AccUoM = o1n.Product.AccUoM,
                    Amount = o1n.Amount * -1,
                    Summ = o1n.Sum * -1,
                });
            var incomes = dbContext.Incomes1
                .Where(i1 => i1.IncomesDoc.Division == qDivision && i1.IncomesDoc.DocDate < qDate)
                .Select(i1n => new
                {
                    ProductId = i1n.Product.ProductId,
                    ProductName = i1n.Product.Name,
                    AccUoM = i1n.Product.AccUoM,
                    Amount = i1n.Amount,
                    Summ = i1n.Sum,
                });
            var balance = outgoings
                .Concat(incomes)
                .GroupBy(g => new { g.ProductId, g.ProductName, g.AccUoM })
                .Select(b => new //Remain
                {
                    ProductId = b.Key.ProductId,
                    ProductName = b.Key.ProductName,
                    AccUoM = b.Key.AccUoM,
                    Quantity = b.Sum(ba => ba.Amount),
                    Cost = b.Sum(ba => ba.Summ),
                });
            //var sql = balance.ToQueryString();
            //Console.WriteLine(sql);
            //var tttt = new StringBuilder();
            //foreach (var b in balance)
            //{
            //    tttt.AppendLine(b.ProductId + "\t" + b.ProductName + "\t" + b.AccUoM + "\t" + b.Quantity + "\t" + b.Cost);
            //}
            //MessageBox.Show(tttt.ToString());

            //TemplateOptions.Default.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
            string[] tplPath = {
                ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                "Templates",
                "remains.html"};
            var tplFilename = System.IO.Path.Combine(tplPath);
            string htmlTemplate;
            try
            {
                htmlTemplate = System.IO.File.ReadAllText(tplFilename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
                return;
            }
            if (_parser.TryParse(htmlTemplate, out var template, out var error))
            {
                //var options = new TemplateOptions { Trimming = TrimmingFlags.TagRight };
                //var options = new TemplateOptions();
                //options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
                //options.MemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;
                //options.MemberAccessStrategy.Register<TransactionModel>();
                //options.Filters.AddFilter("ProductId", (input, args, ctx) => input);
                //options.Filters.AddFilter("ProductName", (input, args, ctx) => input);
                //options.Filters.AddFilter("AccUoM", (input, args, ctx) => input);
                //options.Filters.AddFilter("Quantity", (input, args, ctx) => input);
                //options.Filters.AddFilter("Cost", (input, args, ctx) => input);
                //options.MemberAccessStrategy.Register(new { ProductId = 0, ProductName = "", AccUoM = "", Quantity = 0, Cost = 0 }.GetType());
                //var context = new TemplateContext(
                //                   new { model = balance }, options, true);
                var context = new TemplateContext(balance);
                context.SetValue("balance", balance);
                context.SetValue("RemainsDate", qDate);
                context.SetValue("RemainsDivision", qDivision);

                //// <YYYY-MM-dd>_<wbNumber>_<customer>
                var filename = System.IO.Path.Combine(
                    ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                    "Output",
                    "remains_" + DateTime.Now.ToString("yyyy-MM-dd") + ".html");
                //if (File.Exists(fileName))
                //{
                //    File.Delete(fileName);
                //}
                //File.Copy(tplFileName, fileName);
                //Console.WriteLine(template.Render(context));
                System.IO.File.WriteAllText(filename, template.Render(context));
                var psi = new ProcessStartInfo(filename) { UseShellExecute = true };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show($"Error: {error}");
            }
        }

        public class WbItemNew
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string AccUoM { get; set; }
            public int ScaleUoM { get; set; }
            public double Quantity { get; set; }
        }
        public class WaybillOutputData
        {
            public DateTime docDate { get; set; }
            public string docNumber { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public string supplier { get; set; }
            public string customer { get; set; }
            public string customerPersonRank { get; set; }
            public string customerPersonName { get; set; }
            public int customerKits { get; set; }
            //public int WbItemsCount { get; set; }
            //public string WbItemsCountStr { get; set; }
            public IEnumerable<WbItemNew> entries { get; set; }
        }

        private void btnYielding_Click(object sender, RoutedEventArgs e)
        {
            //    xlWorkSheet.Cells[19, 2] = idx--;
            //    xlWorkSheet.Cells[19, 3] = item.name;
            //    xlWorkSheet.Cells[19, 6] = item.id;
            //    xlWorkSheet.Cells[19, 7] = item.measure;
            //    xlWorkSheet.Cells[19, 8] = "б/к";
            //    xlWorkSheet.Cells[19, 9] = (item.count * wCount) / item.factor;
            //    xlWorkSheet.Cells[19, 11] = (item.count * wCount) / item.factor;

            var outputData = new WaybillOutputData();
            //outputData.docDate = DateTime.Now;
            //outputData.docNumber = "XXX";
            var dPeriod = new ZsuProdPeriod(dpDate.SelectedDate.Value);
            outputData.startDate = dPeriod.GetStartDate();
            outputData.endDate = dPeriod.GetEndDate();
            outputData.supplier = cbDivisions.SelectedItem as string;
            //outputData.customer = "Одержувач";
            //outputData.customerPersonRank = "в/звання";
            //outputData.customerPersonName = "Ім'я Прізвище";
            //outputData.customerKits = 100;

            var wbInfoDlg = new WayBillWeek(dPeriod);
            wbInfoDlg.dpWbDate.SelectedDate = DateTime.Now;
            var rv = wbInfoDlg.ShowDialog();
            switch (rv)
            {
                case false:
                case null:
                    return;
            }
            outputData.docNumber = wbInfoDlg.tbWbNumber.Text.Trim();
            outputData.customer = wbInfoDlg.tbWbCustomer.Text.Trim();
            outputData.customerPersonRank = wbInfoDlg.tbWbRecipientRank.Text.Trim();
            outputData.customerPersonName = wbInfoDlg.tbWbRecipient.Text.Trim();
            outputData.docDate = wbInfoDlg.dpWbDate.SelectedDate.Value;
            outputData.customerKits = int.Parse(wbInfoDlg.tbWbFedNumber.Text.Trim());
            if (wbInfoDlg.cbWbLimitPeriod.IsChecked == true)
            {
                outputData.startDate = wbInfoDlg.dpWbPeriodStart.SelectedDate.Value;
                outputData.endDate = wbInfoDlg.dpWbPeriodEnd.SelectedDate.Value;
            }

            var layout = lbLayouts.SelectedItem as Layout;
            var entries = dbContext.LayoutEntries
                .Where(le => le.LayoutId == layout.LayoutId
                    && le.Date >= outputData.startDate
                    && le.Date < outputData.endDate.Date.AddDays(1)) // Date Start, Date End
                .GroupBy(g => new { g.ProductId, ProductName = g.Product.Name, AccUoM = g.Product.AccUoM, ScaleUoM = g.Product.ScaleUoM })
                .Select(e => new WbItemNew
                {
                    ProductId = e.Key.ProductId,
                    ProductName = e.Key.ProductName,
                    AccUoM = e.Key.AccUoM,
                    ScaleUoM = e.Key.ScaleUoM,
                    Quantity = e.Sum(gg => gg.Amount),
                })
                .OrderBy(le => le.ProductId);
            //var sql = entries.ToQueryString();
            //Console.WriteLine(sql);
            //foreach(var entry in entries)
            //{
            //    Console.WriteLine(entry.ProductId + "\t" + entry.ProductName);
            //}
            outputData.entries = entries;

            if (wbInfoDlg.rbWbOutputXls.IsChecked == true)
            {
                var templatePath = System.IO.Path.Combine(
                    ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                    "Templates",
                    "WaybillTemplate.xls"
                    );
                // <YYYY-MM-dd>_<wbNumber>_<customer>
                //string partName = wbDate.ToString("yyyy-MM-dd") + "_" + wbNumber + "_" + wbConsumer + ".xls";
                //string fileName = Path.Combine(ConfigurationManager.AppSettings.Get("OutputDirectory"), partName);
                var outputPath = System.IO.Path.Combine(
                    ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                    "Output",
                    "waybill_" + outputData.docDate.ToString("yyyy-MM-dd") + "_" + outputData.docNumber + "_" + outputData.customer + ".xls");
                //"waybill_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls");
                var report = new XlsReportWaybill();
                if (report.TryGenerate(templatePath, outputPath, outputData, out var error))
                {
                    var psi = new ProcessStartInfo(outputPath) { UseShellExecute = true };
                    Process.Start(psi);
                }
                else
                {
                    MessageBox.Show($"Error: {error}");
                }
            }
            else
            {
                string[] tplPath = {
                ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                "Templates",
                "waybill.html"};
                var tplFilename = System.IO.Path.Combine(tplPath);
                string htmlTemplate;
                try
                {
                    htmlTemplate = System.IO.File.ReadAllText(tplFilename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                    return;
                }
                if (_parser.TryParse(htmlTemplate, out var template, out var error))
                {
                    //var options = new TemplateOptions { Trimming = TrimmingFlags.TagRight };
                    //var options = new TemplateOptions();
                    //options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
                    //options.MemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;
                    //options.MemberAccessStrategy.Register<TransactionModel>();
                    //options.Filters.AddFilter("ProductId", (input, args, ctx) => input);
                    //options.Filters.AddFilter("ProductName", (input, args, ctx) => input);
                    //options.Filters.AddFilter("AccUoM", (input, args, ctx) => input);
                    //options.Filters.AddFilter("Quantity", (input, args, ctx) => input);
                    //options.Filters.AddFilter("ScaleUoM", (input, args, ctx) => input);
                    //options.MemberAccessStrategy.Register(new { ProductId = 0, ProductName = "", AccUoM = "", Quantity = 0, ScaleUoM = 0 }.GetType());
                    //var context = new TemplateContext(
                    //                   new { model = balance }, options, true);

                    if (!Utils.UpperName(outputData.customerPersonName, out var wbCInitials, out var wbCUpper)) { }

                    var context = new TemplateContext(entries);
                    context.SetValue("DocumentDate", outputData.docDate);
                    context.SetValue("DocumentNumber", outputData.docNumber);
                    context.SetValue("PeriodStart", outputData.startDate);
                    context.SetValue("PeriodEnd", outputData.endDate);
                    context.SetValue("Supplier", outputData.supplier);
                    context.SetValue("Customer", outputData.customer);
                    context.SetValue("CustomerPersonRank", outputData.customerPersonRank);
                    context.SetValue("CustomerPersonName", outputData.customerPersonName);
                    context.SetValue("CustomerPersonNameInitials", wbCInitials);
                    context.SetValue("CustomerPersonNameUpper", wbCUpper);
                    context.SetValue("WbKits", outputData.customerKits);
                    context.SetValue("WbItemsCount", entries.Count());
                    context.SetValue("WbItemsCountStr", Utils.Num2Text(entries.Count()));
                    context.SetValue("WbItems", entries);

                    //// <YYYY-MM-dd>_<wbNumber>_<customer>
                    var filename = System.IO.Path.Combine(
                        ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                        "Output",
                        "waybill_" + outputData.docDate.ToString("yyyy-MM-dd") + "_" + outputData.docNumber + "_" + outputData.customer + ".html");
                    //"waybill_" + DateTime.Now.ToString("yyyy-MM-dd") + ".html");
                    //if (File.Exists(fileName))
                    //{
                    //    File.Delete(fileName);
                    //}
                    //File.Copy(tplFileName, fileName);
                    //Console.WriteLine(template.Render(context));
                    System.IO.File.WriteAllText(filename, template.Render(context));
                    var psi = new ProcessStartInfo(filename) { UseShellExecute = true };
                    Process.Start(psi);
                }
                else
                {
                    MessageBox.Show($"Error: {error}");
                }
            }
        }

        private void btnAgreements_Click(object sender, RoutedEventArgs e)
        {
            var details = dbContext.AgreementDetails
                .GroupBy(g => new
                {
                    Number = g.Agreement.Number,
                    Date = g.Agreement.Date,
                    Supplier = g.Agreement.Supplier,
                })
                //.GroupBy(g => g.Agreement)
                .Select(s => new
                {
                    Number = s.Key.Number,
                    Date = s.Key.Date,
                    Supplier = s.Key.Supplier,
                    Price = s.Sum(gg => gg.Price * gg.Amount / gg.Product.ScaleUoM),
                });
            //var sql = details.ToQueryString();
            //MessageBox.Show(sql);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var entry in details)
            {
                //stringBuilder.AppendLine(entry.Number + "\t" + entry.Date.ToShortDateString() + "\t" + entry.Supplier == null ? "" : entry.Supplier + "\t" + entry.Price);
                stringBuilder.Append(entry.Number)
                    .Append('\t')
                    .Append(entry.Date.ToShortDateString())
                    .Append('\t')
                    .Append(entry.Supplier == null ? "" : entry.Supplier)
                    .Append('\t')
                    .AppendLine(entry.Price.ToString());
            }
            MessageBox.Show(stringBuilder.ToString());
        }

        public List<ProductRemain> GetDivisionRemainsToDate(string division, DateTime date)
        {
            //var test0 = dbContext.Outgoings1.IsNullOrEmpty();
            //var test1 = dbContext.Outgoings1.Find(1);
            //var test2 = test1.OutgoingsDoc;
            //var oDocs = dbContext.Outgoings;
            var outgoings = dbContext.Outgoings1
                .Where(o1 => o1.OutgoingsDoc.Division == division && o1.OutgoingsDoc.DocDate < date)
                .Select(o1n => new ProductRemain
                {
                    Product = o1n.Product,
                    Quantity = o1n.Amount * -1,
                    Cost = o1n.Sum,
                });
            //var oo_sql = outgoings.ToQueryString();
            //MessageBox.Show(oo_sql);
            var oo = outgoings.ToList();
            var incomes = dbContext.Incomes1
                .Where(i1 => i1.IncomesDoc.Division == division && i1.IncomesDoc.DocDate < date)
                .Select(i1n => new ProductRemain
                {
                    Product = i1n.Product,
                    Quantity = i1n.Amount,
                    Cost = i1n.Sum,
                });
            var ii = incomes.ToList();
            var balance = outgoings
                .Concat(incomes)
                .GroupBy(g => g.Product)
                .Select(b => new ProductRemain
                {
                    Product = b.Key,
                    Quantity = b.Sum(ba => ba.Quantity),
                    Cost = b.Sum(ba => ba.Cost),
                });
            //var sql = balance.ToQueryString();
            //Console.WriteLine(sql);
            //var tttt = new StringBuilder();
            //foreach (var b in balance)
            //{
            //    tttt.AppendLine(b.ProductId + "\t" + b.ProductName + "\t" + b.AccUoM + "\t" + b.Quantity + "\t" + b.Cost);
            //}
            //MessageBox.Show(tttt.ToString());

            return balance.ToList();
            //var balanceR = new List<ProductRemain>();
            //foreach (var o in balance.ToList())
            //{
            //    if (o == null) continue;
            //    string productId = o.ProductId;
            //    if (null == productId)
            //    {
            //        MessageBox.Show("ProductId IS NULL!!!");
            //    }
            //    else
            //    {
            //        if (null == dbContext.Products.Find(productId))
            //        {
            //            MessageBox.Show("Can not find Product by ProductId: " + productId);
            //        }
            //        else
            //        {
            //            balanceR.Add(new ProductRemain
            //            {
            //                Product = dbContext.Products.Find(productId),
            //                Quantity = o.Quantity,
            //                Cost = o.Cost,
            //            });
            //        }
            //    }
            //}
            //return balanceR;
        }

        List<Tuple<IncomesDoc, List<ProductRemain>>> GetDivisionIncomesForPeriod(string division, ZsuProdPeriod period)
        {
            var incomes = dbContext.Incomes1
                .Where(i1 => i1.IncomesDoc.Division == division && i1.IncomesDoc.DocDate >= period.GetStartDate() && i1.IncomesDoc.DocDate < period.GetEndDate())
                .Select(i1n => new Tuple<int, ProductRemain>(
                    i1n.IncomesDocId,
                    new ProductRemain
                    {
                        Product = i1n.Product,
                        Quantity = i1n.Amount,
                        Cost = i1n.Sum == null ? 0 : (double)i1n.Sum,
                    }));
            Dictionary<int, List<ProductRemain>> keyValuePairs = new Dictionary<int, List<ProductRemain>>();
            foreach (var i in incomes)
            {
                if (!keyValuePairs.ContainsKey(i.Item1))
                {
                    keyValuePairs[i.Item1] = new List<ProductRemain>();
                }
                keyValuePairs[i.Item1].Add(i.Item2);
            }
            Dictionary<int, IncomesDoc> documents = new Dictionary<int, IncomesDoc>();
            foreach (var i in keyValuePairs.Keys)
            {
                documents[i] = dbContext.Incomes.Find(i);
            }
            List<Tuple<IncomesDoc, List<ProductRemain>>> returnValue = new List<Tuple<IncomesDoc, List<ProductRemain>>>();
            foreach (var i in documents.Keys)
            {
                returnValue.Add(new Tuple<IncomesDoc, List<ProductRemain>>(documents[i], keyValuePairs[i]));
            }
            return returnValue;
        }

        Dictionary<string, double[]> GetDivisionDailyOutgoingsForPeriod(string division, ZsuProdPeriod period)
        {
            var outgoings = dbContext.Outgoings1
                .Where(o1 => o1.OutgoingsDoc.Division == division
                    //&& o1.OutgoingsDoc.DocDate >= period.GetStartDate() 
                    //&& o1.OutgoingsDoc.DocDate < period.GetEndDate()
                    && o1.OutgoingsDoc.ProdPeriodYear == period.Year
                    && o1.OutgoingsDoc.ProdPeriodPeriod == period.Period
                    && o1.OutgoingsDoc.ProdPeriodWeek == period.Week
                    //&& o1.OutgoingsDoc.WritingOffType == "списання за меню-розкладкою"
                    && o1.OutgoingsDoc.WritingOffSum != null)
                .GroupBy(g => new
                {
                    LayoutDate = g.OutgoingsDoc.LayoutDate.Date,
                    ProductId = g.ProductId,
                })
                .Select(s => new
                {
                    Date = s.Key.LayoutDate,
                    ProductId = s.Key.ProductId,
                    Quantity = s.Sum(sa => sa.Amount),
                    Cost = s.Sum(sa => sa.Sum),
                });

            var dailyOutygoings = new Dictionary<string, double[]>();
            var periodStartDate = period.GetStartDate();
            foreach (var item in outgoings)
            {
                //var dayIdx = Convert.ToInt32((item.Date - periodStartDate).TotalDays);
                var dayIdx = Convert.ToInt32(Math.Floor((item.Date - periodStartDate).TotalDays));
                if (!dailyOutygoings.ContainsKey(item.ProductId))
                {
                    dailyOutygoings[item.ProductId] = new double[7];
                }
                dailyOutygoings[item.ProductId][dayIdx] = item.Quantity;
            }

            return dailyOutygoings;
        }

        Dictionary<int, double[]> GetDivisionDailyFoodKitsForPeriod(string division, ZsuProdPeriod period)
        {
            /*
            var outgoings = dbContext.OutgoingsDocFeds
                .Where(o => o.OutgoingsDoc.Division == division
                    && o.OutgoingsDoc.ProdPeriodYear == period.Year
                    && o.OutgoingsDoc.ProdPeriodPeriod == period.Period
                    && o.OutgoingsDoc.ProdPeriodWeek == period.Week
                    && o.OutgoingsDoc.WritingOffSum != null)
                    //&& (o.FedLayout.IsSetProgrammatically == null || o.FedLayout.IsSetProgrammatically == false))
                .Select(s => new
                {
                    FedNumber = s.FedLayout.FedNumber,
                    MealId = s.FedLayout.MealId,
                    Weight = s.FedLayout.Weight,
                    Date = s.FedLayout.Date,
                    //ISP = s.FedLayout.IsSetProgrammatically,
                });
            */
            var outgoings = dbContext.OutgoingsDocFeds
                .Where(o => o.OutgoingsDoc.Division == division
                    && o.OutgoingsDoc.ProdPeriodYear == period.Year
                    && o.OutgoingsDoc.ProdPeriodPeriod == period.Period
                    && o.OutgoingsDoc.ProdPeriodWeek == period.Week
                    && o.OutgoingsDoc.WritingOffSum != null)
                .Select(s => s.FedLayout.LayoutId)
                .Distinct();
            //var oq = outgoings.ToQueryString();
            var fedLayoutItems = dbContext.FedLayouts
                .Where(fl => outgoings.Contains(fl.LayoutId)
                    && (fl.IsSetProgrammatically == null || fl.IsSetProgrammatically == false)
                    && fl.Weight != 0)
                .GroupBy(g => new
                {
                    MealId = g.MealId,
                    Weight = g.Weight,
                    Date = g.Date.Date,
                })
                .Select(s => new
                {
                    FedNumber = s.Sum(sa => sa.FedNumber),
                    MealId = s.Key.MealId,
                    Weight = s.Key.Weight,
                    Date = s.Key.Date,
                });
            //var fq = fedLayoutItems.ToQueryString();

            /*
             *             var layouts = dbContext.FedLayouts.Where(fl => fl.LayoutId == layout.LayoutId && (fl.IsSetProgrammatically == null || fl.IsSetProgrammatically == false));
        //var layouts = dbContext.FedLayouts.Where(fl => fl.LayoutId == layout.LayoutId && fl.Date >= startDate && fl.Date <= endDate);

        var dictMeals = new Dictionary<int, int>();
        int idx = 0;
        foreach (var meal in dbContext.Meals)
        {
            //dictMeals[meal.Name] = meal.MealId;
            dictMeals[meal.MealId] = idx++;
        }
        var dayLines = new DayLine[7];
        for (int i = 0; i < 7; i++)
        {
            dayLines[i] = new DayLine();
            dayLines[i].Date = startDate.AddDays(i);
            dayLines[i].TotalFed = float.NaN;
            dayLines[i].MealsCount = new int[dictMeals.Count];
            dayLines[i].MealsPercent = new int[dictMeals.Count];
        }
        //StringBuilder stringBuilder = new StringBuilder();
        foreach (var fl in layouts)
        {
            var dayIdx = fl.Date.Subtract(startDate).Days;
            dayLines[dayIdx].MealsCount[dictMeals[fl.MealId]] = Convert.ToInt32(fl.FedNumber);
            dayLines[dayIdx].MealsPercent[dictMeals[fl.MealId]] = Convert.ToInt32(fl.Weight);
            //stringBuilder.Append(dayIdx.ToString())
            //    .Append('\t')
            //    .Append(dictMeals[fl.MealId].ToString())
            //    .Append('\t')
            //    .Append(fl.FedNumber.ToString())
            //    .Append('\t')
            //    .AppendLine(fl.Weight.ToString());
        }
        //MessageBox.Show(stringBuilder.ToString());
        for (int i = 0; i < 7; i++)
        {
            dayLines[i].TotalFed = 0;
            for (var mealIdx = 0; mealIdx < dictMeals.Count; mealIdx++)
            {
                dayLines[i].TotalFed += dayLines[i].MealsCount[mealIdx] * dayLines[i].MealsPercent[mealIdx];
            }
            dayLines[i].TotalFed /= 100;
        }
             */

            var dailyFoodKits = new Dictionary<int, double[]>();
            var periodStartDate = period.GetStartDate();
            dailyFoodKits[0] = new double[7];
            foreach (var item in fedLayoutItems)
            {
                //if (item.ISP == true) continue;
                var dayIdx = Convert.ToInt32((item.Date - periodStartDate).TotalDays);
                if (!dailyFoodKits.ContainsKey(item.MealId))
                {
                    dailyFoodKits[item.MealId] = new double[7];
                }
                dailyFoodKits[item.MealId][dayIdx] += item.FedNumber;
                dailyFoodKits[0][dayIdx] += item.FedNumber * item.Weight / 100;
            }

            return dailyFoodKits;
        }

        private void btnWritingOff_Click(object sender, RoutedEventArgs e)
        {
            string division = cbDivisions.SelectedItem as string;
            //var date = DateTime.Parse("2023-03-15");
            var date = dpDate.SelectedDate.Value;

            var zsuPeriod = new ZsuProdPeriod(date);
            var zsuPeriodStart = zsuPeriod.GetStartDate();
            var zsuPeriodEnd = zsuPeriod.GetEndDate();

            var remainsBefore = GetDivisionRemainsToDate(division, zsuPeriodStart);
            var remainsAfter = GetDivisionRemainsToDate(division, zsuPeriodEnd);
            var incomes = GetDivisionIncomesForPeriod(division, zsuPeriod);
            var outgoings = GetDivisionDailyOutgoingsForPeriod(division, zsuPeriod);
            var foodKits = GetDivisionDailyFoodKitsForPeriod(division, zsuPeriod);

            var products = dbContext.Products.Select(p => new
            {
                ProductId = p.ProductId,
                Name = p.Name,
                AccUoM = p.AccUoM
            }).OrderBy(p => p.ProductId);
            var productsCount = products.Count();

            var productIndices = new Dictionary<string, int>();
            var productsVisibility = new bool[productsCount];
            var productIds = new string[productsCount];
            var productNames = new string[productsCount];
            var productUoMs = new string[productsCount];
            int prodIdx = 0;
            foreach (var product in products)
            {
                productIndices[product.ProductId] = prodIdx;
                productIds[prodIdx] = product.ProductId;
                productNames[prodIdx] = product.Name;
                productUoMs[prodIdx] = product.AccUoM;
                prodIdx++;
            }
            var productRemainsBefore = new double[productsCount];
            var productRemainsIncome = new double[productsCount];
            foreach (var remain in remainsBefore)
            {
                productsVisibility[productIndices[remain.Product.ProductId]] = true;
                productRemainsBefore[productIndices[remain.Product.ProductId]] = remain.Quantity;
                productRemainsIncome[productIndices[remain.Product.ProductId]] = remain.Quantity;
            }
            var productRemainsAfter = new double[productsCount];
            foreach (var remain in remainsAfter)
            {
                productsVisibility[productIndices[remain.Product.ProductId]] = true;
                productRemainsAfter[productIndices[remain.Product.ProductId]] = remain.Quantity;
            }
            var productIncomeDocs = new IncomesDoc[incomes.Count()];
            var productIncomeProds = new double[incomes.Count() * productsCount];
            int docIdx = 0;
            foreach (var income in incomes)
            {
                productIncomeDocs[docIdx] = income.Item1;
                foreach (var incomeProds in income.Item2)
                {
                    productsVisibility[productIndices[incomeProds.Product.ProductId]] = true;
                    productRemainsIncome[productIndices[incomeProds.Product.ProductId]] += incomeProds.Quantity;
                    productIncomeProds[(docIdx * productsCount) + productIndices[incomeProds.Product.ProductId]] = incomeProds.Quantity;
                }
                docIdx++;
            }
            var fedDays = new DateTime[7 + 1]; // Next day after this period
            for (var dayIdx = 0; dayIdx <= 7; dayIdx++)
            {
                fedDays[dayIdx] = zsuPeriodStart.AddDays(dayIdx);
            }
            var fedTotal = new double[7];
            double fedTotalTotal = 0;
            var fedBreakfast = new double[7];
            var fedLaunch = new double[7];
            var fedDinner = new double[7];
            var fedProds = new double[7 * productsCount];
            var fedProdsTotal = new double[productsCount];
            var foodKitsArray = foodKits.Keys.ToArray();
            // Total, Breakfast, Launch, Dinner
            if (foodKitsArray.Count() < 4)
            {
            }
            else
            { 
                for (var dayIdx = 0; dayIdx < 7; dayIdx++)
                {
                    if (foodKits[foodKitsArray[0]].Count() < dayIdx + 1)
                    {
                        break;
                    }
                    fedTotal[dayIdx] = foodKits[foodKitsArray[0]][dayIdx];
                    fedTotalTotal += fedTotal[dayIdx];
                    fedBreakfast[dayIdx] = foodKits[foodKitsArray[1]][dayIdx];
                    fedLaunch[dayIdx] = foodKits[foodKitsArray[2]][dayIdx];
                    fedDinner[dayIdx] = foodKits[foodKitsArray[3]][dayIdx];
                    foreach (var prodId in outgoings.Keys)
                    {
                        fedProds[(dayIdx * productsCount) + productIndices[prodId]] = outgoings[prodId][dayIdx];
                        fedProdsTotal[productIndices[prodId]] += outgoings[prodId][dayIdx];
                    }
                }
            }

            string[] tplPath = {
                ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                "Templates",
                "reportcard.html"};
            var tplFilename = System.IO.Path.Combine(tplPath);
            string htmlTemplate;
            try
            {
                htmlTemplate = System.IO.File.ReadAllText(tplFilename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
                return;
            }
            if (_parser.TryParse(htmlTemplate, out var template, out var error))
            {
                var context = new TemplateContext();
                context.SetValue("Division", division);
                context.SetValue("ProductIds", productIds);
                context.SetValue("ProductNames", productNames);
                context.SetValue("ProductUoMs", productUoMs);
                context.SetValue("ProductVisibilities", productsVisibility);
                context.SetValue("ProductRemainsBefore", productRemainsBefore);
                context.SetValue("ProductRemainsIncome", productRemainsIncome);
                context.SetValue("ProductIncomeDocs", productIncomeDocs);
                context.SetValue("ProductIncomeProds", productIncomeProds);
                context.SetValue("FedDays", fedDays);
                context.SetValue("FedTotal", fedTotal);
                context.SetValue("FedTotalTotal", fedTotalTotal);
                context.SetValue("FedBreakfast", fedBreakfast);
                context.SetValue("FedLunch", fedLaunch);
                context.SetValue("FedDinner", fedDinner);
                context.SetValue("FedProds", fedProds);
                context.SetValue("FedProdsTotal", fedProdsTotal);
                context.SetValue("ProductRemainsAfter", productRemainsAfter);

                //// <YYYY-MM-dd>_<wbNumber>_<customer>
                var filename = System.IO.Path.Combine(
                    ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                    "Output",
                    "reportcard_" + DateTime.Now.ToString("yyyy-MM-dd") + ".html");
                //if (File.Exists(fileName))
                //{
                //    File.Delete(fileName);
                //}
                //File.Copy(tplFileName, fileName);
                //Console.WriteLine(template.Render(context));
                System.IO.File.WriteAllText(filename, template.Render(context));
                var psi = new ProcessStartInfo(filename) { UseShellExecute = true };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show($"Error: {error}");
            }

            /*
                        var filename = System.IO.Path.Combine(
                            ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                            "Output",
                            "test.txt");
                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);
                        }
                        using (StreamWriter sw = new StreamWriter(filename))
                        {
                            sw.WriteLine("Залишки '" + division + "' на " + zsuPeriodStart.ToShortDateString() + ":");
                            foreach (var remain in remainsBefore)
                            {
                                sw.WriteLine("\t" + remain.Product.ProductId + "\t" + remain.Quantity + "\t" + remain.Product.Name);
                            }
                            sw.WriteLine("Постачання продуктів:");
                            foreach (var income in incomes)
                            {
                                sw.WriteLine("Документ " + income.Item1.Number + " від " + income.Item1.DocDate.ToShortDateString());
                                foreach (var remain in income.Item2)
                                {
                                    sw.WriteLine("\t" + remain.Product.ProductId + "\t" + remain.Quantity + "\t" + remain.Product.Name);
                                }
                            }
                            sw.WriteLine("Кількість осіб, що харчується:");
                            sw.Write("\t\t");
                            foreach (var mealId in foodKits.Keys)
                            {
                                if (mealId == 0)
                                {
                                    sw.Write("Всього\t");
                                }
                                else
                                {
                                    sw.Write(dbContext.Meals.Find(mealId).Name + "\t");
                                }
                            }
                            sw.WriteLine();
                            for (int i = 0; i < 7; i++)
                            {
                                sw.Write(zsuPeriod.GetStartDate().AddDays(i).ToShortDateString() + "\t");
                                foreach (var mealId in foodKits.Keys)
                                {
                                    sw.Write(foodKits[mealId][i] + "\t");
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine("Витрата продуктів:");
                            sw.Write("\t");
                            for (int i = 0; i < 7; i++)
                            {
                                sw.Write(zsuPeriod.GetStartDate().AddDays(i).ToShortDateString() + "\t");
                            }
                            sw.WriteLine();
                            foreach (var outgoing in outgoings.Keys)
                            {
                                sw.Write(outgoing + "\t");
                                for (int i = 0; i < 7; i++)
                                {
                                    sw.Write(outgoings[outgoing][i] + "\t");
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine("Залишки '" + division + "' на " + zsuPeriodEnd.AddDays(1).ToShortDateString() + ":");
                            foreach (var remain in remainsAfter)
                            {
                                sw.WriteLine("\t" + remain.Product.ProductId + "\t" + remain.Quantity + "\t" + remain.Product.Name);
                            }
                        }
            */
            var dailyOutgoings = GetDivisionMealsOutgoingsForDate(division, zsuPeriod);
            string[] tplPath1 = {
                ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                "Templates",
                "reportcardwb.html"};
            var tplFilename1 = System.IO.Path.Combine(tplPath1);
            string htmlTemplate1;
            try
            {
                htmlTemplate1 = System.IO.File.ReadAllText(tplFilename1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
                return;
            }
            if (_parser.TryParse(htmlTemplate1, out var template1, out var error1))
            {
                var context = new TemplateContext();
                context.SetValue("Division", division);
                context.SetValue("DataDays", dailyOutgoings);
                context.SetValue("FedDays", fedDays);

                //// <YYYY-MM-dd>_<wbNumber>_<customer>
                var filename = System.IO.Path.Combine(
                    ConfigurationManager.AppSettings.Get("DataDirectory") as string,
                    "Output",
                    "reportcardwb_" + DateTime.Now.ToString("yyyy-MM-dd") + ".html");
                //if (File.Exists(fileName))
                //{
                //    File.Delete(fileName);
                //}
                //File.Copy(tplFileName, fileName);
                //Console.WriteLine(template.Render(context));
                System.IO.File.WriteAllText(filename, template1.Render(context));
                var psi = new ProcessStartInfo(filename) { UseShellExecute = true };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show($"Error: {error1}");
            }
        }

        // Report Card WayBill Product Line
        class RCWBPL
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string UoM { get; set; }
            public float AmountBreakfast { get; set; }
            public float AmountLunch { get; set; }
            public float AmountDinner { get; set; }
            public float AmountTotal { get; set; }
        }

        List<RCWBPL>[] GetDivisionMealsOutgoingsForDate(string division, ZsuProdPeriod period)
        {
            var outgoings = dbContext.Outgoings1
                .Where(o1 => o1.OutgoingsDoc.Division == division
                    //&& o1.OutgoingsDoc.DocDate >= period.GetStartDate() 
                    //&& o1.OutgoingsDoc.DocDate < period.GetEndDate()
                    && o1.OutgoingsDoc.ProdPeriodYear == period.Year
                    && o1.OutgoingsDoc.ProdPeriodPeriod == period.Period
                    && o1.OutgoingsDoc.ProdPeriodWeek == period.Week
                    //&& o1.OutgoingsDoc.WritingOffType == "списання за меню-розкладкою"
                    && o1.OutgoingsDoc.WritingOffSum != null)
                .GroupBy(g => new
                {
                    LayoutDate = g.OutgoingsDoc.LayoutDate.Date,
                    MealId = g.MealId,
                    ProductId = g.ProductId,
                })
                .Select(s => new
                {
                    Date = s.Key.LayoutDate,
                    ProductId = s.Key.ProductId,
                    MealId = s.Key.MealId,
                    Quantity = s.Sum(sa => sa.Amount),
                    Cost = s.Sum(sa => sa.Sum),
                }); //OrderBy LayoutDate, ProductId

            //------------------------------------------------------
            var rcDays = new SortedDictionary<string, RCWBPL>[7];
            for (var idx = 0; idx < rcDays.Length; idx++)
            {
                rcDays[idx] = new SortedDictionary<string, RCWBPL>();
            }
            var periodStartDate = period.GetStartDate();
            foreach (var item in outgoings)
            {
                //var dayIdx = Convert.ToInt32((item.Date - periodStartDate).TotalDays);
                var dayIdx = Convert.ToInt32(Math.Floor((item.Date - periodStartDate).TotalDays));
                if (!rcDays[dayIdx].ContainsKey(item.ProductId))
                {
                    var itemProduct = dbContext.Products.Find(item.ProductId);
                    rcDays[dayIdx][item.ProductId] = new RCWBPL
                    {
                        Name = itemProduct.Name,
                        Id = item.ProductId,
                        UoM = itemProduct.AccUoM
                    };
                }
                switch (item.MealId)
                {
                    case 1:
                        rcDays[dayIdx][item.ProductId].AmountBreakfast = (float)item.Quantity;
                        break;
                    case 3:
                        rcDays[dayIdx][item.ProductId].AmountLunch = (float)item.Quantity;
                        break;
                    case 5:
                        rcDays[dayIdx][item.ProductId].AmountDinner = (float)item.Quantity;
                        break;
                }
                rcDays[dayIdx][item.ProductId].AmountTotal =
                    rcDays[dayIdx][item.ProductId].AmountBreakfast
                    + rcDays[dayIdx][item.ProductId].AmountLunch
                    + rcDays[dayIdx][item.ProductId].AmountDinner;
            }

            //------------------------------------------------------
            var dailyOutgoings = new List<RCWBPL>[7];
            for (var idx = 0; idx < dailyOutgoings.Length; idx++)
            {
                dailyOutgoings[idx] = new List<RCWBPL>();
                //    var r1 = new RCWBPL
                //    {
                //        Name = "Буряк столовий свіжий",
                //        Id = "1004",
                //        UoM = "кг",
                //        AmountBreakfast = 1.3f,
                //        AmountLunch = 0,
                //        AmountDinner = 0.7f,
                //        AmountTotal = 2
                //    };
                //    var r2 = new RCWBPL
                //    {
                //        Name = "Сіль кухонна",
                //        Id = "1625",
                //        UoM = "кг",
                //        AmountBreakfast = 0.3f,
                //        AmountLunch = 0.1f,
                //        AmountDinner = 0.1f,
                //        AmountTotal = 0.5f
                //    };
                //    var r3 = new RCWBPL
                //    {
                //        Name = "Яйця курячі першого ґатунку",
                //        Id = "2056",
                //        UoM = "шт",
                //        AmountBreakfast = 0.25f,
                //        AmountLunch = 0.5f,
                //        AmountDinner = 0.25f,
                //        AmountTotal = 1
                //    };
                //    dailyOutgoings[idx].Add(r1);
                //    dailyOutgoings[idx].Add(r2);
                //    dailyOutgoings[idx].Add(r3);

                foreach (var d in rcDays[idx].Values)
                {
                    dailyOutgoings[idx].Add(d);
                }
            }

            return dailyOutgoings;
        }

        private void aboutLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true };
            Process.Start(psi);
            e.Handled = true;
        }
    }
}
