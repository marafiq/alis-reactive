using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARuleStopsAtItsFirstFailure(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_40_0)]
    public async Task the_browser_lists_the_rules_first_failure_alone_as_the_endpoint_answers()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.AdmissionWard);
        await ward.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).FillAsync("north");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Keep the ward to three letters." });
        await ward.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).FillAsync("abc");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Write the ward in capitals." });
        Assert.That(ward.SentTo(CareEndpoints.AdmissionWards), Is.EqualTo(0));
        await ward.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).FillAsync("ABC");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#saved")).ToHaveTextAsync("ABC");
        await Expect(ward.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
