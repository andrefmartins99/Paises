namespace Library.Services
{
    using System.Net;
    using Models;

    public class NetworkStatusService
    {
        /// <summary>
        /// Check if there is internet connection
        /// </summary>
        /// <returns>If there is internet connection, it returns a successful response, if not it returns an unsuccessful response</returns>
        public Response CheckConnection()
        {
            var client = new WebClient();

            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSuccess = true
                    };
                }
            }
            catch
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "No Internet connection"
                };
            }
        }
    }
}
