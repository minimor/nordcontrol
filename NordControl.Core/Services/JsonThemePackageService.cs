using System.Text.Json;
using NordControl.Core.Models;
using NordControl.Core.Modules;

namespace NordControl.Core.Services;

public sealed class JsonThemePackageService : IThemePackageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public IReadOnlyList<ThemePackage> GetBuiltInThemes()
    {
        return ThemePackageCatalog.BuiltInThemes;
    }

    public ThemePackage GetSelectedTheme(AppSettings settings)
    {
        settings.Normalize();
        return ThemePackageCatalog.GetThemeOrDefault(settings.Customization.SelectedThemePackageKey);
    }

    public ThemePackageOperationResult ExportTheme(ThemePackage theme, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ThemePackageOperationResult.Failed("Choose a valid theme export path.");
        }

        try
        {
            var normalizedTheme = Normalize(theme);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(normalizedTheme, SerializerOptions);
            File.WriteAllText(filePath, json);
            return ThemePackageOperationResult.Succeeded($"Exported {normalizedTheme.Name}.", filePath);
        }
        catch (IOException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not export theme: {ex.Message}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not access theme export path: {ex.Message}", filePath);
        }
    }

    public ThemePackageOperationResult ImportTheme(string filePath, out ThemePackage? theme)
    {
        theme = null;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ThemePackageOperationResult.Failed("Choose a valid theme import path.");
        }

        if (!File.Exists(filePath))
        {
            return ThemePackageOperationResult.Failed("Theme file was not found.", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            theme = JsonSerializer.Deserialize<ThemePackage>(json, SerializerOptions);
            if (theme is null)
            {
                return ThemePackageOperationResult.Failed("Theme file did not contain a valid theme package.", filePath);
            }

            theme = Normalize(theme);
            return ThemePackageOperationResult.Succeeded($"Imported {theme.Name}.", filePath);
        }
        catch (JsonException)
        {
            return ThemePackageOperationResult.Failed("Theme file JSON is invalid.", filePath);
        }
        catch (IOException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not read theme file: {ex.Message}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not access theme file: {ex.Message}", filePath);
        }
    }

    public ThemePackage Normalize(ThemePackage theme)
    {
        return ThemePackageNormalizer.Normalize(theme);
    }
}
