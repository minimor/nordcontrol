namespace NordControl.Core.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";

    public string LastSelectedModuleKey { get; set; } = "dashboard";

    public WindowManagerSettings WindowManager { get; set; } = new();

    public CustomizationSettings Customization { get; set; } = new();

    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Theme))
        {
            Theme = "Dark";
        }

        if (string.IsNullOrWhiteSpace(LastSelectedModuleKey))
        {
            LastSelectedModuleKey = "dashboard";
        }

        WindowManager ??= new WindowManagerSettings();
        WindowManager.Normalize();

        Customization ??= new CustomizationSettings();
        Customization.Normalize();
    }
}
