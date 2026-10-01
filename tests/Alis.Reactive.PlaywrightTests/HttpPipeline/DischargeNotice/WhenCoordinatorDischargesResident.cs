namespace Alis.Reactive.PlaywrightTests.HttpPipeline.DischargeNotice;

/// <summary>
/// As a coordinator, I want a family notice that failed to show only what the notice service said,
/// so that I never read the discharge confirmation as the reason the family was not told.
/// </summary>
/// <remarks>
/// The family notice is a request chained after the discharge, and both notice services are down:
/// Margaret Hill's family is reached by text message, whose service answers 503 with no body; Arthur
/// Bell's by email, whose service answers 503 with its reason. Each failure writes the reason line
/// before the status line, so once the status line shows the failure the reason line is final.
/// </remarks>
[TestFixture]
public class WhenCoordinatorDischargesResident : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/DischargeNotice";
    private const string NoticeServiceDown = "the server responded with a status of 503";

    private ILocator Discharge(string resident) => Page.Locator($"#discharge-{resident}-btn");
    private ILocator DischargeStatus(string resident) => Page.Locator($"#{resident}-discharge-status");
    private ILocator FamilyStatus(string resident) => Page.Locator($"#{resident}-family-status");
    private ILocator FamilyReason(string resident) => Page.Locator($"#{resident}-family-reason");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task a_family_notice_that_fails_without_a_reason_shows_the_family_was_not_notified_and_no_reason()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Discharge("margaret"));

        await Expect(FamilyStatus("margaret")).ToHaveTextAsync("The family was not notified.");
        await Expect(FamilyReason("margaret")).ToHaveTextAsync("");
        await Expect(DischargeStatus("margaret")).ToHaveTextAsync("Margaret Hill is discharged.");
        AssertNoConsoleErrorsExcept(NoticeServiceDown);
    }

    [Test]
    public async Task a_family_notice_that_fails_with_a_reason_shows_the_services_reason()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Discharge("arthur"));

        await Expect(FamilyStatus("arthur")).ToHaveTextAsync("The family was not notified.");
        await Expect(FamilyReason("arthur")).ToHaveTextAsync("The email service is down for maintenance.");
        await Expect(DischargeStatus("arthur")).ToHaveTextAsync("Arthur Bell is discharged.");
        AssertNoConsoleErrorsExcept(NoticeServiceDown);
    }
}
