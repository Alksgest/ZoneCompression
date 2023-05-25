using System.Text;
using ZoneCompression.Core.Models;
using ZoneCompression.Core.Util;

namespace ZoneCompression.Core.Algo;

public static class TextCompressor
{
    public static CompressedData Compress(string input)
    {
        var codeDictionaries = CodeLetterDictionaryGenerator.GenerateLetterToCodeDictionaries(input);
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

        var slice = codesDictionaryWithContentSize.InputSlice!;
        var codeToLetterDictionary =
            CodeLetterDictionaryGenerator.GenerateCodeToLetterDictionary(
                codesDictionaryWithContentSize.CodesDictionary);

        foreach (var ch in slice)
        {
            var code = codeToLetterDictionary[ch];
            var zone = ByteHelper.CutUpperBytes(code);

            if (currentZone == 0)
            {
                currentZone = zone;
                resultBytes.Add(code);
                continue;
            }

            if (currentZone == zone)
            {
                var cutCode = ByteHelper.CutLowerBytes(code);

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

                currentByte = ByteHelper.CutLowerBytes(code);
                isCurrentByteTouched = true;
            }
        }

        var dictBytes = SerializationHelper.SerializeDictionary(codesDictionaryWithContentSize.CodesDictionary);

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
        var letterToCodeDictionary =
            SerializationHelper.DeserializeDictionary(sequence.Dictionary, sequence.DictionarySize);
        var zoneCount = ZoneHelper.GetZoneCount(letterToCodeDictionary.Count);
        var zoneCodes = ZoneHelper.GetZoneCodes(zoneCount);

        var bytes = sequence.Data;

        for (var i = 0; i < bytes.Length; ++i)
        {
            var b = bytes[i];

            var zone = ZoneHelper.GetZone(b, currentZone, zoneCodes);

            if (currentZone == 0)
            {
                currentZone = zone;

                var ch = letterToCodeDictionary[b];
                builder.Append(ch);

                continue;
            }

            if (zone == currentZone)
            {
                var leftPartOfByte = ByteHelper.CutUpperBytes(b);

                var actualCode1 = (byte)(zone | leftPartOfByte >> 4);

                var ch1 = letterToCodeDictionary[actualCode1];
                builder.Append(ch1);

                var rightPartOfByte = ByteHelper.CutLowerBytes(b);

                var shifted = (byte)(rightPartOfByte << 4);
                var isRightPartIsZone = ZoneHelper.CheckIfZone(shifted, zoneCodes);

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
}