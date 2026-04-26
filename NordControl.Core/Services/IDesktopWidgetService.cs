using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IDesktopWidgetService
{
    IReadOnlyList<DesktopWidgetDefinition> GetDefinitions();

    DesktopWidgetSettings LoadSettings(AppSettings settings);

    WidgetOperationResult SaveSettings(DesktopWidgetSettings settings);

    WidgetOperationResult ShowWidgets();

    WidgetOperationResult HideWidgets();

    WidgetOperationResult ToggleWidgets();

    WidgetOperationResult ResetWidgetLayout();
}
