using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace twin_db
{
    public static class HtmlDownloader
    {
        private static HttpClient htClient = new HttpClient();
        public static async Task<WebPage> DownloadPageAsync(string URL)
        {
            WebPage wp =new WebPage(URL);
            wp.content = await htClient.GetStringAsync(URL);
            wp.Valide();

            return wp;
        }
    }
}