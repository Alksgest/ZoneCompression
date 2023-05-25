namespace ZoneCompression.Compression;

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
    
}