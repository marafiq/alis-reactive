using System;
using System.Collections.Generic;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListOperationIsHandedAText
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string CountsTheLetters =
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
        "{\"kind\":\"set\",\"path\":[\"counted\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
        "{\"kind\":\"get\",\"path\":[\"letters\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}}}]}]}";

    private static readonly TestCaseData[] EachOperation =
    [
        Operation("Count", letters => _ = letters.Count()),
        Operation("Count", letters => _ = letters.Count(letter => letter == 'a')),
        Operation("Where", letters => _ = letters.Where(letter => letter == 'a')),
        Operation("Select", letters => _ = letters.Select(letter => letter)),
        Operation("Any", letters => _ = letters.Any()),
        Operation("Any", letters => _ = letters.Any(letter => letter == 'a')),
        Operation("All", letters => _ = letters.All(letter => letter == 'a')),
        Operation("Find", letters => _ = letters.Find(letter => letter == 'a')),
        Operation("Find", letters => _ = letters.Find(letter => letter == 'a', letter => letter)),
        Operation("OrderBy", letters => _ = letters.OrderBy(letter => letter)),
        Operation("OrderBy", letters => _ = letters.OrderBy<char, int>(letter => null)),
        Operation("OrderBy", letters => _ = letters.OrderBy(letter => letter.ToString(), StringComparer.Ordinal)),
        Operation("OrderByDescending", letters => _ = letters.OrderByDescending(letter => letter)),
        Operation("OrderByDescending", letters => _ = letters.OrderByDescending<char, int>(letter => null)),
        Operation("OrderByDescending", letters => _ = letters.OrderByDescending(letter => letter.ToString(), StringComparer.Ordinal)),
        Operation("Sum", letters => _ = letters.Sum(letter => (int)letter)),
        Operation("Sum", letters => _ = letters.Sum(letter => (double)letter)),
        Operation("Sum", letters => _ = letters.Sum(letter => (decimal)letter)),
    ];

    [TestCaseSource(nameof(EachOperation))]
    public void a_text_a_helper_hands_on_is_refused_naming_the_operation(string operation, Action<ITypedSource<IEnumerable<char>>> run)
        => Assert.That(
            () => Refused(cart => run(cart.Note)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The source passed to " + operation + " is text, and a text is no list in the browser; run " + operation +
                " over a list, such as a List<T> or an array, or test the text with Contains, StartsWith, EndsWith, MinLength or Matches."));

    [Test]
    public void a_list_of_letters_the_same_helper_hands_on_is_counted()
        => Assert.That(Rendered(cart => cart.Counted.Set(CountOf(cart.Letters))), Is.EqualTo(ScriptOpen + CountsTheLetters + ScriptClose));

    [Test]
    public void the_plan_of_a_list_of_letters_counted_is_valid_by_the_plan_schema()
        => Assert.That(PlanSchema.ProblemsOf(CountsTheLetters), Is.Empty);

    private static TestCaseData Operation(string operation, Action<ITypedSource<IEnumerable<char>>> run) => new TestCaseData(operation, run).SetArgDisplayNames(operation);

    private static TypedSource<int> CountOf(ITypedSource<IEnumerable<char>> letters) => letters.Count();

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        return Views.TextOf(html.RenderPlan(plan));
    }

    private static void Refused(Action<DoseCart> step)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        html.RenderPlan(plan);
    }
}
