namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

/// <summary>
/// A plan of its own for one element (<c>ElementPlan</c>), identified by the element's id: it composes with no
/// other plan, so two cards of one model, on a page with no plan of its own, each send only the input they
/// rendered.
/// </summary>
[TestFixture]
public sealed class WhenAnElementHasAPlanOfItsOwn : AppPageTest
{
    [Test]
    public async Task each_element_plan_sends_only_the_inputs_it_rendered()
    {
        await Open("/Plans/Contacts");
        await Page.GetByLabel("The next of kin's name", new() { Exact = true }).FillAsync("Tom");
        await Page.GetByLabel("The alternate's name", new() { Exact = true }).FillAsync("Ann");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the next of kin", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#nextofkin-saved")).ToHaveTextAsync("next of kin Tom, alternate none");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the alternate", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#alternate-saved")).ToHaveTextAsync("next of kin none, alternate Ann");
    }
}
