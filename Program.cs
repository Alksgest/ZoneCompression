using System.Text;
using ZoneCompression.Compression;

namespace kr2;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // const string input = "аАбБвВгГґҐдДеЕєЄжЖзЗиИіІїЇйЙкКлЛмМнНоОпПрРсСтТуУфФхХцЦчЧшШщЩьюЮяЯ" +
        //                      "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";

        var input = File.ReadAllText("text.txt");

        var compressedData = ExtraZoneCompression.Compress(input);

        Console.WriteLine(compressedData);

        var decompressed = ExtraZoneCompression.Decompress(compressedData);

        Console.WriteLine(input);

        var dataSize = compressedData.Sequences.Sum(el => el.DataSize);
        var dictsSize = compressedData.Sequences.Sum(el => el.DictionarySize);

        Console.WriteLine($"Count of dictionaries which were user for compression: {compressedData.Sequences.Length}");
        Console.WriteLine($"Size of dictionaries: {dictsSize}");
        Console.WriteLine($"Input length (string): {input.Length}");
        Console.WriteLine($"Input length (unicode): {Encoding.Unicode.GetByteCount(input)}");
        Console.WriteLine($"Input length (UTF8): {Encoding.UTF8.GetByteCount(input)}");
        Console.WriteLine($"Input length (bytes): {sizeof(char) * input.Length}");
        Console.WriteLine($"Compressed length (bytes): {dataSize}");
        Console.WriteLine(
            $"Compression coefficient: {(float)sizeof(char) * input.Length / dataSize}");

        // Console.WriteLine(decompressed);

        using var stream = File.CreateText("processed.txt");
        
        stream.Write(decompressed);
    }
}