using System.Collections.Concurrent;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.DischargeNotice;

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

    [Test]
    public async Task a_discharge_the_office_refuses_sends_the_family_no_notice()
    {
        await NavigateAndBoot();
        var phoneNotices = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/DischargeNotice/NotifyFamily") && (request.PostData ?? "").Contains("phone"))
                phoneNotices.Enqueue(request.Url);
        };

        await ClickWhenStable(Discharge("rosa"));
        await Expect(DischargeStatus("rosa")).ToHaveTextAsync("Rosa Diaz cannot be discharged until her physician signs the discharge order.");

        await ClickWhenStable(Discharge("margaret"));
        await Expect(FamilyStatus("margaret")).ToHaveTextAsync("The family was not notified.");

        await Expect(FamilyStatus("rosa")).ToHaveTextAsync("Family not contacted yet.");
        Assert.That(phoneNotices, Is.Empty, "A refused discharge sends no family notice.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409", NoticeServiceDown);
    }
}
