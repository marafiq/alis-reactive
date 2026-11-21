using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsItemTemplateBranches(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.template_conditional)]
    public async Task each_item_shows_the_branch_its_severity_takes()
    {
        var severe = await NewWorld();
        await severe.Page.GotoAsync(CarePages.AllergySeverities);
        await severe.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(severe.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin Severe", "Latex Mild" });

        var moderate = await NewWorld();
        await moderate.Page.GotoAsync(CarePages.AllergySeveritiesOfAModeratePenicillin);
        await moderate.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(moderate.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin Mild", "Latex Mild" });
    }
}
