using System.Text.Json;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid";

    private async Task NavigateAndWaitForInitialLoad()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-status"))
            .ToHaveTextAsync("initial data loaded", new() { Timeout = 10000 });
    }

    [Test]
    public async Task page_loads_with_initial_data()
    {
        await NavigateAndWaitForInitialLoad();

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grid_displays_ten_rows_per_page()
    {
        await NavigateAndWaitForInitialLoad();

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows)
            .ToHaveCountAsync(10, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_a_column_fetches_sorted_data_and_echoes_action()
    {
        Assert.That(
            typeof(FusionGridAction).GetProperty("Target"),
            Is.Null);

        await NavigateAndWaitForInitialLoad();
        using (var plan = JsonDocument.Parse(await Page.Locator("#plan-json").TextContentAsync() ?? "{}"))
        {
            Assert.That(HasResponseBodyRead(plan.RootElement, pathMustBeEmpty: true), Is.True,
                "SetDataSource(json) must emit the canonical whole response-body read with member=responseBody and an empty path.");
            Assert.That(HasResponseBodyRead(plan.RootElement, pathMustBeEmpty: false), Is.False,
                "This Grid page must not emit responseBody as a member-path read for whole-response SetDataSource(json).");
        }

        var nameHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Name" });
        await Expect(nameHeader).ToBeVisibleAsync(new() { Timeout = 5000 });
        var response = await Page.RunAndWaitForResponseAsync(
            async () => await nameHeader.ClickAsync(),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/Data")
                && observed.Request.Method == "POST");
        var requestBody = response.Request.PostData ?? "";

        Assert.That(requestBody, Does.Contain("\"eventName\":\"dataStateChange\""));
        Assert.That(requestBody, Does.Contain("\"requiresCounts\":true"));
        Assert.That(requestBody, Does.Contain("\"actionName\":\"actionBegin\""));
        Assert.That(requestBody, Does.Contain("\"actionType\":\"actionBegin\""));
        Assert.That(requestBody, Does.Contain("\"actionCancel\":false"));
        Assert.That(requestBody, Does.Contain("\"sorted\""));
        Assert.That(requestBody, Does.Contain("\"name\":\"name\""));
        Assert.That(requestBody, Does.Contain("\"direction\":\"ascending\""));
        using (var requestJson = JsonDocument.Parse(requestBody))
        {
            var root = requestJson.RootElement;
            Assert.That(root.TryGetProperty("where", out _), Is.False);
            Assert.That(root.TryGetProperty("search", out _), Is.False);
            Assert.That(root.TryGetProperty("group", out _), Is.False);
        }

        Assert.That(response.Status, Is.EqualTo(200));
        using var responseJson = JsonDocument.Parse(await response.TextAsync());
        var responseRoot = responseJson.RootElement;
        Assert.That(responseRoot.TryGetProperty("result", out var result), Is.True);
        Assert.That(responseRoot.TryGetProperty("count", out var count), Is.True);
        Assert.That(result.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(result.GetArrayLength(), Is.EqualTo(10));
        Assert.That(count.GetInt32(), Is.EqualTo(200));
        var firstReturnedName = result[0].GetProperty("name").GetString() ?? "";
        Assert.That(firstReturnedName, Is.Not.Empty);

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        await Expect(Page.Locator("#residents-grid .e-row")).ToHaveCountAsync(10, new() { Timeout = 10000 });
        await Expect(Page.Locator("#residents-grid .e-row").First.Locator(".e-rowcell").First)
            .ToHaveTextAsync(firstReturnedName, new() { Timeout = 5000 });
        await Expect(Page.Locator("#residents-grid .e-parentmsgbar"))
            .ToContainTextAsync("200 items", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("sorting", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-name"))
            .ToHaveTextAsync("dataStateChange", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-requires-counts"))
            .ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-name"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-event-type"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-column"))
            .ToHaveTextAsync("name", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-direction"))
            .ToHaveTextAsync("Ascending", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_fetches_next_page_with_correct_skip()
    {
        Assert.That(
            typeof(FusionGridAction).GetProperty("PreviousPageSize"),
            Is.Null);
        Assert.That(
            typeof(FusionGridAction).GetProperty("Rows"),
            Is.Null);
        Assert.That(
            typeof(FusionGridAction).GetProperty("Target"),
            Is.Null);

        await NavigateAndWaitForInitialLoad();

        var pager = Page.Locator("#residents-grid .e-pagercontainer");
        await Expect(pager).ToBeVisibleAsync(new() { Timeout = 5000 });

        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await page2.ClickAsync(),
            observed => observed.Url.Contains("/Sandbox/Components/Grid/Data")
                && observed.Method == "POST");
        var requestBody = request.PostData ?? "";

        Assert.That(requestBody, Does.Contain("\"eventName\":\"dataStateChange\""));
        Assert.That(requestBody, Does.Contain("\"skip\":10"));
        Assert.That(requestBody, Does.Contain("\"take\":10"));
        Assert.That(requestBody, Does.Contain("\"requiresCounts\":true"));
        Assert.That(requestBody, Does.Contain("\"actionName\":\"actionBegin\""));
        Assert.That(requestBody, Does.Contain("\"actionType\":\"actionBegin\""));
        Assert.That(requestBody, Does.Contain("\"actionCancel\":false"));
        Assert.That(requestBody, Does.Contain("\"actionRequestType\":\"paging\""));
        Assert.That(requestBody, Does.Contain("\"actionCurrentPage\":2"));
        Assert.That(requestBody, Does.Contain("\"actionPreviousPage\":1"));
        Assert.That(requestBody, Does.Contain("\"actionPageSize\":10"));
        using (var requestJson = JsonDocument.Parse(requestBody))
        {
            var root = requestJson.RootElement;
            Assert.That(root.TryGetProperty("where", out _), Is.False);
            Assert.That(root.TryGetProperty("search", out _), Is.False);
            Assert.That(root.TryGetProperty("group", out _), Is.False);
            Assert.That(root.TryGetProperty("sorted", out _), Is.False);
            Assert.That(root.TryGetProperty("actionPreviousPageSize", out _), Is.False);
            Assert.That(root.TryGetProperty("actionRows", out _), Is.False);
            Assert.That(root.TryGetProperty("actionTarget", out _), Is.False);
        }

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-name"))
            .ToHaveTextAsync("dataStateChange", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-take"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-requires-counts"))
            .ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-name"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-event-type"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-page"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-previous-page"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-page-size"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task external_filter_reloads_grid_with_fewer_results()
    {
        await NavigateAndWaitForInitialLoad();

        var numericInput = Page.Locator("input[id$='__MinAge']");
        await Expect(numericInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await numericInput.ClickAsync();
        await numericInput.FillAsync("90");
        await numericInput.PressAsync("Tab");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("filtered", new() { Timeout = 10000 });

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_then_paging_reports_paging_action()
    {
        await NavigateAndWaitForInitialLoad();

        var ageHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Age" });
        await ClickWhenStable(ageHeader);
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 10000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_grid_behaviors()
    {
        await NavigateAndWaitForInitialLoad();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions for SetDataSource");
        Assert.That(planJson, Does.Contain("\"dataSource\""),
            "Plan must target the dataSource property");
        Assert.That(planJson, Does.Contain("residents-grid"),
            "Plan must reference the grid element ID");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must declare fusion vendor");

        AssertNoConsoleErrors();
    }

    private static bool HasResponseBodyRead(JsonElement element, bool pathMustBeEmpty)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("member", out var member) &&
                member.ValueKind == JsonValueKind.String &&
                member.GetString() == "responseBody" &&
                element.TryGetProperty("path", out var path) &&
                path.ValueKind == JsonValueKind.Array &&
                (path.GetArrayLength() == 0) == pathMustBeEmpty)
            {
                return true;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (HasResponseBodyRead(property.Value, pathMustBeEmpty))
                    return true;
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (HasResponseBodyRead(item, pathMustBeEmpty))
                    return true;
            }
        }

        return false;
    }
}
