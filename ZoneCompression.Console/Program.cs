using System.Text;
using ZoneCompression.Core.Algo;

namespace ZoneCompression.Console;

internal static class Program
{
    private static void Main()
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        // const string input = "аАбБвВгГґҐдДеЕєЄжЖзЗиИіІїЇйЙкКлЛмМнНоОпПрРсСтТуУфФхХцЦчЧшШщЩьюЮяЯ" +
        //                      "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";

        var input = File.ReadAllText("text.txt");

        var compressedData = TextCompressor.Compress(input);

        System.Console.WriteLine(compressedData);

        var decompressed = TextCompressor.Decompress(compressedData);

        System.Console.WriteLine(input);

        var dataSize = compressedData.Sequences.Sum(el => el.DataSize);
        var dictsSize = compressedData.Sequences.Sum(el => el.DictionarySize);

        System.Console.WriteLine($"Count of dictionaries which were user for compression: {compressedData.Sequences.Length}");
        System.Console.WriteLine($"Size of dictionaries: {dictsSize}");
        System.Console.WriteLine($"Input length (string): {input.Length}");
        System.Console.WriteLine($"Input length (unicode): {Encoding.Unicode.GetByteCount(input)}");
        System.Console.WriteLine($"Input length (UTF8): {Encoding.UTF8.GetByteCount(input)}");
        System.Console.WriteLine($"Input length (bytes): {sizeof(char) * input.Length}");
        System.Console.WriteLine($"Compressed length (bytes): {dataSize}");
        System.Console.WriteLine(
            $"Compression coefficient: {(float)sizeof(char) * input.Length / dataSize}");

        // Console.WriteLine(decompressed);

        using var stream = File.CreateText("processed.txt");
        
        stream.Write(decompressed);
    }
}