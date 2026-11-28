using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Schema;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAViewRendersItsPlan
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string UnderEnglish =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Schema.Ward\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},";

    private const string AWardsPlan = UnderEnglish + "\"vendors\":{},\"objects\":{},\"handlers\":[]}";

    private const string EventTargets = "{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}";

    private const string ARoundMarkedOnLoad = UnderEnglish +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":" + EventTargets + "}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":" + EventTargets + ",\"data\":{\"kind\":\"none\"}}," +
        "\"round-status\":{\"kind\":\"element\",\"id\":\"round-status\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\"," +
        "\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"done\"}}]}]}";

    [Test]
    public void a_plan_with_no_handler_is_one_script_of_plan_data_with_every_field()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + AWardsPlan + ScriptClose));
    }

    [Test]
    public void an_on_load_that_sets_an_elements_text_is_a_handler_of_the_document_with_one_set()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("round-status").Text.Set("done")));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ARoundMarkedOnLoad + ScriptClose));
    }

    [Test]
    public void the_plan_of_an_on_load_that_sets_an_elements_text_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(ARoundMarkedOnLoad), Is.Empty);

    [Test]
    public void the_plan_a_view_renders_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(AWardsPlan), Is.Empty);

    [Test]
    public void a_plan_that_lacks_a_field_is_not_valid_by_the_plan_schema()
        => Assert.That(
            PlanSchema.ProblemsOf(AWardsPlan.Replace("\"trace\":{\"kind\":\"off\"},", string.Empty)),
            Is.EqualTo(new[] { "the plan: required" }));
}

public sealed record Ward(string Name);
