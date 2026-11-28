using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.ValuesAndSources;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS043TextIsJoinedByConcat : DiagnosticAnalyzer
{
    public const string Id = "ALIS043";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string Sources = "Alis.Reactive.ValuesAndSources.ITypedSource`1";
    private const string MadeIntoTextByItsOwnMethod = "ToString";

    private static readonly DiagnosticDescriptor MadeIntoText = new(
        Id,
        "A text is joined with a source by pipeline.Concat",
        "'{0}' is a source, read in the browser when its step runs, and .NET makes a text of it here, the name of its type; pass '{0}' itself where a source is taken, and join it into a text with pipeline.Concat",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MadeIntoText);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var sources = start.Compilation.GetTypeByMetadataName(Sources);
        if (sources is null) return;

        start.RegisterOperationAction(join => Joined(join, sources), OperationKind.Binary);
        start.RegisterOperationAction(join => JoinedInPlace(join, sources), OperationKind.CompoundAssignment);
        start.RegisterOperationAction(hole => Filled(hole, sources), OperationKind.Interpolation);
        start.RegisterOperationAction(call => Called(call, sources), OperationKind.Invocation);
    }

    private static void Joined(OperationAnalysisContext join, INamedTypeSymbol sources)
    {
        var binary = (IBinaryOperation)join.Operation;
        if (!MakesText(binary.OperatorMethod, binary.Type)) return;

        Report(join, binary.LeftOperand, sources);
        Report(join, binary.RightOperand, sources);
    }

    private static void JoinedInPlace(OperationAnalysisContext join, INamedTypeSymbol sources)
    {
        var compound = (ICompoundAssignmentOperation)join.Operation;
        if (!MakesText(compound.OperatorMethod, compound.Type)) return;

        Report(join, compound.Value, sources);
    }

    private static void Filled(OperationAnalysisContext hole, INamedTypeSymbol sources)
        => Report(hole, ((IInterpolationOperation)hole.Operation).Expression, sources);

    private static void Called(OperationAnalysisContext call, INamedTypeSymbol sources)
    {
        var invocation = (IInvocationOperation)call.Operation;
        var method = invocation.TargetMethod;
        if (method.Name == MadeIntoTextByItsOwnMethod)
        {
            Report(call, invocation.Instance, sources);
            return;
        }

        var isAStaticMethodOfText = method.IsStatic && method.ContainingType.SpecialType == SpecialType.System_String;
        if (!isAStaticMethodOfText) return;

        foreach (var value in invocation.Arguments.SelectMany(argument => Parts(argument.Value)))
            Report(call, value, sources);
    }

    private static bool MakesText(IMethodSymbol? appsOwnOperator, ITypeSymbol? type)
        => appsOwnOperator is null && type?.SpecialType == SpecialType.System_String;

    private static IEnumerable<IOperation> Parts(IOperation value)
        => Held(value) switch
        {
            IArrayCreationOperation { Initializer: { } items } => items.ElementValues,
            ICollectionExpressionOperation items => items.Elements,
            _ => [value],
        };

    private static void Report(OperationAnalysisContext context, IOperation? operand, INamedTypeSymbol sources)
    {
        var held = Held(operand);
        if (held?.Type is not { } type || !IsASource(type, sources)) return;

        context.ReportDiagnostic(Diagnostic.Create(MadeIntoText, held.Syntax.GetLocation(), held.Syntax.ToString()));
    }

    private static bool IsASource(ITypeSymbol type, INamedTypeSymbol sources)
        => Is(type, sources) || type.AllInterfaces.Any(contract => Is(contract, sources));

    private static bool Is(ITypeSymbol type, INamedTypeSymbol sources)
        => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, sources);

    private static IOperation? Held(IOperation? value)
    {
        var held = value;
        while (held is IConversionOperation converted)
            held = converted.Operand;
        return held;
    }
}
