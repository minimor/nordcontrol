namespace NordControl.Core.Models;

public sealed record WindowsPersonalizationState(
    string AppsTheme,
    string SystemTheme,
    bool TransparencyEffectsEnabled,
    string AccentColorHex,
    bool AccentColorOnTitleBars,
    string WallpaperPath,
    DateTime LastLoadedAt);
