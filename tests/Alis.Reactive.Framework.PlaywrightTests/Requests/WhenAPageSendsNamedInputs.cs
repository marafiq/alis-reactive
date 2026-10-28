namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A request that names the inputs it sends (<c>g.Form(m =&gt; m.A, m =&gt; m.B)</c>), each reaching an endpoint that
/// says what it bound: only the inputs named, each by its binding name and read as MVC binds it (blank text as
/// nothing, a number left empty as zero), into a request
/// member of a type the input's value binds to (the same type, one that may be empty, a base type, another
/// collection of the same items), matched ignoring case, through a list item or into a member that takes any
/// name; and in a partial rendered for a member of the page's model, under that member's prefix.
/// </summary>
[TestFixture]
public sealed class WhenAPageSendsNamedInputs : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private async Task Type(string label, string text) => await Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    // A text file as the person's computer keeps it, chosen with the file picker labelled for it.
    private async Task Choose(string label, string name, string text)
    {
        var folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "alis-requests-" + Guid.NewGuid().ToString("N")));
        var file = Path.Combine(folder.FullName, name);
        await File.WriteAllTextAsync(file, text);
        await Page.GetByLabel(label).SetInputFilesAsync(file);
    }

    [Test]
    public async Task a_named_form_sends_the_inputs_it_names_and_no_other()
    {
        await Open("/Requests/SignIn");
        await Type("Visitor", "Ada");
        await Type("Party", "3");
        await Type("Note", "Brought flowers");

        await Button("Sign in").ClickAsync();

        await Expect(Line("signed-in")).ToHaveTextAsync("Ada, party of 3, note none");
    }

    [Test]
    public async Task a_named_input_left_blank_is_sent_as_mvc_binds_an_empty_input()
    {
        await Open("/Requests/SignIn");
        await Type("Visitor", "   ");
        await Type("Party", "");

        await Button("Sign in").ClickAsync();

        await Expect(Line("signed-in")).ToHaveTextAsync("no one, party of 0, note none");
    }

    [Test]
    public async Task a_named_input_reaches_a_request_member_whose_name_differs_only_in_case()
    {
        await Open("/Requests/SignIn");
        await Type("Pass", "12");

        await Button("Show the pass").ClickAsync();

        await Expect(Line("pass")).ToHaveTextAsync("pass 12");
    }

    [Test]
    public async Task a_named_input_reaches_a_request_member_of_its_type_that_may_be_empty()
    {
        await Open("/Requests/SignIn");
        await Type("Party", "3");

        await Button("Count the party").ClickAsync();

        await Expect(Line("party")).ToHaveTextAsync("party of 3");
    }

    [Test]
    public async Task a_named_input_of_a_list_item_reaches_the_item_the_request_lists()
    {
        await Open("/Requests/SignIn");
        await Type("First line", "Fragile");

        await Button("Note the first line").ClickAsync();

        await Expect(Line("line")).ToHaveTextAsync("first line: Fragile");
    }

    [Test]
    public async Task a_named_input_of_a_dictionary_entry_reaches_the_entry_of_the_requests_dictionary()
    {
        await Open("/Requests/SignIn");
        await Type("Dose for 1042", "two tablets");

        await Button("Record the dose").ClickAsync();

        await Expect(Line("dose")).ToHaveTextAsync("dose for 1042: two tablets");
    }

    [Test]
    public async Task a_named_input_below_a_request_member_that_takes_any_value_reaches_it()
    {
        await Open("/Requests/SignIn");
        await Type("Guide", "Ben");

        await Button("Keep the guide").ClickAsync();

        await Expect(Line("extras")).ToHaveTextAsync("guide {\"Name\":\"Ben\"}");
    }

    [Test]
    public async Task a_named_input_reaches_a_request_member_of_a_type_its_own_type_derives_from()
    {
        await Open("/Requests/CarePlan");
        await Choose("Care plan", "plan.txt", "Walk daily.");

        await Button("Send the care plan as a sequence").ClickAsync();

        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }

    [Test]
    public async Task a_named_input_reaches_a_request_member_that_is_another_collection_of_the_same_items()
    {
        await Open("/Requests/CarePlan");
        await Choose("Care plan", "plan.txt", "Walk daily.");

        await Button("Send the care plan as an array").ClickAsync();

        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }

    [Test]
    public async Task a_named_input_bound_to_a_sequence_reaches_a_request_member_that_is_another_collection_of_its_items()
    {
        await Open("/Requests/Consents");
        await Choose("Consents", "consent.txt", "Signed.");

        await Button("Send the consents").ClickAsync();

        await Expect(Line("received")).ToHaveTextAsync("received consent.txt, 7 bytes of text/plain: Signed.");
    }

    [Test]
    public async Task a_named_input_in_a_partial_is_sent_under_the_prefix_of_the_member_it_was_rendered_for()
    {
        await Open("/Requests/Escort?gather=AsTheCard");
        await Type("Escort", "Ben");

        await Button("Send the escort").ClickAsync();

        await Expect(Line("escort-bound")).ToHaveTextAsync("escort Ben");
    }

    [Test]
    public async Task a_named_input_in_a_partial_reaches_the_page_request_that_binds_it_under_the_prefix()
    {
        await Open("/Requests/Escort?gather=AsTheVisit");
        await Type("Escort", "Ben");

        await Button("Send the escort").ClickAsync();

        await Expect(Line("escort-bound")).ToHaveTextAsync("escort Ben");
    }
}
