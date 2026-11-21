using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NumbersAsText))]
[Category(Hosts.NumbersAsTextName)]
public sealed class WhenAFoundNumberWrittenAsTextReachesAMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_14_0)]
    public async Task the_method_adds_the_number_the_text_holds()
    {
        var fiveVials = await NewWorld();
        await fiveVials.Page.GotoAsync(CarePages.SupplyScanOfFiveVials);
        await fiveVials.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the supply", Exact = true }).ClickAsync();
        await Expect(fiveVials.Page.Locator("#supply-name")).ToHaveTextAsync("Saline");
        await fiveVials.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add", Exact = true }).ClickAsync();

        await Expect(fiveVials.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount", Exact = true })).ToHaveTextAsync("6");

        var emptyBox = await NewWorld();
        await emptyBox.Page.GotoAsync(CarePages.SupplyScanOfAnEmptyBox);
        await emptyBox.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the supply", Exact = true }).ClickAsync();
        await Expect(emptyBox.Page.Locator("#supply-name")).ToHaveTextAsync("Saline");
        await emptyBox.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add", Exact = true }).ClickAsync();
        await Expect(emptyBox.Page.Locator("#scan-status")).ToHaveTextAsync("Added");

        await Expect(emptyBox.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount", Exact = true })).ToHaveTextAsync("1");
    }
}
