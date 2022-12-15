using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PoePriceChecker
{
    class Config
    {
        public bool IntregityCheck { get; set; }
        private Dictionary<string, string> FileRead;


        public Config(object dataToWrite)
        {
            //our object is going to be containing what to write e.g { "Hotkey", "TheHotkey", "Mod", "Value" }, or { "SMS", "Value" }
        }

        private bool integritycheck()
        {
            bool check = false;
            //I have to iterate through all keys and values, are there the proper key value pairs? if I have 6 hot keys that'd be 12 values, sms +1 = 13, etc.

            string[] fd = File.ReadAllLines(@"config.cfg");

            return check;
        }




        public bool Write(object data)
        {
            bool result = false;

            if (!integritycheck())
                return result;

            return result;
        }




        public bool Read(object data)
        {
            bool result = false;

            if (!integritycheck())
                return result;


            return result;
        }

        ~Config()
        {

        }
    }
}
