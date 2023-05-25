using ZoneCompression.Core.Models;
using ZoneCompression.Core.Util;

namespace ZoneCompression.Core.Algo;

internal static class CodeLetterDictionaryGenerator
{
    internal static IEnumerable<CodesDictionaryWithContentSize> GenerateLetterToCodeDictionaries(string input)
    {
        var frequencyDictionaries = CreateFrequencyDictionaries(input);

        var outputDictionaries = new List<CodesDictionaryWithContentSize>();

        var index = 0;
        var cursor = 0;

        foreach (var fDict in frequencyDictionaries)
        {
            var list = fDict
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            var letterToCodeDictionary = new Dictionary<byte, char>();

            var zoneCount = ZoneHelper.GetZoneCount(list.Count);
            var zoneCodes = ZoneHelper.GetZoneCodes(zoneCount);
            var numberCodes = ZoneHelper.GetNumberCodes(zoneCount);

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

            var countToTake = fDict.Sum(el => el.Value);

            outputDictionaries.Add(new CodesDictionaryWithContentSize
            {
                CodesDictionary = letterToCodeDictionary,
                ContentSize = fDict.Count * sizeof(char),
                InputSlice = input.Skip(cursor).Take(countToTake)
            });

            index = 0;
            cursor += countToTake;
        }

        return outputDictionaries;
    }

    private static IEnumerable<Dictionary<char, int>> CreateFrequencyDictionaries(string input)
    {
        var dictionaries = new List<Dictionary<char, int>>();
        var dict = new Dictionary<char, int>();

        foreach (var ch in input)
        {
            if (dict.TryGetValue(ch, out _))
            {
                dict[ch]++;
            }
            else
            {
                if (dict.Count >= ZoneHelper.MaxElementsCount)
                {
                    dictionaries.Add(dict);
                    dict = new Dictionary<char, int>();
                }

                dict[ch] = 1;
            }
        }

        dictionaries.Add(dict);

        return dictionaries;
    }

    internal static Dictionary<char, byte> GenerateCodeToLetterDictionary(Dictionary<byte, char> dict)
    {
        return dict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}