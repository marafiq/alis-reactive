using System.Linq;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Validation;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenABlockChecksItsInputsLive
{
    private const string Registered = """{"kind":"plugin","name":"alis.form","key":"Alis.Reactive.ModuleTests.Validation.SuppliesRules:default","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"form","registration":{"validator":"Alis.Reactive.ModuleTests.Validation.SuppliesRules","ruleSet":"default","start":{"count":0},"scalars":[{"path":["count"],"shape":{"kind":"number","of":"integer","text":"view","overflow":"nothing"},"before":"The value \u0027","after":"\u0027 is not valid for Count."}],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["count"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"gt","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":0}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Count\u0027 must be greater than \u00270\u0027."}]}],"groups":[]}]}}}}""";

    private const string SentByIntake = """{"validator":"Alis.Reactive.ModuleTests.Validation.SuppliesRules","ruleSet":"default","body":{"kind":"record","fields":[{"name":"format","value":{"kind":"lit","value":"urlEncoded"}},{"name":"value","value":{"kind":"call","path":["sent"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"intake"},{"kind":"lit","value":"Alis.Reactive.ModuleTests.Validation.Supplies"}]}}]}}""";

    [Test]
    public void an_element_container_is_checked_live_by_its_models_registered_validator_under_the_default_rule_set_over_what_it_sends()
    {
        var html = Views.Of(new Supplies(), _ => { }, services => services.AddReactiveFluentValidation(validation => validation.Add<SuppliesRules>()));
        var plan = html.ReactivePlan<Supplies>();
        using (html.BeginRequestContainer(plan, "intake")) { }
        var json = JsonOf(html.RenderPlan(plan));
        using var written = JsonDocument.Parse(json);
        var objects = written.RootElement.GetProperty("objects");
        Assert.That(objects.GetProperty("$model").GetProperty("data").GetProperty("containers").GetRawText(), Is.EqualTo("""{"intake":{"model":"Alis.Reactive.ModuleTests.Validation.Supplies","prefix":{"name":"","path":[]},"summary":"intake_summary","live":[""" + SentByIntake + "]}}"));
        Assert.That(objects.GetProperty("$form:Alis.Reactive.ModuleTests.Validation.SuppliesRules:default").GetRawText(), Is.EqualTo(Registered));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_block_whose_model_has_no_registered_validator_is_checked_live_by_none()
    {
        var html = Views.Of(new Supplies(), _ => { }, services => services.AddReactiveFluentValidation(validation => validation.Add<DeputyCardRules>()));
        var plan = html.ReactivePlan<Supplies>();
        using (html.BeginRequestContainer(plan, "intake"))
        {
            using (html.BeginRequestContainer(plan, "kin")) { }
        }
        var json = JsonOf(html.RenderPlan(plan));
        using var written = JsonDocument.Parse(json);
        var objects = written.RootElement.GetProperty("objects");
        Assert.That(objects.GetProperty("$model").GetProperty("data").GetProperty("containers").GetRawText(), Is.EqualTo("""{"intake":{"model":"Alis.Reactive.ModuleTests.Validation.Supplies","prefix":{"name":"","path":[]},"summary":"intake_summary","live":[]},"kin":{"model":"Alis.Reactive.ModuleTests.Validation.Supplies","prefix":{"name":"","path":[]},"summary":"kin_summary","live":[]}}"""));
        Assert.That(objects.EnumerateObject().Select(member => member.Name), Is.EqualTo(new[] { "$model" }));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    private static string JsonOf(Microsoft.AspNetCore.Html.IHtmlContent rendered)
        => Views.TextOf(rendered)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
}
