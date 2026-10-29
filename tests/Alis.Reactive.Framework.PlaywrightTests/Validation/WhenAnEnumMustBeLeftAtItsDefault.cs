namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// <c>Empty</c> on an enum, decided in the browser as the server decides it: the member the enum's default
/// binds as is empty, so a guest's meal from the regular menu is ordered; any other menu is held beside the
/// menu, and the server's own rules return it with the same message.
/// </summary>
[TestFixture]
public sealed class WhenAnEnumMustBeLeftAtItsDefault : AppPageTest
{
    private readonly List<string> _orders = new();

    // Every meal that reached the ordering endpoint, as the network carried it.
    [SetUp]
    public void WatchTheOrderingEndpoint()
    {
        _orders.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/GuestMeal", StringComparison.Ordinal))
                lock (_orders) _orders.Add(request.Method);
        };
    }

    private ILocator MenuRefused =>
        Page.Locator("#meal-form > div")
            .Filter(new() { Has = Page.GetByLabel("Menu") })
            .GetByText("Guests are served the regular menu.", new() { Exact = true });

    private ILocator MealStatus => Page.Locator("#meal-status");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_meal_from_the_regular_menu_is_ordered()
    {
        await Open("/Validation/GuestMeal");

        await Click("Order the meal");

        await Expect(MealStatus).ToHaveTextAsync("ordered from the Regular menu");
    }

    [Test]
    public async Task a_meal_from_another_menu_is_held_beside_the_menu()
    {
        await Open("/Validation/GuestMeal");
        await Page.GetByLabel("Menu").SelectOptionAsync("LowSodium");

        await Click("Order the meal");

        await Expect(MenuRefused).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_orders) Assert.That(_orders, Is.Empty, "the browser held the order");
    }

    [Test]
    public async Task the_server_returns_a_meal_from_another_menu_with_the_same_message()
    {
        await Open("/Validation/GuestMeal");
        await Page.GetByLabel("Menu").SelectOptionAsync("LowSodium");

        await Click("Ask the kitchen to make it");

        await Expect(MealStatus).ToHaveTextAsync("returned by the kitchen");
        await Expect(MenuRefused).ToBeVisibleAsync();
    }
}
