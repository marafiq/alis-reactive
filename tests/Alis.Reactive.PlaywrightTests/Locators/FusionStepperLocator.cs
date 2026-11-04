using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Steps => Root.Locator(".e-step-container");

    public ILocator Step(string label) => Steps.Filter(new() { HasText = label });

    public ILocator StepLabel(string label) => Root.GetByText(label, new() { Exact = true });

    public ILocator OpenStep => Root.Locator(".e-step-container.e-step-selected");

    public ILocator Tooltip => _page.Locator(".e-tooltip-wrap.e-stepper-tooltip");
}
