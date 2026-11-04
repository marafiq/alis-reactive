using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenNurseAcknowledgesAStatus : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator BoardNote => Page.Locator("#resident-board-note");
    private ILocator AcknowledgeButton => Page.Locator("#btn-acknowledge");

    [Test]
    public async Task acknowledging_under_a_name_shows_the_receipt_naming_the_nurse_and_this_screen()
    {
        var plan = await NavigateAndConnect();
        await plan.TextBox(m => m.AcknowledgedBy).Fill("RN Patel");

        await ClickWhenStable(AcknowledgeButton);

        await Expect(BoardNote).ToHaveTextAsync("Acknowledged by RN Patel on 1 screen.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task acknowledging_without_a_name_is_refused()
    {
        await NavigateAndConnect();

        await ClickWhenStable(AcknowledgeButton);

        await Expect(BoardNote).ToHaveTextAsync("Say who is acknowledging.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    private async Task<PagePlan<RealTimeModel>> NavigateAndConnect()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/resident-status", 10000);
        return await PagePlan<RealTimeModel>.FromPage(Page);
    }
}
