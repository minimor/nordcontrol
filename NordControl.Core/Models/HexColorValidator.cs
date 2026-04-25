namespace NordControl.Core.Models;

public static class HexColorValidator
{
    public static bool IsValidHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || value[0] != '#')
        {
            return false;
        }

        return value
            .Skip(1)
            .All(character =>
                character is >= '0' and <= '9' ||
                character is >= 'a' and <= 'f' ||
                character is >= 'A' and <= 'F');
    }
}
