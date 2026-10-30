namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// A page written against a declaration newer than the component it runs (grammar 1.3). A member path rooted
/// at the component is checked: a read, a write or a call of a member the component does not have, and a read
/// through a member that holds nothing, each fail only their own pipeline (the step before ran, the step after
/// did not, and the next gesture works). A member read off an event's payload is optional: one the component
/// does not pass reads as nothing, and the pipeline goes on.
/// </summary>
[TestFixture]
public sealed class WhenADeclarationIsNewerThanItsComponent : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_read_of_a_member_the_component_does_not_have_fails_only_its_own_pipeline()
    {
        await Open("/Vendors/NewerDeclaration");

        await Button("Read the pins").ClickAsync();
        await Expect(Page.Locator("#tried")).ToHaveTextAsync("reading the pins");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Round");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_write_to_a_member_the_component_does_not_have_fails_only_its_own_pipeline()
    {
        await Open("/Vendors/NewerDeclaration");

        await Button("Mark the slate pinned").ClickAsync();
        await Expect(Page.Locator("#tried")).ToHaveTextAsync("marking it pinned");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Round");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_call_of_a_method_the_component_does_not_have_fails_only_its_own_pipeline()
    {
        await Open("/Vendors/NewerDeclaration");

        await Button("Pin the slate").ClickAsync();
        await Expect(Page.Locator("#tried")).ToHaveTextAsync("pinning it");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Round");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_read_through_a_member_that_holds_nothing_fails_only_its_own_pipeline()
    {
        await Open("/Vendors/NewerDeclaration");

        await Button("Read the last card's name").ClickAsync();
        await Expect(Page.Locator("#tried")).ToHaveTextAsync("reading the last card");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Round");
        await Expect(Page.Locator("#finished")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_payload_member_the_component_does_not_pass_reads_as_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Vendors/NewerDeclaration");

        await Page.Locator("#slate").GetByRole(AriaRole.Button, new() { Name = "Add row", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#added-read")).ToHaveTextAsync("read");
        await Expect(Page.Locator("#added-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#added-row")).ToBeEmptyAsync();
    }
}
