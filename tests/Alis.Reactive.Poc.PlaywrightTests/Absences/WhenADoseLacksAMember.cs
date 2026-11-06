namespace Alis.Reactive.Poc.PlaywrightTests.Absences;

[TestFixture]
public sealed class WhenADoseLacksAMember : AppPageTest
{
    [Test]
    public async Task a_number_the_app_omits_at_its_default_reads_its_default()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no refills left", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_text_the_app_omits_when_null_reads_the_value_its_initializer_starts_it_with()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses measured in mg", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_find_whose_field_the_found_dose_lacks_reads_the_fields_start()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Heparin's unit", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#heparin-unit")).ToHaveTextAsync("mg");
    }

    [Test]
    public async Task a_member_its_holder_populates_reads_the_start_of_the_holders_own_object()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses kept on the fridge shelf", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_member_its_holder_replaces_reads_the_start_of_a_new_object_of_its_type()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose spare is in the cupboard", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_member_named_as_one_every_object_inherits_reads_its_start_when_the_dose_lacks_it()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no maker", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task a_holder_that_lacks_a_member_its_type_requires_stops_a_read_through_it_and_the_steps_after_it()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List batch expiries", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listing expiries");
        await Expect(Page.Locator("#census .census-names li")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_holder_that_holds_the_members_its_type_requires_reads_on()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Aspirin's batch expiry", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#aspirin-expiry")).ToHaveTextAsync("2027-01");
    }

    [Test]
    public async Task a_member_of_a_holder_the_endpoint_cannot_create_stops_the_read_and_the_steps_after_it_when_absent()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List seal breakers", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listing breakers");
        await Expect(Page.Locator("#census .census-names li")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_member_of_a_polymorphic_holder_stops_the_read_and_the_steps_after_it_when_absent()
    {
        await Open("/Doses/Absences");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List delivery sites", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listing sites");
        await Expect(Page.Locator("#census .census-names li")).ToHaveCountAsync(0);
    }
}
