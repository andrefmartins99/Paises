namespace WPF
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Library.Models;
    using Library.Services;
    using SharpVectors.Converters;
    using SharpVectors.Renderers.Wpf;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Country> Countries { get; set; }

        public bool Load { get; set; }
        //== true -> internet connection, data retrieved from api
        //== false -> no internet connection, data retrieved from local database

        public DispatcherTimer Timer { get; set; }

        public List<string> Clocks { get; set; }
        //List with the current date/time of the timezones of the selected country

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
            var connection = NetworkStatusService.CheckConnection();//Check if there is internet connection

            if (connection.IsSuccess == true)
            {
                txbStatus.Text = "Retrieving data from api...";

                await LoadApiData();
                await CountryFlagService.DownloadFlags(Countries);
                Load = true;
            }
            else
            {
                await LoadLocalData();
                Load = false;
                btnMaps.Visibility = Visibility.Hidden;
            }

            CreateTimer();
            cbCountries.ItemsSource = Countries;

            if (Load == true)
            {
                txbStatus.Text = "Data retrieved from api.";

                await SavetoDatabase();

                txbStatus.Text = "Database updated.";
            }
            else
            {
                if (Countries.Count > 0)
                {
                    txbStatus.Text = "No internet connection. Data retrieved from database.";
                }
                else
                {
                    txbStatus.Text = "No internet connection. No data found in the database. The first run of the program needs to be made with internet connection. Please try again later.";
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

            FixCurrencies();
        }

        /// <summary>
        /// Fix issues with currencies
        /// </summary>
        private void FixCurrencies()
        {
            Countries[34].Currencies.RemoveAt(0);
            Countries[145].Currencies.RemoveAt(0);
            Countries[155].Currencies.RemoveAt(1);
            Countries[170].Currencies.RemoveAt(0);
            Countries[209].Currencies.RemoveAt(1);
            Countries[225].Currencies.RemoveAt(1);
            Countries[249].Currencies.RemoveAt(8);

            foreach (var country in Countries)
            {
                foreach (var currency in country.Currencies)
                {
                    if (currency.Code == "USD")
                    {
                        currency.Name = "United States Dollar";
                    }

                    if (currency.Code == "ZAR")
                    {
                        currency.Symbol = "R";
                    }

                    if (currency.Code == "ILS")
                    {
                        currency.Name = "Israeli new shekel";
                    }
                }
            }
        }

        /// <summary>
        /// Create DispatcherTimer
        /// </summary>
        private void CreateTimer()
        {
            Timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1)
            };

            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Countries != null)
            {
                if (cbCountries.SelectedIndex != -1)
                {
                    if (Clocks != null)
                    {
                        if (Clocks.Count == Countries[cbCountries.SelectedIndex].Timezones.Count)
                        {
                            for (int i = 0, j = 0; i < Clocks.Count; i++, j++)
                            {
                                if (Countries[cbCountries.SelectedIndex].Timezones[j] == "UTC")
                                {
                                    Clocks[i] = DateTime.UtcNow.ToString();
                                }
                                else
                                {
                                    if (Countries[cbCountries.SelectedIndex].Timezones[j] == "UTC+04")
                                    {
                                        Clocks[i] = DateTime.UtcNow.AddHours(4).ToString();
                                    }
                                    else if (Countries[cbCountries.SelectedIndex].Timezones[j] == "UTC+11")
                                    {
                                        Clocks[i] = DateTime.UtcNow.AddHours(11).ToString();
                                    }
                                    else if (Countries[cbCountries.SelectedIndex].Timezones[j][3] == '+')
                                    {
                                        Clocks[i] = DateTime.UtcNow.Add(TimeSpan.Parse(Countries[cbCountries.SelectedIndex].Timezones[j].Replace("UTC+", string.Empty))).ToString();
                                    }
                                    else
                                    {
                                        Clocks[i] = DateTime.UtcNow.Subtract(TimeSpan.Parse(Countries[cbCountries.SelectedIndex].Timezones[j].Replace("UTC-", string.Empty))).ToString();
                                    }
                                }
                            }

                            icClock.ItemsSource = null;
                            icClock.ItemsSource = Clocks;
                        }
                    }
                }
            }
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
                txbStatus.Text = "Updating database...";
            }
        }

        private void cbCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowData();
        }

        /// <summary>
        /// Show data of the selected country
        /// </summary>
        private void ShowData()
        {
            int country = cbCountries.SelectedIndex;

            sPCountries.DataContext = Countries[country];
            ShowFlag(country);
            InitClocks(Countries[country].Timezones.Count);
        }

        /// <summary>
        /// Show the flag of the selected country
        /// </summary>
        /// <param name="country">comboBox selected index</param>
        private void ShowFlag(int country)
        {
            try
            {
                string fileName = $"{Countries[country].Alpha3Code}.svg";
                string path = Path.Combine(Environment.CurrentDirectory, @"Flags\", fileName);

                WpfDrawingSettings settings = new WpfDrawingSettings();
                FileSvgReader converter = new FileSvgReader(settings);

                DrawingGroup drawing = converter.Read(path);

                if (drawing != null)
                {
                    flagImage.Source = new DrawingImage(drawing);
                }
                else
                {
                    DialogService.ShowMessageBox("Error", $"The flag of {Countries[country].Name} wasn´t found. Please restart the app.");
                    flagImage.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\NotFound\" + "NotFound.jpg", UriKind.Absolute));
                }
            }
            catch
            {
                if (Load == true)
                {
                    flagImage.Source = null;
                    DialogService.ShowMessageBox("Error", $"The flag of {Countries[country].Name} wasn´t found. Please restart the app.");
                    flagImage.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\NotFound\" + "NotFound.jpg", UriKind.Absolute));
                }
                else
                {
                    flagImage.Source = null;
                    DialogService.ShowMessageBox("Error", $"The flag of {Countries[country].Name} wasn´t found." + Environment.NewLine + "Please restart the app with internet connection.");
                    flagImage.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\NotFound\" + "NotFound.jpg", UriKind.Absolute));
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (txbStatus.Text == "Updating database...")
            {
                DialogService.ShowMessageBox("Warning", "Database is being updated. Please try again later.");
                e.Cancel = true;
            }
        }

        private void txbBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var text = (sender as TextBlock).Text;

            foreach (var country in Countries)
            {
                if (text == country.Alpha3Code)
                {
                    (sender as TextBlock).Text = country.Name;
                    return;
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var text = (sender as Hyperlink).DataContext;

            for (int i = 0; i < Countries.Count; i++)
            {
                if (text.ToString() == Countries[i].Alpha3Code)
                {
                    cbCountries.SelectedIndex = i;
                }
            }
        }

        /// <summary>
        /// Create the list Clocks with the number of items equal to the number of timezones of the selected country
        /// </summary>
        /// <param name="timezonesCount">Number of timezones of the selected country</param>
        private void InitClocks(int timezonesCount)
        {
            Clocks = new List<string>();
            string clock = string.Empty;

            for (int i = 0; i < timezonesCount; i++)
            {
                Clocks.Add(clock);
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            DialogService.ShowMessageBox("About", "Developed by André Martins" + Environment.NewLine + "Version 1.0" + Environment.NewLine + "Date: 02/06/2020");
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            DialogService.ShowMessageBox("Help", "- Hover your cursor over a word in bold to know the meaning of it." + Environment.NewLine + Environment.NewLine + "Example: Hover your cursor over the demonym to find out what it is." + Environment.NewLine + Environment.NewLine + "- Hover you mouse over the regional bloc acronym or currency code to find out the name of the regional bloc/currency." + Environment.NewLine + Environment.NewLine + "- Tip: Check the name of a country in border(s).");
        }

        private void btnMaps_Click(object sender, RoutedEventArgs e)
        {
            var answer = DialogService.ShowMessageBox("Maps", "You are being redirected to Google Maps." + Environment.NewLine + "Do you want to proceed?");

            if (answer == MessageBoxResult.Yes)
            {
                OpenMaps();
            }
        }

        /// <summary>
        /// Open Google Maps in the latitude and longitude of the selected country
        /// </summary>
        public void OpenMaps()
        {
            int country = cbCountries.SelectedIndex;
            string lat = Countries[country].Latlng[0].ToString().Replace(',', '.');
            string lng = Countries[country].Latlng[1].ToString().Replace(',', '.');
            string alt;

            if (Countries[country].Area < 30)
            {
                alt = "14z";
            }
            else if (Countries[country].Area < 100)
            {
                alt = "12z";
            }
            else if (Countries[country].Area < 500)
            {
                alt = "10z";
            }
            else if (Countries[country].Area < 5000)
            {
                alt = "8z";
            }
            else if (Countries[country].Area < 200000)
            {
                alt = "6z";
            }
            else if (Countries[country].Area < 2000000)
            {
                alt = "5z";
            }
            else if (Countries[country].Area < 10000000)
            {
                alt = "4z";
            }
            else
            {
                alt = "3z";
            }

            string link = $"https://www.google.pt/maps/place/{lat},{lng}/@{lat},{lng},{alt}";

            try
            {
                Process.Start(link);
            }
            catch (Exception ex)
            {
                DialogService.ShowMessageBox("Error", ex.Message);
            }
        }
    }
}
