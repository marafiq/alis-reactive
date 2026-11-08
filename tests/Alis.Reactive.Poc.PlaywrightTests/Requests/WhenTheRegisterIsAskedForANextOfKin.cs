namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenTheRegisterIsAskedForANextOfKin : AppPageTest
{
    [Test]
    public async Task a_member_of_an_object_set_by_name_is_bound_by_the_endpoint()
    {
        await Open("/Residents/NextOfKin");
        await Page.GetByLabel("Resident name").FillAsync("Helen Park");

        await Page.GetByLabel("Resident name").PressAsync("Tab");

        await Expect(Page.Locator("#kin-set")).ToHaveTextAsync("Sam Park");
    }

    [Test]
    public async Task a_member_of_an_object_sent_in_the_query_is_bound_by_the_endpoint()
    {
        await Open("/Residents/NextOfKin");
        await Page.GetByLabel("Resident name").FillAsync("Helen Park");
        await Page.GetByLabel("Next of kin", new() { Exact = true }).FillAsync("Ana Park");

        await Page.GetByLabel("Next of kin", new() { Exact = true }).PressAsync("Tab");

        await Expect(Page.Locator("#kin-asked")).ToHaveTextAsync("Ana Park");
    }
}
