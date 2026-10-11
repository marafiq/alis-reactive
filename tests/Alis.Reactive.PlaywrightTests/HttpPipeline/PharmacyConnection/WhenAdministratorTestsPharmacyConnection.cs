using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.PharmacyConnection;

/// <summary>
/// As an administrator, I want to send a test order to a partner pharmacy and read its reply, so that
/// medication orders only go to a pharmacy that is answering.
/// </summary>
/// <remarks>
/// The server relays the pharmacy's answer as its status code and the body of its reply, a member the
/// response names <c>ResponseBody</c>. Riverside Pharmacy accepts the order; Northgate Pharmacy is down
/// for maintenance and says so.
/// </remarks>
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
