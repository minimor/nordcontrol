using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IThemePackageService
{
    IReadOnlyList<ThemePackage> GetBuiltInThemes();

    ThemePackage GetSelectedTheme(AppSettings settings);

    ThemePackageOperationResult ExportTheme(ThemePackage theme, string filePath);

    ThemePackageOperationResult ImportTheme(string filePath, out ThemePackage? theme);

    ThemePackage Normalize(ThemePackage theme);
}
