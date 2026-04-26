using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IThemePackageService
{
    string UserThemesDirectory { get; }

    IReadOnlyList<ThemePackage> GetBuiltInPackages();

    IReadOnlyList<ThemePackage> GetUserPackages();

    IReadOnlyList<ThemePackage> GetAllPackages();

    ThemePackage GetSelectedPackage(AppSettings settings);

    ThemePackageOperationResult ExportPackage(ThemePackage theme, string filePath);

    ThemePackageOperationResult ImportPackage(string filePath);

    ThemePackageOperationResult SaveUserPackage(ThemePackage theme);

    ThemePackage Normalize(ThemePackage theme);
}
