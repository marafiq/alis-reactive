using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy;

namespace Alis.Reactive.PlaywrightTests.Validation.PetPolicy;

/// <summary>
/// As the developer of the pet policy page, I want a condition only the server can read declared so and
/// kept with the server, and the same condition refused as the page renders when it is not declared, so
/// that no rule changes sides in silence.
/// </summary>
[TestFixture]
public class WhenThePetPolicyIsDecidedByTheServer : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/PetPolicy";
    private const string ServerRefusal = "the server responded with a status of 400";

    private ConcurrentQueue<string> _savesSent = null!;

    private ILocator SavePolicy => Page.Locator("#save-policy-btn");
    private ILocator PolicyStatus => Page.Locator("#policy-status");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");

    private ILocator Field(Expression<Func<PetPolicyModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator ErrorFor(Expression<Func<PetPolicyModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}_error");

    private async Task NavigateAndBoot()
    {
        _savesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/PetPolicy/Save")) _savesSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task Write(Expression<Func<PetPolicyModel, object?>> member, string text)
    {
        await Field(member).FillAsync(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task a_long_tag_with_no_pet_name_is_sent_and_the_server_asks_for_the_name()
    {
        await NavigateAndBoot();
        await Write(m => m.Tags[0], "Hypoallergenic");

        await ClickWhenStable(SavePolicy);

        await Expect(ErrorFor(m => m.PetName)).ToHaveTextAsync("Name the pet.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(PolicyStatus).ToHaveTextAsync("The policy was not saved.");
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page has no form of the tags query, so it sent the policy and the server decided.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_short_tag_with_no_pet_name_is_saved()
    {
        await NavigateAndBoot();
        await Write(m => m.Tags[0], "Cat");

        await ClickWhenStable(SavePolicy);

        await Expect(PolicyStatus).ToHaveTextAsync("Policy saved with no pet.");
        Assert.That(_savesSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_same_condition_written_as_a_plain_when_is_refused_as_the_page_renders()
    {
        var response = await Page.GotoAsync($"{BaseUrl}{Path}/Refused", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(500), "A condition the browser cannot read, not declared the server's, does not render.");
        await Expect(Page.Locator("body")).ToContainTextAsync("has no browser form");
        await Expect(Page.Locator("body")).ToContainTextAsync("WhenOnServer");
    }
}
