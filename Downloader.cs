using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace twin_db
{
    public class Downloader<T>
    {
        public delegate IEnumerable<T> parser(WebPage wp); 
        public delegate void dbSave(IEnumerable<T> toSave, string URL); 

        private SemaphoreSlim semaphore;

        private parser parserFunc;
        private dbSave dbSaveFunc;

        public Downloader(int paralelism, parser parserFunc, dbSave dbSaveFunc)
        {
            this.semaphore = new SemaphoreSlim(paralelism);
            this.parserFunc = parserFunc;
            this.dbSaveFunc = dbSaveFunc;
        }

        public async Task<List<Tuple<string,bool>>> StartDownloadAsync(IProgress<Tuple<int, string>> progress, List<string> URLs)
        {
            List<Task> taskList = new List<Task>();
            List<Tuple<string,bool>> output = new List<Tuple<string,bool>>();
            int total = URLs.Count;
            int started = 0;

            foreach(string URL in URLs)
            {   
                if (progress != null)
                {
                    started++;
                    progress.Report(new Tuple<int, string>(started * 100 / total, started.ToString() + " out of " + total.ToString()));
                }                
                await semaphore.WaitAsync();
                taskList.Add(TaskAsync(URL));

            }
            await Task.WhenAll(taskList);            

            //compile output <URL, valid_download>
            foreach(Task<Tuple<string,bool>> t in taskList)
            {
                output.Add(t.Result);
            }
            return output;
        }

        private async Task<Tuple<string,bool>> TaskAsync(string URL)
        {
            var webPage = await HtmlDownloader.DownloadPageAsync(URL);
            semaphore.Release();

            var parsed = parserFunc(webPage);

            dbSaveFunc(parsed, URL);

            return new Tuple<string,bool>(URL, webPage.OK);
        }
    }
}