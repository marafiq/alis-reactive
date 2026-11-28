using System.Text.RegularExpressions;
using System.Web;

namespace Alis.Reactive.Poc.PlaywrightTests.Handovers;

[TestFixture]
public sealed class WhenTheRegisterIsAskedFromLondon : AppPageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "Europe/London";
        return options;
    }

    [Test]
    public async Task a_moment_on_a_clock_at_offset_zero_is_sent_with_z()
    {
        await Open("/Handovers/Ask");

        var asked = await Page.RunAndWaitForRequestAsync(
            () => Page.GetByRole(AriaRole.Button, new() { Name = "Ask the register", Exact = true }).ClickAsync(), new Regex("/Handovers/Register"));

        Assert.That(HttpUtility.ParseQueryString(new Uri(asked.Url).Query)["StartsAt"], Is.EqualTo("2026-11-09T07:00:00Z"));
    }
}
