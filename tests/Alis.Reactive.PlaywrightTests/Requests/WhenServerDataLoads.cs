namespace Alis.Reactive.PlaywrightTests.Requests;

[TestFixture]
public class WhenServerDataLoads : PlaywrightTestBase
{
    [Test]
    public async Task PageShowsResidentsAfterLoad()
    {
        await NavigateTo("/Sandbox/Http");

        // DomReady fires GET /Sandbox/Http/Residents → result text updates
        var result = Page.Locator("#load-result");
        await Expect(result).ToContainTextAsync("Loaded 2 residents", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task SuccessMessageAppearsAfterSave()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click save button → POST with name "John Doe" → success
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        var result = Page.Locator("#save-result");
        await Expect(result).ToContainTextAsync("Save succeeded", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task BothDataSetsAppearAfterChainedLoad()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click chain button → GET Residents → Chained GET Facilities
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Chain" }).ClickAsync();

        await Expect(Page.Locator("#chain-residents")).ToContainTextAsync("Residents loaded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-facilities")).ToContainTextAsync("Facilities loaded", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task AllDataAppearsAfterParallelLoad()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click parallel button → both fire concurrently
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        await Expect(Page.Locator("#parallel-residents")).ToContainTextAsync("Residents loaded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-facilities")).ToContainTextAsync("Facilities loaded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-all")).ToContainTextAsync("All parallel requests completed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task HttpPageRendersWithCorrectTitle()
    {
        await NavigateTo("/Sandbox/Http");
        await Expect(Page).ToHaveTitleAsync("HTTP Requests — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task PutUpdateShowsResult()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click PUT button → sends PUT with JSON body → success
        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }).ClickAsync();

        var result = Page.Locator("#put-result");
        await Expect(result).ToContainTextAsync("PUT succeeded", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task DeleteWithConfirmShowsResult()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click DELETE button → confirm dialog appears
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Resident #42" }).ClickAsync();

        // Wait for confirm dialog and click OK
        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await okButton.ClickAsync();

        // Verify DELETE result
        var result = Page.Locator("#delete-result");
        await Expect(result).ToContainTextAsync("DELETE succeeded", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task FormDataPostShowsReceivedFields()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click Submit Form button → POST FormData with IncludeAll → success
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Form" }).ClickAsync();

        var result = Page.Locator("#formdata-result");
        await Expect(result).ToContainTextAsync("FormData sent", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task SearchShowsResults()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click search button → GET with ?q=John → success
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for 'John'" }).ClickAsync();

        var result = Page.Locator("#search-result");
        await Expect(result).ToContainTextAsync("Search results loaded", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ValidationErrorRoutesTo422Handler()
    {
        await NavigateTo("/Sandbox/Http");

        // Wait for DomReady to finish
        await Expect(Page.Locator("#load-result")).ToContainTextAsync("Loaded", new() { Timeout = 5000 });

        // Click validate button → POST empty data → 422 validation errors
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate (will fail)" }).ClickAsync();

        var result = Page.Locator("#multi-err-result");
        await Expect(result).ToContainTextAsync("422", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));

        // 422 is expected — browser logs it as a network error
        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task NativeActionLinkDeleteWithConfirmDoesNotDeleteWhenCancelled()
    {
        await NavigateTo("/Sandbox/Http");

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        await Page.GetByTestId("native-action-link-42").ClickAsync();

        var cancelButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "Cancel" });
        await Expect(cancelButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await cancelButton.ClickAsync();

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task NativeActionLinkDeleteWithConfirmDeletesAndRefreshesGridWhenConfirmed()
    {
        await NavigateTo("/Sandbox/Http");

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        await Page.GetByTestId("native-action-link-42").ClickAsync();

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await okButton.ClickAsync();

        await Expect(Page.Locator("#native-action-link-status"))
            .ToContainTextAsync("Deleted resident #42", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-41"))
            .ToContainTextAsync("Resident #41", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-43"))
            .ToContainTextAsync("Resident #43", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task StandaloneNativeActionLinkLoadsItsOwnSuccessTarget()
    {
        await NavigateTo("/Sandbox/Http");

        await Page.GetByTestId("standalone-native-action-link").ClickAsync();

        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToContainTextAsync("Standalone NativeActionLink succeeded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToContainTextAsync("Standalone NativeActionLink response loaded.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
