using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PoePriceChecker
{
    class DigestCaches
    {
        public Dictionary<string, List<string>> cache { get; set; }
        private BackgroundWorker ReadThread;

        public DigestCaches(string[] caches)
        {
            cache = new Dictionary<string, List<string>>();
            ReadThread = new BackgroundWorker();
            ReadThread.DoWork += new DoWorkEventHandler(ReadFiles);

            if (!ReadThread.IsBusy)
                ReadThread.RunWorkerAsync();
        }


        private void ReadFiles(object sender, DoWorkEventArgs e)
        {
            string[] fName = (string[])e.Argument;

            object[] ExitErr = new object[2] {0, ""};

            foreach(string s in fName)
            {
                switch (s) //"Currency", "Fragments", "Incubator", "Scarab", "Fossil", "Resonator", "Essences", "DivinationCard", "Prophecy", "SkillGem", "BaseType", "UniqueMap", "Map"
                {
                    case "DivinationCard":
                        break;
                    case "Currency":
                        break;
                    case "Fragments":
                        break;
                    case "Incubator":
                        break;
                    case "Scarab":
                        break;
                    case "Fossil":
                        break;
                    case "Resonator":
                        break;
                    case "Essences":
                        break;
                    case "Prophecy":
                        break;
                    case "SkillGem":
                        break;
                    case "BaseType":
                        break;
                    case "UniqueMap":
                        break;
                    case "Map":
                        break;
                    default:
                        ExitErr[0]=1;
                        ExitErr[1]="Unable to complete switch.";
                        break;
                }
            }
        }

    }
}
