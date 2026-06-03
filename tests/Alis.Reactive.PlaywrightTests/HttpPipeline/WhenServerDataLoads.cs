namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenServerDataLoads : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task WaitForDomReadyGet()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await WaitForTraceMessage("booted", 10000);
        // DomReady GET fires automatically; wait for response data before interacting.
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

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

    [Test]
    public async Task post_with_gather_echoes_received_name()
    {
        await WaitForDomReadyGet();

        await ClickButton("Save");

        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_with_gather_shows_server_message()
    {
        await WaitForDomReadyGet();

        await ClickButton("Save");

        await Expect(Page.Locator("#save-message")).ToHaveTextAsync("Saved: John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_with_gather_applies_success_class()
    {
        await WaitForDomReadyGet();

        await ClickButton("Save");

        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_get_loads_resident_names_first()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Chain");

        await Expect(Page.Locator("#chain-resident-first")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-resident-second")).ToHaveTextAsync("Jane Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_get_loads_facility_names_after_residents()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Chain");

        // Facilities load only after the residents request completes.
        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-facility-second")).ToHaveTextAsync("West Wing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_get_hides_spinner_only_after_second_request()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Chain");

        // Spinner hides in the chained second response route.
        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_loads_resident_names()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Parallel");

        await Expect(Page.Locator("#parallel-resident-first")).ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-resident-second")).ToHaveTextAsync("Jane Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_loads_facility_names()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Parallel");

        await Expect(Page.Locator("#parallel-facility-first")).ToHaveTextAsync("Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-facility-second")).ToHaveTextAsync("West Wing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_get_fires_all_settled_after_both_complete()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Parallel");

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

        await ClickButton("Load Parallel");

        await Expect(Page.Locator("#parallel-all")).ToHaveTextAsync(
            "All parallel requests completed!", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task put_sends_updated_name_and_server_echoes_it()
    {
        await WaitForDomReadyGet();

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }));

        await Expect(Page.Locator("#put-received-name")).ToHaveTextAsync("Updated Name", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task put_sends_facility_id_and_server_echoes_it()
    {
        await WaitForDomReadyGet();

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }));

        await Expect(Page.Locator("#put-received-facility")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task put_applies_success_class()
    {
        await WaitForDomReadyGet();

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Update Resident" }));

        await Expect(Page.Locator("#put-received-name")).ToHaveTextAsync("Updated Name", new() { Timeout = 5000 });
        await Expect(Page.Locator("#put-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task delete_with_confirm_sends_correct_id()
    {
        await WaitForDomReadyGet();

        await ClickButton("Delete Resident #42");

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await ClickWhenStable(okButton);

        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task delete_with_confirm_applies_success_class()
    {
        await WaitForDomReadyGet();

        await ClickButton("Delete Resident #42");

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await ClickWhenStable(okButton);

        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#delete-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task delete_with_confirm_hides_spinner_after_response()
    {
        await WaitForDomReadyGet();

        await ClickButton("Delete Resident #42");

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 3000 });
        await ClickWhenStable(okButton);

        await Expect(Page.Locator("#delete-id")).ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#delete-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_sends_three_fields()
    {
        await WaitForDomReadyGet();

        await ClickButton("Submit Form");

        await Expect(Page.Locator("#formdata-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_preserves_field_names()
    {
        await WaitForDomReadyGet();

        await ClickButton("Submit Form");

        // Field names verify the model-binding names sent in the FormData payload.
        await Expect(Page.Locator("#formdata-fields")).ToHaveTextAsync(
            "FirstName, LastName, Email", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_applies_success_class()
    {
        await WaitForDomReadyGet();

        await ClickButton("Submit Form");

        await Expect(Page.Locator("#formdata-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#formdata-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task search_query_param_arrives_at_server()
    {
        await WaitForDomReadyGet();

        await ClickButton("Search for 'John'");

        await Expect(Page.Locator("#search-query")).ToHaveTextAsync("John", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task search_returns_correct_match_count()
    {
        await WaitForDomReadyGet();

        await ClickButton("Search for 'John'");

        // John matches John Doe and Bob Johnson in the sandbox data.
        await Expect(Page.Locator("#search-match-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task search_applies_success_class()
    {
        await WaitForDomReadyGet();

        await ClickButton("Search for 'John'");

        await Expect(Page.Locator("#search-query")).ToHaveTextAsync("John", new() { Timeout = 5000 });
        await Expect(Page.Locator("#search-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task error_422_routes_to_correct_response_route()
    {
        await WaitForDomReadyGet();

        await ClickButton("Validate (will fail)");

        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task error_422_applies_warning_class()
    {
        await WaitForDomReadyGet();

        await ClickButton("Validate (will fail)");

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

        await ClickButton("Validate (will fail)");

        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#multi-err-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task native_action_link_grid_loads_initial_rows()
    {
        await WaitForDomReadyGet();

        // Seed rows prove Into() partial injection rendered the grid.
        await Expect(Page.GetByTestId("native-action-link-row-41"))
            .ToContainTextAsync("Resident #41", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });
        await Expect(Page.GetByTestId("native-action-link-row-43"))
            .ToContainTextAsync("Resident #43", new() { Timeout = 5000 });

        await Expect(Page.GetByTestId("native-action-link-row-41")).ToContainTextAsync("John Doe");
        await Expect(Page.GetByTestId("native-action-link-row-42")).ToContainTextAsync("Jane Smith");
        await Expect(Page.GetByTestId("native-action-link-row-43")).ToContainTextAsync("Bob Johnson");

        await Expect(Page.GetByTestId("native-action-link-41")).ToHaveTextAsync("Delete");
        await Expect(Page.GetByTestId("native-action-link-42")).ToHaveTextAsync("Delete");
        await Expect(Page.GetByTestId("native-action-link-43")).ToHaveTextAsync("Delete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task native_action_link_delete_with_confirm_does_not_delete_when_cancelled()
    {
        await WaitForDomReadyGet();

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        await ClickWhenStable(Page.GetByTestId("native-action-link-42"));

        var cancelButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "Cancel" });
        await Expect(cancelButton).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ClickWhenStable(cancelButton);

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task native_action_link_delete_with_confirm_deletes_and_refreshes_grid_when_confirmed()
    {
        await WaitForDomReadyGet();

        await Expect(Page.GetByTestId("native-action-link-row-42"))
            .ToContainTextAsync("Resident #42", new() { Timeout = 5000 });

        await ClickWhenStable(Page.GetByTestId("native-action-link-42"));

        var okButton = Page.Locator("#alisConfirmDialog").GetByRole(AriaRole.Button, new() { Name = "OK" });
        await Expect(okButton).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ClickWhenStable(okButton);

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
    public async Task standalone_native_action_link_loads_its_own_success_target()
    {
        await WaitForDomReadyGet();

        await ClickWhenStable(Page.GetByTestId("standalone-native-action-link"));

        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToContainTextAsync("Standalone NativeActionLink succeeded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToContainTextAsync("Standalone NativeActionLink response loaded.", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standalone_action_link_fires_post_and_shows_result()
    {
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToHaveTextAsync("Standalone link has not run yet");
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToHaveTextAsync("No standalone response yet");

        await ClickWhenStable(Page.GetByTestId("standalone-native-action-link"));

        // POST response HTML is injected into the result container.
        await Expect(Page.Locator("#standalone-native-action-link-result"))
            .ToContainTextAsync("Standalone NativeActionLink response loaded.", new() { Timeout = 5000 });

        await Expect(Page.Locator("#standalone-native-action-link-status"))
            .ToHaveTextAsync("Standalone NativeActionLink succeeded");

        var injectedDiv = Page.Locator("#standalone-native-action-link-result div.text-blue-700");
        await Expect(injectedDiv).ToBeVisibleAsync();
        await Expect(injectedDiv).ToHaveTextAsync("Standalone NativeActionLink response loaded.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task save_error_then_retry_with_valid_data_shows_success()
    {
        await WaitForDomReadyGet();

        // Intercept only the first save to prove retry replaces the error state.
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

        await ClickButton("Save");
        await Expect(Page.Locator("#save-error")).ToHaveTextAsync(
            "Validation failed: Name is required", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-red-600"));

        await ClickButton("Save");
        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        // Success route removes the previous error class.
        await Expect(Page.Locator("#save-result")).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-red-600"));

        await Page.UnrouteAsync("**/Sandbox/HttpPipeline/Http/Save");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task while_loading_spinner_hides_after_both_success_and_error()
    {
        await WaitForDomReadyGet();

        await ClickButton("Save");
        await Expect(Page.Locator("#save-received-name")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#save-spinner")).ToBeHiddenAsync();

        await ClickButton("Validate (will fail)");
        await Expect(Page.Locator("#multi-err-summary")).ToHaveTextAsync(
            "422 — 2 validation error(s): Name, FacilityId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#multi-err-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("422");
    }

    [Test]
    public async Task chained_request_second_hop_only_fires_after_first_completes()
    {
        await WaitForDomReadyGet();

        await ClickButton("Load Chain");

        // Residents must complete before facilities can load.
        await Expect(Page.Locator("#chain-resident-first")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-resident-second")).ToHaveTextAsync("Jane Smith");
        await Expect(Page.Locator("#chain-residents")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));

        await Expect(Page.Locator("#chain-facility-first")).ToHaveTextAsync(
            "Main Campus", new() { Timeout = 5000 });
        await Expect(Page.Locator("#chain-facility-second")).ToHaveTextAsync("West Wing");
        await Expect(Page.Locator("#chain-facilities")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));

        await Expect(Page.Locator("#chain-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_spinners_are_hidden_after_page_fully_loads()
    {
        // After DomReady GET completes, every spinner should be hidden.
        // This catches WhileLoading show/hide mismatches.
        await WaitForDomReadyGet();

        await Expect(Page.Locator("#load-spinner")).ToBeHiddenAsync();

        // Remaining spinners have not been triggered by user actions.
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
        // Parallel spinner must wait for OnAllSettled, not the first response.
        await WaitForDomReadyGet();

        await ClickButton("Load Parallel");

        // Both datasets must arrive before the all-settled route can run.
        await Expect(Page.Locator("#parallel-resident-first")).ToHaveTextAsync(
            "John Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#parallel-facility-first")).ToHaveTextAsync(
            "Main Campus", new() { Timeout = 5000 });

        // OnAllSettled sets completion text; this is the settled signal.
        await Expect(Page.Locator("#parallel-all")).ToHaveTextAsync(
            "All parallel requests completed!", new() { Timeout = 5000 });

        await Expect(Page.Locator("#parallel-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task http_page_renders_with_correct_title()
    {
        await WaitForDomReadyGet();
        await Expect(Page).ToHaveTitleAsync("HTTP Requests — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }
}
