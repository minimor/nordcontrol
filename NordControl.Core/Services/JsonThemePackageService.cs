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

    public JsonThemePackageService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NordControl",
            "themes"))
    {
    }

    public JsonThemePackageService(string userThemesDirectory)
    {
        UserThemesDirectory = userThemesDirectory;
    }

    public string UserThemesDirectory { get; }

    public IReadOnlyList<ThemePackage> GetBuiltInPackages()
    {
        return ThemePackageCatalog.BuiltInThemes;
    }

    public IReadOnlyList<ThemePackage> GetUserPackages()
    {
        try
        {
            Directory.CreateDirectory(UserThemesDirectory);

            return Directory
                .EnumerateFiles(UserThemesDirectory, "*.json")
                .Select(TryLoadThemeFromFile)
                .OfType<ThemePackage>()
                .Where(theme => !ThemePackageCatalog.IsKnownThemeKey(theme.Key))
                .GroupBy(theme => theme.Key, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    public IReadOnlyList<ThemePackage> GetAllPackages()
    {
        return GetBuiltInPackages()
            .Concat(GetUserPackages())
            .ToList();
    }

    public ThemePackage GetSelectedPackage(AppSettings settings)
    {
        settings.Normalize();
        return GetAllPackages()
            .FirstOrDefault(theme => string.Equals(
                theme.Key,
                settings.Customization.SelectedThemePackageKey,
                StringComparison.Ordinal))
            ?? ThemePackageCatalog.DefaultTheme;
    }

    public ThemePackageOperationResult ExportPackage(ThemePackage theme, string filePath)
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

    public ThemePackageOperationResult ImportPackage(string filePath)
    {
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
            var theme = JsonSerializer.Deserialize<ThemePackage>(json, SerializerOptions);
            if (theme is null)
            {
                return ThemePackageOperationResult.Failed("Theme file did not contain a valid theme package.", filePath);
            }

            theme = Normalize(theme);
            theme.Key = GetUniqueUserThemeKey(theme.Key);
            var userThemePath = GetUserThemeFilePath(theme.Key);
            return SaveUserPackage(theme, userThemePath, $"Imported {theme.Name}.");
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

    public ThemePackageOperationResult SaveUserPackage(ThemePackage theme)
    {
        var normalizedTheme = Normalize(theme);
        normalizedTheme.Key = GetUniqueUserThemeKey(normalizedTheme.Key);
        var filePath = GetUserThemeFilePath(normalizedTheme.Key);
        return SaveUserPackage(normalizedTheme, filePath, $"Saved {normalizedTheme.Name}.");
    }

    public ThemePackage Normalize(ThemePackage theme)
    {
        return ThemePackageNormalizer.Normalize(theme);
    }

    private ThemePackageOperationResult SaveUserPackage(ThemePackage theme, string filePath, string message)
    {
        try
        {
            Directory.CreateDirectory(UserThemesDirectory);
            var json = JsonSerializer.Serialize(theme, SerializerOptions);
            File.WriteAllText(filePath, json);
            return ThemePackageOperationResult.Succeeded(message, filePath);
        }
        catch (IOException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not save theme: {ex.Message}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ThemePackageOperationResult.Failed($"Could not access theme folder: {ex.Message}", filePath);
        }
    }

    private ThemePackage? TryLoadThemeFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var theme = JsonSerializer.Deserialize<ThemePackage>(json, SerializerOptions);
            return theme is null ? null : Normalize(theme);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private string GetUniqueUserThemeKey(string requestedKey)
    {
        var normalizedKey = ThemePackageNormalizer.Normalize(new ThemePackage { Key = requestedKey }).Key;
        if (ThemePackageCatalog.IsKnownThemeKey(normalizedKey))
        {
            normalizedKey = $"user-{normalizedKey}";
        }

        var existingKeys = GetUserPackages()
            .Select(theme => theme.Key)
            .ToHashSet(StringComparer.Ordinal);

        if (!existingKeys.Contains(normalizedKey))
        {
            return normalizedKey;
        }

        for (var index = 2; ; index++)
        {
            var candidate = $"{normalizedKey}-{index}";
            if (!existingKeys.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    private string GetUserThemeFilePath(string key)
    {
        return Path.Combine(UserThemesDirectory, $"{key}.json");
    }
}
