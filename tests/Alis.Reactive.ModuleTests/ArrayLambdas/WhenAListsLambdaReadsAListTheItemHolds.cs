using System;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsAListTheItemHolds
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string AnyDose =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.ArrayLambdas.MedicationRound\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"cart\":{\"kind\":\"element\",\"id\":\"cart\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\"," +
        "\"target\":{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":";

    private const string Ends = "}]}}]}]}";

    private const string Dose = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string Check = "{\"kind\":\"ref\",\"name\":\"item2\"}";

    private const string ItsChecks = "{\"kind\":\"data\",\"path\":[{\"name\":\"checks\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":[]}}],\"target\":" + Dose + "}";

    private const string ItsHours = "{\"kind\":\"data\",\"path\":[{\"name\":\"hours\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":[]}}],\"target\":" + Dose + "}";

    private const string ItsRounds = "{\"kind\":\"data\",\"path\":[{\"name\":\"rounds\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":[]}}],\"target\":" + Dose + "}";

    private const string ItsSlots = "{\"kind\":\"data\",\"path\":[{\"name\":\"slots\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":[]}}],\"target\":" + Dose + "}";

    private const string ItsCarer = "{\"kind\":\"data\",\"path\":[{\"name\":\"carer\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Dose + "}";

    private const string ItsCarersName = "{\"kind\":\"data\",\"path\":[" +
        "{\"name\":\"carer\",\"requires\":[],\"absent\":{\"kind\":\"nothing\"}}," +
        "{\"name\":\"name\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"unnamed\"}}],\"target\":" + Dose + "}";

    private const string ChecksName = "{\"kind\":\"data\",\"path\":[{\"name\":\"name\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"unnamed\"}}],\"target\":" + Check + "}";

    private const string CheckedByMuhr = "{\"kind\":\"fn\",\"params\":[\"item2\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ChecksName + ",{\"kind\":\"lit\",\"value\":\"Muhr\"}]}}";

    private const string OverOne = "{\"kind\":\"lit\",\"value\":1}";

    private const string AnyCheck = AnyDose +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsChecks + "," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item2\"],\"body\":{\"kind\":\"lit\",\"value\":true}}]}" + Ends;

    private const string CheckedByItsCarer = AnyDose +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + ItsCarer + ",{\"kind\":\"lit\",\"value\":null}]}," +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsChecks + "," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item2\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ChecksName + "," + ItsCarersName + "]}}]}]}" + Ends;

    private const string EveryCheckByMuhr = AnyDose +
        "{\"kind\":\"call\",\"path\":[\"every\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsChecks + ",\"args\":[" + CheckedByMuhr + "]}" + Ends;

    private const string MoreThanOneCheck = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsChecks + "}," + OverOne + "]}" + Ends;

    private const string MoreThanOneCheckByMuhr = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
        "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsChecks + ",\"args\":[" + CheckedByMuhr + "]}}," + OverOne + "]}" + Ends;

    private const string MoreThanOneHour = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsHours + "}," + OverOne + "]}" + Ends;

    private const string MoreThanOneRound = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsRounds + "}," + OverOne + "]}" + Ends;

    private const string MoreThanOneSlot = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsSlots + "}," + OverOne + "]}" + Ends;

    [Test]
    public void any_with_no_predicate_tests_that_the_list_holds_an_item()
        => Assert.That(Rendered(d => d.Checks.Any()), Is.EqualTo(ScriptOpen + AnyCheck + ScriptClose));

    [Test]
    public void an_inner_lambda_reads_its_own_item_and_the_item_of_the_lambda_around_it()
        => Assert.That(Rendered(d => d.Carer != null && d.Checks.Any(c => c.Name == d.Carer.Name)), Is.EqualTo(ScriptOpen + CheckedByItsCarer + ScriptClose));

    [Test]
    public void all_tests_every_item_of_the_list_the_item_holds()
        => Assert.That(Rendered(d => d.Checks.All(c => c.Name == "Muhr")), Is.EqualTo(ScriptOpen + EveryCheckByMuhr + ScriptClose));

    [Test]
    public void count_with_no_predicate_counts_the_items()
        => Assert.That(Rendered(d => d.Checks.Count() > 1), Is.EqualTo(ScriptOpen + MoreThanOneCheck + ScriptClose));

    [Test]
    public void count_with_a_predicate_counts_the_items_it_keeps()
        => Assert.That(Rendered(d => d.Checks.Count(c => c.Name == "Muhr") > 1), Is.EqualTo(ScriptOpen + MoreThanOneCheckByMuhr + ScriptClose));

    [Test]
    public void a_lists_count_counts_its_items()
        => Assert.That(Rendered(d => d.Hours.Count > 1), Is.EqualTo(ScriptOpen + MoreThanOneHour + ScriptClose));

    [Test]
    public void a_read_only_lists_count_counts_its_items()
        => Assert.That(Rendered(d => d.Rounds.Count > 1), Is.EqualTo(ScriptOpen + MoreThanOneRound + ScriptClose));

    [Test]
    public void an_arrays_length_counts_its_items()
        => Assert.That(Rendered(d => d.Slots.Length > 1), Is.EqualTo(ScriptOpen + MoreThanOneSlot + ScriptClose));

    private static string Rendered(Expression<Func<CartDose, bool>> predicate)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Flagged.Set(cart.Doses.Any(predicate));
        }));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
