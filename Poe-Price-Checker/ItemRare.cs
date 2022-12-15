using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace PoePriceChecker
{
    class ItemRare: IDisposable
    {
        public class RootObject
        {
            public double min { get; set; }
            public double max { get; set; }
            public string currency { get; set; }
            public string warning_msg { get; set; }
            public int error { get; set; }
            /*private*/public List<List<object>> pred_explanation { get; set; }
            /*private*/public double pred_confidence_score { get; set; }
            public string error_msg { get; set; }
        }

        public RootObject ItemData;
        private bool disposed = false;
        private static string itemString;
        private static string league;

        public ItemRare(string iString, string l)
        {
            ItemData = new RootObject();
            itemString = iString;
            league = l;
            ItemData.error = 1;
            ItemData.error_msg = "The class returned before finishing. (Error 103)"; //create struct error type, 103 general error.
            GetItemData();//b64String, league);
        }


        public void GetItemData()//string b64, string league)
        {
            using (WebClient wc = new WebClient())
            {
                try { wc.DownloadStringAsync(new Uri(@"https://www.poeprices.info/api?l=" + league + "&i=" + itemString)); }
                catch (Exception ex) { throw ex; }

                wc.DownloadStringCompleted += (s, e) =>
                {
                    try
                    {
                        //RootObject c = JsonConvert.DeserializeObject<RootObject>(e.Result);
                        ItemData = JsonConvert.DeserializeObject<RootObject>(e.Result);
                        /*ItemData.error = c.error;
                        ItemData.min = c.min;
                        ItemData.max = c.max;
                        ItemData.error_msg = c.error_msg;
                        ItemData.warning_msg = c.warning_msg;
                        ItemData.currency = c.currency;*/
                    }
                    catch (Exception ex) { wc.Dispose(); this.ItemData.error = 2; this.ItemData.error_msg = ex.ToString(); return; }//GetItemData(); return; }
                };
            }
        }


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


        ~ItemRare()
        {
            Dispose(false);
        }
    }
}
