using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnyReadsAListThatReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_4_0)]
    public async Task neither_the_test_nor_its_not_holds_and_the_step_after_them_runs()
    {
        var noList = await NewWorld();
        await noList.Page.GotoAsync(CarePages.WardBoard);
        await noList.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No list", Exact = true }).ClickAsync();
        await Expect(noList.Page.Locator("#report-status")).ToHaveTextAsync("Read");
        await Expect(noList.Page.Locator("#ward-status")).ToHaveTextAsync("Unknown");

        var listed = await NewWorld();
        await listed.Page.GotoAsync(CarePages.WardBoard);
        await listed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No alarms", Exact = true }).ClickAsync();
        await Expect(listed.Page.Locator("#ward-status")).ToHaveTextAsync("Quiet");
        await listed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "One alarm", Exact = true }).ClickAsync();
        await Expect(listed.Page.Locator("#ward-status")).ToHaveTextAsync("Alarms");
    }
}
