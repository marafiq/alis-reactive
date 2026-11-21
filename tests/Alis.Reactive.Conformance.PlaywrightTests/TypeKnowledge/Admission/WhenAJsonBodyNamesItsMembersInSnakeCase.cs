using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TypeKnowledge;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.SnakeCase))]
[Category(Hosts.SnakeCaseName)]
public sealed class WhenAJsonBodyNamesItsMembersInSnakeCase(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M16_F1.X18_5_0)]
    public async Task the_body_names_each_member_as_the_apps_json_options_name_it()
    {
        var snakeCase = await NewWorld();
        await snakeCase.Page.GotoAsync(CarePages.AdmitByGivenName);
        await snakeCase.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true }).FillAsync("Rose");
        await snakeCase.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(snakeCase.Page.Locator("#sent-names")).ToHaveTextAsync("given_name");
        await Expect(snakeCase.Page.Locator("#admitted-name")).ToHaveTextAsync("admitted Rose");

        var camelCase = await NewWorldUnder(Hosts.Net10Name);
        await camelCase.Page.GotoAsync(CarePages.AdmitByGivenName);
        await camelCase.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true }).FillAsync("Rose");
        await camelCase.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(camelCase.Page.Locator("#sent-names")).ToHaveTextAsync("givenName");
        await Expect(camelCase.Page.Locator("#admitted-name")).ToHaveTextAsync("admitted Rose");
    }
}
