using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenAnInputOutsideARowsPrefixIsInItsContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_56_0)]
    public async Task the_rows_send_stops_and_names_the_input_the_container_and_the_prefix()
    {
        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.ShiftRoster);
        await noted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Add a note", Exact = true }).ClickAsync();
        await Expect(noted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Note", Exact = true })).ToBeVisibleAsync();
        await noted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await noted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 5", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#roster-status")).ToHaveTextAsync("Shift 5 saved");
        Assert.That(noted.ConsoleErrors, Is.EqualTo(new[] { "Input 'Alis_Reactive_Conformance_App_Care_ShiftNote__Note' binds 'Note' inside container 'shift-6', whose field prefix 'Rows[1]' does not open it." }));
        Assert.That(noted.SentTo(CareEndpoints.ShiftRosterShifts), Is.EqualTo(1));

        var unloaded = await NewWorld();
        await unloaded.Page.GotoAsync(CarePages.ShiftRoster);
        await unloaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(unloaded.Page.Locator("#roster-status")).ToHaveTextAsync("Shift 6 saved");
    }
}
