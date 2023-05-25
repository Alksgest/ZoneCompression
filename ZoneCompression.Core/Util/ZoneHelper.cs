namespace ZoneCompression.Core.Util;

public static class ZoneHelper
{
    internal const int MaxElementsCount = 64;

    // From 0 to 15
    private static readonly byte[] ZoneNumbersCodes = Enumerable.Range(0, 16).Select(el => (byte)el).ToArray();
    
    internal static byte[] GetNumberCodes(int zoneCount)
    {
        if (zoneCount is > 8 or < 1)
        {
            throw new ArgumentException("Count of zones can not be more then 8");
        }

        return ZoneNumbersCodes.Take(ZoneNumbersCodes.Length - zoneCount).ToArray();
    }

    internal static byte[] GetZoneCodes(int zoneCount)
    {
        if (zoneCount is > 8 or < 1)
        {
            throw new ArgumentException("Count of zones can not be more then 8");
        }

        return ZoneNumbersCodes
            .Skip(ZoneNumbersCodes.Length - zoneCount)
            .Take(zoneCount)
            .Select(el => (byte)(el << 4))
            .ToArray();
    }

    internal static int GetZoneCount(int elementsCount)
    {
        return elementsCount switch
        {
            > MaxElementsCount => throw new ArgumentException("Max elements per dict is 64"),
            <= 1 * 15 => 1,
            <= 2 * 14 => 2,
            <= 3 * 13 => 3,
            <= 4 * 12 => 4,
            <= 5 * 11 => 5,
            <= 6 * 10 => 6,
            <= 7 * 9 => 7,
            <= 8 * 8 => 8,
        };
    }
    
    internal static byte GetZone(byte currentByte, byte currentZone, IEnumerable<byte> zones)
    {
        var zone = ByteHelper.CutUpperBytes(currentByte);

        return CheckIfZone(zone, zones) ? zone : currentZone;
    }
    
    internal static bool CheckIfZone(byte b, IEnumerable<byte> zones)
    {
        return zones.Contains(b);
    }
}