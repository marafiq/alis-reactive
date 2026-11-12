using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPropertysObjectLacksAField(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X5_4_0)]
    public async Task the_field_reads_its_start_and_a_property_that_holds_null_reads_nothing_and_goes_on()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRecordFields);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the record", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#record-name")).ToHaveTextAsync("Morning");
        await Expect(bench.Page.Locator("#record-code")).ToHaveTextAsync("C0");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the record", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the record", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#record-shown")).ToHaveTextAsync("2");
        await Expect(bench.Page.Locator("#record-name")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#record-code")).ToHaveTextAsync("");

        var evening = await NewWorld();
        await evening.Page.GotoAsync(BenchPages.DeclarationsRecordFields);
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hold the evening record", Exact = true }).ClickAsync();
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the record", Exact = true }).ClickAsync();
        await Expect(evening.Page.Locator("#record-name")).ToHaveTextAsync("Evening");
        await Expect(evening.Page.Locator("#record-code")).ToHaveTextAsync("C5");
    }
}
