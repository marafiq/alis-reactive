using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotBringsASecondInputOfOneName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_6_0)]
    public async Task the_containers_read_stops_and_the_console_holds_one_error()
    {
        var twoNights = await NewWorld();
        await twoNights.Page.GotoAsync(CarePages.StayExtension);
        await twoNights.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true }).FillAsync("14");
        await twoNights.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add the extension", Exact = true }).ClickAsync();
        await twoNights.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Extended nights", Exact = true }).FillAsync("7");
        await twoNights.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true }).ClickAsync();
        await Expect(twoNights.Page.Locator("#quote")).ToHaveTextAsync("-");
        Assert.That(twoNights.ConsoleErrors, Has.Count.EqualTo(1));

        var extraNights = await NewWorld();
        await extraNights.Page.GotoAsync(CarePages.StayExtensionOfExtraNights);
        await extraNights.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true }).FillAsync("14");
        await extraNights.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add the extension", Exact = true }).ClickAsync();
        await extraNights.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Extended nights", Exact = true }).FillAsync("7");
        await extraNights.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true }).ClickAsync();
        await Expect(extraNights.Page.Locator("#quote")).ToHaveTextAsync("14");
    }
}
