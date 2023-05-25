namespace kr2.Compression;

using ZCC = ZoneCompressionConstants;

internal static class ZoneCompressionDefaultDictionaries
{
    internal static readonly Dictionary<char, byte> CodeToLetter = new()
    {
        { ' ', ZCC.CodeDLeftBytes | ZCC.Code0 }, { 'о', ZCC.CodeDLeftBytes | ZCC.Code1 },
        { 'а', ZCC.CodeDLeftBytes | ZCC.Code2 },
        { 'и', ZCC.CodeDLeftBytes | ZCC.Code3 }, { 'і', ZCC.CodeDLeftBytes | ZCC.Code4 },
        { 'н', ZCC.CodeDLeftBytes | ZCC.Code5 }, { 'в', ZCC.CodeDLeftBytes | ZCC.Code6 },
        { 'т', ZCC.CodeDLeftBytes | ZCC.Code7 },
        { 'е', ZCC.CodeDLeftBytes | ZCC.Code8 }, { 'р', ZCC.CodeDLeftBytes | ZCC.Code9 },
        { 'с', ZCC.CodeDLeftBytes | ZCC.CodeA }, { 'л', ZCC.CodeDLeftBytes | ZCC.CodeB },
        { 'к', ZCC.CodeDLeftBytes | ZCC.CodeC },
        { 'у', ZCC.CodeELeftBytes | ZCC.Code0 }, { 'д', ZCC.CodeELeftBytes | ZCC.Code1 },
        { 'п', ZCC.CodeELeftBytes | ZCC.Code2 },
        { 'м', ZCC.CodeELeftBytes | ZCC.Code3 }, { 'ь', ZCC.CodeELeftBytes | ZCC.Code4 },
        { 'з', ZCC.CodeELeftBytes | ZCC.Code5 }, { 'й', ZCC.CodeELeftBytes | ZCC.Code6 },
        { 'б', ZCC.CodeELeftBytes | ZCC.Code7 },
        { 'я', ZCC.CodeELeftBytes | ZCC.Code8 }, { 'г', ZCC.CodeELeftBytes | ZCC.Code9 },
        { 'ч', ZCC.CodeELeftBytes | ZCC.CodeA }, { 'ш', ZCC.CodeELeftBytes | ZCC.CodeB },
        { 'х', ZCC.CodeELeftBytes | ZCC.CodeC },
        { 'щ', ZCC.CodeFLeftBytes | ZCC.Code0 }, { 'ж', ZCC.CodeFLeftBytes | ZCC.Code1 },
        { 'ц', ZCC.CodeFLeftBytes | ZCC.Code2 },
        { 'ю', ZCC.CodeFLeftBytes | ZCC.Code3 }, { 'ї', ZCC.CodeFLeftBytes | ZCC.Code4 },
        { 'є', ZCC.CodeFLeftBytes | ZCC.Code5 }, { 'ф', ZCC.CodeFLeftBytes | ZCC.Code6 },
        { '.', ZCC.CodeFLeftBytes | ZCC.Code7 },
        { ';', ZCC.CodeFLeftBytes | ZCC.Code8 }, { ':', ZCC.CodeFLeftBytes | ZCC.Code9 },
        { '!', ZCC.CodeFLeftBytes | ZCC.CodeA }, { '?', ZCC.CodeFLeftBytes | ZCC.CodeB },
        { ',', ZCC.CodeFLeftBytes | ZCC.CodeC },
    };

    internal static readonly Dictionary<byte, char> LetterToCode =
        CodeToLetter.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
}