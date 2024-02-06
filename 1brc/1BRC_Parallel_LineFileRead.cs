using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using csFastFloat;

namespace _1brc
{
    // this is the first idea that came to my mind
    public class _1BRC_Parallel_LineFileRead
    {
        public static void Process()
        {
            var dictionary = new ConcurrentDictionary<string, Summary>();

            Parallel.ForEach(
                File.ReadLines(Utils.InputFileName), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, line =>
                {
                    var lineSpan = line.AsSpan();
                    var index = lineSpan.IndexOf(';');
                    var town = lineSpan[0..index].ToString();

                    var measurement = FastFloatParser.ParseFloat(lineSpan[(index + 1)..]);

                    if (!dictionary.TryGetValue(town, out _))
                    {
                        dictionary.TryAdd(town, new Summary(measurement));
                    }
                    else
                    {
                        dictionary[town].Add(measurement);
                    }                 
                });          

            // write to file data from dictionary

            Utils.WriteFile(dictionary);
        }       
    }
}