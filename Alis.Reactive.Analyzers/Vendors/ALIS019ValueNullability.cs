using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS019ValueNullability : DiagnosticAnalyzer
{
    public const string Id = "ALIS019";

    private static readonly DiagnosticDescriptor ReadNull = new(
        Id,
        "A declaration admits every value its vendor reads",
        "Declaration member '{0}' reads null or undefined from vendor member '{1}'; declare a nullable C# type",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor WriteNull = new(
        Id,
        "A declaration writes only values its vendor takes",
        "Declaration member '{0}' writes null to vendor member '{1}', which does not take null; declare a non-nullable C# type",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor LiveNull = new(
        Id,
        "A live object declaration admits its vendor nullability",
        "Declaration member '{0}' reads a live object that vendor member '{1}' can hold as null or undefined; declare nullable: true",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AnswerNull = new(
        Id,
        "A callback answers only values its vendor takes",
        "Declaration member '{0}' answers vendor callback '{1}' with null, which its result does not take; declare a non-nullable result type",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DocumentedReadNull = new(
        Id,
        "A declaration admits its vendor documented null",
        "Declaration member '{0}' reads null from vendor member '{1}', documented @default null in module '{2}' at '{3}'; declare a nullable C# type",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DocumentedLiveNull = new(
        Id,
        "A live object declaration admits its vendor documented null",
        "Declaration member '{0}' reads a live object documented @default null in module '{2}' at '{3}' on vendor member '{1}'; declare nullable: true",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ReadNull, WriteNull, LiveNull, AnswerNull, DocumentedReadNull, DocumentedLiveNull);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start => RootMemberDoor.Register(start, Read));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.Vendor is not { } member) return;
        var readsNull = door.IsMethod ? member.ResultReadsNull : member.ReadsNull;
        if (door.Door is "LiveObject" or "LiveObjects" or "ReturnedObject" or "ReturnedObjects")
        {
            var nullable = door.Flag("nullable");
            if (readsNull && nullable.Value == false)
                ReportRead(context, door, nullable.Location, LiveNull, DocumentedLiveNull);
            return;
        }

        if (door.ValueType is not { } type) return;
        var admitsNull = type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ? true
            : type.IsValueType ? false
            : type.NullableAnnotation == NullableAnnotation.Annotated ? true
            : type.NullableAnnotation == NullableAnnotation.NotAnnotated ? false : (bool?)null;
        if (door.Door == "Callback")
        {
            if (admitsNull == true && member.CallbackTakesNull == false)
                context.ReportDiagnostic(Diagnostic.Create(AnswerNull, door.ValueLocation, door.Member, door.Name));
            return;
        }

        if (readsNull && admitsNull == false && door.Door is "Property" or "ReadOnly" or "Method" or "MethodAsync")
            ReportRead(context, door, door.ValueLocation, ReadNull, DocumentedReadNull);
        if (admitsNull == true && !member.WritesNull && door.Door is "Property" or "WriteOnly")
            context.ReportDiagnostic(Diagnostic.Create(WriteNull, door.ValueLocation, door.Member, door.Name));
    }

    private static void ReportRead(OperationAnalysisContext context, RootMemberDoor door, Location location, DiagnosticDescriptor rule, DiagnosticDescriptor documented)
    {
        var diagnostic = !door.IsMethod && door.Vendor?.DocumentedNull is { } tag
            ? Diagnostic.Create(documented, location, door.Member, door.Name, tag.Module, tag.Location)
            : Diagnostic.Create(rule, location, door.Member, door.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
