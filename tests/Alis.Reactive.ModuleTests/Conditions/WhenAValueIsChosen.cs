using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAValueIsChosen
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Conditions.Room\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"name\":{\"kind\":\"element\",\"id\":\"name\",\"vendor\":\"native\"}," +
        "\"shown\":{\"kind\":\"element\",\"id\":\"shown\",\"vendor\":\"native\"}," +
        "\"ward\":{\"kind\":\"element\",\"id\":\"ward\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"if\",\"when\":";

    private const string Ends =
        ",\"then\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"shown\"},\"value\":{\"kind\":\"lit\",\"value\":\"Seen\"}}],\"else\":[]}]}]}";

    private const string Name = "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"name\"}}";

    private const string Ward = "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"ward\"}}";

    private const string Fallbacks = OnLoad +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":\"none\"}]},{\"kind\":\"lit\",\"value\":\"Ada\"}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[" + Name + "," +
        "{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[" + Ward + ",{\"kind\":\"lit\",\"value\":\"none\"}]}]},{\"kind\":\"lit\",\"value\":\"Ada\"}]}]}" + Ends;

    private const string Choices = OnLoad +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[{\"kind\":\"op\",\"op\":\"and\",\"args\":[{\"kind\":\"op\",\"op\":\"and\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + IsAda + ",{\"kind\":\"lit\",\"value\":\"on\"},{\"kind\":\"lit\",\"value\":\"off\"}]},{\"kind\":\"lit\",\"value\":\"on\"}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + IsAda + "," + Name + "," + Ward + "]},{\"kind\":\"lit\",\"value\":\"on\"}]}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + IsAda + ",{\"kind\":\"lit\",\"value\":\"on\"}," + Ward + "]},{\"kind\":\"lit\",\"value\":\"on\"}]}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + IsAda + "," + Name + ",{\"kind\":\"lit\",\"value\":\"off\"}]},{\"kind\":\"lit\",\"value\":\"on\"}]}]}" + Ends;

    private const string IsAda = "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":\"Ada\"}]}";

    [Test]
    public void if_null_reads_the_source_then_the_fallback_a_literal_or_a_source()
    {
        var html = Views.Of(new Room("B12"));
        var plan = html.ReactivePlan<Room>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            var ward = pipeline.Element("ward").Text;
            pipeline.When(name.IfNull("none").Eq("Ada").And(name.IfNull(ward.IfNull("none")).Eq("Ada")))
                .Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Fallbacks + ScriptClose));
    }

    [Test]
    public void choose_reads_the_condition_then_the_value_for_true_then_the_value_for_false_each_a_literal_or_a_source()
    {
        var html = Views.Of(new Room("B12"));
        var plan = html.ReactivePlan<Room>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            var ward = pipeline.Element("ward").Text;
            var isAda = name.Eq("Ada");
            pipeline.When(isAda.Choose("on", "off").Eq("on")
                    .And(isAda.Choose<string?>(name, ward).Eq("on"))
                    .And(isAda.Choose("on", ward).Eq("on"))
                    .And(isAda.Choose(name, "off").Eq("on")))
                .Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Choices + ScriptClose));
    }

    [Test]
    public void two_sources_csharp_binds_to_the_form_for_two_values_are_refused_naming_the_type_argument_that_reads_them()
    {
        var html = Views.Of(new Room("B12"));
        var plan = html.ReactivePlan<Room>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var name = pipeline.Element("name").Text;
                _ = name.Eq("Ada").Choose(name, pipeline.Element("ward").Text);
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A choice takes a source as a value the view holds, since C# bound it to Choose's form for two values: " +
                "name the type the sources read, as test.Choose<string?>(first, second), and each is read when the step runs."));
    }

    [Test]
    public void the_plan_of_four_choices_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(Choices), Is.Empty);
}

public sealed record Room(string Bed);
