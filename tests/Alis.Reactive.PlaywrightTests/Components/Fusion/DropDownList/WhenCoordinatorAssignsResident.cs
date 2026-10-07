using System.Text.Json;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DropDownList;

/// <summary>
/// As a care coordinator, I want to select, remove and save a resident assignment
/// so that an unassigned numeric resident ID is kept separate from an assigned resident.
/// </summary>
[TestFixture]
public class WhenCoordinatorAssignsResident : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DropDownList/ResidentAssignment";
    private PagePlan<ResidentAssignmentModel> _plan = null!;
    private DropDownListLocator Resident => _plan.DropDownList(m => m.ResidentId);
    private NativeDropDownLocator Backup => _plan.NativeDropDown(m => m.BackupResidentId);

    private async Task NavigateAndBoot(string query = "")
    {
        await NavigateToAndWaitForBoot(Path + query);
        _plan = await PagePlan<ResidentAssignmentModel>.FromPage(Page);
    }

    [Test]
    public async Task an_unset_resident_stays_unassigned_when_saved()
    {
        await NavigateAndBoot();
        await Expect(Resident.Input).ToHaveValueAsync("");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("No resident assigned.");

        using var body = await SaveAssignment();

        Assert.That(body.RootElement.GetProperty("ResidentId").ValueKind, Is.EqualTo(JsonValueKind.Null));
        await Expect(Page.Locator("#assignment-receipt")).ToHaveTextAsync("Choose a resident before saving the assignment.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task an_existing_numeric_assignment_opens_with_its_resident_selected()
    {
        await NavigateAndBoot("?residentId=1042");

        await Expect(Resident.Input).ToHaveValueAsync("Helen Park");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("1042");
        await Expect(Backup.Input).ToHaveValueAsync("1042");
        await Expect(Page.Locator("#assignment-backup")).ToHaveTextAsync("1042");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_arthur_saves_his_numeric_resident_id()
    {
        await NavigateAndBoot();
        await Resident.Select("Arthur Bell");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("1043");

        using var body = await SaveAssignment();

        Assert.That(body.RootElement.GetProperty("ResidentId").GetInt32(), Is.EqualTo(1043));
        await Expect(Page.Locator("#assignment-receipt")).ToHaveTextAsync("Arthur Bell assigned to East wing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_a_chosen_resident_saves_no_resident()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("1042");
        await Resident.Wrapper.ClickAsync();
        await Resident.Wrapper.GetByRole(AriaRole.Button, new() { Name = "close" }).ClickAsync();
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("No resident assigned.");

        using var body = await SaveAssignment();

        Assert.That(body.RootElement.GetProperty("ResidentId").ValueKind, Is.EqualTo(JsonValueKind.Null));
        await Expect(Page.Locator("#assignment-receipt")).ToHaveTextAsync("Choose a resident before saving the assignment.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task assigning_helen_and_then_removing_the_assignment_updates_the_resident_field()
    {
        await NavigateAndBoot();

        await Page.Locator("#assign-helen").ClickAsync();
        await Expect(Resident.Input).ToHaveValueAsync("Helen Park");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("1042");
        await Page.Locator("#clear-assignment").ClickAsync();

        await Expect(Resident.Input).ToHaveValueAsync("");
        await Expect(Page.Locator("#assignment-resident")).ToHaveTextAsync("No resident assigned.");
        using var body = await SaveAssignment();
        Assert.That(body.RootElement.GetProperty("ResidentId").ValueKind, Is.EqualTo(JsonValueKind.Null));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task choosing_and_clearing_the_backup_keeps_its_numeric_id_separate_from_no_backup()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park");
        await Backup.Select("Arthur Bell");
        await Expect(Page.Locator("#assignment-backup")).ToHaveTextAsync("1043");
        using var selected = await SaveAssignment();
        Assert.That(selected.RootElement.GetProperty("BackupResidentId").GetInt32(), Is.EqualTo(1043));

        await Backup.Select("Choose a backup resident");
        await Expect(Page.Locator("#assignment-backup")).ToHaveTextAsync("No backup assigned.");
        using var cleared = await SaveAssignment();
        Assert.That(cleared.RootElement.GetProperty("BackupResidentId").ValueKind, Is.EqualTo(JsonValueKind.Null));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_and_removing_the_backup_updates_the_native_resident_field()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park");

        await Page.Locator("#assign-backup").ClickAsync();
        await Expect(Backup.Input).ToHaveValueAsync("1043");
        await Expect(Page.Locator("#assignment-backup")).ToHaveTextAsync("1043");
        await Page.Locator("#clear-backup").ClickAsync();
        await Expect(Backup.Input).ToHaveValueAsync("");
        await Expect(Page.Locator("#assignment-backup")).ToHaveTextAsync("No backup assigned.");

        using var body = await SaveAssignment();
        Assert.That(body.RootElement.GetProperty("BackupResidentId").ValueKind, Is.EqualTo(JsonValueKind.Null));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_an_assignment_to_west_preserves_the_string_wing_value()
    {
        await NavigateAndBoot();
        await Page.Locator("#assign-helen").ClickAsync();

        await Page.Locator("#move-west").ClickAsync();
        await Expect(_plan.DropDownList(m => m.Wing).Input).ToHaveValueAsync("West");
        await Expect(Page.Locator("#assignment-wing")).ToHaveTextAsync("West");
        using var body = await SaveAssignment();

        Assert.That(body.RootElement.GetProperty("Wing").GetString(), Is.EqualTo("West"));
        await Expect(Page.Locator("#assignment-receipt")).ToHaveTextAsync("Helen Park assigned to West wing.");
        AssertNoConsoleErrors();
    }

    private async Task<JsonDocument> SaveAssignment()
    {
        var request = await Page.RunAndWaitForRequestAsync(
            () => Page.Locator("#save-assignment").ClickAsync(),
            r => r.Url.EndsWith("/AssignResident") && r.Method == "POST");
        return JsonDocument.Parse(request.PostData!);
    }
}
