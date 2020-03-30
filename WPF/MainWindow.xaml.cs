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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Library.Services;
using Library.Models;

namespace WPF
{
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

                load = false;
            }

            cbCountries.ItemsSource = Countries;

            if (load == true)
            {
                lblStatus.Content = "Data retrieved from api";
            }
            else
            {
                lblStatus.Content = "Data retrieved from database";
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
