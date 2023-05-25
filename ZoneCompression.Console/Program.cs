using System.Text;
using ZoneCompression.Core.Algo;

namespace ZoneCompression.Console;

internal static class Program
{
    private static void TestSimpleFlow(string input)
    {
        var compressedData = TextCompressor.Compress(input);

        var decompressed = TextCompressor.Decompress(compressedData);

        System.Console.WriteLine(input);

        var dataSize = compressedData.Sequences.Sum(el => el.Data.Length);
        var dictsSize = compressedData.Sequences.Sum(el => el.Dictionary.Length);

        System.Console.WriteLine($"Count of dictionaries which were user for compression: {compressedData.Sequences.Count}");
        System.Console.WriteLine($"Size of dictionaries: {dictsSize}");
        System.Console.WriteLine($"Input length (string): {input.Length}");
        System.Console.WriteLine($"Input length (unicode): {Encoding.Unicode.GetByteCount(input)}");
        System.Console.WriteLine($"Input length (UTF8): {Encoding.UTF8.GetByteCount(input)}");
        System.Console.WriteLine($"Input length (bytes): {sizeof(char) * input.Length}");
        System.Console.WriteLine($"Compressed length (bytes): {dataSize}");
        System.Console.WriteLine(
            $"Compression coefficient: {(float)sizeof(char) * input.Length / dataSize}");
    }

    private static void TestByteFlow(string input)
    {
        System.Console.WriteLine($"Input:\n{input}");
        var compressedData = TextCompressor.Compress(input);
        var bytes = (byte[])compressedData;

        var decompressed = TextCompressor.Decompress(bytes);

        System.Console.WriteLine($"Decompressed:\n{decompressed}");
    }
    private static void Main()
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        const string input = "аАбБвВгГґҐдДеЕєЄжЖзЗиИіІїЇйЙкКлЛмМнНоОпПрРсСтТуУфФхХцЦчЧшШщЩьюЮяЯ" +
                             "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";

        // var input = File.ReadAllText("text.txt");

        // TestSimpleFlow(input);
        TestByteFlow(input);
    }
}