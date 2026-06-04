using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator ValidationRoot => _page.Locator("#resident-stepper-validation");

    public ILocator ValidationStepItems => ValidationRoot.Locator(".e-step-container");

    public ILocator NextButton => _page.Locator("#next-step-btn");

    public ILocator PreviousButton => _page.Locator("#previous-step-btn");

    public ILocator ResetButton => _page.Locator("#reset-step-btn");

    public ILocator SetReviewButton => _page.Locator("#set-review-step-btn");

    public ILocator SetCompleteButton => _page.Locator("#set-complete-step-btn");

    public ILocator SetIntakeButton => _page.Locator("#set-intake-step-btn");

    public ILocator RefreshButton => _page.Locator("#refresh-step-btn");

    public ILocator ReviewStep => Root.GetByText("Review");

    public ILocator CompleteStep => Root.GetByText("Complete");

    public ILocator ValidationIntakeStep => ValidationStepItems.Nth(0);

    public ILocator ValidationReviewStep => ValidationStepItems.Nth(1);

    public ILocator ValidationCompleteStep => ValidationStepItems.Nth(2);

    public ILocator ValidationTooltip => _page.Locator(".e-tooltip-wrap.e-stepper-tooltip");
}
