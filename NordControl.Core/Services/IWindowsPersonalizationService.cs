using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IWindowsPersonalizationService
{
    WindowsPersonalizationState GetCurrentState();

    PersonalizationOperationResult SetAppsTheme(string theme);

    PersonalizationOperationResult SetSystemTheme(string theme);

    PersonalizationOperationResult SetTransparencyEffects(bool enabled);

    PersonalizationOperationResult SetAccentColor(string hexColor);

    PersonalizationOperationResult SetAccentColorOnTitleBars(bool enabled);

    PersonalizationOperationResult SetWallpaper(string filePath);
}
