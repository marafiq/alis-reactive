using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenTheRegisterIsAsked : AppPageTest
{
    private static readonly Regex Summaries = new("/Residents/Summary/");

    [Test]
    public async Task leaving_a_name_shows_the_room_the_register_answers_with()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#room")).ToHaveTextAsync("12");
    }

    [Test]
    public async Task a_member_of_an_object_the_answer_holds_is_shown()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#wing")).ToHaveTextAsync("Rose");
    }

    [Test]
    public async Task a_resident_above_the_ground_floor_is_shown_to_need_the_lift()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#access")).ToHaveTextAsync("Needs the lift");
    }

    [Test]
    public async Task a_resident_on_the_ground_floor_is_shown_as_step_free()
    {
        await Open("/Residents/Lookup");
        await LookUp("Helen Park");
        await Expect(Page.Locator("#access")).ToHaveTextAsync("Needs the lift");

        await LookUp("Sam Park");

        await Expect(Page.Locator("#access")).ToHaveTextAsync("Step-free");
    }

    [Test]
    public async Task a_resident_given_a_controlled_medicine_needs_a_second_signature()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#signing")).ToHaveTextAsync("A second nurse signs each round");
    }

    [Test]
    public async Task a_resident_given_no_controlled_medicine_needs_no_second_signature()
    {
        await Open("/Residents/Lookup");
        await LookUp("Helen Park");
        await Expect(Page.Locator("#signing")).ToHaveTextAsync("A second nurse signs each round");

        await LookUp("Sam Park");

        await Expect(Page.Locator("#signing")).ToHaveTextAsync("No second signature");
    }

    [Test]
    public async Task the_ward_typed_reaches_the_endpoint_in_the_query()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Garden");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#asked-ward")).ToHaveTextAsync("Garden");
    }

    [Test]
    public async Task a_member_set_to_text_reaches_the_endpoint()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#asked-home")).ToHaveTextAsync("Maple Court");
    }

    [Test]
    public async Task a_name_holding_a_question_mark_reaches_the_endpoint_whole()
    {
        await Open("/Residents/Lookup");

        await LookUp("Who?");

        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("No resident is named Who?");
    }

    [Test]
    public async Task an_unknown_name_runs_only_the_not_found_route_with_its_body()
    {
        await Open("/Residents/Lookup");

        await LookUp("Ada Byron");

        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("No resident is named Ada Byron");
        await Expect(Page.Locator("#room")).ToHaveTextAsync("no resident yet");
    }

    [Test]
    public async Task a_not_found_answer_that_is_not_json_runs_the_not_found_route_reading_nothing()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("West");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#lookup-route")).ToHaveTextAsync("Not on the register");
        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_register_that_fails_runs_the_route_for_every_other_outcome()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Annex");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("The register could not answer");
    }

    [Test]
    public async Task an_answer_that_is_not_json_runs_the_route_for_every_other_outcome()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("East");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("The register could not answer");
    }

    [Test]
    public async Task an_answer_cut_short_runs_the_route_for_every_other_outcome()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("North");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#lookup-status")).ToHaveTextAsync("The register could not answer");
    }

    [Test]
    public async Task an_empty_name_sends_no_request()
    {
        await Open("/Residents/Lookup");

        var first = await Page.RunAndWaitForRequestAsync(async () =>
        {
            await LookUp("");
            await LookUp("Helen Park");
        }, Summaries);

        Assert.That(first.Url, Is.EqualTo(BaseUrl + "/Residents/Summary/Helen%20Park?Ward=South&Home=Maple+Court"));
    }

    [Test]
    public async Task a_name_holding_a_slash_sends_no_request()
    {
        await Open("/Residents/Lookup");

        var first = await Page.RunAndWaitForRequestAsync(async () =>
        {
            await LookUp("Helen/Park");
            await LookUp("Helen Park");
        }, Summaries);

        Assert.That(first.Url, Is.EqualTo(BaseUrl + "/Residents/Summary/Helen%20Park?Ward=South&Home=Maple+Court"));
    }

    private async Task LookUp(string name)
    {
        await Page.GetByLabel("Resident name").FillAsync(name);
        await Page.GetByLabel("Resident name").PressAsync("Tab");
    }
}
