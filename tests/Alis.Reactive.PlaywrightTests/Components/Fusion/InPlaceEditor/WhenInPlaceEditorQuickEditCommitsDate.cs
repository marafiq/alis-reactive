using System.Collections.Concurrent;
using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenInPlaceEditorQuickEditCommitsDate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task typing_date_and_pressing_enter_posts_and_updates_display()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");

        await Page.Locator("#card-dob .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-dob input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("5/15/2000");

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateDateOfBirth")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        // Enter keydown follows Syncfusion's submit path more reliably than a synthetic save-button click.
        await inner.PressAsync("Enter");

        var request = await requestTask;
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("2000-05-15").Or.Contain("5/15/2000"),
            "POST body must carry the committed date");
        Assert.That(body, Does.Contain("resident-42"),
            "POST body must carry the ResidentId identity from the hidden field");

        await Expect(Page.Locator("#card-dob-display"))
            .Not.ToHaveTextAsync("unchanged", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // The card validates the committed date with DateOfBirthQuickEditValidator: "before today".
    [Test]
    public async Task a_date_of_birth_of_today_is_refused_before_it_is_sent()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");
        var commits = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateDateOfBirth")) commits.Enqueue(request.Url);
        };
        var valueWrapper = Page.Locator("#card-dob .e-editable-value-wrapper").First;
        var inner = Page.Locator("#card-dob input.e-control").First;

        await valueWrapper.ClickAsync();
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync(DateTime.Today.ToString("M/d/yyyy", CultureInfo.InvariantCulture));
        await inner.PressAsync("Enter");

        // Today is the boundary: "before today" refuses it, where "at most today" would accept it.
        await Expect(Page.Locator($"#{DateOfBirthId}_error"))
            .ToHaveTextAsync("'Value' must be before today.", new() { Timeout = 5000 });
        Assert.That(commits, Is.Empty, "A date of birth of today is not sent.");
        await Expect(Page.Locator("#card-dob-display")).ToHaveTextAsync("unchanged");

        await valueWrapper.ClickAsync();
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("5/15/2000");
        await inner.PressAsync("Enter");

        await Expect(Page.Locator("#card-dob-display"))
            .ToHaveTextAsync("2000-05-15 (resident=resident-42)", new() { Timeout = 5000 });
        Assert.That(commits, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    private static readonly string DateOfBirthId = IdGenerator.For<DateOfBirthQuickEdit>(m => m.Value);
}
