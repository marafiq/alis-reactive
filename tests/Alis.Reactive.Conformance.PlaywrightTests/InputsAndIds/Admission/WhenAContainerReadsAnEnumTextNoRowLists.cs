using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAContainerReadsAnEnumTextNoRowLists(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_11_0)]
    public async Task the_read_is_the_members_start()
    {
        var agency = await NewWorld();
        await agency.Page.GotoAsync(CarePages.QuickCover);
        await agency.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Agency cover");
        await Expect(agency.Page.Locator("#cover-line")).ToHaveTextAsync("Two carers on shift");

        var oneCarer = await NewWorld();
        await oneCarer.Page.GotoAsync(CarePages.QuickCover);
        await oneCarer.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("One carer");
        await Expect(oneCarer.Page.Locator("#cover-line")).ToHaveTextAsync("One carer on shift");
    }
}
