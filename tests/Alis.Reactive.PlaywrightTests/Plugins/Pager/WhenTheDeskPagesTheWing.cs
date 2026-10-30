using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.Pager;

namespace Alis.Reactive.PlaywrightTests.Plugins.Pager;

/// <summary>
/// As the night desk, I want to page the Garden wing's staff through the community's pager system from
/// the page, so that a page goes out on the right channel, is counted once it has arrived, and reaches
/// everyone who carries a pager.
/// </summary>
/// <remarks>
/// The pager is JavaScript the page registers by name (Scripts/sandbox-plugins.ts) and declares with every
/// kind of member a plugin can have: a channel read and written, an access code written only, a promise
/// waited for before the delivered count is read, a status object with a method of its own, and a list of
/// pagers used by position and as a whole. Initials is a plugin that is itself a function.
/// </remarks>
[TestFixture]
public class WhenTheDeskPagesTheWing : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Plugins/Pager";

    private ILocator Channel => Page.Locator("#channel");
    private ILocator Status => Page.Locator("#status");
    private ILocator Online => Page.Locator("#online");
    private ILocator Offline => Page.Locator("#offline");
    private ILocator DeviceCount => Page.Locator("#device-count");
    private ILocator FirstDevice => Page.Locator("#first-device");
    private ILocator FirstBattery => Page.Locator("#first-battery");
    private ILocator ChargeNurseCarries => Page.Locator("#charge-nurse-carries");
    private ILocator Priority => Page.Locator("#priority");
    private ILocator SentLine => Page.Locator("#sent-line");
    private ILocator Delivered => Page.Locator("#delivered");
    private ILocator Signed => Page.Locator("#signed");
    private ILocator Receipt => Page.Locator("#receipt");
    private ILocator EstimateLine => Page.Locator("#estimate-line");
    private ILocator EstimateSeconds => Page.Locator("#estimate-seconds");

    private ILocator Message => Page.Locator($"#{IdGenerator.For<WingPagerModel>(m => m.Message)}");
    private ILocator Sender => Page.Locator($"#{IdGenerator.For<WingPagerModel>(m => m.Sender)}");
    private ILocator AccessCode => Page.Locator($"#{IdGenerator.For<WingPagerModel>(m => m.AccessCode)}");

    private ILocator NightChannel => Page.Locator("#night-channel");
    private ILocator UnlockPriority => Page.Locator("#unlock-priority");
    private ILocator SendPage => Page.Locator("#send-page");
    private ILocator SendWithReceipt => Page.Locator("#send-receipt");
    private ILocator Estimate => Page.Locator("#estimate");

    private async Task OpenTheDesk()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Channel).ToHaveTextAsync("day");
    }

    [Test]
    public async Task the_desk_opens_with_the_pager_online_on_the_day_channel_and_three_pagers_out()
    {
        await OpenTheDesk();

        await Expect(Status).ToHaveTextAsync("online, 0 queued");
        await Expect(Online).ToBeVisibleAsync();
        await Expect(Offline).ToBeHiddenAsync();
        await Expect(DeviceCount).ToHaveTextAsync("3");
        await Expect(FirstDevice).ToHaveTextAsync("Desk");
        await Expect(FirstBattery).ToHaveTextAsync("80");
        await Expect(ChargeNurseCarries).ToBeVisibleAsync();
        await Expect(Priority).ToHaveTextAsync("Priority paging is locked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_to_the_night_channel_is_read_back_from_the_pager()
    {
        await OpenTheDesk();

        await NightChannel.ClickAsync();

        await Expect(Channel).ToHaveTextAsync("night");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wrong_access_code_leaves_priority_paging_locked_and_the_right_one_unlocks_it()
    {
        await OpenTheDesk();

        await AccessCode.FillAsync("0000");
        await UnlockPriority.ClickAsync();
        await Expect(Priority).ToHaveTextAsync("Priority paging stays locked: check the code.");

        await AccessCode.FillAsync("4321");
        await UnlockPriority.ClickAsync();
        await Expect(Priority).ToHaveTextAsync("Priority paging is unlocked.");
        AssertNoConsoleErrors();
    }

    // The delivered count is read after the send's promise settles: a page is counted once it has arrived.
    [Test]
    public async Task a_page_is_counted_once_it_has_arrived_and_signed_with_the_senders_initials()
    {
        await OpenTheDesk();
        await Expect(SentLine).ToBeHiddenAsync();
        await Sender.FillAsync("Nurse Patel");

        await SendPage.ClickAsync();

        await Expect(SentLine).ToBeVisibleAsync();
        await Expect(Delivered).ToHaveTextAsync("1");
        await Expect(Signed).ToHaveTextAsync("NP");

        await Sender.FillAsync("Aide Rosa Reyes");
        await SendPage.ClickAsync();
        await Expect(Delivered).ToHaveTextAsync("2");
        await Expect(Signed).ToHaveTextAsync("ARR");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_page_sent_with_a_receipt_says_where_it_went_once_delivered()
    {
        await OpenTheDesk();
        await Message.FillAsync("Walker needed in room 14");

        await SendWithReceipt.ClickAsync();

        await Expect(Receipt).ToHaveTextAsync("Delivered to 3 pagers: Walker needed in room 14");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_urgent_page_reaches_every_pager_in_six_seconds()
    {
        await OpenTheDesk();
        await Expect(EstimateLine).ToBeHiddenAsync();

        await Estimate.ClickAsync();

        await Expect(EstimateLine).ToBeVisibleAsync();
        await Expect(EstimateSeconds).ToHaveTextAsync("6");
        AssertNoConsoleErrors();
    }
}
