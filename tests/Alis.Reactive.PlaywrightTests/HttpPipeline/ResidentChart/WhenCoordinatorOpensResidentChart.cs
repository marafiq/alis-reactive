using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ResidentChart;

/// <summary>
/// As a coordinator, I want a resident's chart to show the medical record number the server sent, so
/// that I do not request a record the resident already has.
/// </summary>
/// <remarks>
/// The chart's MRN member is named in capitals; the server writes it as "mrn". The page shows it, tests
/// it in a condition, and names a flagged chart by it in an event the page dispatches. The record-number
/// list shows each chart's MRN through its row's "mrn" member, as its text field and in its item template.
/// </remarks>
[TestFixture]
public class WhenCoordinatorOpensResidentChart : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ResidentChart";

    private ILocator OpenChart(string resident) => Page.Locator($"#open-{resident}-chart-btn");
    private ILocator ChartName => Page.Locator("#chart-name");
    private ILocator ChartMrn => Page.Locator("#chart-mrn");
    private ILocator ChartRecord => Page.Locator("#chart-record");
    private ILocator FlagHelenChart => Page.Locator("#flag-helen-chart-btn");
    private ILocator ReviewFlag => Page.Locator("#review-flag");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task opening_a_chart_shows_the_residents_medical_record_number()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OpenChart("helen"));

        await Expect(ChartRecord).ToHaveTextAsync("Chart linked to the medical record.");
        await Expect(ChartMrn).ToHaveTextAsync("MRN-20417");
        await Expect(ChartName).ToHaveTextAsync("Helen Park");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_without_a_medical_record_is_told_to_request_one_and_shows_no_earlier_number()
    {
        await NavigateAndBoot();
        await ClickWhenStable(OpenChart("helen"));
        await Expect(ChartMrn).ToHaveTextAsync("MRN-20417");

        await ClickWhenStable(OpenChart("arthur"));

        await Expect(ChartRecord).ToHaveTextAsync("No medical record yet: request one from admissions.");
        await Expect(ChartName).ToHaveTextAsync("Arthur Bell");
        await Expect(ChartMrn).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_record_number_list_shows_each_charts_number_and_opens_the_chosen_chart()
    {
        await NavigateAndBoot();
        var plan = await PagePlan<ResidentChartPageModel>.FromPage(Page);
        var byRecordNumber = plan.DropDownList(m => m.ChartByRecordNumber);

        await byRecordNumber.Open();

        // The item template shows each row's MRN and name.
        await Expect(byRecordNumber.Popup.GetByText("MRN-20417", new() { Exact = true })).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(byRecordNumber.Popup.GetByText("MRN-20533", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(byRecordNumber.Popup.GetByText("Rosa Diaz", new() { Exact = true })).ToBeVisibleAsync();

        await byRecordNumber.Popup.GetByText("MRN-20533", new() { Exact = true }).ClickAsync();

        // The list's text field shows the chosen MRN; its value opens Rosa Diaz's chart.
        await Expect(byRecordNumber.Input).ToHaveValueAsync("MRN-20533", new() { Timeout = 5000 });
        await Expect(ChartName).ToHaveTextAsync("Rosa Diaz", new() { Timeout = 5000 });
        await Expect(ChartMrn).ToHaveTextAsync("MRN-20533");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_a_chart_for_review_names_it_by_its_medical_record_number()
    {
        await NavigateAndBoot();

        await ClickWhenStable(FlagHelenChart);

        await Expect(ReviewFlag).ToHaveTextAsync("MRN-20417");
        AssertNoConsoleErrors();
    }
}
