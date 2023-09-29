using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
        }

        private void btnDbFilename_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.InitialDirectory = tbDbFilename.Text; // Use current value for initial dir
            fileDialog.Filter = "База даних ZSU_PROD_2022|DataBase.accdb";
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case true:
                    var file = fileDialog.FileName;
                    tbDbFilename.Text = file;
                    tbDbFilename.ToolTip = file;

                    // User accepted dialog box
                    break;
                case false:
                    // User canceled dialog box
                    break;
                default:
                    // Indeterminate
                    break;
            }
        }

        private void btnDataDirectory_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.SaveFileDialog();
            //fileDialog.ValidateNames = false;
            //fileDialog.CheckFileExists = false;
            //fileDialog.CheckPathExists = true;
            fileDialog.InitialDirectory = tbDataDirectory.Text; // Use current value for initial dir
            fileDialog.Title = "Select a Directory"; // instead of default "Save As"
            fileDialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            fileDialog.FileName = "select"; // Filename will then be "select.this.directory"
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case true:
                    var path = fileDialog.FileName;
                    // Remove fake filename from resulting path
                    path = path.Replace("\\select.this.directory", "");
                    path = path.Replace(".this.directory", "");
                    // If user has changed the filename, create the new directory
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    // Our final value is in path
                    tbDataDirectory.Text = path;
                    tbDataDirectory.ToolTip = path;

                    // User accepted dialog box
                    break;
                case false:
                    // User canceled dialog box
                    break;
                default:
                    // Indeterminate
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            return;
        }
    }
}
