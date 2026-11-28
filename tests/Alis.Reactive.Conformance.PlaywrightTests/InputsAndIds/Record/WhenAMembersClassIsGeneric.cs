using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMembersClassIsGeneric(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F2.X14_12_0)]
    public async Task a_label_written_for_the_id_with_the_classs_argument_fills_the_input()
    {
        var labelled = await NewWorld();
        await labelled.Page.GotoAsync(CarePages.FluidsChart);
        await labelled.Page.GetByText("Nurse's initials", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await labelled.Page.Keyboard.PressAsync("A");
        await labelled.Page.Keyboard.PressAsync("B");
        await labelled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(labelled.Page.Locator("#charted")).ToHaveTextAsync("initials AB");

        var byArity = await NewWorld();
        await byArity.Page.GotoAsync(CarePages.FluidsChartLabelledByArity);
        await byArity.Page.GetByText("Nurse's initials", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await byArity.Page.Keyboard.PressAsync("A");
        await byArity.Page.Keyboard.PressAsync("B");
        await byArity.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(byArity.Page.Locator("#charted")).ToHaveTextAsync("initials none");
    }
}
