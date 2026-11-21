using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionMatchesAPattern
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string Copied =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Conditions.Bed\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"room\":{\"kind\":\"element\",\"id\":\"room\",\"vendor\":\"native\"}," +
        "\"shown\":{\"kind\":\"element\",\"id\":\"shown\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"matches\",\"args\":[" +
        "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"room\"}}," +
        "{\"kind\":\"lit\",\"value\":\"^[A-Z][0-9]{2}\\\\$(?\\u003C=\\\\$)[\\\\-a-c](?=\\\\n?$)\"}]}," +
        "\"then\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"shown\"},\"value\":{\"kind\":\"lit\",\"value\":\"On the round\"}}],\"else\":[]}]}]}";

    [Test]
    public void the_browser_reads_the_pattern_as_written_with_its_end_spelled_as_dotnet_reads_it()
    {
        var html = Views.Of(new Bed("B12"));
        var plan = html.ReactivePlan<Bed>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.When(pipeline.Element("room").Text.Matches(@"^[A-Z][0-9]{2}\$(?<=\$)[\-a-c]$"))
                .Then(pipeline => pipeline.Element("shown").Text.Set("On the round"))));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Copied + ScriptClose));
    }

    [Test]
    public void the_plan_of_a_matched_pattern_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(Copied), Is.Empty);

    [TestCase(@"^\d+", @"The pattern '^\d+' holds '\d', which .NET and the browser read differently; write [0-9].")]
    [TestCase(@"^\s", @"The pattern '^\s' holds '\s', which .NET and the browser read differently; write [ \t\n\r\f\v].")]
    [TestCase(@"\Aa", @"The pattern '\Aa' holds '\A', which .NET and the browser read differently; write ^.")]
    [TestCase(@"a.c", @"The pattern 'a.c' holds '.', which .NET and the browser read differently; write [^\n].")]
    [TestCase(@"[\w]", @"The pattern '[\w]' holds '\w' inside a class, which .NET and the browser read differently; write a-zA-Z0-9_.")]
    [TestCase(@"\bward", @"The pattern '\bward' holds '\b', which the browser does not read as .NET does; move the work to the server.")]
    [TestCase(@"(?<bed>B)", @"The pattern '(?<bed>B)' holds '(?<', which the browser does not read as .NET does; move the work to the server.")]
    [TestCase(@"\@", @"The pattern '\@' holds '\@', which needs no escape; write @.")]
    [TestCase(@"\u12", @"The pattern '\u12' holds '\u' without 4 hex digits after it.")]
    [TestCase(@"ward\", @"The pattern 'ward\' ends in '\', which escapes nothing; escape it as \\.")]
    [TestCase(@"[]a]", @"The pattern '[]a]' holds ']' first in a class, which the browser reads as the end of an empty class; escape it as \].")]
    [TestCase(@"[a[b]", @"The pattern '[a[b]' holds '[' inside a class, which .NET reads as a subtraction after '-'; escape it as \[.")]
    [TestCase(@"[z-a]", @"The pattern '[z-a]' holds a range from 'z' to 'a', which runs backwards.")]
    [TestCase(@"[a-c-e]", @"The pattern '[a-c-e]' holds '-' right after a range; escape it as \-.")]
    [TestCase(@"[abc", @"The pattern '[abc' opens a class it does not close; close it with ']'.")]
    [TestCase(@"(abc", @"The pattern '(abc' opens a group it does not close; close it with ')'.")]
    [TestCase(@"abc)", @"The pattern 'abc)' holds ')' that closes no group; escape it as \).")]
    [TestCase(@"a]", @"The pattern 'a]' holds ']' outside the construct it closes; escape it as \].")]
    [TestCase(@"*a", @"The pattern '*a' holds '*' where nothing it can repeat comes before it; escape it as \*.")]
    [TestCase(@"(?=a)+", @"The pattern '(?=a)+' holds '+' where nothing it can repeat comes before it; escape it as \+.")]
    [TestCase(@"a{x}", @"The pattern 'a{x}' holds '{' that is not a count such as {2} or {2,5}; escape it as \{.")]
    [TestCase(@"{2}", @"The pattern '{2}' holds '{2}' where nothing it can repeat comes before it.")]
    [TestCase(@"a{5,2}", @"The pattern 'a{5,2}' holds the count '{5,2}', whose least is above its most.")]
    public void a_pattern_the_browser_does_not_read_as_dotnet_does_is_refused_naming_its_construct(string pattern, string message)
    {
        var html = Views.Of(new Bed("B12"));
        var plan = html.ReactivePlan<Bed>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
                pipeline.When(pipeline.Element("room").Text.Matches(pattern)).Then(pipeline => pipeline.Element("shown").Text.Set("On the round"))));
            html.RenderPlan(plan);
        };

        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(message));
    }
}

public sealed record Bed(string Room);
