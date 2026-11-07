namespace Alis.Reactive.Poc.PlaywrightTests.Desks;

[TestFixture]
public sealed class WhenADeskHandsOverAMonitor : AppPageTest
{
    [Test]
    public async Task a_component_the_payload_is_gives_its_read_only_member()
    {
        await Open("/Desks/Desk");

        await Click("Pick ben-monitor");

        await Expect(Page.Locator("#picked")).ToHaveTextAsync("Ben");
    }

    [Test]
    public async Task a_component_the_payload_is_gives_its_property()
    {
        await Open("/Desks/Desk");

        await Click("Pick ada-monitor");

        await Expect(Page.Locator("#picked-status")).ToHaveTextAsync("idle");
    }

    [Test]
    public async Task a_component_the_payload_is_takes_a_write()
    {
        await Open("/Desks/Desk");

        await Click("Pick ada-monitor");

        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: watched");
        await Expect(Page.Locator("#ben-monitor .desk-monitor-status")).ToHaveTextAsync("Ben: idle");
    }

    [Test]
    public async Task a_component_the_payload_is_takes_a_call()
    {
        await Open("/Desks/Desk");

        await Click("Pick ada-monitor");

        await Expect(Page.Locator("#ada-monitor .desk-monitor-paged")).ToHaveTextAsync("Ada paged");
        await Expect(Page.Locator("#ben-monitor .desk-monitor-paged")).ToHaveTextAsync("Ben not paged");
    }

    [Test]
    public async Task a_component_the_payload_holds_gives_its_read_only_member()
    {
        await Open("/Desks/Desk");

        await Click("Hand over ben-monitor");

        await Expect(Page.Locator("#handed")).ToHaveTextAsync("Ben");
    }

    [Test]
    public async Task a_component_the_payload_holds_gives_its_property()
    {
        await Open("/Desks/Desk");
        await Click("Pick ben-monitor");
        await Expect(Page.Locator("#ben-monitor .desk-monitor-status")).ToHaveTextAsync("Ben: watched");

        await Click("Hand over ben-monitor");

        await Expect(Page.Locator("#handed-status")).ToHaveTextAsync("watched");
    }

    [Test]
    public async Task a_component_the_payload_holds_takes_a_write()
    {
        await Open("/Desks/Desk");

        await Click("Hand over ben-monitor");

        await Expect(Page.Locator("#ben-monitor .desk-monitor-status")).ToHaveTextAsync("Ben: handed over");
        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: idle");
    }

    [Test]
    public async Task a_component_the_payload_holds_takes_a_call()
    {
        await Open("/Desks/Desk");

        await Click("Hand over ben-monitor");

        await Expect(Page.Locator("#ben-monitor .desk-monitor-paged")).ToHaveTextAsync("Ben paged");
        await Expect(Page.Locator("#ada-monitor .desk-monitor-paged")).ToHaveTextAsync("Ada not paged");
    }

    [Test]
    public async Task a_component_the_payload_is_takes_a_write_after_an_await()
    {
        await Open("/Desks/Check");

        await Click("Pick ben-monitor");

        await Expect(Page.Locator("#checked")).ToHaveTextAsync("Ben is steady");
        await Expect(Page.Locator("#ben-monitor .desk-monitor-status")).ToHaveTextAsync("Ben: checked");
        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: idle");
    }

    [Test]
    public async Task a_component_the_payload_is_takes_a_write_inside_a_listen()
    {
        await Open("/Desks/Alarms");
        await Click("Pick ada-monitor");

        await Click("Sound Ada's monitor");

        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: alarmed");
        await Expect(Page.Locator("#ben-monitor .desk-monitor-status")).ToHaveTextAsync("Ben: idle");
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
