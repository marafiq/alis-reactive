using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.PharmacyConnection;

[TestFixture]
public class WhenAdministratorTestsPharmacyConnection : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/PharmacyConnection";

    private PagePlan<PharmacyConnectionModel> _plan = null!;

    private ILocator SendTestOrder => Page.Locator("#send-test-order-btn");
    private ILocator Status => Page.Locator("#pharmacy-status");
    private ILocator Reply => Page.Locator("#pharmacy-reply");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<PharmacyConnectionModel>.FromPage(Page);
    }

    [Test]
    public async Task a_pharmacy_that_accepts_the_test_order_shows_connected_with_its_reply()
    {
        await NavigateAndBoot();
        await _plan.NativeDropDown(m => m.PharmacyId).Select("Riverside Pharmacy");

        await ClickWhenStable(SendTestOrder);

        await Expect(Reply).ToHaveTextAsync("Riverside Pharmacy accepted test order 4471.");
        await Expect(Status).ToHaveTextAsync("Connected");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pharmacy_that_is_down_shows_not_connected_with_its_reason()
    {
        await NavigateAndBoot();
        await _plan.NativeDropDown(m => m.PharmacyId).Select("Northgate Pharmacy");

        await ClickWhenStable(SendTestOrder);

        await Expect(Reply).ToHaveTextAsync("Northgate Pharmacy is down for maintenance until 6 AM.");
        await Expect(Status).ToHaveTextAsync("Not connected");
        AssertNoConsoleErrors();
    }
}
