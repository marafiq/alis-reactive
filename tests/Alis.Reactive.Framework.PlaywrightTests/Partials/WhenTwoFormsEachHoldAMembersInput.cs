namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A visit and a sitting are booked on one page, each in a form of its own, and each form holds an input that binds
/// <c>NextOfKin.Name</c>. The visit's rule reads the member from the input inside the visit's form before any
/// elsewhere on the page (grammar.md M8), as the visit's save sends only what its form holds.
/// </summary>
[TestFixture]
public sealed class WhenTwoFormsEachHoldAMembersInput : AppPageTest
{
    private readonly List<string> _bookings = new();

    [SetUp]
    public void WatchTheBookingsEndpoint()
    {
        _bookings.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Partials/BookVisit", StringComparison.Ordinal))
                lock (_bookings) _bookings.Add(request.Url);
        };
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator Field(string label) => Page.Locator(".alis-field").Filter(new() { Has = Input(label) });

    [Test]
    public async Task a_forms_rule_reads_the_member_from_the_input_inside_its_form()
    {
        await Open("/Partials/Bookings");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Book the visit").ClickAsync();

        await Expect(Field("Visitor").GetByText("Name the visitor who brings the next of kin.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_bookings) Assert.That(_bookings, Is.Empty, "the browser held the visit");
    }

    [Test]
    public async Task a_forms_rule_does_not_read_the_member_from_an_input_outside_its_form()
    {
        await Open("/Partials/Bookings");
        await Input("The sitting's next of kin").FillAsync("Bo");

        await Button("Book the visit").ClickAsync();

        await Expect(Page.Locator("#visit-booked")).ToHaveTextAsync("visitor none, next of kin none");
    }
}
