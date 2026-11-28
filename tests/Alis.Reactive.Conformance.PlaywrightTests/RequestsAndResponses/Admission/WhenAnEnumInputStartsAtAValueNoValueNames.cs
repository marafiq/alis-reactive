using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumInputStartsAtAValueNoValueNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_39_0)]
    public async Task the_start_is_left_out_and_an_option_held_as_a_number_is_sent_by_its_csharp_name()
    {
        var unchosen = await NewWorld();
        await unchosen.Page.GotoAsync(CarePages.CareReview);
        await unchosen.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Stable");
        var sent = await unchosen.Page.RunAndWaitForRequestAsync(() => unchosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save review", Exact = true }).ClickAsync(), "**/care-review/reviews");
        Assert.That(sent.PostData, Is.EqualTo("Note=Stable"));
        await Expect(unchosen.Page.Locator("#reviewed")).ToHaveTextAsync("Stable: level 0");

        var assisted = await NewWorld();
        await assisted.Page.GotoAsync(CarePages.CareReview);
        await assisted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Stable");
        await assisted.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Level", Exact = true }).SelectOptionAsync("Assisted");
        var chosen = await assisted.Page.RunAndWaitForRequestAsync(() => assisted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save review", Exact = true }).ClickAsync(), "**/care-review/reviews");
        Assert.That(chosen.PostData, Is.EqualTo("Level=Assisted&Note=Stable"));
        await Expect(assisted.Page.Locator("#reviewed")).ToHaveTextAsync("Stable: level 2");
    }
}
