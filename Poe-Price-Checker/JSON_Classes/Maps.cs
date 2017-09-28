﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoePriceChecker.JSON_Classes
{
    //http://cdn.poe.ninja/api/Data/GetMapOverview?league=Harbinger
    class Maps
    {
        public class Sparkline
        {
            public List<double> data { get; set; }
            public double totalChange { get; set; }
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
            public string variant { get; set; }
            public object prophecyText { get; set; }
            public object artFilename { get; set; }
            public int links { get; set; }
            public int itemClass { get; set; }
            public Sparkline sparkline { get; set; }
            public List<object> implicitModifiers { get; set; }
            public List<object> explicitModifiers { get; set; }
            public string flavourText { get; set; }
            public string itemType { get; set; }
            public double chaosValue { get; set; }
            public double exaltedValue { get; set; }
            public int count { get; set; }
        }

        public class RootObject
        {
            public List<Line> lines { get; set; }
        }
    }
}
