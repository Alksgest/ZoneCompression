namespace ZoneCompression.Core.Models;

public class CodesDictionaryWithInputSlice
{
    public Dictionary<byte, char> CodesDictionary { get; init; } = new();
    public IEnumerable<char>? InputSlice { get; init; }
}