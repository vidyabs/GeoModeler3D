namespace GeoModeler3D.Core.Entities;

/// <summary>
/// UI-independent color representation for the Core layer.
/// Convert to System.Windows.Media.Color in the Rendering layer.
/// </summary>
public readonly record struct EntityColor(byte R, byte G, byte B, byte A = 255)
{
    public static EntityColor Red => new(255, 0, 0);
    public static EntityColor Green => new(0, 255, 0);
    public static EntityColor Blue => new(0, 0, 255);
    public static EntityColor Yellow => new(255, 255, 0);
    public static EntityColor Cyan => new(0, 255, 255);
    public static EntityColor Magenta => new(255, 0, 255);
    public static EntityColor Orange => new(255, 165, 0);
    public static EntityColor White => new(255, 255, 255);
    public static EntityColor Gray => new(128, 128, 128);

    public string ToHex() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    public static EntityColor FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 8) // #AARRGGBB
            return new EntityColor(
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16),
                Convert.ToByte(hex[0..2], 16));
        if (hex.Length == 6) // #RRGGBB
            return new EntityColor(
                Convert.ToByte(hex[0..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16));
        return White;
    }
}
