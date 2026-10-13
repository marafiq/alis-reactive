namespace Alis.Reactive.PlaywrightTests.Components.Fusion.FileUpload;

// Files are chosen on the uploader's file input, as the file dialog does; Syncfusion populates
// filesData[].rawFile, the uploader declares that path as its value, so gather sends the files.
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

    // The documents a coordinator picks in the uploader's file dialog.
    private Task ChooseFiles(params (string Name, string Content, string MimeType)[] files) =>
        Page.Locator($"#{DocumentsInputId}").SetInputFilesAsync(files.Select(file => new FilePayload
        {
            Name = file.Name,
            MimeType = file.MimeType,
            Buffer = System.Text.Encoding.UTF8.GetBytes(file.Content),
        }));

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
    public async Task choosing_documents_prompts_to_upload_them()
    {
        await NavigateAndBoot();
        var prompt = Page.Locator("#documents-ready");
        await Expect(prompt).ToBeHiddenAsync();

        await ChooseFiles(
            ("care-plan.pdf", "%PDF-mock", "application/pdf"),
            ("consent-form.pdf", "%PDF-mock", "application/pdf"));

        await Expect(prompt).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(prompt).ToHaveTextAsync("Documents chosen: 2. Press Upload to add them to the resident's record.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task uploading_two_documents_adds_both_to_the_record()
    {
        await NavigateAndBoot();

        await ChooseFiles(
            ("medical-record.txt", "Patient vitals: BP 120/80", "text/plain"),
            ("consent-form.pdf", "%PDF-mock", "application/pdf"));

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-count")).ToHaveTextAsync("2", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_uploaded_photo_is_listed_by_name()
    {
        await NavigateAndBoot();

        await ChooseFiles(("photo.jpg", "fake-jpeg", "image/jpeg"));

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-files")).ToContainTextAsync("photo.jpg", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task uploading_intake_documents_keeps_the_residents_name_with_them()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{GeneratedTypeScope}ResidentName");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("Eleanor Vance");

        await ChooseFiles(("intake-form.txt", "Intake data", "text/plain"));

        await Page.Locator("#upload-btn").ClickAsync();

        await Expect(Page.Locator("#echo-name"))
            .ToHaveTextAsync("Eleanor Vance", new() { Timeout = 10000 });

        await Expect(Page.Locator("#echo-count")).ToHaveTextAsync("1");
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
