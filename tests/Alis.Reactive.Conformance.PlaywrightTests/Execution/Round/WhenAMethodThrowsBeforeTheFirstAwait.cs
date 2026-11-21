using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMethodThrowsBeforeTheFirstAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_25_3)]
    public async Task the_error_is_logged_once_no_later_step_runs_and_under_skip_the_next_raise_runs_the_handler_again()
    {
        var scanned = await NewWorld();
        await scanned.Page.GotoAsync(CarePages.DoseScan);
        await scanned.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("4");
        await scanned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(scanned.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(scanned.Page.Locator("#scan-history")).ToHaveTextAsync("Scanned");
        Assert.That(scanned.ConsoleErrors, Is.EqualTo(new[] { "There is no held item at 4." }));
        await scanned.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("1");
        await scanned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(scanned.Page.Locator("#scan-history")).ToHaveTextAsync("ScannedScannedCheckedFinished");

        var checkedFirst = await NewWorld();
        await checkedFirst.Page.GotoAsync(CarePages.DoseScan);
        await checkedFirst.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("1");
        await checkedFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(checkedFirst.Page.Locator("#scan-history")).ToHaveTextAsync("ScannedCheckedFinished");
        Assert.That(checkedFirst.ConsoleErrors, Is.Empty);
    }
}
