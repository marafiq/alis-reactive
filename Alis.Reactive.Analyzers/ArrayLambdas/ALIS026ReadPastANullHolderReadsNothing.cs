using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.ArrayLambdas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS026ReadPastANullHolderReadsNothing : DiagnosticSuppressor
{
    public const string Id = "ALIS026";

    private const string MayBeNull = "CS8602";

    private static readonly SuppressionDescriptor ReadsNothing = new(
        Id,
        MayBeNull,
        "A data lambda reads its item in the browser, where a read past a holder that holds null reads nothing");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(ReadsNothing);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var doors = DataLambdaDoors.Of(context.Compilation);
        if (doors is null) return;

        foreach (var warning in context.ReportedDiagnostics)
        {
            var dereferenced = DataLambdaDoors.OperationAt(warning, context);
            if (dereferenced is not null && doors.ReadsNothingPastNull(dereferenced)) context.ReportSuppression(Suppression.Create(ReadsNothing, warning));
        }
    }
}
