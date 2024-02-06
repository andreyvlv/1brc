using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace _1brc
{
    public class _1BRC_Parallel_ByteFileRead_Optimized_IntSummary
    {
        public static void Process()
        {
            const byte Target = (byte)';';
            const byte Eol = (byte)'\n';

            long fileSize = new FileInfo(Utils.InputFileName).Length;
            int concurrent = Environment.ProcessorCount;
            long blockSize = fileSize / concurrent;
            int chunk = 1024 * 256;
            const ushort ChunkOffset = 32;

            // use a dictionary for each thread

            Dictionary<string, IntSummary>[] dictionaries = new Dictionary<string, IntSummary>[concurrent];

            Parallel.For(0, concurrent, i =>
            {
                using (FileStream stream = File.OpenRead(Utils.InputFileName))
                {
                    dictionaries[i] = new();
                    Dictionary<string, IntSummary> dictionary = dictionaries[i];

                    Span<byte> chars = new byte[chunk];
                    Span<byte> cache = new byte[chunk];

                    long positionStart = i * blockSize;
                    long targetLen = positionStart + blockSize;
                    int cacheLen = 0;

                    if (i != 0)
                    {
                        Span<byte> offsetBuffer = new byte[ChunkOffset];

                        stream.Position = positionStart - ChunkOffset;
                        stream.Read(offsetBuffer);

                        int eolIndex = offsetBuffer.LastIndexOf(Eol);
                        if (eolIndex != -1)
                        {
                            eolIndex = ChunkOffset - eolIndex - 1;
                            positionStart -= eolIndex;

                            if (i != concurrent - 1)
                            {
                                targetLen += eolIndex;
                            }
                        }
                    }

                    stream.Position = positionStart;

                    while (stream.Position < targetLen)
                    {
                        stream.Read(chars[cacheLen..]);

                        if (cacheLen > 0)
                        {
                            cache[..cacheLen].CopyTo(chars);
                            cache.Clear();
                            cacheLen = 0;
                        }

                        Span<byte> localBuffer = chars;

                        while (true)
                        {
                            int indexEnd = localBuffer.IndexOf(Eol);
                            if (indexEnd == -1)
                            {
                                cacheLen = localBuffer.Length;
                                localBuffer.CopyTo(cache);
                                chars.Clear();
                                break;
                            }

                            int indexTarget = localBuffer.IndexOf(Target) + 1;

                            Span<byte> nameSpan = localBuffer[..(indexTarget - 1)];
                            Span<byte> valueSpan = localBuffer[indexTarget..indexEnd];
                            localBuffer = localBuffer[(indexEnd + 1)..];

                            var town = Encoding.UTF8.GetString(nameSpan);
                            int value = GetMeasurement(ref valueSpan);

                            // faster replacement of method dictionary.TryGetValue(town, out _) (not work for concurrent dictionary)

                            ref var station = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, town);

                            if (Unsafe.IsNullRef(ref station))
                            {
                                dictionary.TryAdd(town, new IntSummary(value));
                            }
                            else
                            {
                                station.Add(value);
                            }
                        }
                    }
                }
            });

            // merge separated dictionaries (it's more faster than using one concurrent dictionary)          

            var mergedDictionary = new Dictionary<string, IntSummary>();

            foreach (var dict in dictionaries)
            {
                foreach (var kv in dict)
                {
                    if (!mergedDictionary.ContainsKey(kv.Key))
                    {
                        var station = new IntSummary(0);

                        station.min = kv.Value.min;
                        station.max = kv.Value.max;
                        station.sum = kv.Value.sum;
                        station.count = kv.Value.count;

                        mergedDictionary.TryAdd(kv.Key, station);
                    }
                    else
                    {
                        var station = mergedDictionary[kv.Key];

                        if (kv.Value.min < station.min)
                        {
                            station.min = kv.Value.min;
                        }

                        if (kv.Value.max > station.max)
                        {
                            station.max = kv.Value.max;
                        }

                        station.sum += kv.Value.sum;
                        station.count += kv.Value.count;
                    }
                }
            }

            // write to file data from dictionary

            Utils.WriteFile(mergedDictionary);
        }       

        private static int GetMeasurement(ref Span<byte> valueSpan)
        {
            bool negative = valueSpan[0] == (byte)'-';

            int value;

            if (negative)
            {
                if (valueSpan.Length == 5) // for length of negative span is 5, for example [-] [1] [2] [.] [3]
                {
                    value = (valueSpan[1] - '0') * 100 + (valueSpan[2] - '0') * 10 + (valueSpan[4] - '0');
                }
                else // for length of negative span is 4, for example [-] [1] [.] [2]
                {
                    value = (valueSpan[1] - '0') * 10 + (valueSpan[3] - '0');
                }
                value = -value;
            }
            else
            {
                if (valueSpan.Length == 4) // for length of positive span is 4, for example [1] [2] [.] [3]
                {
                    value = (valueSpan[0] - '0') * 100 + (valueSpan[1] - '0') * 10 + (valueSpan[3] - '0');
                }
                else // for length of positive span is 3, for example [1] [2] [.] [3]
                {
                    value = (valueSpan[0] - '0') * 10 + (valueSpan[2] - '0');
                }
            }

            return value;
        }
    }
}
