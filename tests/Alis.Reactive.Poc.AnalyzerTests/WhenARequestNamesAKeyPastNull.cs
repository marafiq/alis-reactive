using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenARequestNamesAKeyPastNull
{
    private const string Dereference = "CS8602";

    private static string Viewed(string statement) => $$"""
        using System;
        using System.Linq.Expressions;
        using Alis.Reactive.Pipelines;

        namespace Ward;

        public sealed class Handover
        {
            public int Round { get; set; }

            public string Lead { get; set; } = "";
        }

        public sealed class Register
        {
            public string Ward { get; set; } = "";

            public Handover? Handover { get; set; }
        }

        public static class View
        {
            public static void Write(QueryParts<Register> request, Handover? cover)
            {
                {{statement}}
            }
        }
        """;

    [TestCase("""request.Query(r => [|r.Handover|].Round, 3);""")]
    [TestCase("""request.Set(r => [|r.Handover|].Lead, "Ana");""")]
    [TestCase("""CondensedRequests.Set(request, r => [|r.Handover|].Lead, "Ana");""")]
    [TestCase("""request.Query(member: r => [|r.Handover|].Round, value: 3);""")]
    public Task a_key_past_a_holder_that_holds_null_is_suppressed(string statement)
        => Fixture.Suppresses<RequestKeysSuppressor>(Dereference, RequestKeysSuppressor.KeyPastANullHolderId, Viewed(statement));

    [Test]
    public Task a_path_past_a_holder_is_kept()
        => Fixture.Keeps<RequestKeysSuppressor>(Dereference, Viewed("""request.Path(r => [|r.Handover|].Lead, "Ana");"""));

    [Test]
    public Task a_value_the_view_holds_beside_a_key_is_kept()
        => Fixture.Keeps<RequestKeysSuppressor>(Dereference, Viewed("""request.Query(r => r.Ward, [|cover|].Lead);"""));

    [Test]
    public Task a_value_the_view_holds_inside_a_key_is_kept()
        => Fixture.Keeps<RequestKeysSuppressor>(Dereference, Viewed("request.Query(r => [|cover|].Round, 3);"));

    [Test]
    public Task a_lambda_no_key_door_takes_is_kept()
        => Fixture.Keeps<RequestKeysSuppressor>(Dereference, Viewed("""Expression<Func<Register, int>> kept = r => [|r.Handover|].Round;"""));
}
