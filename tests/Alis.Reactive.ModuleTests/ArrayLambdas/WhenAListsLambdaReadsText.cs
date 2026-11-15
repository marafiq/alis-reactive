using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsText
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string PlanOpen = "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.ArrayLambdas.MedicationRound\",";

    private const string EnUs = "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\",";

    private const string TrTr = "\"culture\":{\"name\":\"tr-TR\",\"collation\":\"tr-TR\",\"decimal\":\",\",\"group\":\".\",";

    private const string AnyDose =
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

    private const string ItsDrug = "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsNote = "{\"kind\":\"data\",\"path\":[{\"name\":\"note\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}";

    private const string Digoxin = "{\"kind\":\"lit\",\"value\":\"Digoxin\"}";

    private const string DigoxinInCapitals = "{\"kind\":\"lit\",\"value\":\"DIGOXIN\"}";

    private const string DigoxinInSmallLetters = "{\"kind\":\"lit\",\"value\":\"digoxin\"}";

    private const string LongerThanSix = PlanOpen + EnUs + AnyDose +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + ItsDrug + "},{\"kind\":\"lit\",\"value\":6}]}" + Ends;

    private const string Trimmed = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"trim\",\"args\":[" + ItsDrug + "]}," + Digoxin + "]}" + Ends;

    private const string TrimmedAtTheStart = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"trimStart\",\"args\":[" + ItsDrug + "]}," + Digoxin + "]}" + Ends;

    private const string TrimmedAtTheEnd = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"trimEnd\",\"args\":[" + ItsDrug + "]}," + Digoxin + "]}" + Ends;

    private const string UpperInvariant = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"upperInvariant\",\"args\":[" + ItsDrug + "]}," + DigoxinInCapitals + "]}" + Ends;

    private const string LowerInvariant = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"lowerInvariant\",\"args\":[" + ItsDrug + "]}," + DigoxinInSmallLetters + "]}" + Ends;

    private const string UpperInEnglish = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"upper\",\"args\":[" + ItsDrug + "]}," + DigoxinInCapitals + "]}" + Ends;

    private const string UpperInTurkish = PlanOpen + TrTr + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"upperTurkic\",\"args\":[" + ItsDrug + "]}," + DigoxinInCapitals + "]}" + Ends;

    private const string LowerInEnglish = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"lower\",\"args\":[" + ItsDrug + "]}," + DigoxinInSmallLetters + "]}" + Ends;

    private const string LowerInTurkish = PlanOpen + TrTr + AnyDose + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"lowerTurkic\",\"args\":[" + ItsDrug + "]}," + DigoxinInSmallLetters + "]}" + Ends;

    private const string HoldsGox = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"contains\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"gox\"}]}" + Ends;

    private const string HoldsX = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"contains\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"x\"}]}" + Ends;

    private const string StartsWithD = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"startsWith\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"D\"}]}" + Ends;

    private const string EndsWithXin = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"endsWith\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"xin\"}]}" + Ends;

    private const string NoNote = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"nullOrEmpty\",\"args\":[" + ItsNote + "]}" + Ends;

    private const string BlankNote = PlanOpen + EnUs + AnyDose + "{\"kind\":\"op\",\"op\":\"nullOrEmpty\",\"args\":[{\"kind\":\"op\",\"op\":\"trim\",\"args\":[" + ItsNote + "]}]}" + Ends;

    [Test]
    public void a_texts_length_counts_its_code_units()
        => Assert.That(Rendered(d => d.Drug.Length > 6), Is.EqualTo(ScriptOpen + LongerThanSix + ScriptClose));

    [Test]
    public void trim_removes_white_space_as_dotnet_does()
        => Assert.That(Rendered(d => d.Drug.Trim() == "Digoxin"), Is.EqualTo(ScriptOpen + Trimmed + ScriptClose));

    [Test]
    public void trim_start_with_no_characters_removes_white_space_at_the_start()
        => Assert.That(Rendered(d => d.Drug.TrimStart() == "Digoxin"), Is.EqualTo(ScriptOpen + TrimmedAtTheStart + ScriptClose));

    [Test]
    public void trim_end_given_an_empty_array_removes_white_space_at_the_end_as_dotnet_does()
        => Assert.That(Rendered(d => d.Drug.TrimEnd(new char[] { }) == "Digoxin"), Is.EqualTo(ScriptOpen + TrimmedAtTheEnd + ScriptClose));

    [Test]
    public void trim_start_given_characters_is_refused_since_the_browser_removes_white_space_alone()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var trimmed = new[] { 'A', 's' };

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Drug.TrimStart(trimmed) == "goxin"));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Drug.TrimStart(trimmed)' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server."));
    }

    [Test]
    public void to_upper_invariant_upper_cases_by_the_invariant_mapping()
        => Assert.That(Rendered(d => d.Drug.ToUpperInvariant() == "DIGOXIN"), Is.EqualTo(ScriptOpen + UpperInvariant + ScriptClose));

    [Test]
    public void to_lower_invariant_lower_cases_by_the_invariant_mapping()
        => Assert.That(Rendered(d => d.Drug.ToLowerInvariant() == "digoxin"), Is.EqualTo(ScriptOpen + LowerInvariant + ScriptClose));

    [Test]
    public void to_upper_under_a_culture_that_is_not_turkic_upper_cases_the_dotless_i_as_i()
        => Assert.That(Rendered(d => d.Drug.ToUpper() == "DIGOXIN"), Is.EqualTo(ScriptOpen + UpperInEnglish + ScriptClose));

    [Test]
    [SetCulture("tr-TR")]
    public void to_upper_under_a_turkic_culture_upper_cases_i_with_its_dot()
        => Assert.That(Rendered(d => d.Drug.ToUpper() == "DIGOXIN"), Is.EqualTo(ScriptOpen + UpperInTurkish + ScriptClose));

    [Test]
    public void to_lower_under_a_culture_that_is_not_turkic_lower_cases_the_dotted_capital_i_as_i()
        => Assert.That(Rendered(d => d.Drug.ToLower() == "digoxin"), Is.EqualTo(ScriptOpen + LowerInEnglish + ScriptClose));

    [Test]
    [SetCulture("tr-TR")]
    public void to_lower_under_a_turkic_culture_lower_cases_i_to_the_dotless_i()
        => Assert.That(Rendered(d => d.Drug.ToLower() == "digoxin"), Is.EqualTo(ScriptOpen + LowerInTurkish + ScriptClose));

    [Test]
    public void contains_looks_for_a_text_the_view_holds_by_code_unit()
    {
        var fragment = "gox";

        Assert.That(Rendered(d => d.Drug.Contains(fragment)), Is.EqualTo(ScriptOpen + HoldsGox + ScriptClose));
    }

    [Test]
    public void contains_looks_for_a_char_as_a_one_letter_text()
        => Assert.That(Rendered(d => d.Drug.Contains('x')), Is.EqualTo(ScriptOpen + HoldsX + ScriptClose));

    [Test]
    public void starts_with_by_the_ordinal_comparison_tests_the_start_by_code_unit()
        => Assert.That(Rendered(d => d.Drug.StartsWith("D", StringComparison.Ordinal)), Is.EqualTo(ScriptOpen + StartsWithD + ScriptClose));

    [Test]
    public void ends_with_by_the_ordinal_comparison_tests_the_end_by_code_unit()
        => Assert.That(Rendered(d => d.Drug.EndsWith("xin", StringComparison.Ordinal)), Is.EqualTo(ScriptOpen + EndsWithXin + ScriptClose));

    [Test]
    public void starts_with_and_no_comparison_is_refused_since_dotnet_compares_it_by_the_culture()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Ward.StartsWith("N")));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Ward.StartsWith(\"N\")' compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as d.Ward.StartsWith(\"N\", StringComparison.Ordinal)."));
    }

    [Test]
    public void ends_with_by_a_comparison_other_than_ordinal_is_refused_naming_the_comparison()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var comparison = StringComparison.OrdinalIgnoreCase;

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Ward.EndsWith("th", comparison)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Ward.EndsWith(\"th\", comparison)' compares text by StringComparison.OrdinalIgnoreCase; a list's lambda compares text by StringComparison.Ordinal."));
    }

    [Test]
    public void is_null_or_empty_holds_on_a_text_that_reads_nothing_holds_null_or_is_empty()
        => Assert.That(Rendered(d => string.IsNullOrEmpty(d.Note)), Is.EqualTo(ScriptOpen + NoNote + ScriptClose));

    [Test]
    public void is_null_or_white_space_tests_the_trimmed_text()
        => Assert.That(Rendered(d => string.IsNullOrWhiteSpace(d.Note)), Is.EqualTo(ScriptOpen + BlankNote + ScriptClose));

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

    private static TypedSource<bool> AnyOf(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, bool>> predicate) => doses.Any(predicate);
}
