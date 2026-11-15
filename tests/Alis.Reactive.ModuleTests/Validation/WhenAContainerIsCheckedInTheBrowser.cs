using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Validation;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAContainerIsCheckedInTheBrowser
{
    private const string Supplies = """{"validator":"Alis.Reactive.ModuleTests.Validation.SuppliesRules","ruleSet":"default","start":{"count":0},"scalars":[{"path":["count"],"shape":{"kind":"number","of":"integer","text":"view","overflow":"nothing"},"before":"The value \u0027","after":"\u0027 is not valid for Count."}],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["count"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"gt","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":0}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Count\u0027 must be greater than \u00270\u0027."}]}],"groups":[]}]}}""";

    [Test]
    public void the_check_reads_the_containers_inputs_runs_the_named_registration_and_stops_on_a_failure()
    {
        var json = PlanOf<Supplies>(validation => validation.Add<SuppliesRules>().Add<DeputyCardRules>());
        using var plan = JsonDocument.Parse(json);
        Assert.That(plan.RootElement.GetProperty("handlers")[0].GetProperty("do").GetRawText(), Is.EqualTo("""[{"kind":"let","name":"captured1","await":false,"binds":"line","expr":{"kind":"call","path":["captured"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"intake"},{"kind":"lit","value":"Alis.Reactive.ModuleTests.Validation.Supplies"}]}},{"kind":"let","name":"failures1","await":false,"binds":"line","expr":{"kind":"call","path":["validate"],"members":"required","target":{"kind":"obj","id":"$form:Alis.Reactive.ModuleTests.Validation.SuppliesRules:default"},"args":[{"kind":"record","fields":[{"name":"registration","value":{"kind":"lit","value":"Alis.Reactive.ModuleTests.Validation.SuppliesRules:default"}},{"name":"body","value":{"kind":"record","fields":[{"name":"format","value":{"kind":"lit","value":"urlEncoded"}},{"name":"value","value":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"captured1"}}}]}},{"name":"inputs","value":{"kind":"get","path":["inputs"],"members":"required","target":{"kind":"ref","name":"captured1"}}},{"name":"place","value":{"kind":"record","fields":[{"name":"destination","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"summary"}},{"name":"containerId","value":{"kind":"lit","value":"intake"}}]}}]}}]}]}},{"kind":"if","when":{"kind":"op","op":"not","args":[{"kind":"op","op":"empty","args":[{"kind":"ref","name":"failures1"}]}]},"then":[{"kind":"stop","reason":"The inputs of container \u0027intake\u0027 fail the rules Supplies is registered with."}],"else":[]}]"""));
        Assert.That(plan.RootElement.GetProperty("objects").EnumerateObject().Select(member => member.Name), Is.EqualTo(new[] { "$document", "$form:Alis.Reactive.ModuleTests.Validation.SuppliesRules:default", "$model" }));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public async Task a_typed_text_fails_its_member_with_the_binders_message_before_its_rules()
    {
        Assert.That(RegistrationOf<Supplies>(validation => validation.Add<SuppliesRules>()), Is.EqualTo(Supplies));
        Assert.That(await ServerVerdict<Supplies, SuppliesRules>(("Count", "12a")), Is.EqualTo(new[] { ("Count", "The value '12a' is not valid for Count.") }));
        Assert.That(await ServerVerdict<Supplies, SuppliesRules>(("Count", "0")), Is.EqualTo(new[] { ("Count", "'Count' must be greater than '0'.") }));
        Assert.That(await ServerVerdict<Supplies, SuppliesRules>(("Count", "12")), Is.Empty);
    }

    [Test]
    public async Task a_holder_that_starts_null_is_its_types_start_and_its_child_rules_hold_only_while_it_is_held()
    {
        Assert.That(RegistrationOf<DeputyCard>(validation => validation.Add<DeputyCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.DeputyCardRules","ruleSet":"default","start":{"deputy":null},"scalars":[],"objects":[{"path":["deputy"],"start":{"name":"Unnamed","phone":null},"json":"replace"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":["deputy"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the deputy."}]}],"groups":[]}]}]}}"""));
        Assert.That(await ServerVerdict<DeputyCard, DeputyCardRules>(("Deputy.Phone", "555-0101")), Is.Empty);
        Assert.That(await ServerVerdict<DeputyCard, DeputyCardRules>(), Is.Empty);
    }

    [Test]
    public async Task an_empty_text_binds_null_a_condition_reads_the_body_and_form_data_replaces_a_list()
    {
        Assert.That(RegistrationOf<IntakeCard>(validation => validation.Add<IntakeCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.IntakeCardRules","ruleSet":"default","start":{"name":"Unnamed","status":null,"urgent":false,"carers":[{"name":"Ruth"}]},"scalars":[],"objects":[],"lists":[{"path":["carers"],"item":{"name":null},"json":"replace"}],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["status"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"data","path":[{"name":"urgent","requires":[],"absent":{"kind":"start","value":false}}],"target":{"kind":"ref","name":"item1"}}},"message":"\u0027Status\u0027 must not be empty."}]}],"groups":[]},{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the resident."}]}],"groups":[]},{"path":["carers",{"kind":"each"}],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the carer."}]}],"groups":[]}]}]}}"""));
        Assert.That(await ServerVerdict<IntakeCard, IntakeCardRules>(("Name", "")), Is.EqualTo(new[] { ("Name", "Name the resident.") }));
        Assert.That(await ServerVerdict<IntakeCard, IntakeCardRules>(("Status", "")), Is.Empty);
        Assert.That(await ServerVerdict<IntakeCard, IntakeCardRules>(("Status", ""), ("Urgent", "true")), Is.EqualTo(new[] { ("Status", "'Status' must not be empty.") }));
        Assert.That(await ServerVerdict<IntakeCard, IntakeCardRules>(("Carers[0].Name", "")), Is.EqualTo(new[] { ("Carers[0].Name", "Name the carer.") }));
        Assert.That(await ServerVerdict<IntakeCard, IntakeCardRules>(), Is.Empty);
    }

    [Test]
    public async Task an_empty_text_binds_null_in_form_data_and_stays_text_in_json()
    {
        Assert.That(RegistrationOf<WardCard>(validation => validation.Add<WardCardRules>()),
            Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.WardCardRules","ruleSet":"default","start":{"ward":null},"scalars":[],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":null}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 must not be empty."}]}],"groups":[]}]}}"""));
        Assert.That(await ServerVerdict<WardCard, WardCardRules>(("Ward", "")), Is.EqualTo(new[] { ("Ward", "'Ward' must not be empty.") }));
        Assert.That(Verdict(new WardCardRules().Validate(JsonSerializer.Deserialize<WardCard>("""{"ward":""}""", JsonSerializerOptions.Web)!)), Is.Empty);
    }

    [Test]
    public async Task json_on_net10_replaces_an_object_a_member_starts_with_unless_the_member_is_populated()
    {
        Assert.That(RegistrationOf<HomeCard>(validation => validation.Add<HomeCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.HomeCardRules","ruleSet":"default","start":{"lead":{"name":"Ruth","phone":null}},"scalars":[],"objects":[{"path":["lead"],"start":{"name":"","phone":null},"json":"replace"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["lead","name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Lead name is required."}]}],"groups":[]}]}}"""));
        Assert.That(RegistrationOf<PopulatedHomeCard>(validation => validation.Add<PopulatedHomeCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.PopulatedHomeCardRules","ruleSet":"default","start":{"lead":{"name":"Ruth","phone":null}},"scalars":[],"objects":[{"path":["lead"],"start":{"name":"","phone":null},"json":"fill"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["lead","name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Lead name is required."}]}],"groups":[]}]}}"""));
        const string phoneAlone = """{"lead":{"phone":"555-0100"}}""";
        Assert.That(Verdict(new HomeCardRules().Validate(JsonSerializer.Deserialize<HomeCard>(phoneAlone, JsonSerializerOptions.Web)!)), Is.EqualTo(new[] { ("Lead.Name", "Lead name is required.") }));
        Assert.That(Verdict(new PopulatedHomeCardRules().Validate(JsonSerializer.Deserialize<PopulatedHomeCard>(phoneAlone, JsonSerializerOptions.Web)!)), Is.Empty);
        Assert.That(await ServerVerdict<HomeCard, HomeCardRules>(("Lead.Phone", "555-0100")), Is.Empty);
    }

    [Test]
    public void what_the_server_alone_decides_writes_no_browser_test_and_each_message_is_the_servers()
    {
        Assert.That(RegistrationOf<RoundCard>(validation => validation.Add<RoundCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.RoundCardRules","ruleSet":"default","start":{"ward":null,"beds":0,"staff":0},"scalars":[{"path":["beds"],"shape":{"kind":"number","of":"integer","text":"view","overflow":"nothing"},"before":"The value \u0027","after":"\u0027 is not valid for Beds."},{"path":["staff"],"shape":{"kind":"number","of":"integer","text":"view","overflow":"nothing"},"before":"The value \u0027","after":"\u0027 is not valid for Staff."}],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"matches","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":"^[A-Z][0-9](?=\\n?$)"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 is not in the correct format."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":null}]},{"kind":"op","op":"and","args":[{"kind":"op","op":"gte","args":[{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]},{"kind":"op","op":"lte","args":[{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":2}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"The length of \u0027Ward\u0027 must be 2 characters or fewer."}]}],"groups":[]},{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}}"""));
        var round = new RoundCard { Ward = "B12", Beds = 2, Staff = 3 };
        Assert.That(Verdict(new RoundCardRules().Validate(round)), Is.EqualTo(new[]
        {
            ("Ward", "'Ward' is not in the correct format."),
            ("Ward", "The length of 'Ward' must be 2 characters or fewer. You entered 3 characters."),
            ("Staff", "No more staff than beds."),
        }));
    }

    [Test]
    public void a_condition_with_no_browser_form_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<UnreadRoundRules>()),
            Throws.InvalidOperationException.With.Message.StartsWith("The condition '").And.Message.EndsWith(
                "' has no browser form. Write it as tests of model members against fixed values or other members, or declare that the server alone decides it: WhenOnServer or UnlessOnServer."));

    [Test]
    public void a_pattern_that_ignores_case_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<FoldedRoundRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The pattern '^a[0-9]$' ignores case, which no browser flag folds as .NET does; spell both cases, as [Aa]spirin, or mark the rule .ServerOnly()."));

    [Test]
    public void a_container_whose_model_has_no_registered_validator_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<Supplies>(validation => validation.Add<DeputyCardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("Validate(container) checks container 'intake' by the validator registered for Supplies, and none is registered: register one with services.AddReactiveFluentValidation(validation => validation.Add<TValidator>())."));

    private static string RegistrationOf<TModel>(Action<ReactiveFluentValidationBuilder> validators) where TModel : new()
    {
        using var plan = JsonDocument.Parse(PlanOf<TModel>(validators));
        return plan.RootElement.GetProperty("objects").EnumerateObject().Single(member => member.Name.StartsWith("$form:", StringComparison.Ordinal))
            .Value.GetProperty("data").GetProperty("registration").GetRawText();
    }

    private static string PlanOf<TModel>(Action<ReactiveFluentValidationBuilder> validators) where TModel : new()
    {
        var html = Views.Of(new TModel(), _ => { }, services => services.AddReactiveFluentValidation(validators));
        var plan = html.ReactivePlan<TModel>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Validate(pipeline.Container<TModel>("intake"))));
        return Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static async Task<(string, string)[]> ServerVerdict<TModel, TValidator>(params (string Key, string Value)[] values)
        where TModel : class, new()
        where TValidator : ReactiveValidator<TModel>
    {
        var model = new TModel();
        var view = Views.Of(model, _ => { }, services => services.AddReactiveFluentValidation(validation => validation.Add<TValidator>()));
        var controller = new CheckedController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = view.ViewContext.HttpContext.RequestServices },
                ActionDescriptor = new ControllerActionDescriptor(),
            },
        };
        var query = new QueryCollection(values.ToDictionary(value => value.Key, value => new StringValues(value.Value)));
        await controller.TryUpdateModelAsync(model, "", new QueryStringValueProvider(BindingSource.Query, query, CultureInfo.InvariantCulture));
        return Verdict(controller.Validate<TModel, TValidator>(model));
    }

    private static (string, string)[] Verdict(ValidationResult result) => [.. result.Errors.Select(error => (error.PropertyName, error.ErrorMessage))];

    private sealed class CheckedController : Controller;
}
