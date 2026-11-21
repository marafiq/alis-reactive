using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenAResidentIsMarkedPresent : AppPageTest
{
    [Test]
    public async Task a_success_route_that_reads_no_body_runs_whatever_body_the_answer_holds()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Resident name").FillAsync("Helen Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark present" }).ClickAsync();

        await Expect(Page.Locator("#presence")).ToHaveTextAsync("Marked present");
    }

    [Test]
    public async Task a_success_route_that_reads_no_body_asks_for_any_kind()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Resident name").FillAsync("Helen Park");

        var asked = await Page.RunAndWaitForRequestAsync(
            () => Page.GetByRole(AriaRole.Button, new() { Name = "Mark present" }).ClickAsync(), new Regex("/Residents/Present/"));

        Assert.That(await asked.HeaderValueAsync("Accept"), Is.EqualTo("*/*"));
    }
}
