using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1brc
{
    public static class Utils
    {
        public static readonly string InputFileName = "./measurements.txt";       
        public static readonly string OutputFileName = "./measurements-out.txt";


        // version for int summary
        public static void WriteFile(IDictionary<string, IntSummary> mergedDictionary)
        {
            using (var outFile = File.Create(Utils.OutputFileName))
            {
                outFile.Write(Encoding.UTF8.GetBytes("{"));

                foreach (var kv in mergedDictionary.OrderBy(kv => kv.Key))
                {
                    var town = kv.Key;
                    var min = (kv.Value.min / 10f).ToString(CultureInfo.InvariantCulture);
                    var max = (kv.Value.max / 10f).ToString(CultureInfo.InvariantCulture);
                    var mean = Math.Round((kv.Value.sum / 10f) / kv.Value.count, 1).ToString(CultureInfo.InvariantCulture);

                    var str = $"{town}={min}/{mean}/{max}\n";

                    outFile.Write(Encoding.UTF8.GetBytes(str));
                }

                outFile.Write(Encoding.UTF8.GetBytes("}"));
            }
        }

        // version for simple summary
        public static void WriteFile(IDictionary<string, Summary> mergedDictionary)
        {
            using (var outFile = File.Create(Utils.OutputFileName))
            {
                outFile.Write(Encoding.UTF8.GetBytes("{"));

                foreach (var kv in mergedDictionary.OrderBy(kv => kv.Key))
                {
                    var town = kv.Key;
                    var min = kv.Value.min.ToString(CultureInfo.InvariantCulture);
                    var max = kv.Value.max.ToString(CultureInfo.InvariantCulture);
                    var mean = Math.Round(kv.Value.sum / kv.Value.count, 1).ToString(CultureInfo.InvariantCulture);

                    var str = $"{town}={min}/{mean}/{max}\r\n";

                    outFile.Write(Encoding.UTF8.GetBytes(str));
                }

                outFile.Write(Encoding.UTF8.GetBytes("}"));
            }
        }
    }
}
