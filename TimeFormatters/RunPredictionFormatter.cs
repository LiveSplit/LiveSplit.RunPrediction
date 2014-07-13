using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.TimeFormatters
{
    class RunPredictionFormatter : ITimeFormatter
    {
        public TimeAccuracy Accuracy { get; set; }

        public RunPredictionFormatter(TimeAccuracy accuracy)
        {
            Accuracy = accuracy;
        }
        public string Format(TimeSpan? time)
        {
            var formatter = new RegularTimeFormatter(Accuracy);
            if (time == null)
                return "-";
            else
                return formatter.Format(time);
        }
    }
}
