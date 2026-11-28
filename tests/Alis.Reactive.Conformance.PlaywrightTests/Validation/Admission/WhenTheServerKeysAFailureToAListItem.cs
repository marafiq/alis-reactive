using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheServerKeysAFailureToAListItem(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X1_5_0)]
    public async Task the_message_shows_beside_the_name_of_the_member_its_key_indexes_and_in_the_summary()
    {
        var family = await NewWorld();
        await family.Page.GotoAsync(CarePages.FamilyMembers);
        await family.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(family.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second member", Exact = true })
            .GetByText("Name the second member.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(family.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the second member." });
        await Expect(family.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First member", Exact = true })
            .GetByText("Name the second member.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var firstKeyed = await NewWorld();
        await firstKeyed.Page.GotoAsync(CarePages.FamilyMembersWithTheFirstKeyed);
        await firstKeyed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(firstKeyed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First member", Exact = true })
            .GetByText("Name the second member.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(firstKeyed.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the second member." });
        await Expect(firstKeyed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second member", Exact = true })
            .GetByText("Name the second member.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
