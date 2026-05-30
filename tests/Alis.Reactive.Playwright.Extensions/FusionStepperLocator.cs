using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionStepper.
/// </summary>
public sealed class FusionStepperLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionStepperLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered stepper root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The validation stepper root element.</summary>
    public ILocator ValidationRoot => _page.Locator("#resident-stepper-validation");

    /// <summary>The validation stepper items.</summary>
    public ILocator ValidationStepItems => ValidationRoot.Locator(".e-step-container");

    /// <summary>The next command button.</summary>
    public ILocator NextButton => _page.Locator("#next-step-btn");

    /// <summary>The previous command button.</summary>
    public ILocator PreviousButton => _page.Locator("#previous-step-btn");

    /// <summary>The reset command button.</summary>
    public ILocator ResetButton => _page.Locator("#reset-step-btn");

    /// <summary>The command button that writes active step 1.</summary>
    public ILocator SetReviewButton => _page.Locator("#set-review-step-btn");

    /// <summary>The command button that writes active step 2.</summary>
    public ILocator SetCompleteButton => _page.Locator("#set-complete-step-btn");

    /// <summary>The command button that writes active step 0.</summary>
    public ILocator SetIntakeButton => _page.Locator("#set-intake-step-btn");

    /// <summary>The refresh command button.</summary>
    public ILocator RefreshButton => _page.Locator("#refresh-step-btn");

    /// <summary>The Review step label.</summary>
    public ILocator ReviewStep => Root.GetByText("Review");

    /// <summary>The Complete step label.</summary>
    public ILocator CompleteStep => Root.GetByText("Complete");

    /// <summary>The validation Intake step label.</summary>
    public ILocator ValidationIntakeStep => ValidationStepItems.Nth(0);

    /// <summary>The validation Review step label.</summary>
    public ILocator ValidationReviewStep => ValidationStepItems.Nth(1);

    /// <summary>The validation Complete step label.</summary>
    public ILocator ValidationCompleteStep => ValidationStepItems.Nth(2);

    /// <summary>The validation stepper tooltip.</summary>
    public ILocator ValidationTooltip => _page.Locator(".e-tooltip-wrap.e-stepper-tooltip");
}
