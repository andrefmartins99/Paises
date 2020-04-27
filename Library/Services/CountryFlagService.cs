namespace Library.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Models;

    public static class CountryFlagService
    {
        /// <summary>
        /// Download the flags
        /// </summary>
        /// <param name="Countries">List of countries</param>
        /// <returns>Returns a task</returns>
        public static async Task DownloadFlags(List<Country> Countries)
        {
            if (!Directory.Exists("Flags"))
            {
                Directory.CreateDirectory("Flags");
            }

            List<string> Flags = new List<string>();

            await Task.Run(() =>
            {
                foreach (var country in Countries)
                {
                    using (WebClient client = new WebClient())
                    {
                        var url = new Uri(country.Flag);
                        string fileName = $"{country.Alpha3Code}.svg";
                        string path = Path.Combine(Environment.CurrentDirectory, @"Flags\", fileName);

                        client.DownloadFileAsync(url, path);
                    }
                }
            });
        }
    }
}
