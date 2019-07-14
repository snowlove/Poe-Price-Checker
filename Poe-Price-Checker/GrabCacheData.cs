using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace PoePriceChecker
{
    class GrabCacheData
    {

        /********************
         * 
         * *************|
         *              |
         *              |
         *              |
         *              |
         *              |
         *              |
         *              0
         * 
         * 
         * 
         *  This class is due for
         *  Purging, I wrote it
         *  in GOLANG because
         *  I've recoded
         *  >30 iterations of this
         *  class and it's too
         *  buggy.
         * 
         ************************/





        public bool Busy { get; set; }

        private BackgroundWorker[] Downloader;

        public GrabCacheData(object[] ob)
        {
            string[] blah = (string[])ob[1];
            //object[] newob = new object[3] { (string)ob[0], (string[])ob[1], 0};
            Downloader = new BackgroundWorker[blah.Length];
            object[] newob;

            for (int i = 0; i < blah.Length; i++)
            {
                Downloader[i] = new BackgroundWorker();
                Downloader[i].DoWork += new DoWorkEventHandler(DownloadData);
            }
            for (int i = 0; i < blah.Length; i++)
            {
                newob = new object[3] { (string)ob[0], (string[])ob[1], i };
                Downloader[i].RunWorkerAsync(newob);
            }
            //Downloader = new BackgroundWorker();
           // Downloader.DoWork += new DoWorkEventHandler(DownloadData);
           // Downloader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownloadDataCompleted);

            //if (!Downloader.IsBusy)
                //Downloader.RunWorkerAsync(ob);
            //Task.Run(() => Processor(ob));
        }

        private void DownloadDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Busy = false;
            Console.WriteLine("Finished background worker.");
        }

        private void DownloadData(object sender, DoWorkEventArgs e)
        {
            Busy = true;
            object[] obj = (object[])e.Argument;
            string[] cache = (string[])obj[1];
            string League = (string)obj[0];
            int i = (int)obj[2];

            string basedirectory = @"Cache/";

            //foreach (string s in cache)
            for (int j = 0; j < cache.Length; j++)
            {
                if (j == i)
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
                        string URI = string.Format("https://poe.ninja/api/data/itemoverview?league={0}&type={1}", League, cache[i]);
                        Uri URIAbso = new Uri(URI, UriKind.Absolute);
                        Console.WriteLine("Downloading :: " + URI);
                        string b = wc.DownloadString(URIAbso);

                        if (!string.IsNullOrEmpty(b))
                            File.WriteAllText(basedirectory + cache[i] + ".cache", b, Encoding.UTF8);
                        else
                            Console.WriteLine("Error trying to get :: " + cache[i]);
                    }
                //else
                //Console.WriteLine("Skipping.. not divination cards...");
            }
        }

        private static async Task Processor (object[] obj)
        {
            string League = (string)obj[0];
            string[] cache = (string[])obj[1];

            string baseDir = @"Cache/";
            string json;

            //foreach (string s in cache)
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
                        //string b = await wc.DownloadString(new Uri(string.Format("https://poe.ninja/api/data/itemoverview?league={0}&type={1}", League, s), UriKind.Absolute));

                        //var c = await wc.DownloadStringAsync(new Uri(string.Format("https://poe.ninja/api/data/itemoverview?league={0}&type={1}", League, s), UriKind.Absolute));
                        //try
                        //{
                        json = await wc.DownloadStringTaskAsync(new Uri(string.Format("https://poe.ninja/api/data/itemoverview?league={0}&type={1}", League, cache[i]), UriKind.Absolute));
                        //}
                        //catch (WebException ex) { throw ex; }
                        /*wc.DownloadStringCompleted += (se, e) =>
                            {
                                if(!e.Cancelled && e.Error == null && !string.IsNullOrEmpty(e.Result))
                                    File.WriteAllText(baseDir + s + ".cache", b, Encoding.UTF8);
                                else
                                    Console.WriteLine("Failed to grab :: {0}", s);
                            };*/

                        if (!string.IsNullOrEmpty(json))
                            File.WriteAllText(baseDir + cache[i] + ".cache", json, Encoding.UTF8);
                        else
                            Console.WriteLine("Failed to grab :: {0}", cache[i]);
                    }
                }
                catch (Exception ex) { throw; }
                Thread.Sleep(1000);
            }
        }


        /*private async Task<int> DownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            int result = -1;

            if (!e.Cancelled && e.Error == null && !string.IsNullOrEmpty(e.Result))
                File.WriteAllText(baseDir + s + ".cache", b, Encoding.UTF8);
            else
                Console.WriteLine("Failed to grab :: {0}", s);

            return result;
        }*/
    }
}
