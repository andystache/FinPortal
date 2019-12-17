using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.ChartModels
{
    public class ChartJSSeriesBarData
    {
        public List<string> Names { get; set; }
        public List<double> Target { get; set; }
        public List<double> Current { get; set; }
    }
}