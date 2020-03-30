using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Library.Models;
using Newtonsoft.Json;

namespace Library.Services
{
    public static class ApiService
    {
        /// <summary>
        /// Connect to the api and retrieve the data in it
        /// </summary>
        /// <param name="url">url of the api</param>
        /// <param name="controller">folder where the data is</param>
        /// <param name="progress"></param>
        /// <returns>Returns a response, that will have as result a list of countries(if there is no error)</returns>
        public static async Task<Response> GetData(string url, string controller, IProgress<ProgressReport> progress)
        {
            try
            {
                ProgressReport report = new ProgressReport();
                var client = new HttpClient();
                client.BaseAddress = new Uri(url);

                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();//Retrieve everything that is in the api

                if (response.IsSuccessStatusCode == false)//If there´s an error
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore//Handle nulls coming from api
                };

                var countries = JsonConvert.DeserializeObject<List<Country>>(result, jsonSettings);
                report.Countries = countries;
                report.PercentageComplete = (countries.Count * 100) / report.Countries.Count;
                progress.Report(report);

                return new Response
                {
                    IsSuccess = true,
                    Result = countries
                };
            }
            catch (Exception e)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = e.Message
                };
            }
        }
    }
}
