using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenNoticesOfEachKindShow(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F5.M25_X9_0)]
    public async Task success_is_announced_politely_and_an_error_as_an_alert()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.RoundNotices);
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record a dose", Exact = true }).ClickAsync();
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Report a fall", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Status).GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("Dose recorded");
        await Expect(both.Page.GetByRole(AriaRole.Alert).GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("Fall reported");

        var dose = await NewWorld();
        await dose.Page.GotoAsync(CarePages.RoundNotices);
        await dose.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record a dose", Exact = true }).ClickAsync();
        await Expect(dose.Page.GetByRole(AriaRole.Status).GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("Dose recorded");
        await Expect(dose.Page.GetByRole(AriaRole.Alert)).ToBeEmptyAsync();
    }
}
