using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenToastsShowForTheirTime(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F5.X17_9_0)]
    public async Task a_hovered_toast_stays_and_an_error_stays_until_dismissed()
    {
        var round = await NewWorld();
        await round.InstallTheClock();
        await round.Page.GotoAsync(CarePages.DoseToasts);
        await round.HoldTheClock();
        var information = round.Page.GetByRole(AriaRole.Status).GetByRole(AriaRole.Paragraph);
        var error = round.Page.GetByRole(AriaRole.Alert).GetByRole(AriaRole.Paragraph);
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record a dose", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Report a fall", Exact = true }).ClickAsync();
        await information.HoverAsync();
        await round.RunTheClockFor(TimeSpan.FromSeconds(6));
        await Expect(information).ToHaveTextAsync("Dose recorded");
        await Expect(error).ToHaveTextAsync("Fall reported: call the nurse");
        await round.Page.Mouse.MoveAsync(0, 0);
        await round.RunTheClockFor(TimeSpan.FromSeconds(6));
        await Expect(information).ToHaveCountAsync(0);
        await Expect(error).ToHaveTextAsync("Fall reported: call the nurse");
        await round.Page.Keyboard.PressAsync("Escape");
        await round.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Dose round", Exact = true }).ClickAsync();
        await Expect(error).ToHaveTextAsync("Fall reported: call the nurse");
        await round.Page.GetByRole(AriaRole.Alert).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Dismiss", Exact = true }).ClickAsync();
        await Expect(error).ToHaveCountAsync(0);

        var unhovered = await NewWorld();
        await unhovered.InstallTheClock();
        await unhovered.Page.GotoAsync(CarePages.DoseToasts);
        await unhovered.HoldTheClock();
        var dose = unhovered.Page.GetByRole(AriaRole.Status).GetByRole(AriaRole.Paragraph);
        await unhovered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record a dose", Exact = true }).ClickAsync();
        await Expect(dose).ToHaveTextAsync("Dose recorded");
        await unhovered.RunTheClockFor(TimeSpan.FromSeconds(6));
        await Expect(dose).ToHaveCountAsync(0);
    }
}
