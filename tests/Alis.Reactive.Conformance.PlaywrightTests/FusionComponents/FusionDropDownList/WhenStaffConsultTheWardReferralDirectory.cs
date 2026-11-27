using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffConsultTheWardReferralDirectory(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1833_0)]
    public async Task the_published_ward_notice_follows_the_selected_order()
    {
        var priority = await NewWorld();
        await priority.Page.GotoAsync(CarePages.WardReferralDirectory);
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clinical priority", Exact = true }).ClickAsync();
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print call order", Exact = true }).ClickAsync();
        await Expect(priority.Page.Locator("#printed-order")).ToHaveTextAsync("Call the wards in clinical priority order.");
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published order", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow", "Cedar", "Maple" });

        var az = await NewWorld();
        await az.Page.GotoAsync(CarePages.WardReferralDirectory);
        await az.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Alphabetical notice", Exact = true }).ClickAsync();
        await az.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print call order", Exact = true }).ClickAsync();
        await Expect(az.Page.Locator("#printed-order")).ToHaveTextAsync("Work from the first ward name to the last.");
        await az.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published order", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(az.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar", "Maple", "Willow" });

        var za = await NewWorld();
        await za.Page.GotoAsync(CarePages.WardReferralDirectory);
        await za.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reverse notice", Exact = true }).ClickAsync();
        await za.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print call order", Exact = true }).ClickAsync();
        await Expect(za.Page.Locator("#printed-order")).ToHaveTextAsync("Start at the end of the alphabetical ward register.");
        await za.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published order", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(za.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow", "Maple", "Cedar" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1834_0)]
    public async Task referral_search_offers_the_right_hint_or_a_quick_pick_list()
    {
        var clinical = await NewWorld();
        await clinical.Page.GotoAsync(CarePages.WardReferralDirectory);
        await clinical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clinical search", Exact = true }).ClickAsync();
        await clinical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read referral instructions", Exact = true }).ClickAsync();
        await Expect(clinical.Page.Locator("#directory-search-guide")).ToHaveTextAsync("Use the clinical directory before calling the placement team.");
        await clinical.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Referral search", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(clinical.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(clinical.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "Search clinical wards");
        await clinical.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("Ced");
        await Expect(clinical.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar" });

        var handover = await NewWorld();
        await handover.Page.GotoAsync(CarePages.WardReferralDirectory);
        await handover.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Handover search", Exact = true }).ClickAsync();
        await handover.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read referral instructions", Exact = true }).ClickAsync();
        await Expect(handover.Page.Locator("#directory-search-guide")).ToHaveTextAsync("Find the receiving ward before sending the handover.");
        await handover.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Referral search", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(handover.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(handover.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "Search receiving wards");
        await handover.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("Ced");
        await Expect(handover.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar" });

        var quick = await NewWorld();
        await quick.Page.GotoAsync(CarePages.WardReferralDirectory);
        await quick.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quick ward choice", Exact = true }).ClickAsync();
        await quick.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read referral instructions", Exact = true }).ClickAsync();
        await Expect(quick.Page.Locator("#directory-search-guide")).ToHaveTextAsync("Choose the ward from the current referral list.");
        await quick.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Referral search", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(quick.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow", "Cedar", "Maple", "Oak" });
        await Expect(quick.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1835_0)]
    public async Task a_withdrawn_ward_stays_identifiable_and_an_available_ward_accepts_the_transfer()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.WardReferralDirectory);
        var receivingUnit = held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Destination hold", Exact = true });
        await receivingUnit.GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(receivingUnit.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Oak");
        await receivingUnit.GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(receivingUnit.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Willow");
        await held.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Withdraw the highlighted ward", Exact = true }).CheckAsync();
        await held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Destination hold", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("c");
        await Expect(held.Page.Locator("#receiving-desk")).ToHaveTextAsync("Cedar withdrawn from this round.");
        await held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Destination hold", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(held.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Cedar", Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(receivingUnit.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("");
        await held.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#receiving-desk")).ToHaveTextAsync("Maple admissions desk.");
        await Expect(receivingUnit.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");

        var released = await NewWorld();
        await released.Page.GotoAsync(CarePages.WardReferralDirectory);
        await released.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Destination hold", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("c");
        await Expect(released.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Destination hold", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await Expect(released.Page.Locator("#receiving-desk")).ToHaveTextAsync("Call the Cedar charge nurse.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1836_0)]
    public async Task the_available_and_closed_service_directories_show_their_real_template_instructions()
    {
        var urgent = await NewWorld();
        await urgent.Page.GotoAsync(CarePages.WardReferralDirectory);
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Urgent directory", Exact = true }).ClickAsync();
        await urgent.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(urgent.Page.GetByText("Urgent transfer wards", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(urgent.Page.GetByText("Clear the receiving bed before transfer.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(urgent.Page.GetByText("Secure care contacts", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(urgent.Page.GetByText("Recovery contacts", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var routine = await NewWorld();
        await routine.Page.GotoAsync(CarePages.WardReferralDirectory);
        await routine.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Routine directory", Exact = true }).ClickAsync();
        await routine.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(routine.Page.GetByText("Routine placements", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(routine.Page.GetByText("Confirm the handover time.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(routine.Page.GetByText("Memory care wards", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(routine.Page.GetByText("Recovery wards", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WardReferralDirectory);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close service directory", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(closed.Page.GetByText("Telephone the night placement desk.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1837_0)]
    public async Task the_empty_directory_gives_the_vacancy_notice_in_the_working_language()
    {
        var french = await NewWorld();
        await french.Page.GotoAsync(CarePages.WardReferralDirectory);
        await french.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "French notices", Exact = true }).ClickAsync();
        await french.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print vacancy instructions", Exact = true }).ClickAsync();
        await Expect(french.Page.Locator("#printed-vacancy")).ToHaveTextAsync("Contactez le bureau des admissions.");
        await french.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Vacancy language", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(french.Page.GetByText("Aucune place disponible.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var english = await NewWorld();
        await english.Page.GotoAsync(CarePages.WardReferralDirectory);
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "English notices", Exact = true }).ClickAsync();
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print vacancy instructions", Exact = true }).ClickAsync();
        await Expect(english.Page.Locator("#printed-vacancy")).ToHaveTextAsync("Contact the admissions office.");
        await english.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Vacancy language", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(english.Page.GetByText("No ward vacancies.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1838_0)]
    public async Task the_ward_itinerary_uses_its_configured_query_and_exact_page_boundaries()
    {
        var first = await NewWorld();
        await first.Page.GotoAsync(CarePages.WardReferralDirectory);
        await first.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward itinerary", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(first.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await first.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("f");
        await Expect(first.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow" });

        await first.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).FillAsync("");
        await first.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("r");
        await Expect(first.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar", "Maple", "Oak" });

        var remaining = await NewWorld();
        await remaining.Page.GotoAsync(CarePages.WardReferralDirectory);
        await remaining.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward itinerary", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(remaining.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await remaining.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("r");
        await Expect(remaining.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar", "Maple", "Oak" });

        var middle = await NewWorld();
        await middle.Page.GotoAsync(CarePages.WardReferralDirectory);
        await middle.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward itinerary", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(middle.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await middle.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("m");
        await Expect(middle.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar", "Maple" });

        var second = await NewWorld();
        await second.Page.GotoAsync(CarePages.WardReferralDirectory);
        await second.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward itinerary", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(second.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await second.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("p");
        await Expect(second.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Oak" });

        var whole = await NewWorld();
        await whole.Page.GotoAsync(CarePages.WardReferralDirectory);
        await whole.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unrestricted itinerary", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(whole.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await whole.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("f");
        await Expect(whole.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow", "Cedar", "Maple", "Oak" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1839_0)]
    public async Task an_empty_or_focused_transfer_field_carries_its_chosen_caption()
    {
        var always = await NewWorld();
        await always.Page.GotoAsync(CarePages.WardReferralDirectory);
        await always.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Always label destination", Exact = true }).ClickAsync();
        await Expect(always.Page.GetByText("Receiving ward", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var automatic = await NewWorld();
        await automatic.Page.GotoAsync(CarePages.WardReferralDirectory);
        await automatic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Float destination label", Exact = true }).ClickAsync();
        await Expect(automatic.Page.GetByText("Receiving ward", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-bottom");
        await automatic.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer form", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(automatic.Page.GetByText("Receiving ward", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");

        var simple = await NewWorld();
        await simple.Page.GotoAsync(CarePages.WardReferralDirectory);
        await simple.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use a simple field", Exact = true }).ClickAsync();
        await Expect(simple.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer form", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveAttributeAsync("placeholder", "Receiving ward");
        await simple.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer form", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(simple.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer form", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Oak");
    }
}
