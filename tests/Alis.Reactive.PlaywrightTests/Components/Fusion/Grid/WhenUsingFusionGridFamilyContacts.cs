namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As a front-desk coordinator, I want the family contacts grid to refuse a contact I type wrong as I
/// leave the cell, in the words the server would use, so that the directory never holds a family the
/// desk cannot reach, and never refuses one the server accepts.
/// </summary>
/// <remarks>
/// The cells check the rules <c>FamilyContactValidator</c> gives the browser; the server checks every
/// rule again when the changes are saved. Helen Park's contact is the first row, Arthur Bell's the second.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridFamilyContacts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/FamilyContacts";
    private const string GridId = "family-contacts-grid";

    // The grid numbers its columns from 1.
    private const int ContactColumn = 3;
    private const int PhoneColumn = 4;
    private const int EmailColumn = 5;
    private const int PortalColumn = 6;
    private const int VisitsColumn = 7;

    private ILocator Cells(int column) => Page.Locator($"#{GridId} td[aria-colindex='{column}']");
    private ILocator Editor(string field) => Page.Locator($"#{GridId}{field}");
    private ILocator SaveChanges => Page.Locator("#save-family-contacts-btn");
    private ILocator Status => Page.Locator("#family-contacts-status");
    private ILocator Message(string text) => Page.GetByText(text);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator($"#{GridId} .e-row").First).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // Opens Helen Park's cell and types over what is there. The grid shows a cell's editor holding its
    // value a moment after the double-click (a number's "3.00" becomes "3"); a fill before then is lost.
    private async Task EditHelensCell(int column, string field, string current, string value)
    {
        await Cells(column).First.DblClickAsync();
        await Expect(Editor(field)).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Editor(field)).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await Editor(field).FillAsync(value);
    }

    // Moving on to Arthur Bell's cell in the same column checks the cell being left.
    private async Task MoveToArthursCell(int column) => await Cells(column).Nth(1).DblClickAsync();

    [Test]
    public async Task a_contact_left_without_a_name_is_refused_until_one_is_written()
    {
        await NavigateAndBoot();
        await EditHelensCell(ContactColumn, "contactName", "Daniel O'Brien", "");

        await MoveToArthursCell(ContactColumn);

        await Expect(Message("Name the family contact.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Editor("contactName").FillAsync("Grace Park");
        await ClickWhenStable(SaveChanges);
        await Expect(Status).ToHaveTextAsync("Saved 1 family contact.", new() { Timeout = 10000 });
        await Expect(Cells(ContactColumn).First).ToHaveTextAsync("Grace Park");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_contact_name_of_one_letter_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(ContactColumn, "contactName", "Daniel O'Brien", "D");

        await MoveToArthursCell(ContactColumn);

        await Expect(Message("Write the contact's full name.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_contact_name_over_forty_characters_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(ContactColumn, "contactName", "Daniel O'Brien", "Daniel Alexander Montgomery-Park Wellington");

        await MoveToArthursCell(ContactColumn);

        await Expect(Message("Keep the contact's name under 40 characters.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_phone_not_written_as_the_desk_dials_it_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(PhoneColumn, "phone", "555-0142", "5550142");

        await MoveToArthursCell(PhoneColumn);

        await Expect(Message("Enter the phone as 555-0100.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_email_without_an_at_sign_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(EmailColumn, "email", "daniel.obrien@example.com", "daniel.obrien.example.com");

        await MoveToArthursCell(EmailColumn);

        await Expect(Message("Enter an email address the family reads.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The server accepts any address with one @ and something on each side, so the cell does too.
    [Test]
    public async Task an_email_with_an_apostrophe_is_saved_as_the_server_accepts_it()
    {
        await NavigateAndBoot();
        await EditHelensCell(EmailColumn, "email", "daniel.obrien@example.com", "daniel.o'brien@example.com");

        await ClickWhenStable(SaveChanges);

        await Expect(Status).ToHaveTextAsync("Saved 1 family contact.", new() { Timeout = 10000 });
        await Expect(Cells(EmailColumn).First).ToHaveTextAsync("daniel.o'brien@example.com");
        await Expect(Message("Enter an email address the family reads.")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_portal_link_without_its_web_address_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(PortalColumn, "portalLink", "https://family.maplegrove.example/park", "family.maplegrove.example/park");

        await MoveToArthursCell(PortalColumn);

        await Expect(Message("The portal link must be a web address.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // A phone keyboard capitalizes the first letter; the server reads the address the same either way.
    [Test]
    public async Task a_portal_link_typed_with_a_capital_h_is_saved()
    {
        await NavigateAndBoot();
        await EditHelensCell(PortalColumn, "portalLink", "https://family.maplegrove.example/park", "Https://family.maplegrove.example/obrien");

        await ClickWhenStable(SaveChanges);

        await Expect(Status).ToHaveTextAsync("Saved 1 family contact.", new() { Timeout = 10000 });
        await Expect(Cells(PortalColumn).First).ToHaveTextAsync("Https://family.maplegrove.example/obrien");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task more_than_fourteen_visits_a_week_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(VisitsColumn, "visitsPerWeek", "3", "15");

        await MoveToArthursCell(VisitsColumn);

        await Expect(Message("The front desk books at most 14 visits a week.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_negative_number_of_visits_is_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditHelensCell(VisitsColumn, "visitsPerWeek", "3", "-1");

        await MoveToArthursCell(VisitsColumn);

        await Expect(Message("Visits per week cannot be negative.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
