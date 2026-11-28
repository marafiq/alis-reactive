using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenHelenChoosesHerCommunicationPreferences(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X1200_0)]
    public async Task choosing_a_letter_routes_the_saved_plan_to_the_correspondence_desk()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact channel", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#channel-instruction")).ToHaveTextAsync("Send correspondence through the records desk.");
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#channel-instruction")).ToHaveTextAsync("Send correspondence through the records desk.");

        var another = await NewWorld();
        await another.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(another.Page.Locator("#channel-instruction")).ToHaveTextAsync("Call Helen directly.");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1201_0)]
    public async Task zero_notice_retains_the_instruction_to_call_before_arrival()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#notice-instruction")).ToHaveTextAsync("Reserve thirty minutes for preparation.");
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Advance notice", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#notice-instruction")).ToHaveTextAsync("Call before arrival.");
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#notice-instruction")).ToHaveTextAsync("Call before arrival.");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1202_0)]
    public async Task withholding_consent_keeps_interpreter_arrangements_with_helen()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#consent-instruction")).ToHaveTextAsync("Staff may arrange an interpreter.");
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter consent", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#consent-instruction")).ToHaveTextAsync("Discuss support directly with Helen.");
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#consent-instruction")).ToHaveTextAsync("Discuss support directly with Helen.");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1203_0)]
    public async Task either_telephone_or_preparation_time_secures_the_interpreter_ahead_of_the_pack()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Advance notice", Exact = true }).GetByRole(AriaRole.Combobox));
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Book the spoken-language interpreter.");
        await Expect(helen.Page.Locator("#previous-language")).ToHaveTextAsync("English");
        await Expect(helen.Page.Locator("#selected-language")).ToHaveTextAsync("Punjabi · Gurmukhi script");
        await Expect(helen.Page.Locator("#booking")).ToBeVisibleAsync();
        await Expect(helen.Page.Locator("#booking-team")).ToHaveTextAsync("Punjabi scheduling");
        await Expect(helen.Page.Locator("#pack")).ToBeHiddenAsync();
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Book the spoken-language interpreter.");
        await Expect(helen.Page.Locator("#booking-team")).ToHaveTextAsync("Punjabi scheduling");
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact channel", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Prepare the translated admission pack.");
        await ChooseUp(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Advance notice", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Book the spoken-language interpreter.");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1204_0)]
    public async Task a_translated_pack_is_prepared_when_spoken_interpretation_cannot_be_booked()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter consent", Exact = true }).GetByRole(AriaRole.Combobox));
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Prepare the translated admission pack.");
        await Expect(helen.Page.Locator("#pack")).ToBeVisibleAsync();
        await Expect(helen.Page.Locator("#booking")).ToBeHiddenAsync();
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#previous-language")).ToHaveTextAsync("English");
        await Expect(helen.Page.Locator("#selected-language")).ToHaveTextAsync("Punjabi · Gurmukhi script");
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Prepare the translated admission pack.");
        await ChooseUp(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interpreter consent", Exact = true }).GetByRole(AriaRole.Combobox));
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Prepare the translated admission pack.");
        await Expect(helen.Page.Locator("#previous-language")).ToHaveTextAsync("Punjabi");
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1205_0)]
    public async Task returning_to_english_closes_the_booking_and_keeps_the_previous_language_on_file()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await ChooseDown(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#booking")).ToBeVisibleAsync();
        await ChooseUp(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox));
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Use the standard admission guide.");
        await Expect(helen.Page.Locator("#previous-language")).ToHaveTextAsync("Punjabi");
        await Expect(helen.Page.Locator("#selected-language")).ToHaveTextAsync("English · Latin script");
        await Expect(helen.Page.Locator("#booking")).ToBeHiddenAsync();
        await Expect(helen.Page.Locator("#pack")).ToBeHiddenAsync();
        await Save(helen);
        await Expect(helen.Page.Locator("#receipt")).ToHaveTextAsync("Communication plan saved for Helen Martinez.");
        await helen.Page.GotoAsync(CarePages.CommunicationPreferences);
        await Expect(helen.Page.Locator("#previous-language")).ToHaveTextAsync("Punjabi");
        await Expect(helen.Page.Locator("#selected-language")).ToHaveTextAsync("English · Latin script");
        await Expect(helen.Page.Locator("#guidance")).ToHaveTextAsync("Use the standard admission guide.");
    }

    private static async Task ChooseDown(ILocator box)
    {
        await box.PressAsync("Alt+ArrowDown");
        await box.PressAsync("ArrowDown");
        await box.PressAsync("Enter");
    }

    private static async Task ChooseUp(ILocator box)
    {
        await box.PressAsync("Alt+ArrowDown");
        await box.PressAsync("ArrowUp");
        await box.PressAsync("Enter");
    }

    private static Task Save(TestWorld world)
        => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save communication plan", Exact = true }).ClickAsync();
}
