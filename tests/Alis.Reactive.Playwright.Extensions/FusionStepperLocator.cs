using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>A Syncfusion Stepper: its steps, the one it has open, and the tooltip it shows on hover.</summary>
public sealed class FusionStepperLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionStepperLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>Every step, in order.</summary>
    public ILocator Steps => Root.Locator(".e-step-container");

    /// <summary>The step with this label.</summary>
    public ILocator Step(string label) => Steps.Filter(new() { HasText = label });

    /// <summary>The label of the step with this label, where a click on the step lands.</summary>
    public ILocator StepLabel(string label) => Root.GetByText(label, new() { Exact = true });

    /// <summary>The step the stepper has open.</summary>
    public ILocator OpenStep => Root.Locator(".e-step-container.e-step-selected");

    /// <summary>The tooltip a stepper with tooltips shows over the step under the pointer.</summary>
    public ILocator Tooltip => _page.Locator(".e-tooltip-wrap.e-stepper-tooltip");
}
