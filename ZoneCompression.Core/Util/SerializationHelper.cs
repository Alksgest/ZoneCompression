namespace ZoneCompression.Core.Util;

internal static class SerializationHelper
{
    internal static byte[] SerializeDictionary(Dictionary<byte, char> codesDictionary)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var pair in codesDictionary)
        {
            writer.Write(pair.Key);
            writer.Write((short)pair.Value);
        }

        var bytes = stream.ToArray();
        return bytes;
    }
    
    internal static Dictionary<byte, char> DeserializeDictionary(byte[] dict)
    {
        var result = new Dictionary<byte, char>();

        using var stream = new MemoryStream(dict);
        using var reader = new BinaryReader(stream);
        
        // Read keys and values
        for (var i = 0; i < dict.Length / 3; i++)
        {
            var key = reader.ReadByte();
            var value = (char)reader.ReadInt16();

            result[key] = value;
        }

        return result;
    }
}