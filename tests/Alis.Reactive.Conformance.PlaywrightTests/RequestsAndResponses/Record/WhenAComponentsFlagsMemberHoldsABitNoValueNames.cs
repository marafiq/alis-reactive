using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentsFlagsMemberHoldsABitNoValueNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_25_0)]
    public async Task nothing_is_sent_no_later_step_runs_and_the_console_holds_one_error()
    {
        var unnamedBit = await NewWorld();
        await unnamedBit.Page.GotoAsync(CarePages.PanelDaysHold);
        await unnamedBit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hold 129", Exact = true }).ClickAsync();
        await unnamedBit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync();
        await Expect(unnamedBit.Page.Locator("#care-log")).ToHaveTextAsync("-");
        Assert.That(unnamedBit.ConsoleErrors, Has.Count.EqualTo(1));
        Assert.That(unnamedBit.ConsoleErrors.Single(), Does.Contain("r => r.Care").And.Contain("129"));

        var mondayAndTheWeekend = await NewWorld();
        await mondayAndTheWeekend.Page.GotoAsync(CarePages.PanelDaysHold);
        var sent = await mondayAndTheWeekend.Page.RunAndWaitForRequestAsync(() => mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/panel-days/care**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/care/record/panel-days/care?Care=Mon%2C+Weekend"));
        await Expect(mondayAndTheWeekend.Page.Locator("#care-log")).ToHaveTextAsync("Sent");
    }
}
