using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAWebPageIsCheckedByItsUrlRule(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_9_0)]
    public async Task a_space_in_the_path_passes_and_a_port_past_its_range_fails_as_on_the_server()
    {
        var contacts = await NewWorld();
        await contacts.Page.GotoAsync(CarePages.HouseholdContacts);
        await contacts.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Web page", Exact = true }).FillAsync("http://a.com/b c");
        await contacts.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(contacts.Page.Locator("#saved")).ToHaveTextAsync("http://a.com/b c");
        await Expect(contacts.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await contacts.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Web page", Exact = true }).FillAsync("http://a.com:99999");
        await contacts.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(contacts.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Web page", Exact = true })
            .GetByText("Give the family's web page.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(contacts.Page.Locator("#saved")).ToHaveTextAsync("http://a.com/b c");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.HouseholdContacts);
        await plain.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Web page", Exact = true }).FillAsync("http://a.com/b");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#saved")).ToHaveTextAsync("http://a.com/b");
        await Expect(plain.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
