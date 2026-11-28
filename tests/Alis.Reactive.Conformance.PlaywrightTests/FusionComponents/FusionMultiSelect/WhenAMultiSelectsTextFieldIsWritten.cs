using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsTextFieldIsWritten(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.prop_dotted_write)]
    public async Task the_text_field_reads_the_name_written_to_it()
    {
        var relabelled = await NewWorld();
        await relabelled.Page.GotoAsync(CarePages.FieldChange);
        await relabelled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show field", Exact = true }).ClickAsync();
        await Expect(relabelled.Page.Locator("#field")).ToHaveTextAsync("name");
        await relabelled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use labels", Exact = true }).ClickAsync();

        await Expect(relabelled.Page.Locator("#field")).ToHaveTextAsync("label");
    }
}
