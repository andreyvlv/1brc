# 1 BRC Challenge

My C# implementation for the One Billion Row Challenge (1BRC) [gunnarmorling/1brc](https://github.com/gunnarmorling/1brc), inspired by other community members. This implementation hasn't unsafe code, except for the trick in `1BRC_Parallel_ByteFileRead_Optimized.cs` and `1BRC_Parallel_ByteFileRead_Optimized_IntSummary.cs` for fast dictionary `TryGetValue` method.


## Running

My implementation required .NET 8 SDK. Build project from command line like:

```
  dotnet build -c Release
  dotnet publish -r win-x64 -c Release
```
or build this solution in Visual Studio, with "Release" and "x64" in solution configuration option. The `1brc.exe` file will be located in the folder `\1brc\bin\x64\Release\net8.0`

For creating measurements file, use `1brc_Create_Measurement` project in this solution, choose them, build and run. When finished, the program will create a file `measurements.txt` that will need to be placed in the folder `\1brc\bin\x64\Release\net8.0` where located `1brc.exe`.

For using other implementation of algorithm uncomment line in `Program.cs`.
## Perfomance

My system has specs:

    CPU: Intel Core i7 9700 @ 4.5GHz
    RAM: 16GB 3200MHz DDR4
    SSD: Samsung 970 EVO
    OS: Windows 10 (22H2) (19045.3930)

| Implementation | Time |
| --- | :--- |
| `1BRC_Parallel_ByteFileRead_Optimized_IntSummary.cs` | 00:00:9.8169522s |
| `1BRC_Parallel_ByteFileRead_Optimized.cs` | 00:00:11.8032998s |
| `1BRC_Parallel_ByteFileRead.cs` | 00:00:15.6950431s |
| `1BRC_Parallel_LineFileRead.cs` (Naive) | 00:00:54.6072806s |
