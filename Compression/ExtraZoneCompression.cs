using System.Text;
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
        var sequences = codeDictionaries.Select(BitCompress).ToArray();

        return new CompressedData
        {
            Sequences = sequences
        };
    }

    public static string Decompress(CompressedData data)
    {
        var slices = data.Sequences.Select(BitDecompress);

        return string.Join("", slices);
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
            Data = resultBytes.ToArray(),
            Dictionary = dictBytes,
            DictionarySize = dictBytes.Length,
            DataSize = resultBytes.Count
        };
    }

    private static string BitDecompress(CodedSequence sequence)
    {
        var builder = new StringBuilder();
    
        byte currentZone = 0;
        var letterToCodeDictionary = DeserializeDictionary(sequence.Dictionary, sequence.DictionarySize);
        var zoneCount = ZoneCompressionConstants.GetZoneCount(letterToCodeDictionary.Count);
        var zoneCodes = ZoneCompressionConstants.GetZoneCodes(zoneCount);

        var bytes = sequence.Data;

        for (var i = 0; i < bytes.Length; ++i)
        {
            var b = bytes[i];
            
            var zone = GetZone(b, currentZone, zoneCodes);
    
            if (currentZone == 0)
            {
                currentZone = zone;
    
                var ch = letterToCodeDictionary[b];
                builder.Append(ch);
    
                continue;
            }
    
            if (zone == currentZone)
            {
                var leftPartOfByte = (byte)(b & ZoneCompressionConstants.CutTopByte);
    
                var actualCode1 = (byte)(zone | leftPartOfByte >> 4);
    
                var ch1 = letterToCodeDictionary[actualCode1];
                builder.Append(ch1);
    
                var rightPartOfByte = (byte)(b & ZoneCompressionConstants.CutLowByte);
    
                var shifted = (byte)(rightPartOfByte << 4);
                var isRightPartIsZone = CheckIfZone(shifted, zoneCodes);
    
                if (isRightPartIsZone)
                {
                    currentZone = shifted;
                    continue;
                }
    
                var actualCode2 = (byte)(zone | rightPartOfByte);
    
                var ch2 = letterToCodeDictionary[actualCode2];
                builder.Append(ch2);
    
                continue;
            }
    
            currentZone = zone;
    
            var chx = letterToCodeDictionary[b];
            builder.Append(chx);
        }
        
    
        return builder.ToString();
    }

    private static byte[] SerializeDictionary(Dictionary<byte, char> codesDictionary)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var pair in codesDictionary)
        {
            writer.Write(pair.Key);
            writer.Write((short)pair.Value);
        }

        var bytes = stream.ToArray();
        // var tmp = DeserializeDictionary(bytes, bytes.Length);
        return bytes;
    }
    
    private static Dictionary<byte, char> DeserializeDictionary(byte[] dict, int dictSize)
    {
        var result = new Dictionary<byte, char>();

        using var stream = new MemoryStream(dict);
        using var reader = new BinaryReader(stream);
        
        // Read keys and values
        for (var i = 0; i < dictSize / 3; i++)
        {
            var key = reader.ReadByte();
            var value = (char)reader.ReadInt16();

            result[key] = value;
        }

        return result;
    }

    private static IEnumerable<CodesDictionaryWithContentSize> GenerateLetterToCodeDictionaries(string input)
    {
        var frequencyDictionaries = CreateFrequencyDictionaries(input);

        var outputDictionaries = new List<CodesDictionaryWithContentSize>();

        var index = 0;
        var cursor = 0;
        var take = 0;

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

            var countToTake = fDict.CodesDictionary.Sum(el => el.Value);

            outputDictionaries.Add(new CodesDictionaryWithContentSize
            {
                CodesDictionary = letterToCodeDictionary,
                ContentSize = fDict.CodesDictionary.Count * sizeof(char),
                InputSlice = input.Skip(cursor).Take(countToTake)
            });

            index = 0;
            cursor += countToTake;
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
    
    private static byte GetZone(byte currentByte, byte currentZone, IEnumerable<byte> zones)
    {
        var zone = (byte)(currentByte & ZoneCompressionConstants.CutTopByte);

        return CheckIfZone(zone, zones) ? zone : currentZone;
    }
    
    private static bool CheckIfZone(byte b, IEnumerable<byte> zones)
    {
        return zones.Contains(b);
    }
}