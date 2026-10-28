namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// The wrapper an input field renders around its component, by the options it is given (I6): a label names
/// the input it is given for; a required field marks its label and an optional one does not; a field given no
/// options has no label and keeps its message beside it; and a hidden field renders its component alone, so it
/// carries its value with no label, and its message goes to the form's summary.
/// </summary>
[TestFixture]
public sealed class WhenAFieldIsRendered : AppPageTest
{
    private readonly List<string> _signIns = new();

    [SetUp]
    public void WatchTheArrivalEndpoint()
    {
        _signIns.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Inputs/Arrival", StringComparison.Ordinal))
                lock (_signIns) _signIns.Add(request.Method);
        };
    }

    private ILocator Bound => Page.Locator("#bound");

    private ILocator SignIn => Page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true });

    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    private ILocator Label(string text) => Page.Locator("label").Filter(new() { HasText = text });

    [Test]
    public async Task a_label_names_the_input_it_is_given_for()
    {
        await Open("/Inputs/Arrival");

        await Page.GetByLabel("Room", new() { Exact = true }).FillAsync("12");
        await SignIn.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("visitor none, room 12, note none, pass P-7");
    }

    [Test]
    public async Task a_required_field_marks_its_label_and_an_optional_field_does_not()
    {
        await Open("/Inputs/Arrival");

        await Expect(Label("Visitor")).ToHaveTextAsync("Visitor *");
        await Expect(Label("Room")).ToHaveTextAsync("Room");
    }

    [Test]
    public async Task a_field_given_no_options_has_no_label_and_shows_its_message_beside_its_input()
    {
        await Open("/Inputs/Arrival?note=a%20note%20too%20long%20to%20keep");

        await SignIn.ClickAsync();

        await Expect(Page.GetByText("Keep the note to 12 letters.")).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        await Expect(Page.GetByLabel("Note")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_hidden_field_carries_its_value_with_no_label()
    {
        await Open("/Inputs/Arrival");

        await SignIn.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("visitor none, room none, note none, pass P-7");
        await Expect(Page.GetByText("Pass", new() { Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_hidden_fields_message_goes_to_the_forms_summary()
    {
        await Open("/Inputs/Arrival?noPass=true");

        await SignIn.ClickAsync();

        await Expect(Summary).ToContainTextAsync("The desk gives every visitor a pass.");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_signIns) Assert.That(_signIns, Is.Empty, "the browser held the request");
    }
}
