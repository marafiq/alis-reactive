using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenADataLambdaReadsPastNull
{
    private const string Dereference = "CS8602";
    private const string NullableValue = "CS8629";

    private static string Viewed(string statement) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Linq.Expressions;
        using Alis.Reactive.Components;
        using Alis.Reactive.Lists;
        using Alis.Reactive.Pipelines;
        using Alis.Reactive.Values;

        namespace Ward;

        public sealed class Carer
        {
            public string Name { get; set; } = "";
        }

        public sealed class Check
        {
            public Carer? By { get; set; }
        }

        public sealed class Supply
        {
            public int Vials { get; set; }
        }

        public sealed class Dose
        {
            public string Medicine { get; set; } = "";

            public string? Ward { get; set; }

            public Carer? Carer { get; set; }

            public int? Priority { get; set; }

            public Supply? Supply { get; set; }

            public List<Check> Checks { get; set; } = [];
        }

        public sealed class Noted
        {
            public string Note { get; set; } = "";
        }

        public sealed class Round
        {
            public Dose? Rescue { get; set; }
        }

        public sealed class Ledger<T>
        {
            public int Count(Expression<Func<T, bool>> keep) => 0;
        }

        public static class View
        {
            public static void Write(ITypedSource<IEnumerable<Dose>> doses, ReadOnly<Noted?> noted, MethodResult<Noted?> answered, Pipeline<Round> pipeline, Carer? cover, int? limit, List<Dose> served, Ledger<Dose> ledger)
            {
                {{statement}}
            }
        }
        """;

    [TestCase("""doses.Count(d => [|d.Carer|].Name == "Ruth");""")]
    [TestCase("""doses.Where(d => [|d.Carer|].Name == "Ruth");""")]
    [TestCase("""doses.Select(d => [|d.Carer|].Name);""")]
    [TestCase("""doses.Any(d => [|d.Carer|].Name == "Ruth");""")]
    [TestCase("""doses.All(d => [|d.Carer|].Name == "Ruth");""")]
    [TestCase("""doses.Find(d => [|d.Carer|].Name == "Ruth");""")]
    [TestCase("""doses.Find(d => d.Medicine == "Aspirin", d => [|d.Carer|].Name);""")]
    [TestCase("""noted.Read(detail => [|detail|].Note);""")]
    [TestCase("""answered.Read(detail => [|detail|].Note);""")]
    [TestCase("""doses.Count(d => [|d.Ward|].Contains("th"));""")]
    public Task a_read_past_a_holder_in_a_door_that_reads_nothing_past_null_is_suppressed(string statement)
        => Fixture.Suppresses<ReadsNothingSuppressor>(Dereference, ReadsNothingSuppressor.PastANullHolderId, Viewed(statement));

    [TestCase("""doses.Sum(d => [|d.Supply|].Vials);""")]
    [TestCase("""doses.Sum(d => (double)[|d.Supply|].Vials);""")]
    [TestCase("""doses.Sum(d => (decimal)[|d.Supply|].Vials);""")]
    [TestCase("""doses.OrderBy(d => [|d.Carer|].Name);""")]
    [TestCase("""doses.OrderBy(d => [|d.Carer|].Name, StringComparer.Ordinal);""")]
    [TestCase("""doses.OrderByDescending(d => [|d.Carer|].Name);""")]
    [TestCase("""doses.OrderByDescending(d => [|d.Carer|].Name, StringComparer.Ordinal);""")]
    public Task a_read_past_a_holder_in_a_term_or_key_that_stops_past_null_is_kept(string statement)
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed(statement));

    [TestCase("""doses.Sum(d => d.Checks.Count(c => [|c.By|].Name == "Ruth"));""")]
    [TestCase("""doses.OrderBy(d => d.Checks.Any(c => [|c.By|].Name == "Ruth"));""")]
    [TestCase("""doses.Count(d => d.Checks.All(c => [|c.By|].Name == "Ruth"));""")]
    public Task a_read_in_a_predicate_nested_in_a_data_lambda_is_suppressed(string statement)
        => Fixture.Suppresses<ReadsNothingSuppressor>(Dereference, ReadsNothingSuppressor.PastANullHolderId, Viewed(statement));

    [Test]
    public Task a_read_of_the_outer_item_in_a_nested_predicate_is_suppressed()
        => Fixture.Suppresses<ReadsNothingSuppressor>(Dereference, ReadsNothingSuppressor.PastANullHolderId, Viewed("""doses.Count(d => d.Checks.Any(c => c.By != null && c.By.Name == [|d.Carer|].Name));"""));

    [Test]
    public Task a_value_the_view_holds_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""doses.Count(d => d.Medicine == [|cover|].Name);"""));

    [Test]
    public Task a_value_the_view_holds_in_a_nested_predicate_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""doses.Count(d => d.Checks.Any(c => c.By != null && c.By.Name == [|cover|].Name));"""));

    [Test]
    public Task an_input_named_past_a_null_holder_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""pipeline.Input(m => [|m.Rescue|].Medicine);"""));

    [Test]
    public Task a_lambda_over_a_list_the_server_holds_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""served.Count(d => [|d.Carer|].Name == "Ruth");"""));

    [Test]
    public Task a_predicate_nested_in_a_lambda_over_a_list_the_server_holds_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""served.Count(d => d.Checks.Any(c => [|c.By|].Name == "Ruth"));"""));

    [Test]
    public Task an_expression_door_the_framework_does_not_own_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(Dereference, Viewed("""ledger.Count(d => [|d.Carer|].Name == "Ruth");"""));

    [Test]
    public Task the_value_of_a_nullable_in_a_predicate_is_suppressed()
        => Fixture.Suppresses<ReadsNothingSuppressor>(NullableValue, ReadsNothingSuppressor.ValueOfANullId, Viewed("""doses.Count(d => [|d.Priority|].Value != 2);"""));

    [Test]
    public Task a_cast_of_a_nullable_in_a_predicate_is_suppressed()
        => Fixture.Suppresses<ReadsNothingSuppressor>(NullableValue, ReadsNothingSuppressor.ValueOfANullId, Viewed("""doses.Count(d => [|(int)d.Priority|] == 2);"""));

    [Test]
    public Task a_cast_of_a_nullable_in_a_sum_term_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(NullableValue, Viewed("""doses.Sum(d => [|(int)d.Priority|]);"""));

    [Test]
    public Task a_cast_of_a_nullable_the_view_holds_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(NullableValue, Viewed("""doses.Count(d => d.Priority == [|(int)limit|]);"""));

    [Test]
    public Task the_value_of_a_nullable_in_a_sum_term_is_kept()
        => Fixture.Keeps<ReadsNothingSuppressor>(NullableValue, Viewed("""doses.Sum(d => [|d.Priority|].Value);"""));
}
