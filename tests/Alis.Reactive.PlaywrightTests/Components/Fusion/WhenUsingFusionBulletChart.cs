using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionBulletChart : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionBulletChart";

    private FusionBulletChartLocator BulletChart => new(Page, "resident-bullet-chart");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#resident-bullet-chart svg")).ToHaveCountAsync(1, new() { Timeout = 5000 });
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionBulletChart — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_bullet_chart_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-bullet-chart"));
        Assert.That(planJson, Does.Contain("\"tooltipRender\""));
        Assert.That(planJson, Does.Contain("\"bulletChartMouseClick\""));
        Assert.That(planJson, Does.Contain("\"getActualIndex\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task load_and_method_reads_are_proven()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#actual-index-negative")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#actual-index-overflow")).ToHaveTextAsync("0", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_payload_exposes_target_and_mouse_coordinates()
    {
        await NavigateAndBoot();

        await BulletChart.FeatureMeasure.ClickAsync(new() { Force = true });

        await Expect(Page.Locator("#mouse-click-state")).ToHaveTextAsync("clicked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mouse-click-target")).ToContainTextAsync("FeatureMeasure", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mouse-click-x")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mouse-click-y")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tooltip_render_is_mutated_and_reports_target_payload()
    {
        await NavigateAndBoot();

        await BulletChart.FeatureMeasure.HoverAsync();

        await Expect(BulletChart.Tooltip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(BulletChart.Tooltip).ToContainTextAsync("Bullet tooltip rewritten", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-value")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-target")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-name")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-template")).ToHaveTextAsync("none", new() { Timeout = 5000 });
        await Expect(Page.Locator("#tooltip-text")).ToHaveTextAsync("Bullet tooltip rewritten", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
