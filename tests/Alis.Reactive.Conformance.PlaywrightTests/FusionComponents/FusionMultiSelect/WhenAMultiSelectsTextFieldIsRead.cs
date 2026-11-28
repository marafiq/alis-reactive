using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsTextFieldIsRead(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.prop_dotted_read)]
    public async Task the_text_field_is_the_name_the_apps_json_gives_the_member()
    {
        var camelCased = await NewWorld();
        await camelCased.Page.GotoAsync(CarePages.AllergyField);
        await camelCased.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show field", Exact = true }).ClickAsync();

        await Expect(camelCased.Page.Locator("#field")).ToHaveTextAsync("name");

        var pascalCased = await NewWorldUnder(Hosts.PascalCaseName);
        await pascalCased.Page.GotoAsync(CarePages.AllergyField);
        await pascalCased.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show field", Exact = true }).ClickAsync();

        await Expect(pascalCased.Page.Locator("#field")).ToHaveTextAsync("Name");
    }
}
