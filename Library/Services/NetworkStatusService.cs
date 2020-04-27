namespace Library.Services
{
    using System.Net;
    using Models;

    public static class NetworkStatusService
    {
        /// <summary>
        /// Check if there is internet connection
        /// </summary>
        /// <returns>If there is internet connection, it returns a successful response, if not it returns an unsuccessful response</returns>
        public static Response CheckConnection()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.OpenRead("http://clients3.google.com/generate_204");

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
                    IsSuccess = false
                };
            }
        }
    }
}
