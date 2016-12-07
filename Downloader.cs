using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
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

        public async Task<List<Tuple<string,bool>>> StartDownloadAsync(IProgress<int> progress, List<string> URLs)
        {
            List<Task> taskList = new List<Task>();
            List<Tuple<string,bool>> output = new List<Tuple<string,bool>>();

            foreach(string URL in URLs)
            {
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
            Console.WriteLine("+++ started {0}", URL);
            var webPage = await HtmlDownloader.DownloadPageAsync(URL);
            Console.WriteLine("----- ended {0}, {1}", URL, webPage.OK.ToString());

            semaphore.Release();
            Console.WriteLine("-- released {0}", URL);

            var parsed = parserFunc(webPage);

            dbSaveFunc(parsed, URL);

            return new Tuple<string,bool>(URL, webPage.OK);
        }
    }
}