using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.PharmacyDelivery;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel.PharmacyDelivery;

/// <summary>
/// As the night nurse, I want the evening pharmacy delivery handled from one page: the slip opened in the
/// drawer, my signature noticed, a damaged item returned only once I have said yes, the manifest check
/// covering its card while the pharmacy answers, and the delivery marked with who received it, so that
/// nothing about the delivery is left to memory.
/// </summary>
/// <remarks>
/// The drawer, toast, confirm and loader are the page's app-level objects, reached without an id; the slip
/// is a partial loaded into the drawer; Mark delivered is a link with a plan of its own whose request checks the
/// page's rule on the receiver's input and sends it, both inside the receipt the request names.
/// </remarks>
[TestFixture]
public class WhenTheNightNurseReceivesADelivery : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/PharmacyDelivery";
    private const string ManifestUrl = "**/Sandbox/Components/PharmacyDelivery/Manifest";
    private const string MarkDeliveredUrl = "/Sandbox/Components/PharmacyDelivery/MarkDelivered";

    private static readonly Regex DrawerOpen = new("alis-drawer--visible");
    private static readonly Regex DrawerLarge = new("alis-drawer--lg");
    private static readonly Regex ToastSuccess = new(@"\be-toast-success\b");

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator DrawerTitle => Page.Locator("#alis-drawer-title");
    private ILocator DrawerContent => Page.Locator("#alis-drawer-content");
    private ILocator SlipSummary => DrawerContent.Locator("#slip-summary");
    private ILocator SlipWide => Page.Locator("#slip-wide");
    private ILocator SlipStatus => Page.Locator("#slip-status");
    // The layout's toast host (#alisFusionToast) carries e-toast itself; a notice is an e-toast inside the container.
    private ILocator Notices => Page.Locator(".e-toast-container .e-toast");
    private ILocator Notice => Notices.First;
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");
    private ILocator ConfirmQuestion => ConfirmDialog.Locator(".e-dlg-content");
    private ILocator ReturnStatus => Page.Locator("#return-status");
    private ILocator CartReview => Page.Locator("#cart-review");
    private ILocator Loader => Page.Locator("#alis-loader");
    private ILocator LoaderOverManifest => Page.Locator("#manifest-card #alis-loader");
    private ILocator ManifestWait => Page.Locator("#manifest-wait");
    private ILocator Manifest => Page.Locator("#manifest");
    private ILocator DeliveryStatus => Page.Locator("#delivery-status");
    private ILocator ReceivedBy => Page.Locator($"#{IdGenerator.For<PharmacyDeliveryModel>(m => m.ReceivedBy)}");
    private ILocator ReceivedByMessage => Page.Locator("span[data-valmsg-for='ReceivedBy']");

    private ILocator OpenSlip => Page.Locator("#open-slip");
    private ILocator CloseSlip => Page.Locator("#close-slip");
    private ILocator AcknowledgeSlip => Page.Locator("#acknowledge-slip");
    private ILocator SignDelivery => Page.Locator("#sign-delivery");
    private ILocator DismissNotice => Page.Locator("#dismiss-notice");
    private ILocator ReturnItem => Page.Locator("#return-item");
    private ILocator CheckManifest => Page.Locator("#check-manifest");
    private ILocator MarkDelivered => Page.Locator("#mark-delivered");

    private async Task OpenTheDelivery()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(DeliveryStatus).ToHaveTextAsync("Not marked delivered.");
    }

    private ConcurrentQueue<string> CapturePosts()
    {
        var posts = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST") posts.Enqueue(request.Url);
        };
        return posts;
    }

    [Test]
    public async Task opening_the_slip_shows_it_in_the_wide_drawer_and_the_slip_can_be_acknowledged_from_there()
    {
        await OpenTheDelivery();
        await Expect(SlipWide).ToBeHiddenAsync();

        await OpenSlip.ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(DrawerOpen, new() { Timeout = 5000 });
        await Expect(Drawer).ToHaveClassAsync(DrawerLarge);
        await Expect(DrawerTitle).ToHaveTextAsync("Delivery slip");
        await Expect(SlipSummary).ToHaveTextAsync("12 items from Garden Pharmacy, delivered at 8:40 PM.");
        await Expect(SlipWide).ToBeVisibleAsync();

        await AcknowledgeSlip.ClickAsync();
        await Expect(SlipStatus).ToHaveTextAsync("Slip acknowledged by the night nurse.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_slip_from_the_page_closes_the_drawer()
    {
        await OpenTheDelivery();
        await OpenSlip.ClickAsync();
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen, new() { Timeout = 5000 });

        await CloseSlip.ClickAsync();

        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Drawer).ToHaveAttributeAsync("aria-hidden", "true", new() { Timeout = 5000 });
        await Expect(SlipStatus).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task signing_for_the_delivery_shows_the_notice_until_it_is_dismissed()
    {
        await OpenTheDelivery();
        await Expect(Notices).ToHaveCountAsync(0);

        await SignDelivery.ClickAsync();

        await Expect(Notice).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Notice.Locator(".e-toast-title")).ToHaveTextAsync("Delivery signed");
        await Expect(Notice.Locator(".e-toast-content")).ToHaveTextAsync("Signed for by the night nurse at 8:40 PM.");
        await Expect(Notice).ToHaveClassAsync(ToastSuccess);

        await DismissNotice.ClickAsync();
        await Expect(Notices).ToHaveCountAsync(0, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task returning_the_damaged_item_waits_for_a_yes_and_then_goes_to_the_pharmacy()
    {
        await OpenTheDelivery();
        var posts = CapturePosts();

        await ReturnItem.ClickAsync();

        await Expect(ConfirmDialog).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ConfirmQuestion).ToHaveTextAsync("Return the damaged item to the pharmacy?");
        await Expect(ReturnStatus).ToHaveTextAsync("No returns tonight.");
        await Expect(CartReview).ToHaveTextAsync("Cart not reviewed yet.");
        Assert.That(posts, Is.Empty, "Nothing is returned before the nurse answers.");

        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "OK" }));

        await Expect(ReturnStatus).ToHaveTextAsync("Damaged item returned to the pharmacy.");
        await Expect(CartReview).ToHaveTextAsync("Cart reviewed by the night nurse.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task declining_the_return_keeps_the_item_on_the_cart_and_sends_nothing()
    {
        await OpenTheDelivery();
        var posts = CapturePosts();

        await ReturnItem.ClickAsync();
        await Expect(ConfirmDialog).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }));

        await Expect(ReturnStatus).ToHaveTextAsync("Return cancelled; the item stays on the cart.");
        await Expect(CartReview).ToHaveTextAsync("Cart reviewed by the night nurse.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        Assert.That(posts, Is.Empty, "A declined return sends nothing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_manifest_covers_the_manifest_card_until_the_pharmacy_answers()
    {
        var manifestReleased = new SemaphoreSlim(0);
        await Page.RouteAsync(ManifestUrl, async route =>
        {
            await manifestReleased.WaitAsync();
            await route.ContinueAsync();
        });
        await OpenTheDelivery();
        await Expect(ManifestWait).ToBeHiddenAsync();

        await CheckManifest.ClickAsync();

        await Expect(LoaderOverManifest).ToBeVisibleAsync();
        await Expect(ManifestWait).ToBeVisibleAsync();
        await Expect(Manifest).ToHaveTextAsync("Manifest not checked.");

        manifestReleased.Release();

        await Expect(Manifest).ToHaveTextAsync("12 items on the manifest, 4 of them controlled.");
        await Expect(Loader).ToHaveAttributeAsync("aria-hidden", "true");
        await Expect(LoaderOverManifest).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_the_delivery_with_no_receiver_asks_who_received_it_and_sends_nothing()
    {
        await OpenTheDelivery();
        var posts = CapturePosts();
        await ReceivedBy.FillAsync("");

        await MarkDelivered.ClickAsync();

        await Expect(ReceivedByMessage).ToHaveTextAsync("Say who received the delivery.");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(DeliveryStatus).ToHaveTextAsync("Not marked delivered.");
        Assert.That(posts, Is.Empty, "The link holds its request until someone is named as the receiver.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_the_delivery_sends_who_received_it()
    {
        await OpenTheDelivery();
        await ReceivedBy.FillAsync("Nurse Okafor");

        await MarkDelivered.ClickAsync();

        await Expect(DeliveryStatus).ToHaveTextAsync("Delivery marked; received by Nurse Okafor.");
        AssertNoConsoleErrors();
    }

    // A Shift click is the browser's: the link's address opens in a window of its own, and the page sends
    // nothing and marks nothing.
    [Test]
    public async Task a_shift_click_on_mark_delivered_opens_the_slip_in_a_new_window_and_marks_nothing()
    {
        await OpenTheDelivery();
        var posts = CapturePosts();

        var slipWindow = await Context.RunAndWaitForPageAsync(async () =>
        {
            await MarkDelivered.ClickAsync(new() { Modifiers = new[] { KeyboardModifier.Shift } });
        });

        await Expect(slipWindow).ToHaveURLAsync(new Regex($"{Regex.Escape(MarkDeliveredUrl)}$"), new() { Timeout = 10000 });
        await Expect(slipWindow.Locator("body")).ToContainTextAsync("The delivery slip is open.", new() { Timeout = 10000 });
        await Expect(DeliveryStatus).ToHaveTextAsync("Not marked delivered.");
        Assert.That(posts, Is.Empty, "A Shift click sends no request from the page.");
        AssertNoConsoleErrors();
    }

    // An Alt click is the browser's too: Chromium saves the link's address as a file, and the page sends
    // nothing and marks nothing.
    [Test]
    public async Task an_alt_click_on_mark_delivered_saves_the_slip_and_marks_nothing()
    {
        await OpenTheDelivery();
        var posts = CapturePosts();

        var download = await Page.RunAndWaitForDownloadAsync(async () =>
        {
            await MarkDelivered.ClickAsync(new() { Modifiers = new[] { KeyboardModifier.Alt } });
        });

        Assert.That(download.Url, Does.EndWith(MarkDeliveredUrl));
        await Expect(DeliveryStatus).ToHaveTextAsync("Not marked delivered.");
        Assert.That(posts, Is.Empty, "An Alt click sends no request from the page.");
        AssertNoConsoleErrors();
    }
}
