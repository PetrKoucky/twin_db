using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace twin_db
{
    public static class HtmlDownloader
    {
        private static HttpClient htClient = new HttpClient();
        public static async Task<WebPage> DownloadPageAsync(string URL)
        {
            DateTime start = DateTime.Now;
            WebPage wp =new WebPage(URL);
            try
            {            
                var res = await htClient.GetByteArrayAsync(URL);
                wp.content = Encoding.UTF8.GetString(res, 0 , res.Length).ToString();
                wp.Valide();
                Logger.Log("Downloaded in " + DateTime.Now.Subtract(start).ToString() + ", " + wp.URL);
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to download URL: " + wp.URL);
            }

            return wp;
        }
    }
}