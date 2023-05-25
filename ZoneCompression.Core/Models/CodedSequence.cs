namespace ZoneCompression.Core.Models;

public class CodedSequence
{
    public int DictionarySize { get; init; }
    public int DataSize { get; init; }
    public byte[] Dictionary { get; init; }
    public byte[] Data { get; init; }
}