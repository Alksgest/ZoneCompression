namespace kr2.Compression;

internal static class ZoneCompressionConstants
{
    internal const byte Code0 = 0; // 0000-0000        
    internal const byte Code1 = 1; // 0000-0001        
    internal const byte Code2 = 2; // 0000-0010        
    internal const byte Code3 = 3; // 0000-0011        
    internal const byte Code4 = 4; // 0000-0100        
    internal const byte Code5 = 5; // 0000-0101        
    internal const byte Code6 = 6; // 0000-0110        
    internal const byte Code7 = 7; // 0000-0111        
    internal const byte Code8 = 8; // 0000-1000        
    internal const byte Code9 = 9; // 0000-1001        
    internal const byte CodeA = 10; // 0000-1010       
    internal const byte CodeB = 11; // 0000-1011       
    internal const byte CodeC = 12; // 0000-1100       

    internal const byte CodeD = 13; // 0000-1101       
    internal const byte CodeE = 14; // 0000-1110       
    internal const byte CodeF = 15; // 0000-1111       // 1111-0000
    
    internal static readonly byte[] ZoneNumbersCodes =
    {
        Code0, Code1, Code2, Code3,
        Code4, Code5, Code6, Code7,
        Code8, Code9,
        CodeA, CodeB, CodeC,
        CodeD, CodeE, CodeF 
    };

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

    internal const byte CodeDLeftBytes = 208; // CodeD << 4
    internal const byte CodeELeftBytes = 224; // CodeE << 4
    internal const byte CodeFLeftBytes = 240; // CodeF << 4

    internal const byte CutLowByte = 15;
    internal const byte CutTopByte = 240;
}