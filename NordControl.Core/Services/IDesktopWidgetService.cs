using NordControl.Core.Models;

namespace NordControl.Core.Services;

public interface IDesktopWidgetService
{
    bool IsWidgetsVisible { get; }

    int ActiveWidgetCount { get; }

    IReadOnlyList<DesktopWidgetDefinition> GetDefinitions();

    DesktopWidgetSettings LoadSettings(AppSettings settings);

    WidgetOperationResult SaveSettings(DesktopWidgetSettings settings);

    WidgetOperationResult ShowWidgets();

    WidgetOperationResult HideWidgets();

    WidgetOperationResult ToggleWidgets();

    WidgetOperationResult ResetWidgetLayout();

    WidgetOperationResult SaveWidgetBounds(string widgetId, double x, double y, double width, double height);

    WidgetOperationResult HideWidget(string widgetId, bool disable);

    string GetWidgetLayoutSummary();
}
