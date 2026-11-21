using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaMatchesAPattern
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

    private const string ItsNote = "{\"kind\":\"data\",\"path\":[{\"name\":\"note\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}";

    private const string EndsInFood = "{\"kind\":\"lit\",\"value\":\"food(?=\\\\n?$)\"}";

    [Test]
    public void a_pattern_the_view_writes_matches_its_end_as_dotnet_reads_it()
        => Assert.That(Rendered(d => Regex.IsMatch(d.Note!, "food$")), Is.EqualTo(Plan(Matched(EndsInFood))));

    [Test]
    public void an_escaped_dollar_matches_the_character()
        => Assert.That(Rendered(d => Regex.IsMatch(d.Note!, @"\$")), Is.EqualTo(Plan(Matched("{\"kind\":\"lit\",\"value\":\"\\\\$\"}"))));

    [Test]
    public void a_pattern_the_view_holds_in_a_local_matches_as_one_written_in_place()
    {
        var endsInFood = "food$";

        Assert.That(Rendered(d => Regex.IsMatch(d.Note!, endsInFood)), Is.EqualTo(Plan(Matched(EndsInFood))));
    }

    [Test]
    public void a_lookbehind_under_culture_invariant_matches_as_with_no_option()
        => Assert.That(
            Rendered(d => Regex.IsMatch(d.Note!, "(?<=after )food", RegexOptions.CultureInvariant)),
            Is.EqualTo(Plan(Matched("{\"kind\":\"lit\",\"value\":\"(?\\u003C=after )food\"}"))));

    [Test]
    public void no_option_matches_as_the_pattern_alone()
        => Assert.That(Rendered(d => Regex.IsMatch(d.Note!, "food", RegexOptions.None)), Is.EqualTo(Plan(Matched("{\"kind\":\"lit\",\"value\":\"food\"}"))));

    [Test]
    public void a_construct_the_browser_reads_otherwise_is_refused_naming_the_spelling_both_read_alike()
        => Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, @"^\d")),
            Throws.InvalidOperationException.With.Message.EqualTo(@"'IsMatch(d.Note, ""^\d"")' holds '\d', which .NET and the browser read differently; write [0-9]."));

    [Test]
    public void ignore_case_is_refused_naming_both_cases_spelled()
        => Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, "aspirin", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'IsMatch(d.Note, \"aspirin\", IgnoreCase, CultureInvariant)' passes RegexOptions.IgnoreCase, which no browser flag folds as .NET does; spell both cases, such as [Aa]spirin."));

    [Test]
    public void an_option_other_than_culture_invariant_is_refused()
        => Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, "^food", RegexOptions.Multiline)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'IsMatch(d.Note, \"^food\", Multiline)' passes an option other than RegexOptions.CultureInvariant, which the browser does not read as .NET does; move the work to the server."));

    [Test]
    public void a_pattern_read_from_the_item_is_refused()
        => Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, d.Drug)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'IsMatch(d.Note, d.Drug)' reads its pattern from the item, and a pattern read in the browser cannot be checked; pass a pattern the view holds, or move the work to the server."));

    [Test]
    public void options_read_from_the_item_have_no_browser_form()
        => Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, "food", d.Matching)),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'IsMatch(d.Note, \"food\", d.Matching)' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server."));

    [Test]
    public void a_pattern_that_is_null_when_the_view_renders_is_refused()
    {
        string? none = null;

        Assert.That(
            () => Refused(d => Regex.IsMatch(d.Note!, none!)),
            Throws.InvalidOperationException.With.Message.EqualTo("'IsMatch(d.Note, none)' matches a pattern that is null when the view renders."));
    }

    private static string Matched(string pattern) => "{\"kind\":\"op\",\"op\":\"matches\",\"args\":[" + ItsNote + "," + pattern + "]}";

    private static string Plan(string body) => ScriptOpen + AnyDose + body + Ends + ScriptClose;

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

    private static void Refused(Expression<Func<CartDose, bool>> predicate)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Flagged.Set(AnyOf(cart.Doses, predicate));
        }));
        html.RenderPlan(plan);
    }

    private static TypedSource<bool> AnyOf(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, bool>> predicate) => doses.Any(predicate);
}
