using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormSaveSendsOnePhoneOfANullDeputy(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X16_3_0)]
    public async Task the_deputy_reads_as_the_endpoint_binds_a_new_one_and_the_save_goes_through()
    {
        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.DeputyAppointment);
        await unnamed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555-0101");
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.Locator("#saved")).ToHaveTextAsync("Unnamed, 555-0101");
        await Expect(unnamed.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var nameless = await NewWorld();
        await nameless.Page.GotoAsync(CarePages.DeputyAppointmentOfANamelessDeputy);
        await nameless.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555-0101");
        await nameless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(nameless.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the deputy." });
        await Expect(nameless.Page.Locator("#saved")).ToBeEmptyAsync();
    }
}
