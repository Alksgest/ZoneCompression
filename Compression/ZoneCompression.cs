using System.Text;
using kr2.Compression;

namespace kr2;

public enum ZoneCompressionMode
{
    Default = 0,
    CountFrequency
}

public class ZoneCompression
{
    private Dictionary<char, byte>? _codeToLetterDictionary;
    private Dictionary<byte, char>? _letterToCodeDictionary;

    private int _zoneCount = 3;

    public string? Input { get; set; }

    public int ZoneCount
    {
        get => _zoneCount;
        set
        {
            if (_zoneCount is > 8 or < 1)
            {
                throw new ArgumentException("Count of zones can not be more then 8");
            }

            _zoneCount = value;
        }
    }

    public ZoneCompressionMode Mode { get; set; } = ZoneCompressionMode.Default;

    public static ZoneCompression CreateInstance()
    {
        return new ZoneCompression();
    }

    public IEnumerable<byte> Compress(string input)
    {
        if (Mode == ZoneCompressionMode.CountFrequency)
        {
            Input = input;

            _letterToCodeDictionary = GenerateLetterToCodeDictionary(Input);
            _codeToLetterDictionary = GenerateCodeToLetterDictionary(_letterToCodeDictionary!);

            return BitCompress(Input, _codeToLetterDictionary!);
        }

        ZoneCount = 3;
        return BitCompress(input.ToLower(), ZoneCompressionDefaultDictionaries.CodeToLetter);
    }

    public string Decompress(IEnumerable<byte> bytes)
    {
        if (Mode == ZoneCompressionMode.CountFrequency)
        {
            return BitDecompress(bytes, _letterToCodeDictionary!);
        }

        ZoneCount = 3;
        return BitDecompress(bytes, ZoneCompressionDefaultDictionaries.LetterToCode);
    }

    private static IEnumerable<byte> BitCompress(string input, IReadOnlyDictionary<char, byte> codeToLetterDictionary)
    {
        byte currentByte = 0;
        byte currentZone = 0;

        var isCurrentByteTouched = false;

        var arr = new List<byte>();

        foreach (var ch in input)
        {
            var code = codeToLetterDictionary[ch];
            var zone = (byte)(code & ZoneCompressionConstants.CutTopByte);

            if (currentZone == 0)
            {
                currentZone = zone;
                arr.Add(code);
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

                    arr.Add(currentByte);
                    isCurrentByteTouched = false;
                    currentByte = 0;
                }

                continue;
            }

            currentZone = zone;

            if (!isCurrentByteTouched)
            {
                arr.Add(code);
            }
            else
            {
                var b = (byte)(currentByte << 4 | zone >> 4);
                arr.Add(b);

                currentByte = (byte)(code & ZoneCompressionConstants.CutLowByte);
                isCurrentByteTouched = true;
            }
        }

        return arr;
    }

    private string BitDecompress(IEnumerable<byte> bytes, IReadOnlyDictionary<byte, char> letterToCodeDictionary)
    {
        byte currentZone = 0;

        var builder = new StringBuilder();
        
        var zoneCodes = ZoneCompressionConstants.GetZoneCodes(_zoneCount);

        foreach (var b in bytes)
        {
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

    private static Dictionary<char, byte> GenerateCodeToLetterDictionary(Dictionary<byte, char> dict)
    {
        return dict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    private Dictionary<byte, char> GenerateLetterToCodeDictionary(string input)
    {
        var fDict = CreateFrequencyDictionary(input);

        var zoneCodes = ZoneCompressionConstants.GetZoneCodes(_zoneCount);
        var numberCodes = ZoneCompressionConstants.GetNumberCodes(_zoneCount);

        var maxCharCount = zoneCodes.Length * numberCodes.Length;

        if (fDict.Count > maxCharCount)
        {
            throw new ArgumentException(
                $"Characters count was {fDict.Count} but max count is {maxCharCount}");
        }

        var orderedList = fDict.OrderByDescending(el => el.Value).ToList();

        var index = 0;

        var letterToCodeDictionary = new Dictionary<byte, char>();

        foreach (var zone in zoneCodes)
        {
            var shouldBreak = false;
            foreach (var code in numberCodes)
            {
                var finaleCode = (byte)(zone | code);

                if (orderedList.Count < index + 1)
                {
                    shouldBreak = true;
                    break;
                }

                letterToCodeDictionary[finaleCode] = orderedList[index].Key;
                ++index;
            }

            if (shouldBreak)
            {
                break;
            }
        }

        return letterToCodeDictionary;
    }

    private static Dictionary<char, float> CreateFrequencyDictionary(string input)
    {
        var count = input.Length;
        var countDict = input.Aggregate(new Dictionary<char, float>(), (accum, el) =>
        {
            if (accum.TryGetValue(el, out _))
            {
                accum[el]++;
            }
            else
            {
                accum[el] = 1;
            }

            return accum;
        });

        var frequencyDict = countDict
            .Select(el =>
            {
                var value = el.Value / count;
                return (el.Key, Value: value);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        return frequencyDict;
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