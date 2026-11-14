using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionTestsAText
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Conditions.Visit\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"name\":{\"kind\":\"element\",\"id\":\"name\",\"vendor\":\"native\"}";

    private const string Shown = ",\"shown\":{\"kind\":\"element\",\"id\":\"shown\",\"vendor\":\"native\"}";

    private const string AndShown = Shown + Handlers;

    private const string Handlers =
        "}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"if\",\"when\":";

    private const string Ends =
        ",\"then\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"shown\"},\"value\":{\"kind\":\"lit\",\"value\":\"Seen\"}}],\"else\":[]}]}]}";

    private const string Name = "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"name\"}}";

    private const string Ward = "{\"kind\":\"get\",\"path\":[\"textContent\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"ward\"}}";

    private const string PartsLookedFor = OnLoad + Shown +
        ",\"ward\":{\"kind\":\"element\",\"id\":\"ward\",\"vendor\":\"native\"}" + Handlers +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[{\"kind\":\"op\",\"op\":\"and\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"contains\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":\"Ha\"}]}," +
        "{\"kind\":\"op\",\"op\":\"startsWith\",\"args\":[" + Name + "," + Ward + "]}]}," +
        "{\"kind\":\"op\",\"op\":\"endsWith\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":\"rt\"}]}]}" + Ends;

    private const string LongEnough = OnLoad + AndShown +
        "{\"kind\":\"op\",\"op\":\"gte\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + Name + "}," +
        "{\"kind\":\"lit\",\"value\":3}]}" + Ends;

    private const string NullOrNot = OnLoad + AndShown +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":null}]}," +
        "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + Name + ",{\"kind\":\"lit\",\"value\":null}]}]}" + Ends;

    private const string EmptyOrNot = OnLoad + AndShown +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"nullOrEmpty\",\"args\":[" + Name + "]}," +
        "{\"kind\":\"op\",\"op\":\"not\",\"args\":[{\"kind\":\"op\",\"op\":\"nullOrEmpty\",\"args\":[" + Name + "]}]}]}" + Ends;

    private const string AmongTheValues = OnLoad + AndShown +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[\"Ada\",\"Ben\"]}," + Name + "]}," +
        "{\"kind\":\"op\",\"op\":\"not\",\"args\":[{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[]}," + Name + "]}]}]}" + Ends;

    [Test]
    public void a_text_test_writes_the_text_it_tests_then_the_part_it_looks_for()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            pipeline.When(name.Contains("Ha").And(name.StartsWith(pipeline.Element("ward").Text)).And(name.EndsWith("rt")))
                .Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + PartsLookedFor + ScriptClose));
    }

    [Test]
    public void the_plan_of_the_parts_looked_for_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(PartsLookedFor), Is.Empty);

    [Test]
    public void min_length_compares_the_length_of_a_held_text_with_the_number()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.When(pipeline.Element("name").Text.MinLength(3)).Then(pipeline => pipeline.Element("shown").Text.Set("Seen"))));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + LongEnough + ScriptClose));
    }

    [Test]
    public void a_min_length_below_zero_is_refused_since_no_text_is_shorter_than_none()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
                pipeline.When(pipeline.Element("name").Text.MinLength(-1)).Then(pipeline => pipeline.Element("shown").Text.Set("Seen"))));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo("Condition minimum text length must be zero or greater."));
    }

    [Test]
    public void is_null_and_not_null_compare_the_value_with_null()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            pipeline.When(name.IsNull().Or(name.NotNull())).Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + NullOrNot + ScriptClose));
    }

    [Test]
    public void is_empty_over_a_text_is_null_or_empty_and_not_empty_is_its_negation()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            pipeline.When(name.IsEmpty().Or(name.NotEmpty())).Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + EmptyOrNot + ScriptClose));
    }

    [Test]
    public void in_looks_for_the_value_in_one_list_of_the_values_and_not_in_is_its_negation()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var name = pipeline.Element("name").Text;
            pipeline.When(name.In("Ada", "Ben").Or(name.NotIn())).Then(pipeline => pipeline.Element("shown").Text.Set("Seen"));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + AmongTheValues + ScriptClose));
    }

    [Test]
    public void the_plan_of_a_value_among_values_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(AmongTheValues), Is.Empty);
}

public sealed record Visit(string Resident);
