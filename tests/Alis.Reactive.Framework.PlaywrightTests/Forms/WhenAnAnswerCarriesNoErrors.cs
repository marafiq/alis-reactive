namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

[TestFixture]
public sealed class WhenAnAnswerCarriesNoErrors : AppPageTest
{
    private const string NameTheVisitor = "Name the visitor.";
    private const string SayWhom = "Say whom the visitor came to see.";

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator VisitorField => Page.Locator(".alis-field").Filter(new() { Has = Page.GetByLabel("Visitor", new() { Exact = true }) });

    private ILocator Summary => Page.Locator("#gate-form_validation_summary");

    [Test]
    public async Task an_answer_of_text_clears_the_messages_an_earlier_answer_showed()
    {
        await Open("/Forms/Gatehouse");
        await Button("Sign the visitor in").ClickAsync();
        await Expect(VisitorField.GetByText(NameTheVisitor)).ToBeVisibleAsync();
        await Expect(Summary).ToHaveTextAsync(SayWhom);

        await Button("Sign in while the gate's server is down").ClickAsync();

        await Expect(VisitorField.GetByText(NameTheVisitor)).ToBeHiddenAsync();
        await Expect(Summary).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_dropped_connection_clears_the_messages_an_earlier_answer_showed()
    {
        await Open("/Forms/Gatehouse");
        await Button("Sign the visitor in").ClickAsync();
        await Expect(VisitorField.GetByText(NameTheVisitor)).ToBeVisibleAsync();
        await Expect(Summary).ToHaveTextAsync(SayWhom);

        await Button("Sign in when the gate's line drops").ClickAsync();

        await Expect(VisitorField.GetByText(NameTheVisitor)).ToBeHiddenAsync();
        await Expect(Summary).ToBeHiddenAsync();
    }
}
