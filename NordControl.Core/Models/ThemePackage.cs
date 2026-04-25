namespace NordControl.Core.Models;

public sealed class ThemePackage
{
    public string Key { get; set; } = "fluent-dark";

    public string Name { get; set; } = "Fluent Dark";

    public string Description { get; set; } = "A calm dark utility look with crisp blue accents.";

    public string Author { get; set; } = "NordControl";

    public string Version { get; set; } = "1.0.0";

    public string AccentColorHex { get; set; } = "#4CC2FF";

    public string SecondaryAccentColorHex { get; set; } = "#8CF5D2";

    public string BackgroundColorHex { get; set; } = "#101418";

    public string SurfaceColorHex { get; set; } = "#161D24";

    public string TextColorHex { get; set; } = "#F4F7FA";

    public string MutedTextColorHex { get; set; } = "#8FA1B3";

    public string BorderColorHex { get; set; } = "#273442";

    public bool EnableGlass { get; set; } = true;

    public double GlassOpacity { get; set; } = 0.72;

    public double CornerRadius { get; set; } = 8;

    public string Mood { get; set; } = "Focused";

    public List<string> Tags { get; set; } = ["dark", "utility"];
}
