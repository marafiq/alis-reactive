using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnIntakeSendsADecimalUnderGerman(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_5_0)]
    public async Task the_form_binds_one_and_a_half_and_the_plain_form_binds_fifteen()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.StrengthIntakeInGerman);
        await intake.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("1,5");
        await intake.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(intake.Page.Locator("#saved-strength")).ToHaveTextAsync("1.5");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.StrengthIntakeInGerman);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync();
        await Expect(plain.Page.FrameLocator("#sent-strength").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("15");
    }
}
