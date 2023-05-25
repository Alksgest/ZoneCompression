namespace ZoneCompression.Core.Models;

public class CodesDictionaryWithContentSize
{
    public Dictionary<byte, char> CodesDictionary { get; init; } = new();
    public int ContentSize { get; set; }
    public IEnumerable<char>? InputSlice { get; init; }
}