using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("de-DE")]
public sealed class WhenAMemberOfThePagesUrlIsRead
{
    private const string Template = "/care/residents/wards/{Ward}";
    private const string Document = "{\"kind\":\"obj\",\"id\":\"$document\"}";
    private const string Text = "{\"kind\":\"string\",\"date\":\"none\",\"negativeZero\":\"0\"}";
    private const string Whole = "{\"kind\":\"number\",\"of\":\"integer\",\"text\":\"invariant\",\"overflow\":\"nothing\"}";

    [Test]
    public void a_member_is_read_from_the_query_by_its_binding_name_and_reads_its_start_in_place_of_text_that_does_not_convert()
    {
        var rendered = Rendered("", pipeline => Board(pipeline).Beds.Set(pipeline.PageUrl<WardOnFilePage>().Read(page => page.Beds)));

        Assert.That(FirstStep(rendered, "value"), Is.EqualTo(Queried("Beds", Whole, "2")));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void a_nested_member_is_read_by_its_path_as_mvc_names_it()
    {
        var rendered = Rendered("", pipeline => Board(pipeline).Ward.Set(pipeline.PageUrl<WardOnFilePage>().Read(page => page.Stay.Ward)));

        Assert.That(FirstStep(rendered, "value"), Is.EqualTo(Queried("Stay.Ward", Text, "\"North\"")));
    }

    [Test]
    public void a_placeholder_member_is_read_from_the_path_after_the_apps_path_base_and_without_the_template_from_the_query()
    {
        var byTemplate = Rendered("/home", pipeline => Board(pipeline).Ward.Set(pipeline.PageUrl<WardOnFilePage>(Template).Read(page => page.Ward)));
        var byQuery = Rendered("/home", pipeline => Board(pipeline).Ward.Set(pipeline.PageUrl<WardOnFilePage>().Read(page => page.Ward)));

        Assert.That(FirstStep(byTemplate, "value"), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[{\"kind\":\"as\",\"expr\":{\"kind\":\"call\",\"path\":[\"segment\"],\"members\":\"required\",\"target\":" + Document + "," +
            "\"args\":[{\"kind\":\"lit\",\"value\":[\"home\",\"care\",\"residents\",\"wards\",\"{Ward}\"]},{\"kind\":\"lit\",\"value\":4}]},\"shape\":" + Text + "},{\"kind\":\"lit\",\"value\":\"\"}]}"));
        Assert.That(FirstStep(byQuery, "value"), Is.EqualTo(Queried("Ward", Text, "\"\"")));
        Assert.That(PlanSchema.ProblemsOf(byTemplate), Is.Empty);
    }

    [Test]
    public void a_template_whose_placeholder_carries_a_constraint_reads_the_placeholder_by_its_name()
    {
        var rendered = Rendered("", pipeline => Board(pipeline).Ward.Set(pipeline.PageUrl<WardOnFilePage>("~/care/residents/wards/{ward:alpha}/").Read(page => page.Ward)));

        Assert.That(FirstStep(rendered, "value"), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[{\"kind\":\"as\",\"expr\":{\"kind\":\"call\",\"path\":[\"segment\"],\"members\":\"required\",\"target\":" + Document + "," +
            "\"args\":[{\"kind\":\"lit\",\"value\":[\"care\",\"residents\",\"wards\",\"{ward}\"]},{\"kind\":\"lit\",\"value\":3}]},\"shape\":" + Text + "},{\"kind\":\"lit\",\"value\":\"\"}]}"));
    }

    [Test]
    public void a_time_compared_from_the_url_keys_its_start_in_place_of_text_outside_its_forms()
    {
        var rendered = Rendered("", pipeline => pipeline.When(pipeline.PageUrl<WardOnFilePage>().Read(page => page.Starts).Eq(new TimeOnly(12, 0)))
            .Then(pipeline => pipeline.Element("noon").Text.Set("noon")));

        Assert.That(FirstStep(rendered, "when"), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"timeKey\",\"args\":[" + Queried("Starts", Text, "null") + ",{\"kind\":\"lit\",\"value\":null}]}," +
            "{\"kind\":\"op\",\"op\":\"timeKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"12:00:00\"}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [TestCase("/care/residents/wards/{Ward?}", "The page's URL template '/care/residents/wards/{Ward?}' holds '{Ward?}': the page's URL reads a placeholder that is a whole segment, {Name} or {Name:constraint}, with no default value, optional mark or catch-all. (Parameter 'template')")]
    [TestCase("/care/residents/wards/{Ward=North}", "The page's URL template '/care/residents/wards/{Ward=North}' holds '{Ward=North}': the page's URL reads a placeholder that is a whole segment, {Name} or {Name:constraint}, with no default value, optional mark or catch-all. (Parameter 'template')")]
    [TestCase("/care/residents/{*Ward}", "The page's URL template '/care/residents/{*Ward}' holds '{*Ward}': the page's URL reads a placeholder that is a whole segment, {Name} or {Name:constraint}, with no default value, optional mark or catch-all. (Parameter 'template')")]
    [TestCase("/care/residents/ward-{Ward}", "The page's URL template '/care/residents/ward-{Ward}' holds 'ward-{Ward}': the page's URL reads a placeholder that is a whole segment, {Name} or {Name:constraint}, with no default value, optional mark or catch-all. (Parameter 'template')")]
    [TestCase("/care/residents/wards/{Room}", "The page's URL template '/care/residents/wards/{Room}' names {Room}, which WardOnFilePage does not declare: name each placeholder after the member of WardOnFilePage its path holds. (Parameter 'template')")]
    [TestCase("/care/{Ward}/beds/{ward}", "The page's URL template '/care/{Ward}/beds/{ward}' names {ward} twice. (Parameter 'template')")]
    [TestCase("/care//wards/{Ward}", "The page's URL template '/care//wards/{Ward}' holds an empty segment: write one '/' between two segments. (Parameter 'template')")]
    [TestCase("/care/wards?ward={Ward}", "The page's URL template '/care/wards?ward={Ward}' holds 'wards?ward={Ward}': the page's URL reads a placeholder that is a whole segment, {Name} or {Name:constraint}, with no default value, optional mark or catch-all. (Parameter 'template')")]
    [TestCase("/care/wards?ward=North", "The page's URL template '/care/wards?ward=North' holds a query or a fragment: a template names the page's path alone. (Parameter 'template')")]
    public void a_template_the_page_url_cannot_match_is_refused_naming_what_it_holds(string template, string message)
        => Assert.That(() => Rendered("", pipeline => pipeline.Element("ward").Text.Set(pipeline.PageUrl<WardOnFilePage>(template).Read(page => page.Ward))),
            Throws.ArgumentException.With.Message.EqualTo(message));

    [Test]
    public void a_template_of_null_is_refused()
        => Assert.That(() => Rendered("", pipeline => pipeline.PageUrl<WardOnFilePage>(null!)),
            Throws.ArgumentNullException.With.Message.EqualTo("Value cannot be null. (Parameter 'template')"));

    [Test]
    public void a_member_that_holds_a_list_is_refused()
        => Assert.That(() => Rendered("", pipeline => pipeline.PageUrl<WardOnFilePage>().Read(page => page.Rooms)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'page => page.Rooms' reads Rooms, which the app's JSON options write as a list: a member of the page's URL holds one value, so read a member that holds one."));

    [Test]
    public void a_member_that_holds_an_enum_is_refused()
        => Assert.That(() => Rendered("", pipeline => pipeline.PageUrl<WardOnFilePage>().Read(page => page.Shift)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'page => page.Shift' reads Shift, an enum the page's URL holds by its name or its number, which the browser does not read as the enum's value: read a member that holds text, or read it on the server."));

    [Test]
    public void a_read_past_a_member_of_one_value_is_refused()
        => Assert.That(() => Rendered("", pipeline => pipeline.Element("ward").Text.Set(pipeline.PageUrl<WardOnFilePage>().Read(page => page.Ward.Length))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'page => page.Ward.Length' reads the page's URL by more than its members: a member of the page's URL is read by a member path, as page => page.Ward or page => page.Stay.Ward."));

    [Test]
    public void a_member_lambda_of_null_is_refused()
        => Assert.That(() => Rendered("", pipeline => pipeline.PageUrl<WardOnFilePage>().Read<string>(null!)),
            Throws.ArgumentNullException.With.Message.EqualTo("Value cannot be null. (Parameter 'member')"));

    private static string Queried(string key, string shape, string start)
        => "{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[{\"kind\":\"as\",\"expr\":{\"kind\":\"call\",\"path\":[\"query\"],\"members\":\"required\",\"target\":" + Document + "," +
            "\"args\":[{\"kind\":\"lit\",\"value\":\"" + key + "\"}]},\"shape\":" + shape + "},{\"kind\":\"lit\",\"value\":" + start + "}]}";

    private static WardBoard Board(Pipeline<WardOnFilePage> pipeline) => pipeline.Component<WardBoard>("board");

    private static string Rendered(string pathBase, Action<Pipeline<WardOnFilePage>> write)
    {
        var html = Views.Of(new WardOnFilePage());
        html.ViewContext.HttpContext.Request.PathBase = pathBase;
        var plan = html.ReactivePlan<WardOnFilePage>();
        html.On(plan, trigger => trigger.DomReady(write));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string FirstStep(string rendered, string field)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty(field).GetRawText();
    }
}

public sealed class WardOnFilePage
{
    public string Ward { get; set; } = "";

    public int Beds { get; set; } = 2;

    public WardStay Stay { get; set; } = new();

    public TimeOnly? Starts { get; set; }

    public List<int> Rooms { get; set; } = [];

    public CareShift Shift { get; set; }
}

public sealed class WardBoard : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string> Ward => Property<string>();

    public Property<int> Beds => Property<int>();
}

public sealed class WardStay
{
    public string Ward { get; set; } = "North";
}

public enum CareShift
{
    Early,
    Late,
}
