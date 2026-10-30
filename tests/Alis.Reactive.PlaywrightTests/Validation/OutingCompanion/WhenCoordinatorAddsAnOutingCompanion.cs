using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.OutingCompanion;

/// <summary>
/// As an activities coordinator, I want a companion's details asked for only while a companion is on
/// the outing, so that adding one, removing one, or planning the outing alone each saves what it should.
/// </summary>
/// <remarks>
/// The companion is a partial loaded into a slot of the outing form. Its rules arrive with it, check
/// the form from the moment it is on the page, and leave with it when the slot is loaded with
/// something else. The server checks the companion whenever one was sent.
/// </remarks>
[TestFixture]
public class WhenCoordinatorAddsAnOutingCompanion : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/OutingCompanion";
    private const string SavedAlone = "Outing to the botanical garden saved for Helen Park.";
    private const string SavedWithRuth = "Outing to the botanical garden saved for Helen Park with Ruth Hale.";

    private PagePlan<OutingModel> _plan = null!;
    private ConcurrentQueue<string> _outingsSent = null!;
    private ConcurrentQueue<string> _confirmationsSent = null!;

    private ILocator AddCompanion => Page.Locator("#add-companion-btn");
    private ILocator RemoveCompanion => Page.Locator("#remove-companion-btn");
    private ILocator ConfirmCompanion => Page.Locator("#confirm-companion-btn");
    private ILocator SaveOuting => Page.Locator("#save-outing-btn");
    private ILocator OutingStatus => Page.Locator("#outing-status");
    private ILocator CompanionStatus => Page.Locator("#companion-status");
    private ILocator NoCompanion => Page.Locator("#no-companion");

    // The companion's inputs arrive after boot, so they are found by the id the model member gives them.
    private ILocator CompanionField(Expression<Func<OutingModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private async Task NavigateAndBoot()
    {
        _outingsSent = new ConcurrentQueue<string>();
        _confirmationsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/OutingCompanion/Save")) _outingsSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/OutingCompanion/ConfirmCompanion")) _confirmationsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<OutingModel>.FromPage(Page);
        await Expect(NoCompanion).ToBeVisibleAsync();
    }

    private async Task PlanTheOuting()
    {
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await _plan.TextBox(m => m.Destination).Fill("the botanical garden");
        await Page.Keyboard.PressAsync("Tab");
    }

    private async Task AddACompanion()
    {
        await ClickWhenStable(AddCompanion);
        await Expect(CompanionField(m => m.Companion!.Name)).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NoCompanion).ToHaveCountAsync(0);
    }

    private async Task Write(Expression<Func<OutingModel, object?>> member, string text)
    {
        await CompanionField(member).FillAsync(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task an_outing_without_a_companion_is_saved_without_asking_for_one()
    {
        await NavigateAndBoot();
        await PlanTheOuting();

        await ClickWhenStable(SaveOuting);

        await Expect(OutingStatus).ToHaveTextAsync(SavedAlone);
        Assert.That(_outingsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_companion_asks_for_the_companions_name_before_the_outing_is_saved()
    {
        await NavigateAndBoot();
        await PlanTheOuting();
        await AddACompanion();

        await ClickWhenStable(SaveOuting);

        await Expect(_plan.ErrorFor(m => m.Companion!.Name)).ToHaveTextAsync("Enter the companion's name.");
        await Expect(OutingStatus).ToHaveTextAsync("No outing saved yet.");
        Assert.That(_outingsSent, Is.Empty, "The companion's rules check the form while the companion is on it.");

        await Write(m => m.Companion!.Name, "Ruth Hale");
        await Write(m => m.Companion!.Phone, "555-0142");
        await ClickWhenStable(SaveOuting);
        await Expect(OutingStatus).ToHaveTextAsync(SavedWithRuth);
        Assert.That(_outingsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_companion_removed_from_the_outing_is_no_longer_asked_for()
    {
        await NavigateAndBoot();
        await PlanTheOuting();
        await AddACompanion();
        await ClickWhenStable(SaveOuting);
        await Expect(_plan.ErrorFor(m => m.Companion!.Name)).ToHaveTextAsync("Enter the companion's name.");

        await ClickWhenStable(RemoveCompanion);
        await Expect(NoCompanion).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(CompanionField(m => m.Companion!.Name)).ToHaveCountAsync(0);

        await ClickWhenStable(SaveOuting);
        await Expect(OutingStatus).ToHaveTextAsync(SavedAlone);
        Assert.That(_outingsSent, Has.Count.EqualTo(1), "The companion's rules left with the companion.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_companion_added_again_after_removal_is_asked_for_again()
    {
        await NavigateAndBoot();
        await PlanTheOuting();
        await AddACompanion();
        await ClickWhenStable(RemoveCompanion);
        await Expect(NoCompanion).ToBeVisibleAsync(new() { Timeout = 5000 });
        await AddACompanion();

        await ClickWhenStable(SaveOuting);

        await Expect(_plan.ErrorFor(m => m.Companion!.Name)).ToHaveTextAsync("Enter the companion's name.");
        await Expect(OutingStatus).ToHaveTextAsync("No outing saved yet.");
        Assert.That(_outingsSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_companion_needs_the_companions_phone()
    {
        await NavigateAndBoot();
        await PlanTheOuting();
        await AddACompanion();
        await Write(m => m.Companion!.Name, "Ruth Hale");

        await ClickWhenStable(ConfirmCompanion);

        await Expect(_plan.ErrorFor(m => m.Companion!.Phone)).ToHaveTextAsync("Enter the companion's phone.");
        await Expect(CompanionStatus).ToHaveTextAsync("Companion not confirmed yet.");
        Assert.That(_confirmationsSent, Is.Empty);

        await Write(m => m.Companion!.Phone, "555-0142");
        await ClickWhenStable(ConfirmCompanion);
        await Expect(CompanionStatus).ToHaveTextAsync("Ruth Hale confirmed for the outing.");
        Assert.That(_confirmationsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_the_companions_name_clears_its_message_before_the_field_is_left()
    {
        await NavigateAndBoot();
        await PlanTheOuting();
        await AddACompanion();
        await ClickWhenStable(SaveOuting);
        await Expect(_plan.ErrorFor(m => m.Companion!.Name)).ToHaveTextAsync("Enter the companion's name.");

        await CompanionField(m => m.Companion!.Name).PressSequentiallyAsync("R");

        await Expect(_plan.ErrorFor(m => m.Companion!.Name)).ToHaveTextAsync("");
        await Expect(CompanionField(m => m.Companion!.Name)).ToBeFocusedAsync();
        Assert.That(_outingsSent, Is.Empty);
        AssertNoConsoleErrors();
    }
}
