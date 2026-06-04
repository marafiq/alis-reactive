namespace Alis.Reactive.PlaywrightTests.Components.Fusion.FileUpload;

/// <summary>
/// Proves <c>FusionFileUpload</c> form-mode gather, multipart <c>FormData</c> payloads,
/// and server echo behavior for resident document uploads.
/// </summary>
/// <remarks>
/// File injection uses DataTransfer on the native input so Syncfusion populates
/// <c>filesData[].rawFile</c>; gather then reads <c>filesData</c> for FormData.
/// </remarks>
[TestFixture]
public class WhenFileUploaded : PlaywrightTestBase
{
    private const string PagePath = "/Sandbox/Components/FileUpload";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FileUploadModel__";
    private const string DocumentsInputId = GeneratedTypeScope + "Documents";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(PagePath, ".e-upload");
    }

    private async Task SetFilesThroughDataTransfer(params (string Name, string Content, string MimeType)[] files)
    {
        var fileSpecs = files.Select(f => new { f.Name, f.Content, f.MimeType }).ToArray();
        await Page.EvaluateAsync(
            @"(args) => {
                const { elementId, files } = args;
                const el = document.getElementById(elementId);
                const dt = new DataTransfer();
                for (const f of files) {
                    dt.items.add(new File([f.Content], f.Name, { type: f.MimeType }));
                }
                el.files = dt.files;
                el.dispatchEvent(new Event('change', { bubbles: true }));
            }",
            new { elementId = DocumentsInputId, files = fileSpecs });
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionFileUpload — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("form-data"),
            "Plan must contain form-data content type for file upload POST");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task file_picker_renders_syncfusion_uploader()
    {
        await NavigateAndBoot();
        var uploader = Page.Locator(".e-upload");
        await Expect(uploader).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task file_input_element_exists()
    {
        await NavigateAndBoot();
        var fileInput = Page.Locator($"#{DocumentsInputId}");
        await Expect(fileInput).ToBeAttachedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_files_and_submitting_sends_to_server()
    {
        await NavigateAndBoot();

        await SetFilesThroughDataTransfer(
            ("medical-record.txt", "Patient vitals: BP 120/80", "text/plain"),
            ("consent-form.pdf", "%PDF-mock", "application/pdf"));

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

        await SetFilesThroughDataTransfer(("photo.jpg", "fake-jpeg", "image/jpeg"));

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

        var nameInput = Page.Locator($"#{GeneratedTypeScope}ResidentName");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("Eleanor Vance");

        await SetFilesThroughDataTransfer(("intake-form.txt", "Intake data", "text/plain"));

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-name"))
            .ToHaveTextAsync("Eleanor Vance", new() { Timeout = 10000 });

        var count = await Page.Locator("#echo-count").TextContentAsync();
        Assert.That(count, Is.EqualTo("1"),
            "Server should receive 1 file alongside the scalar field");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        await NavigateAndBoot();
        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True);
        AssertNoConsoleErrors();
    }
}
