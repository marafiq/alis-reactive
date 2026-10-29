namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// An input whose binding names no class to scope its id is refused when the page renders (ADR-0003 rule 1),
/// with the message the developer reads: a member an interface declares, read on the interface.
/// </summary>
[TestFixture]
public sealed class WhenAnInputsIdCannotFollowItsMember : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Partials/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_partial_typed_to_an_interface_is_refused_naming_the_fix()
    {
        await Render("TypeAPartialToAnInterface");

        await Expect(Message(
            "Input 'Visitor' binds a member that INamedVisitor declares. An input's id is scoped by the class that declares " +
            "its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a " +
            "base class, that declares it.")).ToBeVisibleAsync();
    }
}
