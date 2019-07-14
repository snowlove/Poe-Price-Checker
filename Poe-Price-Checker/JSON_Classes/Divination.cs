using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace PoePriceChecker.JSON_Classes
{
    //https://poe.ninja/api/Data/GetDivinationCardsOverview?league=Legion
    class Divination: IDisposable
    {
        public class Sparkline
        {
            public List<double> data { get; set; }
            public double totalChange { get; set; }
        }

        public class ExplicitModifier
        {
            public string text { get; set; }
            public bool optional { get; set; }
        }

        public class Line
        {
            public int id { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
            public int mapTier { get; set; }
            public int levelRequired { get; set; }
            public string baseType { get; set; }
            public int stackSize { get; set; }
            public object variant { get; set; }
            public object prophecyText { get; set; }
            public string artFilename { get; set; }
            public int links { get; set; }
            public int itemClass { get; set; }
            public Sparkline sparkline { get; set; }
            public List<object> implicitModifiers { get; set; }
            public List<ExplicitModifier> explicitModifiers { get; set; }
            public string flavourText { get; set; }
            public string itemType { get; set; }
            public double chaosValue { get; set; }
            public double exaltedValue { get; set; }
            public int count { get; set; }
        }

        public class RootObject
        {
            public List<Line> lines { get; set; }
            public int error { get; set; }
        }

        public RootObject ItemData;
        private bool disposed = false;

        public Divination(string b64)
        {
            //GetItemData(b64);
        }

        /*private void GetItemData(string b64)
        {
            using (WebClient wc = new WebClient())
            {
                try { wc.DownloadStringAsync(new Uri(@"https://www.poeprices.info/api?l=Legion&i=" + b64)); }
                catch (Exception ex) { throw ex; }

                wc.DownloadStringCompleted += (s, e) =>
                {
                    try
                    {
                        ItemData = JsonConvert.DeserializeObject<RootObject>(e.Result);
                        ItemData.error = c.error;
                        ItemData.min = c.min;
                        ItemData.max = c.max;
                        ItemData.error_msg = c.error_msg;
                        ItemData.warning_msg = c.warning_msg;
                        ItemData.currency = c.currency;
                    }
                    catch (Exception ef) { throw ef; }
                };
            }
        }*/

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    ItemData = null;

                disposed = true;
            }
        }


        ~Divination()
        {
            Dispose(false);
        }
    }
}
