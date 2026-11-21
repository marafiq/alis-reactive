using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorPlansTheDiet(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X170_0)]
    public async Task the_kitchen_receives_the_working_flags_meals_and_both_document_and_label_positions()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Dietary needs", Exact = true }).SelectOptionAsync("Vegetarian");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Dietary needs", Exact = true }).SelectOptionAsync("Vegetarian, LowSodium");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Kitchen note", Exact = true }).FillAsync("Serve soup first");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Consent", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Authority", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Meal service label 1", Exact = true }).FillAsync("Quiet table");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save diet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#diet-status")).ToHaveTextAsync("Diet saved: DIET8");
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. Serve soup first.");
        var preview = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview meals", Exact = true }).ClickAsync(), "**/move-ins/meal-plan?**");
        Assert.That(preview.Url, Does.EndWith("/care/move-ins/meal-plan?DraftId=D100&DietaryNeeds=Vegetarian%2C+LowSodium&MealCount=3&Kinds=MedicationList&Kinds=Authority&OptionalLabels=Quiet+table&OptionalLabels="));
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Vegetarian, low sodium: 3 meals");
        await Expect(world.Page.Locator("#meal-instructions")).ToHaveTextAsync("MedicationList, Authority. Quiet table, unassigned.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. Serve soup first.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview meals", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#meal-instructions")).ToHaveTextAsync("MedicationList, Authority. Quiet table, unassigned.");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Dietary needs", Exact = true }).SelectOptionAsync("LowSodium");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Meals per day", Exact = true }).FillAsync("4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save diet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#diet-status")).ToHaveTextAsync("Diet saved: DIET9");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 10");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: low sodium (2), 4 meals. Serve soup first.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X171_0)]
    public async Task an_unknown_imported_dietary_flag_is_refused_and_the_confirmed_plan_reopens()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.AReferralCarriesDiet(8));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Dietary needs", Exact = true }).SelectOptionAsync("8");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save diet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#diet-status")).ToHaveTextAsync("This referral contains unsupported dietary needs.");
        Assert.That(world.SentTo(CareEndpoints.MoveInDiet), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X172_0)]
    public async Task a_kitchen_restriction_uses_the_community_name_and_preserves_its_confirmed_plan()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Dietary needs", Exact = true }).SelectOptionAsync("Pureed");
        await Expect(world.Page.Locator("#kitchen-readiness")).ToHaveTextAsync("Kitchen review required");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save diet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#diet-status")).ToHaveTextAsync("Pureed meals are unavailable at Harbor House.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview meals", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Pureed meals are unavailable at Harbor House.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X173_0)]
    public async Task a_lost_preview_response_preserves_the_confirmed_kitchen_plan()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Meals per day", Exact = true }).FillAsync("5");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInMealPlan));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview meals", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Meal preview unavailable.");
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X174_0)]
    public async Task confirmed_new_care_retires_a_held_preview_and_reopens_with_the_retained_diet()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. No added salt.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Meals per day", Exact = true }).FillAsync("5");
        var preview = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInMealPlan);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview meals", Exact = true }).ClickAsync();
        var held = await preview.Held();
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Reading meal preview");
        await world.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Care changed. Preview meals for the new care level.");
        await held.Release();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessment code", Exact = true }).FillAsync("MC-7");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Representative contact", Exact = true }).FillAsync("CT1");
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Care changed. Preview meals for the new care level.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Meals per day", Exact = true }).FillAsync("4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await Expect(world.Page.Locator("#meal-preview")).ToHaveTextAsync("Care changed. Preview meals for the new care level.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-care")).ToHaveTextAsync("Memory, assessment MC-7, secure unit consent given, representative CT1");
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 4 meals. No added salt.");
    }
}
