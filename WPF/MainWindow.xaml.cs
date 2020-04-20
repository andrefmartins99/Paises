namespace WPF
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
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
            LoadData();
        }

        private async void LoadData()
        {
            bool load;

            var connection = NetworkStatusService.CheckConnection();//Check if there is internet connection

            if (connection.IsSuccess == true)
            {
                await LoadApiData();
                load = true;
            }
            else
            {
                await LoadLocalData();
                load = false;
            }

            cbCountries.ItemsSource = Countries;

            if (load == true)
            {
                lblStatus.Content = "Data retrieved from api. Updating database...";

                await DatabaseService.SaveData(Countries);

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
        /// <returns></returns>
        private async Task LoadApiData()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            var response = await ApiService.GetData("http://restcountries.eu", "/rest/v2/all", progress);

            Countries = (List<Country>)response.Result;
        }

        /// <summary>
        /// Get the data from the database
        /// </summary>
        /// <returns></returns>
        private async Task LoadLocalData()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            Countries = await DatabaseService.GetData(progress);
        }

        private void ReportProgress(object sender, ProgressReport e)
        {
            pBStatus.Value = e.PercentageComplete;
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
        }
    }
}
