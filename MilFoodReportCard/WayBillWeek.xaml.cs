using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MilFoodReportCard
{
    /// <summary>
    /// Interaction logic for WayBillWeek.xaml
    /// </summary>
    public partial class WayBillWeek : Window
    {
        private ZsuProdPeriod period;

        public WayBillWeek(ZsuProdPeriod period)
        {
            InitializeComponent();

            this.period = period;

            dpWbPeriodStart.SelectedDate = period.GetStartDate();
            dpWbPeriodEnd.SelectedDate = period.GetEndDate();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (tbWbNumber.Text.Trim() == ""
                || tbWbCustomer.Text.Trim() == ""
                || tbWbRecipient.Text.Trim() == ""
                || dpWbDate.SelectedDate == null
                || tbWbFedNumber.Text.Trim() == "")
            {
                return;
            }

            if (cbWbLimitPeriod.IsChecked == true)
            {
                DateTime wbPeriodStart;
                DateTime wbPeriodEnd;
                if (dpWbPeriodStart.SelectedDate is DateTime startVal)
                {
                    wbPeriodStart = startVal;
                }
                else
                {
                    return;
                }
                if (dpWbPeriodEnd.SelectedDate is DateTime endVal)
                {
                    wbPeriodEnd = endVal;
                }
                else
                {
                    return;
                }
                if (wbPeriodStart.Date < period.GetStartDate().Date || wbPeriodStart.Date > period.GetEndDate().Date)
                {
                    return;
                }
                if (wbPeriodEnd.Date < period.GetStartDate().Date || wbPeriodEnd.Date > period.GetEndDate().Date)
                {
                    return;
                }
                if (wbPeriodStart.Date > wbPeriodEnd.Date)
                {
                    return;
                }
            }

            DialogResult = true;
        }

        private void cbWbLimitPeriod_Checked(object sender, RoutedEventArgs e)
        {
            if (cbWbLimitPeriod.IsChecked != null && cbWbLimitPeriod.IsChecked == true)
            {
                dpWbPeriodStart.IsEnabled = true;
                dpWbPeriodEnd.IsEnabled = true;
            } else
            {
                dpWbPeriodStart.IsEnabled = false;
                dpWbPeriodEnd.IsEnabled = false;
            }
        }
    }
}
