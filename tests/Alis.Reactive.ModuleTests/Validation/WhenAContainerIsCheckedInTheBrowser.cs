using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
    private const string StaffBesideBeds = """{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}""";

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
    public async Task an_empty_item_of_a_list_of_texts_binds_null_on_net10_and_its_list_says_so()
    {
        Assert.That(ListsOf<NotesCard>(validation => validation.Add<NotesCardRules>()), Is.EqualTo("""[{"path":["notes"],"item":null,"json":"replace","emptyField":"null"}]"""));
        Assert.That(await ServerVerdict<NotesCard, NotesCardRules>(("Notes[0]", "Fed at noon"), ("Notes[1]", "")), Is.EqualTo(new[] { ("Notes[1]", "Write each note.") }));
        Assert.That(await ServerVerdict<NotesCard, NotesCardRules>(("Notes[0]", "Fed at noon"), ("Notes[1]", "Walked")), Is.Empty);
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
        Assert.That(RegistrationOf<IntakeCard>(validation => validation.Add<IntakeCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.IntakeCardRules","ruleSet":"default","start":{"name":"Unnamed","status":null,"urgent":false,"carers":[{"name":"Ruth"}]},"scalars":[],"objects":[],"lists":[{"path":["carers"],"item":{"name":null},"json":"replace","emptyField":"null"}],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["status"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"data","path":[{"name":"urgent","requires":[],"absent":{"kind":"start","value":false}}],"target":{"kind":"ref","name":"item1"}}},"message":"\u0027Status\u0027 must not be empty."}]}],"groups":[]},{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the resident."}]}],"groups":[]},{"path":["carers",{"kind":"each"}],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the carer."}]}],"groups":[]}]}]}}"""));
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
    public void a_decimal_rule_that_counts_its_trailing_zeros_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<DoseCard>(validation => validation.Add<CountedZerosRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Dose' counts the zeros its decimal ends in (PrecisionScale with ignoreTrailingZeros: false), and the browser holds a number, which ends in none; pass ignoreTrailingZeros: true, or mark the rule .ServerOnly()."));

    [Test]
    public void a_pattern_read_culture_invariant_writes_the_browser_test_a_pattern_with_no_option_writes()
    {
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<InvariantWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"matches","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":"^[0-9]\u002B(?=\\n?$)"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 is not in the correct format."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<PlainWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"matches","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":"^[0-9]\u002B(?=\\n?$)"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 is not in the correct format."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_pattern_with_an_option_other_than_culture_invariant_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<MultilineWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The pattern '^[0-9]+$' passes an option other than RegexOptions.CultureInvariant, which the browser does not read as .NET does; move the work to the server, or mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_that_checks_whether_a_dictionary_is_empty_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<DoseCard>(validation => validation.Add<EmptyRoundsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Rounds' checks whether a dictionary is empty, and the browser's rules count a list's items, not a dictionary's entries; mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_that_orders_text_against_a_fixed_value_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<OrderedWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Ward' orders text, which .NET orders by the culture's comparer and the browser cannot order as .NET does; mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_that_orders_text_against_another_member_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<IntakeCard>(validation => validation.Add<OrderedStatusRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Status' orders text, which .NET orders by the culture's comparer and the browser cannot order as .NET does; mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_that_holds_text_between_two_bounds_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<BoundedWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Ward' orders text, which .NET orders by the culture's comparer and the browser cannot order as .NET does; mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_that_orders_a_value_by_its_types_own_comparer_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<IntakeCard>(validation => validation.Add<OrderedUrgencyRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Urgent' compares a value by its type's own comparer, which the browser does not have; mark the rule .ServerOnly()."));

    [Test]
    public void a_message_that_names_what_only_the_endpoint_knows_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<NamedStaffRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The message 'No more than {ComparisonValue} staff.' names {ComparisonValue}, which only the endpoint knows when the check runs, so the browser's copy cannot say it; write the message without it, or mark the rule .ServerOnly()."));

    [Test]
    public void a_message_whose_every_sentence_names_a_sent_value_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<CountedWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("Every sentence of the message 'You entered {TotalLength} letters.' names a value the request sends, which the browser's copy leaves out, so no text is left for the browser to show; write a sentence that names none, or mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_with_dependent_rules_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<DependentWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Ward' declares DependentRules, which the endpoint checks only while the rule passes, a condition no browser rule reads; write them under When with the rule's test as the condition, or mark the rule .ServerOnly()."));

    [Test]
    public async Task a_rule_that_stops_passes_its_later_check_where_an_earlier_one_failed_as_the_endpoint_answers_its_first_failure_alone()
    {
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<StoppedWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 must not be empty."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"cond","args":[{"kind":"op","op":"cond","args":[{"kind":"lit","value":true},{"kind":"op","op":"cond","args":[{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]},{"kind":"lit","value":false},{"kind":"lit","value":true}]},{"kind":"lit","value":false}]},{"kind":"lit","value":true},{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":null}]},{"kind":"op","op":"and","args":[{"kind":"op","op":"gte","args":[{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]},{"kind":"op","op":"lte","args":[{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":2}]}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"The length of \u0027Ward\u0027 must be 2 characters or fewer."}]}],"groups":[]}]}"""));
        Assert.That(await ServerVerdict<RoundCard, StoppedWardRules>(("Ward", "")), Is.EqualTo(new[] { ("Ward", "'Ward' must not be empty.") }));
        Assert.That(await ServerVerdict<RoundCard, StoppedWardRules>(("Ward", "abc")), Is.EqualTo(new[] { ("Ward", "The length of 'Ward' must be 2 characters or fewer. You entered 3 characters.") }));
    }

    [Test]
    public void a_rule_that_stops_after_a_check_only_the_server_decides_leaves_its_later_checks_to_the_server()
        => Assert.That(RulesOf<RoundCard>(validation => validation.Add<StoppedAfterTheServerWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[]}"""));

    [Test]
    public void a_rule_that_stops_checks_its_child_validator_only_where_its_earlier_checks_passed()
    {
        Assert.That(RulesOf<DeputyCard>(validation => validation.Add<StoppedDeputyRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["deputy"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":null}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Deputy\u0027 must not be empty."}]}],"groups":[]},{"path":[],"when":{"kind":"fn","params":["instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"cond","args":[{"kind":"lit","value":true},{"kind":"op","op":"cond","args":[{"kind":"op","op":"neq","args":[{"kind":"data","path":[{"name":"deputy","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"instance"}},{"kind":"lit","value":null}]},{"kind":"lit","value":false},{"kind":"lit","value":true}]},{"kind":"lit","value":false}]}]}},"fields":[],"groups":[{"path":["deputy"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the deputy."}]}],"groups":[]}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(PlanOf<DeputyCard>(validation => validation.Add<StoppedDeputyRules>())), Is.Empty);
    }

    [Test]
    public void an_email_card_or_web_address_rule_writes_its_operator_where_a_null_passes_and_an_empty_web_address_too()
    {
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<EmailWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"emailAddress","args":[{"kind":"ref","name":"value"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 is not a valid email address."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<CardWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"creditCard","args":[{"kind":"ref","name":"value"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 is not a valid credit card number."}]}],"groups":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(PlanOf<RoundCard>(validation => validation.Add<WebWardRules>())), Is.Empty);
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<WebWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"ref","name":"value"}]},{"kind":"op","op":"url","args":[{"kind":"ref","name":"value"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 must be a valid URL."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_precision_rule_writes_its_precision_and_scale_as_literals_beside_the_number()
    {
        Assert.That(RulesOf<DoseCard>(validation => validation.Add<ScaledDoseRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["dose"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"precisionScale","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":4},{"kind":"lit","value":2}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Dose\u0027 must not be more than 4 digits in total, with allowance for 2 decimals."}]}],"groups":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(PlanOf<DoseCard>(validation => validation.Add<ScaledDoseRules>())), Is.Empty);
    }

    [Test]
    public void a_flags_rule_writes_each_flags_held_form_name_and_number_the_largest_first()
    {
        Assert.That(RulesOf<DayCard>(validation => validation.Add<DayCardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["days"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]},{"kind":"op","op":"inFlags","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":[{"held":4,"name":"Wed","number":4},{"held":2,"name":"Tue","number":2},{"held":1,"name":"Mon","number":1}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Days\u0027 has a range of values which does not include \u0027Mon, Tue\u0027."}]}],"groups":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(PlanOf<DayCard>(validation => validation.Add<DayCardRules>())), Is.Empty);
    }

    [Test]
    public void a_rule_that_stops_writes_its_first_check_alone_for_the_browser()
        => Assert.That(RulesOf<RoundCard>(validation => validation.Add<StoppedBeforeTheServerWardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["ward"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"\u0027Ward\u0027 must not be empty."}]}],"groups":[]}]}"""));

    [Test]
    public void a_rule_for_each_item_that_stops_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<IntakeCard>(validation => validation.Add<StoppedCarersRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'Carers' stops at its first failing item (CascadeMode.Stop), and the browser checks every item, so it would show failures the endpoint never gives; mark the rule .ServerOnly()."));

    [Test]
    public void a_rule_over_no_member_path_is_refused_where_the_view_renders()
        => Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<TrimmedWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'card => card.Ward.Trim()' checks no member of the model, and the browser's rule names the member it checks by its path; write the rule for a member, as RuleFor(x => x.Member) or RuleForEach(x => x.Items) does, or mark the rule .ServerOnly()."));

    [Test]
    public void each_item_is_compared_with_the_member_its_validators_model_holds_beside_it()
    {
        Assert.That(RulesOf<MedicationRound>(validation => validation.Add<MedicationRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"data","path":[{"name":"least","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<OpenMedicationRound>(validation => validation.Add<OpenMedicationRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"and","args":[{"kind":"op","op":"not","args":[{"kind":"op","op":"eq","args":[{"kind":"data","path":[{"name":"least","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":null}]}]},{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"data","path":[{"name":"least","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(PlanOf<OpenMedicationRound>(validation => validation.Add<OpenMedicationRoundRules>())), Is.Empty);
    }

    [Test]
    public async Task the_endpoint_fails_an_item_below_its_peer_and_every_item_beside_a_null_peer()
    {
        Assert.That(await ServerVerdict<MedicationRound, MedicationRoundRules>(("Least", "5"), ("Amounts[0]", "4")), Is.EqualTo(new[] { ("Amounts[0]", "Give at least the least dose.") }));
        Assert.That(await ServerVerdict<MedicationRound, MedicationRoundRules>(("Least", "5"), ("Amounts[0]", "5")), Is.Empty);
        Assert.That(await ServerVerdict<MedicationRound, MedicationRoundRules>(("Least", "5"), ("Amounts[0]", "6")), Is.Empty);
        Assert.That(await ServerVerdict<MedicationRound, MedicationRoundRules>(("Least", "5"), ("Amounts[0]", "5"), ("Amounts[1]", "4")), Is.EqualTo(new[] { ("Amounts[1]", "Give at least the least dose.") }));
        Assert.That(await ServerVerdict<OpenMedicationRound, OpenMedicationRoundRules>(("Least", "5"), ("Amounts[0]", "4")), Is.EqualTo(new[] { ("Amounts[0]", "Give at least the least dose.") }));
        Assert.That(await ServerVerdict<OpenMedicationRound, OpenMedicationRoundRules>(("Least", "5"), ("Amounts[0]", "6")), Is.Empty);
        Assert.That(await ServerVerdict<OpenMedicationRound, OpenMedicationRoundRules>(("Amounts[0]", "6")), Is.EqualTo(new[] { ("Amounts[0]", "Give at least the least dose.") }));
    }

    [Test]
    public void a_check_that_compares_a_date_the_apps_json_options_convert_is_refused_naming_the_rules_check()
    {
        Assert.That(() => PlanOf<Visit>(json => json.Converters.Add(new DayText()), validation => validation.Add<VisitRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(v => v.At).GreaterThan' compares a DateTime the app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds; compare it on the server."));
        Assert.That(() => PlanOf<Visit>(json => json.Converters.Add(new DayText()), validation => validation.Add<ArrivedVisitRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(v => v.At).NotEmpty' compares a DateTime the app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds; compare it on the server."));
        Assert.That(RulesOf<Visit>(validation => validation.Add<VisitRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["at"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"gt","args":[{"kind":"op","op":"dateKey","args":[{"kind":"ref","name":"item1"}]},{"kind":"op","op":"dateKey","args":[{"kind":"lit","value":"2026-11-01T00:00:00"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Book a visit after November 1."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void an_item_its_lists_own_json_writes_compared_with_a_member_is_refused_naming_the_rules_check()
    {
        Assert.That(() => PlanOf<ChartedRound>(validation => validation.Add<ChartedRoundRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleForEach(round => round.Amounts).GreaterThanOrEqualTo' compares two members, and its own JSON number handling writes 'round.Amounts' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));
        Assert.That(() => PlanOf<SealedRound>(validation => validation.Add<SealedRoundRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleForEach(round => round.Amounts).GreaterThanOrEqualTo' compares two members, and its own JSON converter writes 'round.Amounts' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));
    }

    [Test]
    public void a_fixed_value_is_written_in_the_form_its_member_holds()
    {
        Assert.That(RulesOf<DoseChart>(validation => validation.Add<PlannedDoseRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["planned"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":1}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart the dose as given."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":0}]},{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":1}]}]},{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":2}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart a status the round knows."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":0}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart the dose."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<DoseChart>(validation => validation.Add<ChartedDoseRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["charted"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Given"}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart the dose as given."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"or","args":[{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Due"}]},{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Given"}]}]},{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Held"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart a status the round knows."},{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Due"}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart the dose."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void each_item_is_compared_with_a_fixed_value_written_in_the_form_its_list_holds()
    {
        Assert.That(RulesOf<MedicationRound>(validation => validation.Add<PlainSetDoseRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":2.5}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give the set dose."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<ChartedRound>(validation => validation.Add<SetDoseRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eqDecimal","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"2.5"}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give the set dose."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void an_ordering_or_a_range_of_a_value_held_in_its_own_form_is_refused_naming_the_rules_check()
    {
        Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<CountedVialsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Vials).GreaterThan' orders 'chart.Vials', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));
        Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<BoundedVialsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Vials).InclusiveBetween' orders 'chart.Vials', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));
        Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<WeighedMilligramsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Milligrams).GreaterThan' orders 'chart.Milligrams', which its own JSON converter writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));
        Assert.That(() => PlanOf<ChartedRound>(validation => validation.Add<PositiveAmountsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleForEach(round => round.Amounts).GreaterThan' orders 'round.Amounts', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));
    }

    [Test]
    public void a_decimal_its_own_converter_writes_compared_with_a_fixed_value_is_refused_naming_the_rules_check()
        => Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<ExactMilligramsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Milligrams).Equal' compares a decimal its own converter writes as text, whose text the converter decides; compare it on the server."));

    [Test]
    public void a_member_its_own_number_handling_writes_compared_with_another_member_is_refused_naming_the_rules_check()
        => Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<SuppliedVialsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Vials).LessThanOrEqualTo' compares two members, and its own JSON number handling writes 'chart.Vials' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));

    [Test]
    public void a_length_check_over_a_text_its_own_converter_writes_is_refused_naming_the_rules_check()
        => Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<ShortWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Ward).MaximumLength' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_pattern_over_a_text_its_own_converter_writes_is_refused_naming_the_rules_check()
        => Assert.That(() => PlanOf<DoseChart>(validation => validation.Add<CodedWardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(chart => chart.Ward).Matches' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_list_its_own_number_handling_writes_refuses_a_null_test_and_takes_an_emptiness_test_as_a_condition_does()
    {
        Assert.That(RulesOf<MedicationRound>(validation => validation.Add<PlainAmountsRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":null}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Chart the amounts."}]}],"groups":[]}]}"""));
        Assert.That(() => PlanOf<ChartedRound>(validation => validation.Add<ChartedAmountsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(round => round.Amounts).NotNull' compares 'round.Amounts', which its own JSON number handling writes unlike JavaScript's value, and .NET compares the object it holds; compare it on the server."));
        Assert.That(RulesOf<ChartedRound>(validation => validation.Add<FilledAmountsRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts"],"tests":[{"check":{"kind":"fn","params":["value","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"empty","args":[{"kind":"ref","name":"value"}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give a dose."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_check_of_a_list_its_own_converter_writes_whole_is_refused_naming_the_rules_check()
    {
        Assert.That(() => PlanOf<SealedRound>(validation => validation.Add<SealedSetDoseRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleForEach(round => round.Amounts).Equal' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));
        Assert.That(() => PlanOf<SealedRound>(validation => validation.Add<SealedAmountsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(round => round.Amounts).NotEmpty' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));
    }

    [Test]
    public void a_rule_whose_path_reads_past_a_member_its_own_converter_writes_is_refused_naming_the_path_as_its_validator_wrote_it()
    {
        Assert.That(() => PlanOf<SignedHomeCard>(validation => validation.Add<SignedLeadNameRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'card.Lead.Name' reads past 'card.Lead', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'card.Lead' itself, or read past it on the server."));
        Assert.That(RulesOf<HomeCard>(validation => validation.Add<HomeCardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["lead","name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Lead name is required."}]}],"groups":[]}]}"""));
        Assert.That(() => PlanOf<SignedHouseholdCard>(validation => validation.Add<SignedHouseholdPhonesRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'card.Household.Phones' reads past 'card.Household', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'card.Household' itself, or read past it on the server."));
        Assert.That(RulesOf<HouseholdCard>(validation => validation.Add<HouseholdPhonesRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["household","phones",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give each phone."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_nested_rule_under_a_member_its_own_converter_writes_is_refused_naming_the_holder_as_its_validator_wrote_it()
    {
        const string leadNamed = """{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":["lead"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"not","args":[{"kind":"op","op":"nullOrEmpty","args":[{"kind":"op","op":"trim","args":[{"kind":"ref","name":"item1"}]}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Name the lead."}]}],"groups":[]}]}]}""";
        Assert.That(() => PlanOf<SignedHomeCard>(validation => validation.Add<SignedLeadRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'lead.Name' reads past 'card.Lead', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'card.Lead' itself, or read past it on the server."));
        Assert.That(RulesOf<HomeCard>(validation => validation.Add<HomeLeadRules>()), Is.EqualTo(leadNamed));
        Assert.That(() => PlanOf<SignedHomeCard>(validation => validation.Add<SignedLeadChildRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'l.Name' reads past 'card.Lead', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'card.Lead' itself, or read past it on the server."));
        Assert.That(RulesOf<HomeCard>(validation => validation.Add<HomeLeadChildRules>()), Is.EqualTo(leadNamed));
        Assert.That(() => PlanOf<SignedHomeCard>(validation => validation.Add<SignedIncludedLeadRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'lead.Name' reads past 'card.Lead', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'card.Lead' itself, or read past it on the server."));
        Assert.That(RulesOf<HomeCard>(validation => validation.Add<HomeIncludedLeadRules>()), Is.EqualTo(leadNamed));
    }

    [Test]
    public void a_rule_marked_server_only_past_a_member_its_own_converter_writes_is_the_servers_and_is_not_refused()
    {
        Assert.That(RulesOf<SignedHomeCard>(validation => validation.Add<SignedLeadNameServerRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[]}"""));
        Assert.That(Verdict(new SignedLeadNameServerRules().Validate(new SignedHomeCard { Lead = new Lead { Name = "" } })), Is.EqualTo(new[] { ("Lead.Name", "Lead name is required.") }));
        Assert.That(RulesOf<SignedHomeCard>(validation => validation.Add<SignedServerLeadRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":["lead"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[]}]}"""));
    }

    [Test]
    public void a_peer_the_apps_options_do_not_write_reads_its_start_as_the_endpoint_binds_it()
    {
        Assert.That(RulesOf<HiddenBedsCard>(validation => validation.Add<HiddenBedsRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"Beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<StaffedRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<HiddenLeastRound>(validation => validation.Add<HiddenLeastRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"data","path":[{"name":"Least","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<MedicationRound>(validation => validation.Add<MedicationRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"data","path":[{"name":"least","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_member_the_apps_options_do_not_write_compared_with_its_peer_reads_its_start_under_its_name_as_the_validator_wrote_it()
    {
        Assert.That(RulesOf<HiddenBedsCard>(validation => validation.Add<HiddenBedsBesideStaffRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["Beds"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"data","path":[{"name":"Beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"A bed for each of the staff."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<StaffedRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_peer_that_reads_past_a_member_its_own_converter_writes_is_refused_naming_the_peer_as_the_validator_wrote_it()
    {
        Assert.That(() => PlanOf<SignedLeadCard>(validation => validation.Add<SignedLeadCardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'c.Lead.Name' reads past 'c.Lead', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'c.Lead' itself, or read past it on the server."));
        Assert.That(RulesOf<LeadCard>(validation => validation.Add<LeadCardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["resident"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"neq","args":[{"kind":"data","path":[{"name":"resident","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"lead","requires":[],"absent":{"kind":"start","value":"Ruth"}},{"name":"name","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"The lead is someone other than the resident."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_peer_its_own_number_handling_writes_beside_the_rules_member_is_refused_naming_the_peer_as_the_validator_wrote_it()
    {
        Assert.That(() => PlanOf<CountedBedsCard>(validation => validation.Add<CountedBedsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'RuleFor(card => card.Staff).LessThanOrEqualTo' compares two members, and its own JSON number handling writes 'c.Beds' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<BedsBesideStaffRules>()), Is.EqualTo(StaffBesideBeds));
    }

    [Test]
    public void a_peer_cast_to_a_narrower_number_is_refused_naming_the_peer_as_the_validator_wrote_it()
    {
        Assert.That(() => PlanOf<WingCard>(validation => validation.Add<WingCardRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'(int)c.Beds' casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself."));
        Assert.That(RulesOf<SmallWingCard>(validation => validation.Add<SmallWingCardRules>()), Is.EqualTo(StaffBesideBeds));
    }

    [Test]
    public void a_peer_with_no_browser_form_is_refused_naming_the_peer_as_the_validator_wrote_it()
    {
        Assert.That(() => PlanOf<RoundCard>(validation => validation.Add<HashedBedsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'c.Beds.GetHashCode()' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server."));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<BedsBesideStaffRules>()), Is.EqualTo(StaffBesideBeds));
    }

    [Test]
    public void a_member_the_apps_options_do_not_write_inside_a_peers_computation_reads_its_start()
    {
        Assert.That(RulesOf<HiddenOpenBedsCard>(validation => validation.Add<HiddenOpenBedsRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"op","op":"coalesce","args":[{"kind":"data","path":[{"name":"Beds","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<OpenBedsCard>(validation => validation.Add<OpenBedsRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"lte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"op","op":"coalesce","args":[{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"No more staff than beds."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_rule_over_a_member_the_apps_options_do_not_write_tests_its_start_under_its_condition()
    {
        Assert.That(RegistrationOf<HiddenBedsCard>(validation => validation.Add<HiddenBedsCountRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.HiddenBedsCountRules","ruleSet":"default","start":{"staff":0,"Beds":0},"scalars":[],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["Beds"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":4}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"gt","args":[{"kind":"data","path":[{"name":"Beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}},"message":"Four beds on the round."}]}],"groups":[]}]}}"""));
        Assert.That(RulesOf<RoundCard>(validation => validation.Add<RoundBedsCountRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["beds"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":4}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"gt","args":[{"kind":"data","path":[{"name":"beds","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}},"message":"Four beds on the round."}]}],"groups":[]}]}"""));
    }

    [Test]
    public async Task a_member_the_apps_options_leave_out_in_a_child_validators_scope_starts_with_its_holder_as_the_endpoint_binds_it()
    {
        Assert.That(RegistrationOf<CodedDeputyCard>(validation => validation.Add<CodedDeputyCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.CodedDeputyCardRules","ruleSet":"default","start":{"deputy":{"name":null,"Code":"A1"}},"scalars":[],"objects":[{"path":["deputy"],"start":{"name":null,"Code":"Z9"},"json":"replace"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":["deputy"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["Code"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"A1"}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Code the deputy A1."}]}],"groups":[]}]}]}}"""));
        Assert.That(await ServerVerdict<CodedDeputyCard, CodedDeputyCardRules>(("Deputy.Name", "Ann")), Is.Empty);
    }

    [Test]
    public void a_peer_and_a_condition_that_read_a_member_the_apps_options_leave_out_in_a_child_validators_scope_read_it_from_its_holder_as_the_endpoint_binds_it()
        => Assert.That(RegistrationOf<CodedDeputyCard>(validation => validation.Add<CodedDeputyNameCardRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.CodedDeputyNameCardRules","ruleSet":"default","start":{"deputy":{"name":null,"Code":"A1"}},"scalars":[],"objects":[{"path":["deputy"],"start":{"name":null,"Code":"Z9"},"json":"replace"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":["deputy"],"when":{"kind":"fn","params":["value"],"body":{"kind":"op","op":"neq","args":[{"kind":"ref","name":"value"},{"kind":"lit","value":null}]}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["name"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"neq","args":[{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"data","path":[{"name":"Code","requires":[],"absent":{"kind":"start","value":"Z9"}}],"target":{"kind":"ref","name":"item1"}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"neq","args":[{"kind":"data","path":[{"name":"Code","requires":[],"absent":{"kind":"start","value":"Z9"}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":"Z9"}]}},"message":"Name the deputy apart from the code."}]}],"groups":[]}]}]}}"""));

    [Test]
    public void a_member_the_apps_options_leave_out_past_a_holder_starts_with_its_holder_as_the_endpoint_binds_it()
        => Assert.That(RegistrationOf<CodedDeputyCard>(validation => validation.Add<CodedDeputyPastItsHolderRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.CodedDeputyPastItsHolderRules","ruleSet":"default","start":{"deputy":{"name":null,"Code":"A1"}},"scalars":[],"objects":[{"path":["deputy"],"start":{"name":null,"Code":"Z9"},"json":"replace"}],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["deputy","Code"],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"A1"}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Code the deputy A1."}]}],"groups":[]}]}}"""));

    [Test]
    public async Task each_item_of_a_list_the_apps_options_leave_out_is_checked_over_the_lists_start()
    {
        Assert.That(RegistrationOf<HiddenDosesRound>(validation => validation.Add<HiddenDosesRoundRules>()), Is.EqualTo("""{"validator":"Alis.Reactive.ModuleTests.Validation.HiddenDosesRoundRules","ruleSet":"default","start":{"Doses":[5,4]},"scalars":[],"objects":[],"lists":[],"rules":{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["Doses",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["item1","instance"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":5}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least five."}]}],"groups":[]}]}}"""));
        Assert.That(await ServerVerdict<HiddenDosesRound, HiddenDosesRoundRules>(), Is.EqualTo(new[] { ("Doses[1]", "Give at least five.") }));
    }

    [Test]
    public void a_member_the_apps_options_leave_out_past_a_holder_its_own_converter_writes_is_refused_naming_the_rule_as_the_validator_wrote_it()
        => Assert.That(() => PlanOf<SignedHomeCard>(validation => validation.Add<SignedLeadCodeRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'card => card.Lead.Code' reads a member of 'Lead' that the app's JSON options do not write, so the browser never holds it."));

    [Test]
    public void a_peer_that_reads_an_override_the_apps_options_leave_out_is_refused_naming_the_peer_as_the_validator_wrote_it()
    {
        Assert.That(() => PlanOf<OverriddenBedsCard>(json => json.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { LeaveOutTheOverriddenBeds } }, validation => validation.Add<OverriddenBedsRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("'card => card.Beds' reads a member of 'OverriddenBedsCard' that the app's JSON options do not write, so the browser never holds it."));
        Assert.That(RulesOf<OverriddenBedsCard>(validation => validation.Add<OverriddenBedsRules>()), Is.EqualTo(StaffBesideBeds));
    }

    [Test]
    public void a_lambda_inside_a_peer_names_its_item_one_level_below_the_checked_model()
        => Assert.That(RulesOf<RotaCard>(validation => validation.Add<RotaCardRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["staff"],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"data","path":[{"name":"staff","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"ref","name":"item1"}},{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"data","path":[{"name":"carers","requires":[],"absent":{"kind":"start","value":[]}}],"target":{"kind":"ref","name":"item1"}},"args":[{"kind":"fn","params":["item2"],"body":{"kind":"op","op":"neq","args":[{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item2"}},{"kind":"lit","value":null}]}}]}}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"A shift for each named carer."}]}],"groups":[]}]}"""));

    [Test]
    public void a_member_the_apps_options_do_not_write_inside_the_peer_of_each_items_comparison_reads_its_start()
    {
        Assert.That(RulesOf<HiddenOpenLeastRound>(validation => validation.Add<HiddenOpenLeastRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"op","op":"coalesce","args":[{"kind":"data","path":[{"name":"Least","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
        Assert.That(RulesOf<OpenMedicationRound>(validation => validation.Add<OpenLeastRoundRules>()), Is.EqualTo("""{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[],"groups":[{"path":[],"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"fields":[{"path":["amounts",{"kind":"each"}],"tests":[{"check":{"kind":"fn","params":["value","item1"],"body":{"kind":"op","op":"gte","args":[{"kind":"ref","name":"value"},{"kind":"op","op":"coalesce","args":[{"kind":"data","path":[{"name":"least","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}]}},"when":{"kind":"fn","params":["item1"],"body":{"kind":"lit","value":true}},"message":"Give at least the least dose."}]}],"groups":[]}]}"""));
    }

    [Test]
    public void a_rule_over_no_member_path_beside_a_peer_is_refused_as_a_rule_over_no_member_path_whatever_member_it_reads()
    {
        Assert.That(() => PlanOf<HiddenOpenBedsCard>(validation => validation.Add<HiddenOpenBedsBesideStaffRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'card => (card.Beds ?? 0)' checks no member of the model, and the browser's rule names the member it checks by its path; write the rule for a member, as RuleFor(x => x.Member) or RuleForEach(x => x.Items) does, or mark the rule .ServerOnly()."));
        Assert.That(() => PlanOf<OpenBedsCard>(validation => validation.Add<OpenBedsBesideStaffRules>()),
            Throws.InvalidOperationException.With.Message.EqualTo("The rule of 'card => (card.Beds ?? 0)' checks no member of the model, and the browser's rule names the member it checks by its path; write the rule for a member, as RuleFor(x => x.Member) or RuleForEach(x => x.Items) does, or mark the rule .ServerOnly()."));
    }

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

    private static string ListsOf<TModel>(Action<ReactiveFluentValidationBuilder> validators) where TModel : new()
    {
        using var plan = JsonDocument.Parse(RegistrationOf<TModel>(validators));
        return plan.RootElement.GetProperty("lists").GetRawText();
    }

    private static string RulesOf<TModel>(Action<ReactiveFluentValidationBuilder> validators) where TModel : new()
    {
        using var plan = JsonDocument.Parse(RegistrationOf<TModel>(validators));
        return plan.RootElement.GetProperty("rules").GetRawText();
    }

    private static string PlanOf<TModel>(Action<ReactiveFluentValidationBuilder> validators) where TModel : new() => PlanOf<TModel>(_ => { }, validators);

    private static string PlanOf<TModel>(Action<JsonSerializerOptions> appJson, Action<ReactiveFluentValidationBuilder> validators) where TModel : new()
    {
        var html = Views.Of(new TModel(), appJson, services => services.AddReactiveFluentValidation(validators));
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

    private static void LeaveOutTheOverriddenBeds(JsonTypeInfo type)
    {
        if (type.Type != typeof(OverriddenBedsCard)) return;
        type.Properties.Remove(type.Properties.Single(member => member.Name == "beds"));
    }

    private static (string, string)[] Verdict(ValidationResult result) => [.. result.Errors.Select(error => (error.PropertyName, error.ErrorMessage))];

    private sealed class CheckedController : Controller;
}
