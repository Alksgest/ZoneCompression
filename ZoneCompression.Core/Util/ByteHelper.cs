namespace ZoneCompression.Core.Util;

internal static class ByteHelper
{
    // internal const byte CutLowerByte = 15;
    // internal const byte CutUpperByte = 240;

    internal static byte CutLowerBytes(byte b)
    {
        return (byte)(b & 15);
    }
    
    internal static byte CutUpperBytes(byte b)
    {
        return (byte)(b & 240);
    }
}