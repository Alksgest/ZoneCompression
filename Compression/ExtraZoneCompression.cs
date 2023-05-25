using System.Runtime.Serialization.Formatters.Binary;
using kr2.Compression;

namespace ZoneCompression.Compression;

public class FrequencyDictionaryWithContentSize
{
    public Dictionary<char, int> CodesDictionary { get; set; } = new();
}

public class CodesDictionaryWithContentSize
{
    public Dictionary<byte, char> CodesDictionary { get; set; } = new();
    public int ContentSize { get; set; }
    public IEnumerable<char> InputSlice { get; set; }
}

public struct CodedSequence
{
    public int ZoneCount;
    public int DictionarySize;
    public int DataSize;
    public byte[] Dictionary;
    public byte[] Data;
}

public struct CompressedData
{
    public CodedSequence[] Sequences;
}

public class ExtraZoneCompression
{
    public static CompressedData Compress(string input)
    {
        var codeDictionaries = GenerateLetterToCodeDictionaries(input);
        var sequences = codeDictionaries.Select(BitCompress);

        return new CompressedData
        {
            Sequences = sequences.ToArray()
        };
    }

    private static CodedSequence BitCompress(CodesDictionaryWithContentSize codesDictionaryWithContentSize)
    {
        byte currentByte = 0;
        byte currentZone = 0;

        var isCurrentByteTouched = false;

        var resultBytes = new List<byte>();

        var slice = codesDictionaryWithContentSize.InputSlice;
        var codeToLetterDictionary = GenerateCodeToLetterDictionary(codesDictionaryWithContentSize.CodesDictionary);

        foreach (var ch in slice)
        {
            var code = codeToLetterDictionary[ch];
            var zone = (byte)(code & ZoneCompressionConstants.CutTopByte);

            if (currentZone == 0)
            {
                currentZone = zone;
                resultBytes.Add(code);
                continue;
            }

            if (currentZone == zone)
            {
                var cutCode = (byte)(code & ZoneCompressionConstants.CutLowByte);

                if (!isCurrentByteTouched)
                {
                    currentByte = cutCode;
                    isCurrentByteTouched = true;
                }
                else
                {
                    currentByte <<= 4;
                    currentByte = (byte)(currentByte | cutCode);

                    resultBytes.Add(currentByte);
                    isCurrentByteTouched = false;
                    currentByte = 0;
                }

                continue;
            }

            currentZone = zone;

            if (!isCurrentByteTouched)
            {
                resultBytes.Add(code);
            }
            else
            {
                var b = (byte)(currentByte << 4 | zone >> 4);
                resultBytes.Add(b);

                currentByte = (byte)(code & ZoneCompressionConstants.CutLowByte);
                isCurrentByteTouched = true;
            }
        }

        var dictBytes = SerializeDictionary(codesDictionaryWithContentSize.CodesDictionary);

        return new CodedSequence
        {
            // ZoneCount = ,
            Data = resultBytes.ToArray(),
            Dictionary = dictBytes,
            DictionarySize = dictBytes.Length,
            DataSize = resultBytes.Count
        };
    }

    // private string BitDecompress(IEnumerable<byte> bytes, IReadOnlyDictionary<byte, char> letterToCodeDictionary)
    // {
    //     byte currentZone = 0;
    //
    //     var builder = new StringBuilder();
    //
    //     var zoneCodes = ZoneCompressionConstants.GetZoneCodes(_zoneCount);
    //
    //     foreach (var b in bytes)
    //     {
    //         var zone = GetZone(b, currentZone, zoneCodes);
    //
    //         if (currentZone == 0)
    //         {
    //             currentZone = zone;
    //
    //             var ch = letterToCodeDictionary[b];
    //             builder.Append(ch);
    //
    //             continue;
    //         }
    //
    //         if (zone == currentZone)
    //         {
    //             var leftPartOfByte = (byte)(b & ZoneCompressionConstants.CutTopByte);
    //
    //             var actualCode1 = (byte)(zone | leftPartOfByte >> 4);
    //
    //             var ch1 = letterToCodeDictionary[actualCode1];
    //             builder.Append(ch1);
    //
    //             var rightPartOfByte = (byte)(b & ZoneCompressionConstants.CutLowByte);
    //
    //             var shifted = (byte)(rightPartOfByte << 4);
    //             var isRightPartIsZone = CheckIfZone(shifted, zoneCodes);
    //
    //             if (isRightPartIsZone)
    //             {
    //                 currentZone = shifted;
    //                 continue;
    //             }
    //
    //             var actualCode2 = (byte)(zone | rightPartOfByte);
    //
    //             var ch2 = letterToCodeDictionary[actualCode2];
    //             builder.Append(ch2);
    //
    //             continue;
    //         }
    //
    //         currentZone = zone;
    //
    //         var chx = letterToCodeDictionary[b];
    //         builder.Append(chx);
    //     }
    //
    //     return builder.ToString();
    // }

    private static byte[] SerializeDictionary(Dictionary<byte, char> codesDictionary)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        // // Write the count of dictionary entries
        // writer.Write(codesDictionary.Count);

        foreach (var pair in codesDictionary)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }

        return stream.ToArray();
    }

    public static List<CodesDictionaryWithContentSize> GenerateLetterToCodeDictionaries(string input)
    {
        var frequencyDictionaries = CreateFrequencyDictionaries(input);

        var index = 0;

        var outputDictionaries = new List<CodesDictionaryWithContentSize>();

        var cursor = 0;

        foreach (var fDict in frequencyDictionaries)
        {
            var list = fDict.CodesDictionary
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            var letterToCodeDictionary = new Dictionary<byte, char>();

            var zoneCount = ZoneCompressionConstants.GetZoneCount(list.Count);
            var zoneCodes = ZoneCompressionConstants.GetZoneCodes(zoneCount);
            var numberCodes = ZoneCompressionConstants.GetNumberCodes(zoneCount);

            foreach (var zone in zoneCodes)
            {
                var shouldBreak = false;
                foreach (var code in numberCodes)
                {
                    var finaleCode = (byte)(zone | code);

                    if (list.Count < index + 1)
                    {
                        shouldBreak = true;
                        break;
                    }

                    letterToCodeDictionary[finaleCode] = list[index].Key;
                    ++index;
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            outputDictionaries.Add(new CodesDictionaryWithContentSize
            {
                CodesDictionary = letterToCodeDictionary,
                ContentSize = fDict.CodesDictionary.Count * sizeof(char),
                InputSlice = input.Skip(cursor).Take(fDict.CodesDictionary.Count)
            });

            index = 0;
            cursor += fDict.CodesDictionary.Count;
        }

        return outputDictionaries;
    }

    private static IEnumerable<FrequencyDictionaryWithContentSize> CreateFrequencyDictionaries(string input)
    {
        var dicts = new List<FrequencyDictionaryWithContentSize>();
        var dict = new FrequencyDictionaryWithContentSize();

        foreach (var ch in input)
        {
            if (dict.CodesDictionary.TryGetValue(ch, out _))
            {
                dict.CodesDictionary[ch]++;
            }
            else
            {
                if (dict.CodesDictionary.Count >= ZoneCompressionConstants.MaxElementsCount)
                {
                    dicts.Add(dict);
                    dict = new FrequencyDictionaryWithContentSize();
                }

                dict.CodesDictionary[ch] = 1;
            }
        }

        dicts.Add(dict);

        return dicts;
    }

    private static Dictionary<char, byte> GenerateCodeToLetterDictionary(Dictionary<byte, char> dict)
    {
        return dict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}