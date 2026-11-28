using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAQueryMemberHoldsReservedCharacters(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_11_0)]
    public async Task the_endpoint_binds_the_term_as_it_was_typed()
    {
        var reserved = await NewWorld();
        await reserved.Page.GotoAsync(CarePages.ResidentSearch);
        await reserved.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Search residents", Exact = true }).FillAsync("a b+c&d");
        await reserved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(reserved.Page.Locator("#searched-term")).ToHaveTextAsync("a b+c&d");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.ResidentSearch);
        await plain.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Search residents", Exact = true }).FillAsync("plain");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#searched-term")).ToHaveTextAsync("plain");
    }
}
