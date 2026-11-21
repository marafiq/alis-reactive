using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.ArrayLambdas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS027NullablesValueReadsNothing : DiagnosticSuppressor
{
    public const string Id = "ALIS027";

    private const string MayBeNull = "CS8629";

    private static readonly SuppressionDescriptor ReadsNothing = new(
        Id,
        MayBeNull,
        "A data lambda reads its item in the browser, where a nullable's value, or a cast to it, reads nothing when the nullable holds null");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(ReadsNothing);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var doors = DataLambdaDoors.Of(context.Compilation);
        if (doors is null) return;

        foreach (var warning in context.ReportedDiagnostics)
        {
            var nullable = DataLambdaDoors.OperationAt(warning, context);
            if (nullable is not null && ReadsItsValue(nullable) && doors.ReadsNothingPastNull(nullable)) context.ReportSuppression(Suppression.Create(ReadsNothing, warning));
        }
    }

    private static bool ReadsItsValue(IOperation nullable)
    {
        var readThroughValue = nullable.Parent is IPropertyReferenceOperation;
        return readThroughValue || IsACastToItsValue(nullable);
    }

    private static bool IsACastToItsValue(IOperation nullable)
        => nullable is IConversionOperation { Operand.Type: INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } held } cast
            && SymbolEqualityComparer.Default.Equals(held.TypeArguments[0], cast.Type);
}
