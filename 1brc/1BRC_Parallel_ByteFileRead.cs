using csFastFloat;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace _1brc
{
    // This and the following ideas are inspired by other contributors and their articles
    public class _1BRC_Parallel_ByteFileRead
    {
        public static void Process()
        {
            const byte Target = (byte)';';
            const byte Eol = (byte)'\n';

            var dictionary = new ConcurrentDictionary<string, Summary>();

            long fileSize = new FileInfo(Utils.InputFileName).Length;
            int concurrent = Environment.ProcessorCount;
            long blockSize = fileSize / concurrent;
            int chunk = 1024 * 256;
            const ushort ChunkOffset = 32;

            Parallel.For(0, concurrent, i =>
            {
                using (FileStream stream = File.OpenRead(Utils.InputFileName))
                {
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

                            if (!dictionary.TryGetValue(town, out _))
                            {
                                dictionary.TryAdd(town, new Summary(val));
                            }
                            else
                            {
                                dictionary[town].Add(val);
                            }
                        }
                    }
                }
            });

            Utils.WriteFile(dictionary);
        }       
    }
}