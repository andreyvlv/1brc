using System.Diagnostics;

namespace _1brc
{
    public class Program
    {
        static void Main(string[] args)
        {          
            var stopwatch = Stopwatch.StartNew();

            //_1BRC_Parallel_LineFileRead.Process();

            //_1BRC_Parallel_ByteFileRead.Process();

            //_1BRC_Parallel_ByteFileRead_Optimized.Process();

            _1BRC_Parallel_ByteFileRead_Optimized_IntSummary.Process();

            stopwatch.Stop();
               
            Console.WriteLine($"Challenged in {stopwatch}");
            Console.ReadKey();                                
        }
    }
}
