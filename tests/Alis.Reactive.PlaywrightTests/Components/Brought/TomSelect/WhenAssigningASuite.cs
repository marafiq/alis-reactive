using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.TomSelect;

// Journey: an admissions coordinator assigns an incoming resident to a suite type. The suite type
// is a Tom Select dropdown — a component the sandbox installs from npm, constructs on alis:mount
// and declares in its own assembly (BroughtComponents/TomSelect.cs). Choosing a suite quotes its
// monthly rate (a Syncfusion field) and what is open; the coordinator can take the recommended
// suite, clear the choice, reserve the suite, and add a companion's suite from a partial.
[TestFixture]
public class WhenAssigningASuite : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TomSelect";

    private PagePlan<SuiteAssignmentModel> _plan = null!;

    private TomSelectField SuiteType => new(Page, _plan.FindComponent(m => m.SuiteType)!.ElementId);
    private TomSelectField CompanionSuiteType => new(Page, CompanionSuiteTypeId);
    private NumericTextBoxLocator MonthlyRate => _plan.NumericTextBox(m => m.MonthlyRate);
    private ILocator SuiteTypeError => _plan.ErrorFor(m => m.SuiteType);

    private ILocator Availability => Page.Locator("#suite-availability");
    private ILocator Confirmation => Page.Locator("#suite-confirmation");
    private ILocator WelcomePacket => Page.Locator("#welcome-packet");
    private ILocator MoveInChecklist => Page.Locator("#move-in-checklist");
    private ILocator MoveInReady => Page.Locator("#move-in-ready");
    private ILocator CompanionRate => Page.Locator("#companion-rate");
    private ILocator CompanionSlot => Page.Locator("#companion-slot");

    private ILocator UseRecommendedSuite => Page.Locator("#suite-recommend");
    private ILocator ClearChoice => Page.Locator("#suite-clear");
    private ILocator ReserveSuite => Page.Locator("#suite-reserve");
    private ILocator AddCompanion => Page.Locator("#companion-add");
    private ILocator RemoveCompanion => Page.Locator("#companion-remove");

    // The companion partial renders CompanionSuiteModel.CompanionSuiteType; its ID is the
    // framework's deterministic ID for that model member.
    private static readonly string CompanionSuiteTypeId = IdGenerator.For<CompanionSuiteModel>(m => m.CompanionSuiteType);

    private async Task OpenAssignment()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<SuiteAssignmentModel>.FromPage(Page);
        await Expect(SuiteType.Control).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task choosing_a_suite_quotes_its_monthly_rate_and_what_is_open()
    {
        await OpenAssignment();

        await SuiteType.Choose("memory-care");

        await Expect(MonthlyRate.Input).ToHaveValueAsync("6,800", new() { Timeout = 5000 });
        await Expect(Availability).ToHaveTextAsync("2 memory care suites are open in the Garden wing.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The field wrapper writes the label and, for a required field, its "*" marker. The label is the
    // sibling before the field's message slot, so it is found the same way whatever the input renders.
    [Test]
    public async Task the_suite_type_is_marked_required_and_the_monthly_rate_is_not()
    {
        await OpenAssignment();

        await Expect(LabelOf(m => m.SuiteType)).ToHaveTextAsync("Suite type *");
        await Expect(LabelOf(m => m.MonthlyRate)).ToHaveTextAsync("Monthly rate ($)");
        AssertNoConsoleErrors();
    }

    private ILocator LabelOf(Expression<Func<SuiteAssignmentModel, object?>> field)
        => _plan.ErrorFor(field).Locator("xpath=preceding-sibling::label");

    [Test]
    public async Task taking_the_recommended_suite_selects_it_and_quotes_it()
    {
        await OpenAssignment();

        await UseRecommendedSuite.ClickAsync();

        await Expect(SuiteType.Selection).ToHaveTextAsync("Memory Care Suite", new() { Timeout = 5000 });
        await Expect(MonthlyRate.Input).ToHaveValueAsync("6,800", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_choice_withdraws_the_quote()
    {
        await OpenAssignment();
        await SuiteType.Choose("studio");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("4,200", new() { Timeout = 5000 });

        await ClearChoice.ClickAsync();

        await Expect(SuiteType.Selection).ToHaveCountAsync(0);
        await Expect(MonthlyRate.Input).ToHaveValueAsync("0", new() { Timeout = 5000 });
        await Expect(Availability).ToHaveTextAsync("Choose a suite type to see what is open.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reserving_without_a_suite_type_says_what_is_missing()
    {
        await OpenAssignment();

        await ReserveSuite.ClickAsync();

        await Expect(SuiteTypeError).ToHaveTextAsync("Choose a suite type.", new() { Timeout = 5000 });
        await Expect(Confirmation).ToBeEmptyAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_suite_clears_the_missing_suite_message()
    {
        await OpenAssignment();
        await ReserveSuite.ClickAsync();
        await Expect(SuiteTypeError).ToHaveTextAsync("Choose a suite type.", new() { Timeout = 5000 });

        await SuiteType.Choose("studio");

        await Expect(SuiteTypeError).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Gather proof: the posted body carries the Tom Select value read from the brought component.
    [Test]
    public async Task reserving_a_suite_posts_the_choice_and_prepares_the_move_in_documents()
    {
        await OpenAssignment();
        await SuiteType.Choose("companion");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("3,600", new() { Timeout = 5000 });

        var reservation = await Page.RunAndWaitForRequestAsync(
            () => ReserveSuite.ClickAsync(),
            request => request.Method == "POST" && request.Url.Contains("/TomSelect/Reserve"));

        using (var body = JsonDocument.Parse(reservation.PostData ?? "{}"))
        {
            // Form() posts each input under its MVC binding name, the name the model binder reads.
            Assert.That(body.RootElement.TryGetProperty("SuiteType", out var suiteType), Is.True,
                $"The reservation posted no SuiteType: {reservation.PostData}");
            Assert.That(suiteType.GetString(), Is.EqualTo("companion"));
        }

        await Expect(Confirmation).ToHaveTextAsync("Companion Suite reserved at $3,600 a month.", new() { Timeout = 5000 });
        await Expect(WelcomePacket).ToHaveTextAsync("Welcome packet prepared.", new() { Timeout = 5000 });
        await Expect(MoveInChecklist).ToHaveTextAsync("Move-in checklist prepared.", new() { Timeout = 5000 });
        await Expect(MoveInReady).ToHaveTextAsync("Move-in documents are ready.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_companion_lets_the_coordinator_quote_the_companion_suite()
    {
        await OpenAssignment();

        await AddCompanion.ClickAsync();
        await Expect(CompanionSuiteType.Control).ToBeVisibleAsync(new() { Timeout = 5000 });
        await CompanionSuiteType.Choose("one-bedroom");

        await Expect(CompanionRate).ToHaveTextAsync("5100", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The companion's dropdown lives in <body>, outside the partial; only alis:unmount destroying
    // the widget removes it.
    [Test]
    public async Task removing_the_companion_leaves_no_dropdown_behind()
    {
        await OpenAssignment();
        await AddCompanion.ClickAsync();
        await Expect(CompanionSuiteType.Dropdown).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await RemoveCompanion.ClickAsync();

        await Expect(CompanionSlot).ToHaveTextAsync("No companion suite on this assignment.", new() { Timeout = 5000 });
        await Expect(CompanionSuiteType.Dropdown).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // Tom Select names its parts after the select's ID: the combobox is {id}-ts-control and the
    // listbox is {id}-ts-dropdown.
    private sealed class TomSelectField(IPage page, string selectId)
    {
        public ILocator Control => page.Locator($"#{selectId}-ts-control");

        public ILocator Dropdown => page.Locator($"#{selectId}-ts-dropdown");

        public ILocator Selection => page.Locator($".ts-wrapper:has(#{selectId}-ts-control) .ts-control .item");

        public async Task Choose(string value)
        {
            await Control.ClickAsync();
            await Dropdown.Locator($"[data-value='{value}']").ClickAsync();
        }
    }
}
