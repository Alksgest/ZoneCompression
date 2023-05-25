namespace ZoneCompression.Core.Models;

public class CompressedData
{
    public List<CodedSequence> Sequences { get; init; } = new();

    public static implicit operator byte[](CompressedData data)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var sequence in data.Sequences)
        {
            writer.Write(sequence.Dictionary.Length);
            writer.Write(sequence.Data.Length);

            writer.Write(sequence.Dictionary);
            writer.Write(sequence.Data);
        }

        return stream.ToArray();
    }

    public static implicit operator CompressedData(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var cursor = 0;

        var compressedData = new CompressedData();

        while (true)
        {
            var dictLength = reader.ReadInt32();
            var dataLength = reader.ReadInt32();

            cursor += 2 * sizeof(int);

            var dictBuff = reader.ReadBytes(dictLength);

            cursor += dictLength;

            var dataBuff = reader.ReadBytes(dataLength);
            cursor += dataLength;

            compressedData.Sequences.Add(new CodedSequence
            {
                Dictionary = dictBuff.ToArray(),
                Data = dataBuff.ToArray()
            });

            if (cursor == data.Length)
            {
                break;
            }
        }


        return compressedData;
    }
}