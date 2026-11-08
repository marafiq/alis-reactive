using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenAWardRegisterIsChecked : AppPageTest
{
    [Test]
    public async Task a_request_with_no_success_route_runs_no_route_when_it_succeeds()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Garden");

        await Page.GetByLabel("Ward").PressAsync("Tab");

        await Expect(Page.Locator("#register-checked")).ToHaveTextAsync("Garden");
        await Expect(Page.Locator("#register")).ToHaveTextAsync("not checked");
    }

    [Test]
    public async Task a_request_with_no_success_route_runs_its_route_for_every_other_outcome_when_it_fails()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Annex");

        await Page.GetByLabel("Ward").PressAsync("Tab");

        await Expect(Page.Locator("#register")).ToHaveTextAsync("The ward's register is offline");
    }

    [Test]
    public async Task a_request_with_no_success_route_asks_for_any_kind()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Garden");

        var asked = await Page.RunAndWaitForRequestAsync(() => Page.GetByLabel("Ward").PressAsync("Tab"), new Regex("/Residents/Register/"));

        Assert.That(await asked.HeaderValueAsync("Accept"), Is.EqualTo("*/*"));
    }
}
