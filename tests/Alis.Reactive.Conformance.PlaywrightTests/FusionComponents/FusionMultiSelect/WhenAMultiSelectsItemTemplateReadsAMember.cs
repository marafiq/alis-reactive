using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsItemTemplateReadsAMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.template_render)]
    public async Task the_list_shows_each_items_name_as_text()
    {
        var asText = await NewWorld();
        await asText.Page.GotoAsync(CarePages.AllergyLabels);
        await asText.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(asText.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin", "<i>Latex</i>" });

        var readRaw = await NewWorld();
        await readRaw.Page.GotoAsync(CarePages.AllergyLabelsReadRaw);
        await readRaw.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(readRaw.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin", "Latex" });
    }
}
