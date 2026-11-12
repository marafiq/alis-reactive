using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestTakesAMemberOfPlaywright
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    private readonly IPage _page = null!;\n    private readonly IConsoleMessage _message = null!;\n\n    [Test]\n    public async Task the_round_is_listed()\n    {\n";
    private const string Closing = "\n    }\n}\n";

    [RefusedCase]
    public Task script_in_the_page_a_stubbed_network_a_one_shot_read_and_a_raised_event_are_refused_each_at_its_name()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        _ = await _page.{|#0:EvaluateAsync<string>|}(\"() => document.title\");\n"
                + "        await _page.{|#1:WaitForFunctionAsync|}(\"() => window.ready\");\n"
                + "        await _page.{|#2:RouteAsync|}(\"**/rounds\", route => Task.CompletedTask);\n"
                + "        _ = await _page.GetByRole(AriaRole.Heading).{|#3:InnerTextAsync|}();\n"
                + "        _ = await _page.GetByRole(AriaRole.Heading).{|#4:IsVisibleAsync|}();\n"
                + "        await _page.GetByRole(AriaRole.Button).{|#5:DispatchEventAsync|}(\"click\");"
                + Closing)
            .Expecting(
                Outside(0, "IPage.EvaluateAsync"),
                Outside(1, "IPage.WaitForFunctionAsync"),
                Outside(2, "IPage.RouteAsync"),
                Outside(3, "ILocator.InnerTextAsync"),
                Outside(4, "ILocator.IsVisibleAsync"),
                Outside(5, "ILocator.DispatchEventAsync"))
            .RunAsync();

    [RefusedCase]
    public Task a_property_read_and_an_event_subscribed_outside_the_lists_are_refused_each_at_its_name()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        _ = _page.{|#0:Url|};\n"
                + "        _page.{|#1:Dialog|} += (_, _) => { };\n"
                + "        _ = _page.GetByRole(AriaRole.Button).{|#2:Page|};\n"
                + "        _ = _page?.{|#3:Url|};"
                + Closing)
            .Expecting(Outside(0, "IPage.Url"), Outside(1, "IPage.Dialog"), Outside(2, "ILocator.Page"), Outside(3, "IPage.Url"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_a_static_class_of_playwrights_outside_the_lists_is_refused_at_its_name()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        {|#0:SetDefaultExpectTimeout|}(1000);\n"
                + "        Assertions.{|#1:SetDefaultExpectTimeout|}(1000);\n"
                + "        using var playwright = await Playwright.{|#2:CreateAsync|}();"
                + Closing)
            .Expecting(Outside(0, "Assertions.SetDefaultExpectTimeout"), Outside(1, "Assertions.SetDefaultExpectTimeout"), TheHarnesss(2, "Playwright.CreateAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task the_gestures_the_finding_the_expecting_and_what_is_read_beside_the_page_are_accepted()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await _page.GotoAsync(\"/rounds\");\n"
                + "        await _page.ReloadAsync();\n"
                + "        await _page.GoBackAsync();\n"
                + "        await _page.GoForwardAsync();\n"
                + "        var button = _page.GetByRole(AriaRole.Button, new() { Name = \"Start\" });\n"
                + "        await button.ClickAsync();\n"
                + "        await button.DblClickAsync();\n"
                + "        await button.HoverAsync();\n"
                + "        var name = _page.GetByLabel(\"Name\");\n"
                + "        await name.FillAsync(\"Ada\");\n"
                + "        await name.ClearAsync();\n"
                + "        await name.PressAsync(\"Enter\");\n"
                + "        await name.PressSequentiallyAsync(\"Ada\");\n"
                + "        await _page.GetByLabel(\"Wing\").SelectOptionAsync(\"North\");\n"
                + "        var box = _page.GetByText(\"On call\");\n"
                + "        await box.CheckAsync();\n"
                + "        await box.UncheckAsync();\n"
                + "        await box.SetCheckedAsync(true);\n"
                + "        await box.DragToAsync(button);\n"
                + "        await _page.Locator(\"#photo\").SetInputFilesAsync(\"photo.png\");\n"
                + "        await _page.Keyboard.PressAsync(\"Escape\");\n"
                + "        await _page.Mouse.MoveAsync(1, 1);\n"
                + "        var row = _page.Locator(\"#rounds\").GetByRole(AriaRole.Row).First.Locator(\"#first\").Filter(new() { HasText = \"Ada\" }).And(button).Or(button).Nth(0).Last;\n"
                + "        var frame = _page.FrameLocator(\"#answer\").FrameLocator(\"#inner\");\n"
                + "        var inside = row.FrameLocator(\"#frame\").Locator(\"#cell\").ContentFrame.GetByLabel(\"Name\").GetByText(\"Ada\").GetByLabel(\"Wing\");\n"
                + "        var paragraph = frame.GetByRole(AriaRole.Paragraph);\n"
                + "        var word = frame.GetByText(\"South\");\n"
                + "        await Expect(row).ToHaveTextAsync(\"Ada\");\n"
                + "        await Expect(name).ToHaveValueAsync(\"Ada\");\n"
                + "        await Expect(inside).ToHaveValuesAsync(new[] { \"North\" });\n"
                + "        await Expect(box).ToBeCheckedAsync();\n"
                + "        await Expect(paragraph).ToBeVisibleAsync();\n"
                + "        await Expect(word).Not.ToBeHiddenAsync();\n"
                + "        await Expect(name).ToBeFocusedAsync();\n"
                + "        await Expect(button).ToHaveClassAsync(\"primary\");\n"
                + "        await Expect(button).ToHaveAttributeAsync(\"type\", \"button\");\n"
                + "        await Expect(button).ToHaveIdAsync(\"start\");\n"
                + "        await Expect(_page.GetByRole(AriaRole.Row)).ToHaveCountAsync(2);\n"
                + "        await Expect(name).ToBeEmptyAsync();\n"
                + "        await Expect(_page).ToHaveURLAsync(\"/rounds\");\n"
                + "        await Expect(_page).Not.ToHaveURLAsync(\"/residents\");\n"
                + "        var request = await _page.RunAndWaitForRequestAsync(() => button.ClickAsync(), \"**/rounds\");\n"
                + "        Assert.That(request.Url, Is.EqualTo(\"/rounds\"));\n"
                + "        Assert.That(request.Method, Is.EqualTo(\"POST\"));\n"
                + "        Assert.That(request.PostData, Is.EqualTo(\"{}\"));\n"
                + "        Assert.That(request.Headers[\"x-round\"], Is.EqualTo(\"1\"));\n"
                + "        Assert.That(await request.HeaderValueAsync(\"x-round\"), Is.EqualTo(\"1\"));\n"
                + "        Assert.That(_message.Type, Is.EqualTo(\"error\"));\n"
                + "        Assert.That(_message.Text, Is.EqualTo(\"No round\"));\n"
                + "        var download = await _page.RunAndWaitForDownloadAsync(() => button.ClickAsync());\n"
                + "        Assert.That(download.SuggestedFilename, Is.EqualTo(\"rounds.csv\"));\n"
                + "        var opened = await _page.RunAndWaitForPopupAsync(() => button.ClickAsync());\n"
                + "        await Expect(opened).ToHaveURLAsync(\"/round/1\");"
                + Closing)
            .RunAsync();

    [RefusedCase]
    public Task a_member_the_harness_takes_is_refused_outside_the_harness()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        _page.{|#0:SetDefaultTimeout|}(1000);\n"
                + "        _page.{|#1:Request|} += (_, _) => { };\n"
                + "        await _page.{|#2:Clock|}.{|#3:InstallAsync|}();\n"
                + "        await _page.{|#4:RequestGCAsync|}();"
                + Closing)
            .Expecting(
                TheHarnesss(0, "IPage.SetDefaultTimeout"),
                TheHarnesss(1, "IPage.Request"),
                TheHarnesss(2, "IPage.Clock"),
                TheHarnesss(3, "IClock.InstallAsync"),
                TheHarnesss(4, "IPage.RequestGCAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task a_member_the_harness_takes_is_accepted_in_the_harness()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "Harness/Run.cs",
                "internal static class Run\n{\n    internal static async Task<IPage> Open()\n    {\n"
                + "        var playwright = await Playwright.CreateAsync();\n"
                + "        var browser = await playwright.Chromium.LaunchAsync();\n"
                + "        var context = await browser.NewContextAsync(new() { Locale = \"en-US\", TimezoneId = \"America/Los_Angeles\" });\n"
                + "        await context.AddCookiesAsync(new[] { new Cookie { Name = \"world\", Value = \"1\", Url = \"http://localhost\" } });\n"
                + "        context.SetDefaultTimeout(1000);\n"
                + "        context.SetDefaultNavigationTimeout(1000);\n"
                + "        var page = await context.NewPageAsync();\n"
                + "        page.SetDefaultTimeout(1000);\n"
                + "        page.SetDefaultNavigationTimeout(1000);\n"
                + "        page.Request += (_, _) => { };\n"
                + "        page.Console += (_, _) => { };\n"
                + "        page.PageError += (_, _) => { };\n"
                + "        page.Download += (_, _) => { };\n"
                + "        page.Popup += (_, _) => { };\n"
                + "        page.Close += (_, _) => { };\n"
                + "        await page.RequestGCAsync();\n"
                + "        await page.Clock.InstallAsync();\n"
                + "        await page.Clock.PauseAtAsync(\"2026-11-11\");\n"
                + "        await page.Clock.FastForwardAsync(1000);\n"
                + "        await page.Clock.RunForAsync(1000);\n"
                + "        await page.Clock.ResumeAsync();\n"
                + "        await page.CloseAsync();\n"
                + "        await context.CloseAsync();\n"
                + "        await browser.CloseAsync();\n"
                + "        return page;\n    }\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_member_outside_every_list_is_refused_in_the_harness_too()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile("Harness/Reader.cs", "internal static class Reader { internal static Task<string> Title(IPage page) => page.{|#0:TitleAsync|}(); }")
            .Expecting(Outside(0, "IPage.TitleAsync"))
            .RunAsync();

    [RefusedCase]
    public Task a_constant_selector_that_is_no_single_id_is_refused_at_the_selector()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        _ = _page.Locator({|#0:\".card\"|});\n"
                + "        _ = _page.Locator({|#1:\"#rounds li\"|});\n"
                + "        _ = _page.FrameLocator({|#2:\"iframe\"|});\n"
                + "        _ = _page.Locator({|#3:\"#\"|});\n"
                + "        _ = _page.GetByRole(AriaRole.List).Locator({|#4:\"#a, #b\"|});\n"
                + "        _ = _page.FrameLocator(\"#answer\").Locator({|#5:\"text=Ada\"|});\n"
                + "        _ = _page.GetByRole(AriaRole.List).FrameLocator({|#6:\"#one.two\"|});\n"
                + "        _ = _page.FrameLocator(\"#answer\").FrameLocator({|#7:\"[name=inner]\"|});"
                + Closing)
            .Expecting(
                NoSingleId(0, ".card"),
                NoSingleId(1, "#rounds li"),
                NoSingleId(2, "iframe"),
                NoSingleId(3, "#"),
                NoSingleId(4, "#a, #b"),
                NoSingleId(5, "text=Ada"),
                NoSingleId(6, "#one.two"),
                NoSingleId(7, "[name=inner]"))
            .RunAsync();

    [AcceptedCase]
    public Task a_selector_of_one_id_a_selector_that_is_no_constant_and_a_locator_for_a_selector_are_accepted()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        const string Rounds = \"#rounds\";\n"
                + "        var computed = \"#\" + Guid.NewGuid();\n"
                + "        _ = _page.Locator(\"#resident-name_2\");\n"
                + "        _ = _page.Locator(Rounds);\n"
                + "        _ = _page.Locator(computed);\n"
                + "        _ = _page.FrameLocator(computed);\n"
                + "        _ = _page.GetByRole(AriaRole.List).Locator(_page.GetByRole(AriaRole.Listitem));"
                + Closing)
            .RunAsync();

    [AcceptedCase]
    public Task an_options_object_an_enum_value_and_a_record_of_playwrights_are_not_read()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var options = new LocatorClickOptions { Button = MouseButton.Middle, Modifiers = new[] { KeyboardModifier.Control }, Force = true };\n"
                + "        _ = options.Button;\n"
                + "        _ = options.Force;\n"
                + "        await _page.GetByRole(AriaRole.Button).ClickAsync(options);\n"
                + "        var cookie = new Cookie { Name = \"world\" };\n"
                + "        _ = cookie.Name;\n"
                + "        _ = AriaRole.Button;"
                + Closing)
            .RunAsync();

    [AcceptedCase]
    public Task a_member_another_rule_names_is_left_to_that_rule()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await _page.WaitForTimeoutAsync(500);\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToContainTextAsync(\"Rounds\");"
                + Closing)
            .RunAsync();

    [AcceptedCase]
    public Task a_member_of_another_assembly_under_an_unlisted_name_is_accepted()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var board = new Board();\n"
                + "        _ = await board.EvaluateAsync(\"rounds\");\n"
                + "        _ = board.Url;\n"
                + "        board.Dialog += (_, _) => { };\n"
                + "        await _page.DisposeAsync();"
                + Closing
                + "internal sealed class Board\n{\n    internal event EventHandler? Dialog;\n    internal string Url => Dialog is null ? \"/\" : \"/open\";\n    internal Task<string> EvaluateAsync(string script) => Task.FromResult(script);\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_text_that_names_a_components_object_or_the_runtimes_global_is_refused_at_the_text()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        const string Reached = {|#0:\"ej2_instances\"|};\n"
                + "        var boot = {|#1:\"window.alis.boot()\"|};\n"
                + "        var value = $\"{|#2:return document.getElementById('wing').ej2_instances[0].value + |}{Reached}{boot}\";\n"
                + "        _ = {|#3:{|#4:@\"window.alis.plans; ej2_instances\"|}|} + value;"
                + Closing)
            .Expecting(Names(0, "ej2_instances"), Names(1, "window.alis"), Names(2, "ej2_instances"), Names(3, "ej2_instances"), Names(4, "window.alis"))
            .RunAsync();

    [AcceptedCase]
    public Task a_text_one_letter_from_either_name_is_accepted()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var instance = \"ej2_instance\";\n"
                + "        var alias = \"window.alias\";\n"
                + "        var cased = \"Window.Alis and EJ2_INSTANCES\";\n"
                + "        _ = instance + alias + cased;"
                + Closing)
            .RunAsync();

    [RefusedCase]
    public Task a_text_that_names_a_components_object_is_refused_in_a_project_with_no_playwright()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Reader.cs", "internal static class Reader { internal const string Reached = {|#0:\"ej2_instances\"|}; }")
            .Expecting(Names(0, "ej2_instances"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_playwright_and_no_such_text_is_accepted()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Board.cs", "internal static class Board { internal static Task<string> EvaluateAsync(string script) => Task.FromResult(script); internal static Task<string> Read() => EvaluateAsync(\"rounds\"); }")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST002TestTakesTheListedMembersOfPlaywright>()
            .WithFile("WhenARoundStarts.cs", Opening + "        await _page.{|CS1061:EvaluateNothingAsync|}(\"rounds\");" + Closing)
            .RunAsync();

    private static DiagnosticResult Outside(int at, string member)
        => new DiagnosticResult(TEST002TestTakesTheListedMembersOfPlaywright.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{member}' is outside the members a test takes of Playwright; find an element by its role, label, text or id, act by a gesture and expect through Expect");

    private static DiagnosticResult TheHarnesss(int at, string member)
        => new DiagnosticResult(TEST002TestTakesTheListedMembersOfPlaywright.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{member}' is the harness's to take; leave it to a file under Harness");

    private static DiagnosticResult NoSingleId(int at, string selector)
        => new DiagnosticResult(TEST002TestTakesTheListedMembersOfPlaywright.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{selector}' is no single id; a locator takes '#' and one id, or the element is found by its role, label or text");

    private static DiagnosticResult Names(int at, string name)
        => new DiagnosticResult(TEST002TestTakesTheListedMembersOfPlaywright.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"This text names '{name}'; a test reads no component's object and no global of the runtime, so expect what the page shows");
}
