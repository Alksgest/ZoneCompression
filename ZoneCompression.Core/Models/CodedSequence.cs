namespace ZoneCompression.Core.Models;

public struct CodedSequence
{
    public int DictionarySize;
    public int DataSize;
    public byte[] Dictionary;
    public byte[] Data;
}