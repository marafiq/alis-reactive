using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATypedValueIsWidenedForANullableProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_7_0)]
    public async Task the_property_takes_the_typed_value_and_a_cleared_field_writes_the_members_start()
    {
        var typed = await NewWorld();
        await typed.Page.GotoAsync(CarePages.IntakeRecord);
        await typed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Intake", Exact = true }).FillAsync("500");
        await typed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record", Exact = true }).ClickAsync();
        await Expect(typed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("500");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.IntakeRecord);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Intake", Exact = true }).ClearAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("0");
    }
}
