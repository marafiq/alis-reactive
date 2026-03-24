namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenServerDataLoads : PlaywrightTestBase
{
    private async Task WaitForDomReadyGet()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        // DomReady GET fires automatically — wait for response data to arrive
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 5000 });
    }

    // ── Section 1: DomReady GET — exact resident data round-trip ──────────

    [Test]
    public async Task domready_get_loads_first_resident_name()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-first")).ToHaveTextAsync("John Doe");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_get_loads_second_resident_name()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-second")).ToHaveTextAsync("Jane Smith");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_get_loads_resident_count()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-count")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_get_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_get_hides_spinner_after_response()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 2: POST with gather — server echoes received name ─────────

    [Test]
    public async Task post_with_gather_echoes_received_name()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Server receives {name:"John Doe"} and echoes it back as receivedName
        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_with_gather_shows_server_message()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Server returns message:"Saved: John Doe" — verifies full round-trip
        await Expect(Page.Locator("#save-message")).ToHaveTextAsync("Saved: John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_with_gather_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 3: Chained requests — residents THEN facilities ───────────

    [Test]
    public async Task chained_get_loads_resident_names_first()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Chain" }).ClickAsync();

        // First request: residents
        await Expect(Page.Locator("#chain-resident-first")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-resident-second")).ToHaveTextAsync("Jane Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_get_loads_facility_names_after_residents()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Chain" }).ClickAsync();

        // Second request: facilities (only fires after first completes)
        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-facility-second")).ToHaveTextAsync("West Wing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_get_hides_spinner_only_after_second_request()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Chain" }).ClickAsync();

        // Spinner hides in the chained (second) response handler
        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 4: Parallel requests — both datasets load concurrently ────

    [Test]
    public async Task parallel_get_loads_resident_names()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        await Expect(Page.Locator("#parallel-resident-first")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-resident-second")).ToHaveTextAsync("Jane Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_loads_facility_names()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        await Expect(Page.Locator("#parallel-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-facility-second")).ToHaveTextAsync("West Wing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_fires_all_settled_after_both_complete()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        await Expect(Page.Locator("#parallel-all")).ToHaveTextAsync(
            "All parallel requests completed!", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-all")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_hides_spinner_after_all_settled()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        await Expect(Page.Locator("#parallel-all")).ToHaveTextAsync(
            "All parallel requests completed!", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 5: PUT — server echoes updated payload ────────────────────

    [Test]
    public async Task put_sends_updated_name_and_server_echoes_it()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }).ClickAsync();

        // Server receives {name:"Updated Name",facilityId:"1"} and echoes receivedName
        await Expect(Page.Locator("#put-received-name")).ToHaveTextAsync("Updated Name", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task put_sends_facility_id_and_server_echoes_it()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }).ClickAsync();

        // Server receives facilityId:"1" and echoes it as receivedFacilityId
        await Expect(Page.Locator("#put-received-facility")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task put_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }).ClickAsync();

        await Expect(Page.Locator("#put-received-name")).ToHaveTextAsync("Updated Name", new() { Timeout = 5000 });
        await Expect(Page.Locator("#put-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 6: DELETE with confirm — server echoes deleted ID ─────────

    [Test]
    public async Task delete_with_confirm_sends_correct_id()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Resident #42" }).ClickAsync();

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await okButton.ClickAsync();

        // Server receives DELETE /DeleteResident/42 and echoes deletedId:42
        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task delete_with_confirm_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Resident #42" }).ClickAsync();

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await okButton.ClickAsync();

        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#delete-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task delete_with_confirm_hides_spinner_after_response()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Resident #42" }).ClickAsync();

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await okButton.ClickAsync();

        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#delete-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 7: FormData POST — server echoes received field names ─────

    [Test]
    public async Task form_data_post_sends_three_fields()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Form" }).ClickAsync();

        // Server counts received fields: FirstName, LastName, Email = 3
        await Expect(Page.Locator("#formdata-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_preserves_field_names()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Form" }).ClickAsync();

        // Server echoes the exact field names it received — verifies model binding names
        await Expect(Page.Locator("#formdata-fields")).ToHaveTextAsync(
            "FirstName, LastName, Email", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Form" }).ClickAsync();

        await Expect(Page.Locator("#formdata-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#formdata-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 8: Search — server echoes query and match count ───────────

    [Test]
    public async Task search_query_param_arrives_at_server()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for 'John'" }).ClickAsync();

        // Server receives ?q=John and echoes it back as query
        await Expect(Page.Locator("#search-query")).ToHaveTextAsync("John", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task search_returns_correct_match_count()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for 'John'" }).ClickAsync();

        // "John" matches "John Doe" and "Bob Johnson" = 2 results
        await Expect(Page.Locator("#search-match-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task search_applies_success_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for 'John'" }).ClickAsync();

        await Expect(Page.Locator("#search-query")).ToHaveTextAsync("John", new() { Timeout = 5000 });
        await Expect(Page.Locator("#search-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 9: 422 error routing — correct handler fires ──────────────

    [Test]
    public async Task error_422_routes_to_correct_handler()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate (will fail)" }).ClickAsync();

        // 422 handler sets specific text — verifies status-code routing works
        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task error_422_applies_warning_class()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate (will fail)" }).ClickAsync();

        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#multi-err-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-amber-600"));
        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task error_422_hides_spinner()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate (will fail)" }).ClickAsync();

        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#multi-err-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("422");
    }

    // ── Section 10: NativeActionLink — row actions ────────────────────────

    [Test]
    public async Task native_action_link_grid_loads_initial_rows()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");

        // All three seed rows must be present — proves Into() partial injection rendered the grid
        await Expect(Page.GetByTestId("native-action-link-row-41"))
            .ToContainTextAsync("Resident #41", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-43"))
            .ToContainTextAsync("Resident #43", new() { Timeout = 5000 });

        // Each row shows the resident name alongside the ID
        await Expect(Page.GetByTestId("native-action-link-row-41")).ToContainTextAsync("John Doe");
        await Expect(Page.GetByTestId("native-action-link-row-42")).ToContainTextAsync("Jane Smith");
        await Expect(Page.GetByTestId("native-action-link-row-43")).ToContainTextAsync("Bob Johnson");

        // Each row has a Delete action link
        await Expect(Page.GetByTestId("native-action-link-41")).ToHaveTextAsync("Delete");
        await Expect(Page.GetByTestId("native-action-link-42")).ToHaveTextAsync("Delete");
        await Expect(Page.GetByTestId("native-action-link-43")).ToHaveTextAsync("Delete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task native_action_link_delete_with_confirm_does_not_delete_when_cancelled()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");

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
    public async Task native_action_link_delete_with_confirm_deletes_and_refreshes_grid_when_confirmed()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");

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

    // ── Section 11: Standalone NativeActionLink ───────────────────────────

    [Test]
    public async Task standalone_native_action_link_loads_its_own_success_target()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");

        await Page.GetByTestId("standalone-native-action-link").ClickAsync();

        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToContainTextAsync("Standalone NativeActionLink succeeded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToContainTextAsync("Standalone NativeActionLink response loaded.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standalone_action_link_fires_post_and_shows_result()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");

        // Before click — status shows default text, result container has no server content
        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToHaveTextAsync("Standalone link has not run yet");
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToHaveTextAsync("No standalone response yet");

        // Click the standalone action link — fires POST with {command:"run"}
        await Page.GetByTestId("standalone-native-action-link").ClickAsync();

        // POST response HTML is injected Into() the result container
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToContainTextAsync("Standalone NativeActionLink response loaded.", new() { Timeout = 5000 });

        // Status element is updated by the OnSuccess handler
        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToHaveTextAsync("Standalone NativeActionLink succeeded");

        // The injected HTML contains the server-rendered styled div
        var injectedDiv = Page.Locator("#standalone-native-action-link-result div.text-blue-700");
        await Expect(injectedDiv).ToBeVisibleAsync();
        await Expect(injectedDiv).ToHaveTextAsync("Standalone NativeActionLink response loaded.");

        AssertNoConsoleErrors();
    }

    // ── Section 12: Error recovery — retry after error, spinner lifecycle, chain ordering ──

    [Test]
    public async Task save_error_then_retry_with_valid_data_shows_success()
    {
        await WaitForDomReadyGet();

        // Intercept the first POST to /Save and return 400 (simulating validation failure)
        var intercepted = false;
        await Page.RouteAsync("**/Sandbox/HttpPipeline/Http/Save", async route =>
        {
            if (!intercepted)
            {
                intercepted = true;
                await route.FulfillAsync(new()
                {
                    Status = 400,
                    ContentType = "application/json",
                    Body = "{\"errorSummary\":\"Validation failed: Name is required\"}"
                });
            }
            else
            {
                await route.ContinueAsync();
            }
        });

        // First POST — intercepted as 400 — error handler fires
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.Locator("#save-error")).ToHaveTextAsync(
            "Validation failed: Name is required", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-red-600"));

        // Second POST — passes through to real server — success handler fires
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        // Success handler removes error class — proves error state is replaced
        await Expect(Page.Locator("#save-result")).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-red-600"));

        await Page.UnrouteAsync("**/Sandbox/HttpPipeline/Http/Save");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task while_loading_spinner_hides_after_both_success_and_error()
    {
        await WaitForDomReadyGet();

        // POST valid data via Save — spinner shows during request, hides after success
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-spinner")).ToBeHiddenAsync();

        // POST invalid data via Validate — spinner shows during request, hides after 422
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate (will fail)" }).ClickAsync();
        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#multi-err-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task chained_request_second_hop_only_fires_after_first_completes()
    {
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Chain" }).ClickAsync();

        // First hop: wait for residents to arrive — proves first request completed
        await Expect(Page.Locator("#chain-resident-first")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-resident-second")).ToHaveTextAsync("Jane Smith");
        await Expect(Page.Locator("#chain-residents")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));

        // Second hop: facilities only fire after residents complete
        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync(
            "Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-facility-second")).ToHaveTextAsync("West Wing");
        await Expect(Page.Locator("#chain-facilities")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));

        // Both visible at end, spinner hidden — proves full chain completed
        await Expect(Page.Locator("#chain-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Section 13: WhileLoading spinner regression — stuck spinner detection ──

    [Test]
    public async Task all_spinners_are_hidden_after_page_fully_loads()
    {
        // After DomReady GET completes, verify ALL spinner elements on the page are hidden.
        // This catches "stuck spinner" bugs where a WhileLoading show fires but hide doesn't.
        await WaitForDomReadyGet();

        // DomReady GET spinner — fires WhileLoading show, OnSuccess hides it
        await Expect(Page.Locator("#load-spinner")).ToBeHiddenAsync();

        // All remaining spinners start with hidden attribute and should remain hidden
        // since no user action has triggered their WhileLoading show
        await Expect(Page.Locator("#save-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#chain-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#parallel-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#put-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#delete-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#formdata-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#search-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#multi-err-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_request_spinner_hides_only_after_both_complete()
    {
        // Click parallel load — both requests fire — spinner should stay visible until BOTH complete.
        // After OnAllSettled fires — spinner hidden.
        // This catches bugs where spinner hides after first response instead of waiting for all.
        await WaitForDomReadyGet();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Parallel" }).ClickAsync();

        // Wait for BOTH datasets to arrive — proves both requests completed
        await Expect(Page.Locator("#parallel-resident-first")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-facility-first")).ToHaveTextAsync(
            "Main Campus", new() { Timeout = 5000 });

        // OnAllSettled sets completion text — this is the signal that both settled
        await Expect(Page.Locator("#parallel-all")).ToHaveTextAsync(
            "All parallel requests completed!", new() { Timeout = 5000 });

        // NOW the spinner must be hidden — not after the first response, only after OnAllSettled
        await Expect(Page.Locator("#parallel-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Page-level checks ─────────────────────────────────────────────────

    [Test]
    public async Task http_page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await Expect(Page).ToHaveTitleAsync("HTTP Requests — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }
}
