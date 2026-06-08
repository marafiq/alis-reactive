using System.Text.Json;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridDirectory : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Directory";

    private async Task NavigateDirectory()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#directory-status"))
            .ToHaveTextAsync("loaded first page", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task components_index_nests_grid_and_hub_lists_directory_and_editing_demos()
    {
        // Organic hierarchy: the Components index surfaces a single Grid card that opens the
        // Grid hub, and the hub lists each scenario (directory, editing, ...) as its own demo.
        // Navigation is driven by clicking visible cards/labels, never hard-coded URLs, so it
        // survives route refactoring.
        await NavigateTo("/Sandbox/Components");
        await ClickWhenStable(Page.Locator("a:has(h3:text-is('Grid'))"));

        await Expect(Page.Locator("a").Filter(new() { HasText = "Resident directory" }).First)
            .ToBeVisibleAsync();
        await Expect(Page.Locator("a").Filter(new() { HasText = "Editing workflows" }).First)
            .ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task public_methods_drive_server_paging_sorting_search_filtering_and_grouping()
    {
        await NavigateDirectory();

        await ClickWhenStable(Page.Locator("#grid-page-2"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("paging", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("8", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-sort-risk"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("sorting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("riskLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-direction")).ToHaveTextAsync("Descending", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-search-memory"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("searching", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-search"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-filter-north"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("filtering", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-filters"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-group-care"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("grouping", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync(
            "240 residents grouped by care level",
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-grouping"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sort_by_method_sends_typed_sorted_payload_and_refreshes_grid()
    {
        await NavigateDirectory();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-sort-risk")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionTarget", out _), Is.False);

        var sorted = root.GetProperty("sorted");
        Assert.That(sorted.GetArrayLength(), Is.EqualTo(1));
        Assert.That(sorted[0].GetProperty("name").GetString(), Is.EqualTo("riskLevel"));
        Assert.That(sorted[0].GetProperty("direction").GetString(), Is.EqualTo("descending"));

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("sortColumn called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("sorting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("riskLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-direction")).ToHaveTextAsync("Descending", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Grace Bennett", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First.Locator(".e-rowcell").Nth(6))
            .ToHaveTextAsync("Moderate", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_sorting_method_clears_active_sort_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridAction).GetProperty("Target"),
            Is.Null);

        await NavigateDirectory();

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-sort-risk")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("sortColumn called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("sorting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("riskLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-direction")).ToHaveTextAsync("Descending", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Grace Bennett", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First.Locator(".e-rowcell").Nth(6))
            .ToHaveTextAsync("Moderate", new() { Timeout = 10000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-clear-sorting")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionColumnName", out _), Is.False);
        Assert.That(root.TryGetProperty("actionDirection", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("actionTarget", out _), Is.False);

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("sorting cleared", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("sorting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-cancel")).Not.ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).Not.ToHaveTextAsync("riskLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-direction")).Not.ToHaveTextAsync("Descending", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First.Locator(".e-rowcell").Nth(6))
            .ToHaveTextAsync("Low", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_method_sends_typed_where_payload_and_refreshes_grid()
    {
        await NavigateDirectory();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-filter-north")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("dataSource", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("isLazyLoad", out _), Is.False);
        Assert.That(root.TryGetProperty("onDemandGroupInfo", out _), Is.False);
        Assert.That(root.TryGetProperty("select", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("table", out _), Is.False);

        var where = root.GetProperty("where");
        Assert.That(where.GetArrayLength(), Is.EqualTo(1));
        var complex = where[0];
        Assert.That(complex.GetProperty("condition").GetString(), Is.EqualTo("and"));
        Assert.That(complex.GetProperty("ignoreCase").GetBoolean(), Is.True);
        Assert.That(complex.GetProperty("ignoreAccent").GetBoolean(), Is.False);
        Assert.That(complex.GetProperty("isComplex").GetBoolean(), Is.True);
        Assert.That(complex.TryGetProperty("matchCase", out _), Is.False);
        Assert.That(complex.TryGetProperty("predicate", out _), Is.False);

        var predicates = complex.GetProperty("predicates");
        Assert.That(predicates.GetArrayLength(), Is.EqualTo(1));
        var predicate = predicates[0];
        Assert.That(predicate.GetProperty("field").GetString(), Is.EqualTo("wing"));
        Assert.That(predicate.GetProperty("operator").GetString(), Is.EqualTo("equal"));
        Assert.That(predicate.GetProperty("value").GetString(), Is.EqualTo("North"));
        Assert.That(predicate.GetProperty("isComplex").GetBoolean(), Is.False);
        Assert.That(predicate.TryGetProperty("matchCase", out _), Is.False);
        Assert.That(predicate.TryGetProperty("predicate", out _), Is.False);

        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("filtering", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        var rows = Page.Locator("#resident-directory-grid .e-row");
        await Expect(rows.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0));
        for (var index = 0; index < rowCount; index++)
        {
            var wingCell = rows.Nth(index).Locator(".e-rowcell").Nth(4);
            await Expect(wingCell).ToHaveTextAsync("North", new() { Timeout = 5000 });
        }

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_filtering_method_clears_active_filter_and_refreshes_grid()
    {
        await NavigateDirectory();

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-filter-north")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });
        var filteredRows = Page.Locator("#resident-directory-grid .e-row");
        await Expect(filteredRows.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        var filteredRowCount = await filteredRows.CountAsync();
        Assert.That(filteredRowCount, Is.GreaterThan(0));
        for (var index = 0; index < filteredRowCount; index++)
        {
            var wingCell = filteredRows.Nth(index).Locator(".e-rowcell").Nth(4);
            await Expect(wingCell).ToHaveTextAsync("North", new() { Timeout = 5000 });
        }

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-clear-filters")),
            IsClearFilteringRequest);
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionAction", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("actionColumns", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCurrentFilterObject", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCurrentFilteringColumn", out _), Is.False);
        Assert.That(root.TryGetProperty("actionType", out _), Is.False);

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("filters cleared", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("refresh", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid()
    {
        await NavigateDirectory();

        var input = Page.Locator("#wing_filterBarcell");
        await Expect(input).ToBeVisibleAsync(new() { Timeout = 10000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () =>
            {
                await input.PressSequentiallyAsync("N");
                await input.PressAsync("Enter");
            },
            IsFilterBarTypingRequest);
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("dataSource", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("isLazyLoad", out _), Is.False);
        Assert.That(root.TryGetProperty("onDemandGroupInfo", out _), Is.False);
        Assert.That(root.TryGetProperty("select", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("table", out _), Is.False);

        var where = root.GetProperty("where");
        Assert.That(where.GetArrayLength(), Is.EqualTo(1));
        var complex = where[0];
        Assert.That(complex.GetProperty("condition").GetString(), Is.EqualTo("and"));
        Assert.That(complex.GetProperty("ignoreCase").GetBoolean(), Is.True);
        Assert.That(complex.GetProperty("ignoreAccent").GetBoolean(), Is.False);
        Assert.That(complex.GetProperty("isComplex").GetBoolean(), Is.True);
        Assert.That(complex.TryGetProperty("matchCase", out _), Is.False);
        Assert.That(complex.TryGetProperty("predicate", out _), Is.False);

        var predicates = complex.GetProperty("predicates");
        Assert.That(predicates.GetArrayLength(), Is.EqualTo(1));
        var predicate = predicates[0];
        Assert.That(predicate.GetProperty("field").GetString(), Is.EqualTo("wing"));
        Assert.That(predicate.GetProperty("operator").GetString(), Is.EqualTo("startswith"));
        Assert.That(predicate.GetProperty("value").GetString(), Is.EqualTo("N"));
        Assert.That(predicate.GetProperty("isComplex").GetBoolean(), Is.False);
        Assert.That(predicate.TryGetProperty("matchCase", out _), Is.False);
        Assert.That(predicate.TryGetProperty("predicate", out _), Is.False);

        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("filtering", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        var rows = Page.Locator("#resident-directory-grid .e-row");
        await Expect(rows.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0));
        for (var index = 0; index < rowCount; index++)
        {
            var wingCell = rows.Nth(index).Locator(".e-rowcell").Nth(4);
            await Expect(wingCell).ToHaveTextAsync("North", new() { Timeout = 5000 });
        }

        AssertNoConsoleErrors();
    }

    private static bool IsFilterBarTypingRequest(IRequest observed)
    {
        if (!observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData") || observed.Method != "POST")
        {
            return false;
        }

        try
        {
            var requestBody = observed.PostData ?? "";
            using var json = JsonDocument.Parse(requestBody);
            var root = json.RootElement;
            if (!root.TryGetProperty("where", out var where) || where.ValueKind != JsonValueKind.Array || where.GetArrayLength() != 1)
            {
                return false;
            }

            var complex = where[0];
            if (!complex.TryGetProperty("isComplex", out var isComplex) || !isComplex.GetBoolean())
            {
                return false;
            }

            if (!complex.TryGetProperty("predicates", out var predicates)
                || predicates.ValueKind != JsonValueKind.Array
                || predicates.GetArrayLength() != 1)
            {
                return false;
            }

            var predicate = predicates[0];
            return predicate.TryGetProperty("field", out var field)
                && field.GetString() == "wing"
                && predicate.TryGetProperty("operator", out var filterOperator)
                && filterOperator.GetString() == "startswith"
                && predicate.TryGetProperty("value", out var value)
                && value.GetString() == "N"
                && predicate.TryGetProperty("isComplex", out var predicateIsComplex)
                && !predicateIsComplex.GetBoolean();
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool IsClearFilteringRequest(IRequest observed)
    {
        if (!observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData") || observed.Method != "POST")
        {
            return false;
        }

        try
        {
            var requestBody = observed.PostData ?? "";
            using var json = JsonDocument.Parse(requestBody);
            var root = json.RootElement;
            return root.TryGetProperty("skip", out var skip)
                && skip.GetInt32() == 0
                && root.TryGetProperty("take", out var take)
                && take.GetInt32() == 8
                && !root.TryGetProperty("where", out _)
                && !root.TryGetProperty("search", out _)
                && !root.TryGetProperty("group", out _)
                && !root.TryGetProperty("sorted", out _);
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    [Test]
    public async Task searching_method_sends_typed_search_payload_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridAction).GetProperty("SearchString"),
            Is.Null);

        await NavigateDirectory();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-search-memory")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = System.Text.Json.JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.TryGetProperty("actionSearchString", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);

        var search = root.GetProperty("search");
        Assert.That(search.GetArrayLength(), Is.EqualTo(1));
        var descriptor = search[0];
        Assert.That(descriptor.GetProperty("key").GetString(), Is.EqualTo("Memory"));
        Assert.That(descriptor.GetProperty("operator").GetString(), Is.EqualTo("contains"));
        Assert.That(descriptor.GetProperty("ignoreCase").GetBoolean(), Is.True);
        Assert.That(descriptor.GetProperty("ignoreAccent").GetBoolean(), Is.False);

        var fields = descriptor.GetProperty("fields").EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.That(fields, Does.Contain("residentName"));
        Assert.That(fields, Does.Contain("careLevel"));
        Assert.That(fields, Does.Contain("wing"));
        Assert.That(descriptor.TryGetProperty("searchString", out _), Is.False);

        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("searching", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        var rows = Page.Locator("#resident-directory-grid .e-row");
        await Expect(rows.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0));
        for (var index = 0; index < rowCount; index++)
        {
            var careLevelCell = rows.Nth(index).Locator(".e-rowcell").Nth(3);
            await Expect(careLevelCell).ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });
        }

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_search_method_clears_active_search_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridAction).GetProperty("SearchString"),
            Is.Null);

        await NavigateDirectory();

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-search-memory")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Memory Care", new() { Timeout = 10000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-clear-search")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionSearchString", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("search cleared", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("searching", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grouping_method_sends_typed_group_payload_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridDataStateChangeArgs).GetProperty("Groups"),
            Is.Null);
        Assert.That(
            typeof(FusionGridAction).GetProperty("PreventFocusOnGroup"),
            Is.Null);

        await NavigateDirectory();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-group-care")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = System.Text.Json.JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        var group = root.GetProperty("group");
        Assert.That(group.GetArrayLength(), Is.EqualTo(1));
        Assert.That(group[0].GetString(), Is.EqualTo("careLevel"));
        Assert.That(root.TryGetProperty("groups", out _), Is.False);
        Assert.That(root.TryGetProperty("preventFocusOnGroup", out _), Is.False);
        Assert.That(root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);

        var sorted = root.GetProperty("sorted");
        Assert.That(sorted.GetArrayLength(), Is.EqualTo(1));
        Assert.That(sorted[0].GetProperty("name").GetString(), Is.EqualTo("careLevel"));
        Assert.That(sorted[0].GetProperty("direction").GetString(), Is.EqualTo("ascending"));

        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("grouping", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("careLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync(
            "240 residents grouped by care level",
            new() { Timeout = 10000 });

        var captions = Page.Locator("#resident-directory-grid .e-groupcaption");
        await Expect(captions.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        var captionTexts = await captions.AllTextContentsAsync();
        Assert.That(captionTexts, Has.Some.Contains("Care Level: Assisted Living"));
        Assert.That(captionTexts, Has.Some.Contains("Care Level: Memory Care"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ungrouping_method_sends_typed_action_payload_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridDataStateChangeArgs).GetProperty("Groups"),
            Is.Null);
        Assert.That(
            typeof(FusionGridDataStateChangeArgs).GetProperty("Aggregates"),
            Is.Null);
        Assert.That(
            typeof(FusionGridAction).GetProperty("PreventFocusOnGroup"),
            Is.Null);

        await NavigateDirectory();

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-group-care")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync(
            "240 residents grouped by care level",
            new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-groupcaption").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-ungroup-care")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("requiresCounts", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("groups", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False);

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("ungroupColumn called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("careLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-groupcaption"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_grouping_method_clears_all_active_groups_and_refreshes_grid()
    {
        Assert.That(
            typeof(FusionGridDataStateChangeArgs).GetProperty("Groups"),
            Is.Null);
        Assert.That(
            typeof(FusionGridDataStateChangeArgs).GetProperty("Aggregates"),
            Is.Null);
        Assert.That(
            typeof(FusionGridAction).GetProperty("PreventFocusOnGroup"),
            Is.Null);

        await NavigateDirectory();

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-group-care")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync(
            "240 residents grouped by care level",
            new() { Timeout = 10000 });

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-group-wing")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("groupColumn wing called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-groupcaption").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#grid-clear-grouping")),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/DirectoryData")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = JsonDocument.Parse(requestBody);
        var root = json.RootElement;

        Assert.That(root.GetProperty("skip").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("take").GetInt32(), Is.EqualTo(8));
        Assert.That(root.TryGetProperty("requiresCounts", out _), Is.False);
        Assert.That(root.TryGetProperty("group", out _), Is.False);
        Assert.That(root.TryGetProperty("groups", out _), Is.False);
        Assert.That(root.TryGetProperty("sorted", out _), Is.False);
        Assert.That(root.TryGetProperty("where", out _), Is.False);
        Assert.That(root.TryGetProperty("search", out _), Is.False);
        Assert.That(root.TryGetProperty("aggregates", out _), Is.False);
        Assert.That(root.TryGetProperty("action", out _), Is.False);
        Assert.That(root.TryGetProperty("actionCancel", out _), Is.False);
        Assert.That(root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False);

        await Expect(Page.Locator("#method-status")).ToHaveTextAsync("grouping cleared", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-take")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("wing", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-groupcaption"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task record_click_reads_typed_row_data_and_cell_coordinates()
    {
        await NavigateDirectory();

        var firstRow = Page.Locator("#resident-directory-grid .e-row").First;
        await Expect(firstRow).ToBeVisibleAsync(new() { Timeout = 10000 });
        var residentCell = firstRow.Locator(".e-rowcell").Nth(1);
        var expectedResident = (await residentCell.InnerTextAsync()).Trim();

        await residentCell.ClickAsync();

        await Expect(Page.Locator("#clicked-resident"))
            .ToHaveTextAsync(expectedResident, new() { Timeout = 10000 });
        await Expect(Page.Locator("#clicked-row"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#clicked-cell"))
            .ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#clicked-event"))
            .ToHaveTextAsync("recordClick", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task row_selected_reads_typed_row_data_previous_index_and_interaction_state()
    {
        await NavigateDirectory();

        var firstRow = Page.Locator("#resident-directory-grid .e-row").Nth(0);
        var secondRow = Page.Locator("#resident-directory-grid .e-row").Nth(1);
        await Expect(firstRow).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(secondRow).ToBeVisibleAsync(new() { Timeout = 10000 });

        await firstRow.Locator(".e-rowcell").Nth(1).ClickAsync();
        await Expect(Page.Locator("#selected-row-index"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });

        var expectedId = (await secondRow.Locator(".e-rowcell").Nth(0).InnerTextAsync()).Trim();
        var expectedResident = (await secondRow.Locator(".e-rowcell").Nth(1).InnerTextAsync()).Trim();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await secondRow.Locator(".e-rowcell").Nth(1).ClickAsync(),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/SelectResident")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";
        using var json = System.Text.Json.JsonDocument.Parse(requestBody);
        var root = json.RootElement;
        Assert.That(root.GetProperty("residentId").GetInt32(), Is.EqualTo(int.Parse(expectedId)));
        Assert.That(root.GetProperty("rowIndex").GetInt32(), Is.EqualTo(1));

        await Expect(Page.Locator("#selected-resident-local"))
            .ToHaveTextAsync(expectedResident, new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-row-index"))
            .ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-previous-row-index"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-interacted"))
            .ToHaveTextAsync("true", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-event"))
            .ToHaveTextAsync("rowSelected", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-resident"))
            .ToHaveTextAsync(expectedResident, new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-summary"))
            .ToContainTextAsync("open tasks", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task row_events_and_method_return_sources_flow_into_http_requests()
    {
        await NavigateDirectory();

        await ClickWhenStable(Page.Locator("#grid-select-second"));
        await Expect(Page.Locator("#selected-row-index")).ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-summary")).ToContainTextAsync("open tasks", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-gather-selection"));
        await Expect(Page.Locator("#selection-indexes")).ToHaveTextAsync("selected row indexes: 1", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-gather-selected-records"));
        await Expect(Page.Locator("#selected-records")).ToContainTextAsync("selected records:", new() { Timeout = 10000 });

        await Page.Locator("#resident-directory-grid .e-row .e-rowcell").First.ClickAsync();
        await Expect(Page.Locator("#clicked-resident")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#clicked-cell")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-selection"));
        await ClickWhenStable(Page.Locator("#grid-gather-selection"));
        await Expect(Page.Locator("#selection-indexes")).ToHaveTextAsync("no selected row indexes", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task virtual_scroll_uses_grid_data_state_to_fetch_later_blocks()
    {
        await NavigateDirectory();

        await Expect(Page.Locator("#virtual-status"))
            .ToHaveTextAsync("loaded first virtual block", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-virtual-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // Real wheel gesture over the virtual content — lets Syncfusion's own
        // scroll handling fire the data-state fetch (no synthetic scroll event).
        var content = Page.Locator("#resident-virtual-grid .e-content").First;
        await content.HoverAsync();
        await Page.Mouse.WheelAsync(0, 1200);

        await Expect(Page.Locator("#virtual-status"))
            .ToHaveTextAsync("virtual block refreshed", new() { Timeout = 10000 });
        await Expect(Page.Locator("#virtual-skip"))
            .Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
