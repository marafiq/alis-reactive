namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenTheRegisterAnswersWithHeaders : AppPageTest
{
    [Test]
    public async Task a_header_is_read_by_its_name_in_any_case()
    {
        await Open("/Residents/Lookup");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Main register");
    }

    [Test]
    public async Task the_route_for_a_status_reads_the_header_of_its_answer()
    {
        await Open("/Residents/Lookup");

        await LookUp("Ada Byron");

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Main register");
    }

    [Test]
    public async Task the_route_for_every_other_outcome_reads_the_header_of_a_failed_status()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Annex");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-note")).ToHaveTextAsync("the register is being repaired");
    }

    [Test]
    public async Task the_route_for_every_other_outcome_reads_the_header_of_an_answer_not_of_the_declared_kind()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("East");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Main register");
    }

    [Test]
    public async Task a_header_the_answer_lacks_shows_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("East");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-note")).ToHaveTextAsync("");
        await Expect(Page.Locator("#failure")).ToHaveTextAsync("It answered what does not read as JSON");
    }

    [Test]
    public async Task a_header_the_answer_sends_twice_reads_its_values_joined_by_a_comma()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Lodge");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-note")).ToHaveTextAsync("the register is being repaired, ask at the front desk");
    }

    [Test]
    public async Task a_header_of_a_network_failure_shows_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Basement");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("");
        await Expect(Page.Locator("#failure")).ToHaveTextAsync("The network failed");
    }

    [Test]
    public async Task an_answer_redirected_within_the_pages_origin_reads_the_header_of_its_final_answer()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Relocated");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#lookup-route")).ToHaveTextAsync("On the register");
        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Main register");
    }

    [Test]
    public async Task an_answer_redirected_to_another_origin_reads_the_header_that_origin_exposes()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Moved");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Elsewhere register");
    }

    [Test]
    public async Task a_safelisted_header_of_an_answer_redirected_to_another_origin_reads_as_that_origin_answers_it()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Moved");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-type")).ToHaveTextAsync("application/json; charset=utf-8");
    }

    [Test]
    public async Task a_header_the_other_origin_does_not_expose_shows_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Moved");

        await LookUp("Helen Park");

        await Expect(Page.Locator("#register-note")).ToHaveTextAsync("");
        await Expect(Page.Locator("#failure")).ToHaveTextAsync("It moved to another site");
    }

    [Test]
    public async Task a_header_name_that_is_a_token_is_read_from_the_answer()
    {
        await Open("/Residents/ReplyHeaderNamed?name=X-Register-Copy");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Look up Helen Park" }).ClickAsync();

        await Expect(Page.Locator("#register-copy")).ToHaveTextAsync("Main register");
    }

    [Test]
    public async Task a_header_name_that_is_no_token_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ReplyHeaderNamed?name=X%20Register");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'X Register' is no header name: a header name is letters, digits and !#$%&'*+-.^_`|~, as X-Requested-With. (Parameter 'name')");
    }

    [Test]
    public async Task set_cookie_is_refused_in_any_case_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ReplyHeaderNamed?name=set-cookie");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'set-cookie' is a header the browser never hands a page (the Fetch Standard's forbidden response header names), so it reads null on every answer: send what the page needs in a header of its own. (Parameter 'name')");
    }

    [Test]
    public async Task set_cookie2_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ReplyHeaderNamed?name=Set-Cookie2");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'Set-Cookie2' is a header the browser never hands a page (the Fetch Standard's forbidden response header names), so it reads null on every answer: send what the page needs in a header of its own. (Parameter 'name')");
    }

    private async Task LookUp(string name)
    {
        await Page.GetByLabel("Resident name").FillAsync(name);
        await Page.GetByLabel("Resident name").PressAsync("Tab");
    }
}
