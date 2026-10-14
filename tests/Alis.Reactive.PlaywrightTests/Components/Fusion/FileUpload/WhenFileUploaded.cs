using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.FileUpload;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.FileUpload;

/// <summary>
/// As a wound-care nurse, I want to attach a resident's signed consent form and a photo of her wound to her record
/// after a wound review, and have the record refuse a file it cannot keep and tell me what it received, so that the
/// wound record holds the right documents and nothing I sent goes missing without my knowing.
/// </summary>
/// <remarks>
/// Margaret Hale's left heel wound is reviewed. The checklist ticks off the consent form while a PDF is chosen and
/// the photo while a JPG or PNG is chosen, following each choice and each removal; an empty file is a scan that did
/// not take and is not added. The form re-checks the files each time the uploader's files change
/// (<c>FilesChanged</c>, which the framework raises once EJ2 has added or removed them, since EJ2's own
/// <c>selected</c> and <c>removing</c> come before). The record keeps PDFs and JPG or PNG photos of at most 5 MB: the
/// server reads the files and refuses the whole attachment when one is too large or of another kind, naming each
/// file it received with its size.
/// The file items' <c>RawFile</c> is read through <c>Files</c>, which the attach sends. Files are chosen through the
/// file dialog; dropped or pasted files reach the same <c>selected</c> event (uploader.js:1264, 936).
/// </remarks>
[TestFixture]
public class WhenFileUploaded : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FileUpload";
    private const string AttachBoth = "Attach the signed consent and the wound photo.";

    private static readonly FilePayload SignedConsent = FileOf("consent-form.pdf", "%PDF-1.7 signed consent", "application/pdf");
    private static readonly FilePayload WoundPhoto = FileOf("wound-photo.jpg", "JPEG wound photo", "image/jpeg");
    private static readonly FilePayload EmptyConsentScan = FileOf("consent-scan.pdf", "", "application/pdf");

    private PagePlan<WoundAttachmentsModel> _plan = null!;

    private FusionFileUploadLocator Files => new(Page, _plan.FindComponent(m => m.Attachments)!.ElementId);
    private ILocator ConsentChosen => Page.Locator("#consent-chosen");
    private ILocator PhotoChosen => Page.Locator("#photo-chosen");
    private ILocator EmptyScan => Page.Locator("#empty-scan");
    private ILocator AttachToRecord => Page.Locator("#attach-files-btn");
    private ILocator Status => Page.Locator("#attach-status");
    private ILocator FilesError => _plan.ErrorFor(m => m.Attachments);

    private static FilePayload FileOf(string name, string content, string mimeType) => new()
    {
        Name = name,
        MimeType = mimeType,
        Buffer = System.Text.Encoding.UTF8.GetBytes(content),
    };

    private async Task OpenTheWoundRecord()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<WoundAttachmentsModel>.FromPage(Page);
        await Expect(Files.Root).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task choosing_the_signed_consent_ticks_it_off_the_checklist()
    {
        await OpenTheWoundRecord();

        await Files.Choose(SignedConsent);

        await Expect(ConsentChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PhotoChosen).ToBeHiddenAsync();
        await Expect(Files.Choices).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_wound_photo_ticks_it_off_the_checklist()
    {
        await OpenTheWoundRecord();

        await Files.Choose(WoundPhoto);

        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ConsentChosen).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_consent_and_the_photo_does_not_ask_for_them()
    {
        await OpenTheWoundRecord();

        await Files.Choose(SignedConsent, WoundPhoto);

        await Expect(ConsentChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PhotoChosen).ToBeVisibleAsync();
        await Expect(FilesError).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_both_after_a_refused_attach_clears_the_request_at_once()
    {
        await OpenTheWoundRecord();
        await ClickWhenStable(AttachToRecord);
        await Expect(FilesError).ToHaveTextAsync(AttachBoth, new() { Timeout = 5000 });

        await Files.Choose(SignedConsent, WoundPhoto);

        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(FilesError).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("Nothing attached yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_empty_scan_is_not_added_and_the_nurse_is_asked_to_scan_it_again()
    {
        await OpenTheWoundRecord();

        await Files.Choose(EmptyConsentScan);

        await Expect(EmptyScan).ToHaveTextAsync("consent-scan.pdf is empty. Scan it again before attaching it.", new() { Timeout = 5000 });
        await Expect(EmptyScan).ToBeVisibleAsync();
        await Expect(Files.Choices).ToHaveCountAsync(0);
        await Expect(ConsentChosen).ToBeHiddenAsync();

        await Files.Choose(SignedConsent);

        await Expect(EmptyScan).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(ConsentChosen).ToBeVisibleAsync();
        await Expect(Files.Choices).ToHaveCountAsync(1);
        await Expect(Files.Choices).ToContainTextAsync("consent-form");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_empty_scan_after_a_refused_attach_keeps_the_request()
    {
        await OpenTheWoundRecord();
        await ClickWhenStable(AttachToRecord);
        await Expect(FilesError).ToHaveTextAsync(AttachBoth, new() { Timeout = 5000 });

        await Files.Choose(EmptyConsentScan);

        await Expect(EmptyScan).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(FilesError).ToHaveTextAsync(AttachBoth);
        await Expect(Files.Choices).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_the_chosen_files_asks_for_the_consent_and_the_photo_again()
    {
        await OpenTheWoundRecord();
        await Files.Choose(SignedConsent, WoundPhoto);
        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(FilesError).ToBeHiddenAsync();

        await Files.Remove(SignedConsent.Name);

        await Expect(FilesError).ToHaveTextAsync(AttachBoth, new() { Timeout = 5000 });
        await Expect(Files.Choices).ToHaveCountAsync(0);
        await Expect(ConsentChosen).ToBeHiddenAsync();
        await Expect(PhotoChosen).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_the_photo_unticks_it_and_keeps_the_consent()
    {
        await OpenTheWoundRecord();
        await Files.Choose(SignedConsent);
        await Expect(ConsentChosen).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Files.Choose(WoundPhoto);
        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Files.Remove(WoundPhoto.Name);

        await Expect(PhotoChosen).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(ConsentChosen).ToBeVisibleAsync();
        await Expect(Files.Choices).ToHaveCountAsync(1);
        await Expect(FilesError).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task attaching_without_files_asks_for_the_consent_and_the_photo()
    {
        await OpenTheWoundRecord();

        await ClickWhenStable(AttachToRecord);

        await Expect(FilesError).ToHaveTextAsync(AttachBoth, new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Nothing attached yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task attaching_the_consent_and_the_photo_records_both_on_margarets_record()
    {
        await OpenTheWoundRecord();
        await Files.Choose(SignedConsent, WoundPhoto);
        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(AttachToRecord);

        await Expect(Status).ToHaveTextAsync(
            "Attached to Margaret Hale's record for the left heel wound: consent-form.pdf (23 bytes) and wound-photo.jpg (16 bytes).",
            new() { Timeout = 10000 });
        await Expect(FilesError).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_file_of_another_kind_is_refused_and_the_record_names_what_it_received()
    {
        await OpenTheWoundRecord();
        await Files.Choose(SignedConsent, FileOf("wound-video.mov", "MOV", "video/quicktime"));
        await Expect(Files.InvalidChoices).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await ClickWhenStable(AttachToRecord);

        await Expect(FilesError).ToHaveTextAsync(
            "Nothing was attached: wound-video.mov is not a PDF, JPG or PNG. Received: consent-form.pdf (23 bytes), wound-video.mov (3 bytes).",
            new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Nothing attached yet.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task a_photo_larger_than_five_megabytes_is_refused_and_the_record_names_what_it_received()
    {
        await OpenTheWoundRecord();
        await Files.Choose(new FilePayload { Name = "wound-photo.jpg", MimeType = "image/jpeg", Buffer = new byte[6 * 1024 * 1024] });
        await Expect(PhotoChosen).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(AttachToRecord);

        await Expect(FilesError).ToHaveTextAsync(
            "Nothing was attached: wound-photo.jpg is larger than 5 MB. Received: wound-photo.jpg (6.0 MB).",
            new() { Timeout = 15000 });
        await Expect(Status).ToHaveTextAsync("Nothing attached yet.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
