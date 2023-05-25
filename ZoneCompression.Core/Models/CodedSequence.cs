namespace ZoneCompression.Core.Models;

public class CodedSequence
{
    public byte[] Dictionary { get; init; }
    public byte[] Data { get; init; }
}