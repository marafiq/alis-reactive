using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoValuesOfAnEnumInputAreHeldAsOneText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X10_0)]
    public async Task the_page_is_refused_where_the_field_renders_naming_both_values_and_the_text_they_share()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.FloorChoiceNumbered);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Input 'Floor' holds First and Ground in one text, '1', the number of First and the name Ground's JSON converter writes, "
                + "so the browser cannot tell which one it holds; give Ground a JsonStringEnumMemberName no value's C# name or number spells.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.WingChoiceNamed);
        await rendered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Wing", Exact = true }).SelectOptionAsync("West");
        var sent = await rendered.Page.RunAndWaitForRequestAsync(() => rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync(), "**/inputs-and-ids/wing-choices");
        Assert.That(sent.PostData, Is.EqualTo("Wing=West"));
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Wing: West");
    }
}
