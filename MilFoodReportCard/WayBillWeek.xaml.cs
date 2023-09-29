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
        public WayBillWeek()
        {
            InitializeComponent();
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

            DialogResult = true;
        }
    }
}
