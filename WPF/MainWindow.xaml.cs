namespace WPF
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.IO;
    using System.Windows.Media;
    using SharpVectors.Renderers.Wpf;
    using SharpVectors.Converters;
    using Library.Services;
    using Library.Models;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Country> Countries { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Countries = new List<Country>();
            LoadData();
        }

        /// <summary>
        /// Load data
        /// </summary>
        private async void LoadData()
        {
            bool load;

            var connection = NetworkStatusService.CheckConnection();//Check if there is internet connection

            if (connection.IsSuccess == true)
            {
                await LoadApiData();
                load = true;
                await CountryFlagService.DownloadFlags(Countries);
            }
            else
            {
                await LoadLocalData();
                load = false;
            }

            cbCountries.ItemsSource = Countries;

            if (load == true)
            {
                lblStatus.Content = "Data retrieved from api.";

                await SavetoDatabase();

                lblStatus.Content = "Database updated.";
            }
            else
            {
                if (Countries.Count > 0)
                {
                    lblStatus.Content = "No internet connection. Data retrieved from database.";
                }
                else
                {
                    lblStatus.Content = "No internet connection. No data found in the database. The first run of the program needs to be made with internet connection. Please try again later.";
                }
            }
        }

        /// <summary>
        /// Get the data from the api
        /// </summary>
        /// <returns>Returns a task</returns>
        private async Task LoadApiData()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            var response = await ApiService.GetData("http://restcountries.eu", "/rest/v2/all", progress);

            if (response.IsSuccess == false)
            {
                DialogService.ShowMessageBox("Error", response.Message);
                return;
            }

            Countries = (List<Country>)response.Result;
        }

        /// <summary>
        /// Get the data from the database
        /// </summary>
        /// <returns>Returns a task</returns>
        private async Task LoadLocalData()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            Countries = await DatabaseService.GetData(progress);
        }

        /// <summary>
        /// Save the data in the database
        /// </summary>
        /// <returns>Returns a task</returns>
        private async Task SavetoDatabase()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            await DatabaseService.SaveData(Countries, progress);
        }

        private void ReportProgress(object sender, ProgressReport e)
        {
            pBStatus.Value = e.PercentageComplete;

            if (Countries.Count == 250 && pBStatus.Value == 0)
            {
                lblStatus.Content = "Updating database...";
            }
        }

        private void cbCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int country;

            country = cbCountries.SelectedIndex;

            lblName.Content = Countries[country].Name;
            lblCapital.Content = Countries[country].Capital;
            lblRegion.Content = Countries[country].Region;
            lblSubRegion.Content = Countries[country].Subregion;
            lblPopulation.Content = Countries[country].Population;
            lblGini.Content = Countries[country].Gini;
            ShowFlag(country);
        }

        /// <summary>
        /// Show the flag of the selected country
        /// </summary>
        /// <param name="country">comboBox selected index</param>
        private void ShowFlag(int country)
        {
            WpfDrawingSettings settings = new WpfDrawingSettings
            {
                IncludeRuntime = true,
                TextAsGeometry = false
            };

            string fileName = $"{Countries[country].Alpha3Code}.svg";
            string path = Path.Combine(Environment.CurrentDirectory, @"Flags\", fileName);

            string svgFile = path;

            FileSvgReader converter = new FileSvgReader(settings);

            DrawingGroup drawing = converter.Read(svgFile);

            if (drawing != null)
            {
                flagimage.Source = new DrawingImage(drawing);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lblStatus.Content.ToString() == "Updating database...")
            {
                DialogService.ShowMessageBox("Warning", "Database is being updated. Please try again later.");
                e.Cancel = true;
            }
        }
    }
}
