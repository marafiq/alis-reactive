using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenHelenSearchesForAnInterpreter(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X1206_0)]
    public async Task keyboard_guidance_is_available_while_choosing_a_language()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        var language = helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox);
        var guidance = helen.Page.GetByText("Use the arrow keys to choose a language and Enter to confirm.", new PageGetByTextOptions { Exact = true });
        await language.ClickAsync();
        await Expect(guidance).ToBeVisibleAsync();
        await language.PressAsync("Tab");
        await Expect(Directory(helen)).ToBeFocusedAsync();
        await Expect(guidance).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1207_0)]
    public async Task the_directory_offers_matching_services_until_the_empty_reply_arrives()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        var box = Directory(helen);
        var opening = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("s");
        var pending = await opening.Held();
        await Expect(helen.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: interpreter-directory");
        await Expect(helen.Page.GetByText("No interpreter services found.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await pending.Release();
        await Expect(helen.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Harbor Interpreting", "Linden Language Services" });
        var armed = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("z");
        var held = await armed.Held();
        await Expect(helen.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Harbor Interpreting", "Linden Language Services" });
        await held.Release();
        await Expect(helen.Page.GetByText("No interpreter services found.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1208_0)]
    public async Task choosing_a_returned_service_shows_its_booking_instruction()
    {
        var swedish = await NewWorld();
        await swedish.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Directory(swedish).PressSequentiallyAsync("sw");
        await Expect(swedish.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Linden Language Services" });
        await Directory(swedish).PressAsync("ArrowDown");
        await Directory(swedish).PressAsync("Enter");
        await Expect(swedish.Page.Locator("#service-booking")).ToHaveTextAsync("Video appointments through extension 248.");

        var spanish = await NewWorld();
        await spanish.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Directory(spanish).PressSequentiallyAsync("sp");
        await Expect(spanish.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Harbor Interpreting" });
        await Directory(spanish).PressAsync("ArrowDown");
        await Directory(spanish).PressAsync("Enter");
        await Expect(spanish.Page.Locator("#service-booking")).ToHaveTextAsync("Telephone appointments through extension 317.");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1209_0)]
    public async Task a_late_directory_reply_keeps_only_the_services_for_the_latest_search()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        var box = Directory(helen);
        var armedFirst = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("s");
        var first = await armedFirst.Held();
        var armedSecond = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("w");
        var second = await armedSecond.Held();
        var busy = helen.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true });
        await Expect(busy).ToHaveTextAsync("Busy: interpreter-directory");
        await second.Release();
        await Expect(helen.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Linden Language Services" });
        await first.Release();
        await Expect(busy).ToHaveTextAsync("Busy: none");
        await Expect(helen.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Linden Language Services" });
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1216_0)]
    public async Task the_directory_spinner_stays_with_the_active_search_until_the_services_or_refusal_arrive()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        var box = Directory(helen);
        var spinner = helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter service", Exact = true }).GetByRole(AriaRole.Img);
        var firstSearch = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("s");
        var first = await firstSearch.Held();
        await Expect(spinner).ToBeVisibleAsync();
        var secondSearch = await helen.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await box.PressSequentiallyAsync("w");
        var second = await secondSearch.Held();
        await first.Release();
        await Expect(spinner).ToBeVisibleAsync();
        await second.Release();
        await Expect(helen.Page.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Linden Language Services" });
        await Expect(spinner).ToBeHiddenAsync();

        var unavailable = await NewWorld();
        await unavailable.Seed(Seed.TheEndpointAnswers(CareEndpoints.CommunicationInterpreters, 500));
        await unavailable.Page.GotoAsync(CarePages.CommunicationPreferences);
        var indicator = unavailable.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter service", Exact = true }).GetByRole(AriaRole.Img);
        var armed = await unavailable.Arm(LanesGate.Respond, CareEndpoints.CommunicationInterpreters);
        await Directory(unavailable).PressSequentiallyAsync("s");
        var held = await armed.Held();
        await Expect(indicator).ToBeVisibleAsync();
        await held.Release();
        await Expect(unavailable.Page.GetByText("Interpreter search is unavailable. Try again.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(indicator).ToBeHiddenAsync();

        var rejected = await NewWorld();
        await rejected.Seed(Seed.TheEndpointAnswers(CareEndpoints.CommunicationInterpreters, 400));
        await rejected.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Directory(rejected).PressSequentiallyAsync("s");
        await Expect(rejected.Page.GetByText("Interpreter search is unavailable. Try again.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(rejected.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter service", Exact = true }).GetByRole(AriaRole.Img)).ToBeHiddenAsync();

        var expired = await NewWorld();
        await expired.Seed(Seed.TheEndpointAnswers(CareEndpoints.CommunicationInterpreters, 401));
        await expired.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Directory(expired).PressSequentiallyAsync("s");
        await Expect(expired.Page.GetByText("Sign in again to search for an interpreter.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(expired.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter service", Exact = true }).GetByRole(AriaRole.Img)).ToBeHiddenAsync();
    }

    private static ILocator Directory(TestWorld world)
        => world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter service", Exact = true }).GetByRole(AriaRole.Combobox);
}
