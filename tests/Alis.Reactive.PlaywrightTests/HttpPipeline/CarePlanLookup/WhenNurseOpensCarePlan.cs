using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.CarePlanLookup;

/// <summary>
/// As a nurse, I want to open a resident's care plan, so that I can check it before my round, and keep
/// using the page whatever came of the last attempt.
/// </summary>
/// <remarks>
/// Open fetches the care plan by the chosen resident's number in the URL and covers the page with the
/// loader while it does; the loader lifts once the request ends. Rosa Diaz, admitted this week, has no
/// care plan on file. The page does not require a resident before Open: with none chosen the URL cannot be
/// built, nothing is sent, and the failure is reported to the developer's console.
/// </remarks>
[TestFixture]
public class WhenNurseOpensCarePlan : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/CarePlanLookup";
    private const string CarePlanUrl = "**/Sandbox/HttpPipeline/CarePlanLookup/CarePlan/*";
    private const string UnbuildableUrlError = "route param \"residentId\" evaluated to null";

    private PagePlan<CarePlanLookupModel> _plan = null!;
    private SemaphoreSlim _carePlanReleased = null!;

    private DropDownListLocator Resident => _plan.DropDownList(m => m.ResidentId);
    private ILocator OpenCarePlan => Page.Locator("#open-care-plan-btn");
    private ILocator CarePlanResident => Page.Locator("#care-plan-resident");
    private ILocator CarePlanSummary => Page.Locator("#care-plan-summary");
    private ILocator Loader => Page.Locator("#alis-loader");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<CarePlanLookupModel>.FromPage(Page);
    }

    // The request is held at the browser until the test releases it, so the loader can be seen covering
    // the page before the answer arrives.
    private async Task HoldCarePlanUntilReleased()
    {
        _carePlanReleased = new SemaphoreSlim(0);
        await Page.RouteAsync(CarePlanUrl, async route =>
        {
            await _carePlanReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
    }

    [Test]
    public async Task opening_a_residents_care_plan_covers_the_page_while_loading_then_shows_the_plan()
    {
        await HoldCarePlanUntilReleased();
        await NavigateAndBoot();
        await Resident.Select("Helen Park");

        await ClickWhenStable(OpenCarePlan);
        await Expect(Loader).ToBeVisibleAsync();
        _carePlanReleased.Release();

        await Expect(CarePlanResident).ToHaveTextAsync("Helen Park");
        await Expect(CarePlanSummary).ToHaveTextAsync("Assisted living: morning medication round, physiotherapy on Tuesdays and Fridays.");
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pressing_open_with_no_resident_chosen_sends_nothing_and_leaves_the_page_usable()
    {
        await NavigateAndBoot();
        var carePlanRequests = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/CarePlanLookup/CarePlan/")) carePlanRequests.Enqueue(request.Url);
        };

        await ClickWhenStable(OpenCarePlan);

        await WaitForTraceMessage(UnbuildableUrlError);
        await Expect(Loader).ToBeHiddenAsync();
        await Expect(CarePlanResident).ToHaveTextAsync("No care plan open.");
        await Expect(CarePlanSummary).ToHaveTextAsync("");
        Assert.That(carePlanRequests, Is.Empty, "No care plan request is sent without a resident.");
        AssertNoConsoleErrorsExcept(UnbuildableUrlError);
    }

    [Test]
    public async Task choosing_a_resident_after_opening_with_none_chosen_opens_their_care_plan()
    {
        await NavigateAndBoot();
        await ClickWhenStable(OpenCarePlan);

        await Resident.Select("Arthur Bell");
        await ClickWhenStable(OpenCarePlan);

        await Expect(CarePlanResident).ToHaveTextAsync("Arthur Bell");
        await Expect(CarePlanSummary).ToHaveTextAsync("Memory care: escorted meals, evening check every two hours.");
        AssertNoConsoleErrorsExcept(UnbuildableUrlError);
    }

    [Test]
    public async Task opening_the_care_plan_of_a_resident_without_one_tells_me_none_is_on_file()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park");
        await ClickWhenStable(OpenCarePlan);
        await Expect(CarePlanResident).ToHaveTextAsync("Helen Park");
        await Resident.Select("Rosa Diaz");

        await ClickWhenStable(OpenCarePlan);

        await Expect(CarePlanSummary).ToHaveTextAsync("No care plan is on file for this resident yet.");
        await Expect(CarePlanResident).ToHaveTextAsync("No care plan open.");
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }
}
