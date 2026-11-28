using System;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsANullable
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

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string ItsPriority = "{\"kind\":\"data\",\"path\":[{\"name\":\"priority\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}";

    private const string ItsRecordedKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"recorded\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}]}";

    private const string HasAPriority = AnyDose + "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + ItsPriority + ",{\"kind\":\"lit\",\"value\":null}]}" + Ends;

    private const string WasRecorded = AnyDose +
        "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + ItsRecordedKey + ",{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"lit\",\"value\":null}]}]}" + Ends;

    private const string PriorityOverTwo = AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"op\",\"op\":\"nullableValue\",\"args\":[" + ItsPriority + "]},{\"kind\":\"lit\",\"value\":2}]}" + Ends;

    [Test]
    public void has_value_tests_that_the_nullable_is_not_null()
        => Assert.That(Rendered(d => d.Priority.HasValue), Is.EqualTo(ScriptOpen + HasAPriority + ScriptClose));

    [Test]
    public void has_value_of_a_date_compares_its_key_with_null()
        => Assert.That(Rendered(d => d.Recorded.HasValue), Is.EqualTo(ScriptOpen + WasRecorded + ScriptClose));

    [Test]
    public void value_reads_the_nullables_value_and_nothing_where_it_holds_null()
        => Assert.That(Rendered(d => d.Priority!.Value > 2), Is.EqualTo(ScriptOpen + PriorityOverTwo + ScriptClose));

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
