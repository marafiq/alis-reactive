namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenADigitCountPassesWhatABrowserNumberPrints : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/PharmacyOrder", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#order-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_count_past_six_decimals_is_the_servers_which_refuses_one_decimal_too_many()
    {
        await Open("/Validation/PharmacyOrder");

        await Write("Amount", "0.123456789");
        await Save();
        await Expect(Field("Amount").GetByText(
            "'Amount' must not be more than 9 digits in total, with allowance for 8 decimals. 10 digits and 9 decimals were found.",
            new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the page carries no rule a browser number cannot print in place, so the request goes");

        await Write("Amount", "0.12345678");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_count_past_six_decimals_takes_a_number_a_browser_prints_with_an_exponent()
    {
        await Open("/Validation/PharmacyOrder");

        await Write("Amount", "0.00000012");
        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_count_past_twenty_one_digits_before_the_point_is_the_servers_which_refuses_one_digit_too_many()
    {
        await Open("/Validation/PharmacyOrder");

        await Write("Ledger", "1000000000000000000000000");
        await Save();
        await Expect(Field("Ledger").GetByText(
            "'Ledger' must not be more than 24 digits in total, with allowance for 0 decimals. 25 digits and 0 decimals were found.",
            new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the page carries no rule a browser number cannot print in place, so the request goes");

        await Write("Ledger", "100000000000000000000000");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_count_past_twenty_one_digits_before_the_point_takes_a_number_a_browser_prints_with_an_exponent()
    {
        await Open("/Validation/PharmacyOrder");

        await Write("Ledger", "1000000000000000000000");
        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_nullable_count_past_six_decimals_is_the_servers_which_refuses_one_decimal_too_many_and_takes_no_value()
    {
        await Open("/Validation/PharmacyOrder");

        await Write("Copay", "0.123456789");
        await Save();
        await Expect(Field("Copay").GetByText(
            "'Copay' must not be more than 9 digits in total, with allowance for 8 decimals. 10 digits and 9 decimals were found.",
            new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the page carries no rule a browser number cannot print in place, so the request goes");

        await Write("Copay", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
