using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MilFoodReportCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void AddOrUpdateAppSettings(string key, string value)
        {
            //try
            //{
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            //}
            //catch (ConfigurationErrorsException)
            //{
            //    Console.WriteLine("Error writing app settings");
            //}
        }

        public App()
        {
            // @TODO Add splashscreen? https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/how-to-add-a-splash-screen-to-a-wpf-application?view=netframeworkdesktop-4.8

            //ConfigurationManager.AppSettings.Set("DatabaseFilename", null);
            //ConfigurationManager.AppSettings.Set("DatabasePassword", null);
            //ConfigurationManager.AppSettings.Set("DataDirectory", null);

            var databaseFilename = ConfigurationManager.AppSettings.Get("DatabaseFilename");
            var databasePassword = ConfigurationManager.AppSettings.Get("DatabasePassword");
            var dataDirectory = ConfigurationManager.AppSettings.Get("DataDirectory");
            //var outputDirectory = ConfigurationManager.AppSettings.Get("OutputDirectory");
            //var templatesDirectory = ConfigurationManager.AppSettings.Get("TemplatesDirectory");
            if (databaseFilename == null
                || databasePassword == null
                || dataDirectory == null)
            {
                var cfgWnd = new ConfigurationWindow();
                cfgWnd.tbDbFilename.Text = databaseFilename == null ? "" : databaseFilename as string;
                cfgWnd.tbDbPassword.Text = databasePassword == null ? "" : databasePassword as string;
                cfgWnd.tbDataDirectory.Text = dataDirectory == null ? "" : dataDirectory as string;
                var result = cfgWnd.ShowDialog();
                switch (result)
                {
                    case true:
                        databaseFilename = cfgWnd.tbDbFilename.Text;
                        databasePassword = cfgWnd.tbDbPassword.Text;
                        dataDirectory = cfgWnd.tbDataDirectory.Text;
                        if (databaseFilename == null
                            || databasePassword == null
                            || dataDirectory == null)
                        {
                            throw new Exception("Application should be configured before first run");
                        }
                        if (databaseFilename.Trim() == ""
                            || databasePassword.Trim() == ""
                            || dataDirectory.Trim() == "")
                        {
                            throw new Exception("Application should be configured before first run");
                        }
                        // https://stackoverflow.com/questions/5274829/configurationmanager-appsettings-how-to-modify-and-save
                        // https://learn.microsoft.com/en-us/dotnet/api/system.configuration.configuration.save?view=windowsdesktop-8.0
                        //ConfigurationManager.AppSettings.Set("DatabaseFilename", databaseFilename.Trim());
                        //ConfigurationManager.AppSettings.Set("DatabasePassword", databasePassword.Trim());
                        //ConfigurationManager.AppSettings.Set("DataDirectory", dataDirectory.Trim());
                        AddOrUpdateAppSettings("DatabaseFilename", databaseFilename.Trim());
                        AddOrUpdateAppSettings("DatabasePassword", databasePassword.Trim());
                        AddOrUpdateAppSettings("DataDirectory", dataDirectory.Trim());

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
            //string outputDirectory = ConfigurationManager.AppSettings.Get("OutputDirectory");
            //if (!Directory.Exists(outputDirectory))
            //{
            //    Directory.CreateDirectory(outputDirectory);
            //}
            var oledb12Installed = new System.Data.OleDb.OleDbEnumerator()
                .GetElements().AsEnumerable()
                .Any(x => x.Field<string>("SOURCES_NAME") == "Microsoft.ACE.OLEDB.12.0");
            if (!oledb12Installed)
            {
                MessageBox.Show("You have no Microsoft.ACE.OLEDB.12.0 provider installed. Check Microsoft Access Database Engine 2016 Redistributable at https://www.microsoft.com/en-us/download/details.aspx?id=54920");
                this.Shutdown(1);
            }
        }
    }
}
