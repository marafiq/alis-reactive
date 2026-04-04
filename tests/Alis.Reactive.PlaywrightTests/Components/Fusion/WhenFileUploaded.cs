using Alis.Reactive.PlaywrightTests.Support.Controls;
using System.Text;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenFileUploaded : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FileUpload";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FileUploadModel__";
    private const string DocumentsId = Scope + "Documents";
    private FileUploadLocator Documents => new(Page, DocumentsId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, ".e-upload");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionFileUpload — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task file_picker_renders_syncfusion_uploader()
    {
        await NavigateAndBoot();
        await Expect(Documents.Wrapper).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task file_input_element_exists()
    {
        await NavigateAndBoot();
        await Expect(Documents.Input).ToBeAttachedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_files_and_submitting_sends_to_server()
    {
        await NavigateAndBoot();

        await Documents.AttachFiles(
            new FilePayload { Name = "medical-record.txt", Buffer = Encoding.UTF8.GetBytes("Patient vitals: BP 120/80"), MimeType = "text/plain" },
            new FilePayload { Name = "consent-form.pdf", Buffer = Encoding.UTF8.GetBytes("%PDF-mock"), MimeType = "application/pdf" });

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-count"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 10000 });

        var count = await Page.Locator("#echo-count").TextContentAsync();
        Assert.That(count, Is.EqualTo("2"),
            "Server should receive exactly 2 files via FormData POST");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_echoes_file_names()
    {
        await NavigateAndBoot();

        await Documents.AttachFiles(
            new FilePayload { Name = "photo.jpg", Buffer = Encoding.UTF8.GetBytes("fake-jpeg"), MimeType = "image/jpeg" });

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-files"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 10000 });

        var files = await Page.Locator("#echo-files").TextContentAsync();
        Assert.That(files, Does.Contain("photo.jpg"),
            "Server should echo back the uploaded file name");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task files_coexist_with_scalar_fields()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{Scope}ResidentName");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("Eleanor Vance");

        await Documents.AttachFiles(
            new FilePayload { Name = "intake-form.txt", Buffer = Encoding.UTF8.GetBytes("Intake data"), MimeType = "text/plain" });

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-name"))
            .ToHaveTextAsync("Eleanor Vance", new() { Timeout = 10000 });

        var count = await Page.Locator("#echo-count").TextContentAsync();
        Assert.That(count, Is.EqualTo("1"),
            "Server should receive 1 file alongside the scalar field");
        AssertNoConsoleErrors();
    }
}
