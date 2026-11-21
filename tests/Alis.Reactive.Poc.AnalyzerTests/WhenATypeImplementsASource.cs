using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenATypeImplementsASource
{
    private const string Message = "implements ITypedSource<T>, which only the framework's sources implement: read a value through a member's Read, a list's operation or a value read from data";

    private static string Written(string declaration) => $$"""
        using System.Collections.Generic;
        using Alis.Reactive.Values;

        namespace Ward;

        public sealed class Round
        {
            public string Carer { get; set; } = "";
        }

        {{declaration}}
        """;

    [Test]
    public Task a_class_that_implements_a_source_is_reported()
        => Fixture.Reports<SourcesAnalyzer>(SourcesAnalyzer.Id, "'HandWrittenRounds' " + Message, Written("public sealed class [|HandWrittenRounds|] : ITypedSource<IEnumerable<Round>>;"), null);

    [Test]
    public Task a_struct_that_implements_a_source_is_reported()
        => Fixture.Reports<SourcesAnalyzer>(SourcesAnalyzer.Id, "'CarerName' " + Message, Written("public readonly record struct [|CarerName|](string Text) : ITypedSource<string>;"), null);

    [Test]
    public Task an_interface_that_extends_a_source_is_reported()
        => Fixture.Reports<SourcesAnalyzer>(SourcesAnalyzer.Id, "'IRoundSource' " + Message, Written("public interface [|IRoundSource|] : ITypedSource<Round>;"), null);

    [Test]
    public Task a_class_that_implements_a_source_through_an_interface_the_framework_does_not_own_is_reported()
        => Fixture.ReportsWith<SourcesAnalyzer>(SourcesAnalyzer.Id, "'RoundSource' " + Message, Written("""
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Alis.Reactive.Contract", "ALIS028")]
            public interface IRoundSource : ITypedSource<Round>;

            public sealed class [|RoundSource|] : IRoundSource;
            """), []);

    [Test]
    public Task a_type_that_reads_the_frameworks_sources_without_implementing_one_is_reported_by_none()
        => Fixture.ReportsNothing<SourcesAnalyzer>(Written("""
            public sealed class RoundBoard
            {
                public RoundBoard(ITypedSource<IEnumerable<Round>> rounds, TypedSource<string> lead)
                {
                    Rounds = rounds;
                    Lead = lead;
                }

                public ITypedSource<IEnumerable<Round>> Rounds { get; }

                public TypedSource<string> Lead { get; }
            }
            """), null);
}
