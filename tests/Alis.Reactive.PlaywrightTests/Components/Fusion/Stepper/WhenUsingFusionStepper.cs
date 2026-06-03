using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Stepper;

[TestFixture]
public class WhenUsingFusionStepper : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionStepper";

    private FusionStepperLocator Stepper => new(Page, "resident-stepper");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("0", new() { Timeout = 5000 });
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionStepper — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_stepper_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-stepper"));
        Assert.That(planJson, Does.Contain("\"activeStep\""));
        Assert.That(planJson, Does.Contain("\"readwrite\""));
        Assert.That(planJson, Does.Contain("\"nextStep\""));
        Assert.That(planJson, Does.Contain("\"previousStep\""));
        Assert.That(planJson, Does.Contain("\"reset\""));
        Assert.That(planJson, Does.Contain("\"refreshProgressbar\""));
        Assert.That(planJson, Does.Contain("\"stepChanging\""));
        Assert.That(planJson, Does.Contain("\"stepClick\""));
        Assert.That(planJson, Does.Contain("\"stepChanged\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task direct_step_writes_move_the_stepper_both_ways()
    {
        await NavigateAndBoot();

        await Stepper.SetCompleteButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("set complete", new() { Timeout = 5000 });

        await Stepper.SetIntakeButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("set intake", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task property_writes_commands_and_clicks_update_the_stepper_and_guard_the_transition()
    {
        await NavigateAndBoot();

        await Stepper.SetReviewButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-state")).ToHaveTextAsync("observed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-active")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-previous")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-cancel")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-state")).ToHaveTextAsync("changed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-active")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-previous")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Stepper.NextButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-guard")).ToHaveTextAsync("blocked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-cancel")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Stepper.CompleteStep.ClickAsync();
        await Expect(Page.Locator("#step-click-state")).ToHaveTextAsync("clicked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-click-active")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-click-previous")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-guard")).ToHaveTextAsync("allowed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changing-cancel")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-active")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-previous")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-changed-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Stepper.PreviousButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await Stepper.ResetButton.ClickAsync();
        await Expect(Page.Locator("#active-step-echo")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#current-step")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("reset", new() { Timeout = 5000 });

        await Stepper.RefreshButton.ClickAsync();
        await Expect(Page.Locator("#progress-state")).ToHaveTextAsync("refreshed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_stepper_renders_validation_state_and_tooltips()
    {
        await NavigateAndBoot();

        await Expect(Stepper.ValidationRoot).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Stepper.ValidationIntakeStep).ToBeVisibleAsync();
        await Expect(Stepper.ValidationReviewStep).ToBeVisibleAsync();
        await Expect(Stepper.ValidationCompleteStep).ToBeVisibleAsync();

        var validationItems = Stepper.ValidationRoot.Locator(".e-step-container");
        await Expect(validationItems.Nth(0)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-valid"));
        await Expect(validationItems.Nth(1)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-error"));
        await Expect(validationItems.Nth(2)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-valid"));
        await Expect(validationItems.Nth(2)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-error"));

        await Expect(Stepper.ValidationRoot.Locator(".e-step-label-optional")).ToBeVisibleAsync();

        await Stepper.ValidationCompleteStep.ClickAsync(new() { Force = true });
        await Expect(Page.Locator("#validation-active-echo")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-step-click-state")).ToHaveTextAsync("pending", new() { Timeout = 2000 });

        await Stepper.ValidationReviewStep.ClickAsync(new() { Force = true });
        await Expect(Page.Locator("#validation-step-click-state")).ToHaveTextAsync("clicked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-step-click-active")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-step-click-previous")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-active-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-current-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await Stepper.ValidationCompleteStep.ClickAsync(new() { Force = true });
        await Expect(Page.Locator("#validation-active-echo")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#validation-current-step")).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Stepper.ValidationCompleteStep.DispatchEventAsync("mouseover");
        await Expect(Stepper.ValidationTooltip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Stepper.ValidationTooltip).ToContainTextAsync("Complete", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
