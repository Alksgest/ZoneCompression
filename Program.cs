using System.Text;
using kr2.Compression;
using ZoneCompression.Compression;

namespace kr2;

internal static class Program
{
    private static void TestZoneCompressionFcMode(string input)
    {
        var zoneCompressionCf = ZoneCompression.CreateInstance();
        zoneCompressionCf.Mode = ZoneCompressionMode.CountFrequency;
        zoneCompressionCf.ZoneCount = 4;

        var c1 = zoneCompressionCf.Compress(input).ToList();
        foreach (var b in c1)
        {
            Console.Write(b);
            Console.Write(" ");
        }

        Console.WriteLine();
        var d1 = zoneCompressionCf.Decompress(c1);
        Console.WriteLine(d1);

        Console.WriteLine($"Input length (bytes): {input.Length}");
        Console.WriteLine($"Input length (bytes): {sizeof(char) * input.Length}");
        Console.WriteLine($"Compressed length (bytes): {c1.Count}");
        Console.WriteLine($"Compression coefficient for CountFrequency mode: {(float)input.Length / c1.Count}");
        Console.WriteLine(
            $"Compression coefficient for CountFrequency mode: {(float)sizeof(char) * input.Length / c1.Count}");
    }

    private static void TestZoneCompressionDefaultMode(string input)
    {
        var zoneCompression = ZoneCompression.CreateInstance();

        var c2 = zoneCompression.Compress(input).ToList();
        foreach (var b in c2)
        {
            Console.Write(b);
            Console.Write(" ");
        }

        Console.WriteLine();
        var d2 = zoneCompression.Decompress(c2);
        Console.WriteLine(d2);

        // Console.WriteLine($"Compression coefficient for Default mode: {(float)input.Length / c2.Count}");
    }

    private static void Main()
    {
        //     Console.OutputEncoding = Encoding.UTF8;
        //
        //     // var input = File.ReadAllText("text.txt");
        //
        //     // 漢 -> 1111-1111-0000-1111
        //     const string input = "словник1-закодовані_байти1-словник2-закодовані_байти2-словникN-закодовані_байтиN";
        //
        //     // словник1-закодовані_байти1-словник2-закодовані_байти2-словникN-закодовані_байтиN
        //
        //     // const string input =
        //     //     "The authority byte is split into two fields. The three least significant bits carry the user’s authority level (0-5). The five most significant bits carry an override reject threshold. If these bits are set to zero, the system reject threshold is used to determine whether a score for this user is considered an accept or reject. If they are not zero, then the value of these bits multiplied by ten will be the threshold score for this user.";
        //
        //     TestZoneCompressionFcMode(input);
        //     // TestZoneCompressionDefaultMode(input);
        //
        //     Console.WriteLine(sizeof(char));

        var input = "аАбБвВгГґҐдДеЕєЄжЖзЗиИіІїЇйЙкКлЛмМнНоОпПрРсСтТуУфФхХцЦчЧшШщЩьюЮяЯ" +
                    "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ";

        var dicts = ExtraZoneCompression.GenerateLetterToCodeDictionaries(input);

        Console.WriteLine(dicts.Count);

        var compressedData = ExtraZoneCompression.Compress(input);

        Console.WriteLine(compressedData);
    }
}