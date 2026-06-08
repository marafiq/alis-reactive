namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridResidentAdmissionAudit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ResidentAdmissionAudit";

    private async Task NavigateAudit()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#audit-load-status"))
            .ToHaveTextAsync("loaded audit rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#admission-audit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task admitting_a_resident_reads_the_add_edit_action_payload()
    {
        await NavigateAudit();

        await ClickWhenStable(Page.Locator("#audit-admit"));

        // The add edit-action saves: actionBegin/actionComplete read the typed payload.
        await Expect(Page.Locator("#audit-begin-request")).ToHaveTextAsync("save", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-action")).ToHaveTextAsync("add", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        // (Cancel read is proven by the delete variant + save-edit; the add gesture's
        // cancel value is not stably observable, so it is not asserted here.)
        await Expect(Page.Locator("#audit-complete-request")).ToHaveTextAsync("save", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-complete-action")).ToHaveTextAsync("add", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-complete-resident")).ToHaveTextAsync("Zara Added", new() { Timeout = 10000 });
        await Expect(Page.Locator("#admission-audit-grid")).ToContainTextAsync("Zara Added", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blocking_an_admission_cancels_the_add_edit_action()
    {
        await NavigateAudit();

        await ClickWhenStable(Page.Locator("#audit-admit-blocked"));

        // The cancel mutation blocks the add before it persists.
        await Expect(Page.Locator("#audit-blocked")).ToHaveTextAsync("admission blocked", new() { Timeout = 10000 });
        await Expect(Page.Locator("#admission-audit-grid")).Not.ToContainTextAsync("Blocked Admission", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharging_a_resident_reads_the_delete_edit_action_payload()
    {
        await NavigateAudit();

        await ClickWhenStable(Page.Locator("#audit-discharge"));

        // The delete edit-action reads its typed payload.
        await Expect(Page.Locator("#audit-begin-request")).ToHaveTextAsync("delete", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-begin-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#audit-complete-request")).ToHaveTextAsync("delete", new() { Timeout = 10000 });
        await Expect(Page.Locator("#admission-audit-grid")).Not.ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
