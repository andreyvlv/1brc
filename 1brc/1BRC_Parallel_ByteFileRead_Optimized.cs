using csFastFloat;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace _1brc
{
    public class _1BRC_Parallel_ByteFileRead_Optimized
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

            Dictionary<string, Summary>[] dictionaries = new Dictionary<string, Summary>[concurrent];

            Parallel.For(0, concurrent, i =>
            {
                using (FileStream stream = File.OpenRead(Utils.InputFileName))
                {
                    dictionaries[i] = new();
                    Dictionary<string, Summary> dictionary = dictionaries[i];

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

                            Span<byte> nameRaw = localBuffer[..(indexTarget - 1)];
                            Span<byte> valueRaw = localBuffer[indexTarget..indexEnd];
                            localBuffer = localBuffer[(indexEnd + 1)..];

                            var town = Encoding.UTF8.GetString(nameRaw);
                            var val = FastFloatParser.ParseFloat(valueRaw);

                            // faster replacement of method dictionary.TryGetValue(town, out _) (not work for concurrent dictionary)

                            ref var station = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, town);

                            if (Unsafe.IsNullRef(ref station))
                            {
                                dictionary.TryAdd(town, new Summary(val));
                            }
                            else
                            {
                                station.Add(val);
                            }
                        }
                    }
                }
            });

            // merge separated dictionaries (it's more faster than using one concurrent dictionary)          

            var mergedDictionary = new Dictionary<string, Summary>();

            foreach (var dict in dictionaries)
            {
                foreach (var kv in dict)
                {
                    if (!mergedDictionary.ContainsKey(kv.Key))
                    {
                        var station = new Summary(0);

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
    }
}