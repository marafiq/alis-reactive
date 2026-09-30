using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a nurse, I want a care level change to carry the resident's physician sign-off decision as
/// the server made it, so that no resident is routed for a sign-off they do not need, or misses one.
/// </summary>
[TestFixture]
public class WhenCareLevelChangeIsRequested : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeHiddenField/CareLevelChange";

    private ILocator NewCareLevel => Page.Locator($"#{IdGenerator.For<CareLevelChangeModel>(m => m.RequestedCareLevel)}");
    private ILocator RequestChange => Page.Locator("#request-care-level-change-btn");
    private ILocator ChangeStatus => Page.Locator("#care-level-change-status");

    private async Task RequestAssistedLivingFor(int residentId)
    {
        await NavigateTo($"{Path}?residentId={residentId}");
        await WaitForTraceMessage("booted", 10000);
        await NewCareLevel.SelectOptionAsync(new SelectOptionValue { Label = "Assisted Living" });
        await ClickWhenStable(RequestChange);
    }

    [Test]
    public async Task a_resident_who_needs_no_physician_sign_off_is_requested_without_one()
    {
        await RequestAssistedLivingFor(1042);

        await Expect(ChangeStatus).ToHaveTextAsync(
            "Change to Assisted Living requested for resident #1042; no physician sign-off needed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_who_needs_a_physician_sign_off_is_requested_with_one()
    {
        await RequestAssistedLivingFor(1043);

        await Expect(ChangeStatus).ToHaveTextAsync(
            "Change to Assisted Living requested for resident #1043; physician sign-off required.");
        AssertNoConsoleErrors();
    }
}
