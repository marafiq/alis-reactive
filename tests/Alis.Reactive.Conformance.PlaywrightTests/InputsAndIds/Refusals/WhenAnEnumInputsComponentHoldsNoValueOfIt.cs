using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumInputsComponentHoldsNoValueOfIt(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X9_0)]
    public async Task the_page_is_refused_where_the_field_renders_naming_the_enum_and_the_components_value()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.RotaCoverTicked);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Input 'Cover' binds RotaCover, and its component's value is IReadOnlyList<string>, which holds no value of RotaCover: "
                + "render the field with a component whose value is text or RotaCover.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.RotaCoverChosen);
        await rendered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Two carers");
        var sent = await rendered.Page.RunAndWaitForRequestAsync(() => rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync(), "**/inputs-and-ids/rotas");
        Assert.That(sent.PostData, Is.EqualTo("Cover=TwoCarers"));
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Cover: TwoCarers");
    }
}
