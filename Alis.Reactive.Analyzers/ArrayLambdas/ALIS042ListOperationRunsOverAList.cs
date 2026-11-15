using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.ArrayLambdas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS042ListOperationRunsOverAList : DiagnosticAnalyzer
{
    public const string Id = "ALIS042";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string Operations = "Alis.Reactive.ArrayLambdas.DataLists";
    private const string Sources = "Alis.Reactive.ValuesAndSources.ITypedSource`1";

    private static readonly DiagnosticDescriptor OverAText = new(
        Id,
        "A list's operation runs over a list",
        "'{0}' is a source of text, and a text is no list in the browser; run {1} over a list, such as a List<T> or an array, or test the text with Contains, StartsWith, EndsWith, MinLength or Matches",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(OverAText);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var operations = start.Compilation.GetTypeByMetadataName(Operations);
        var sources = start.Compilation.GetTypeByMetadataName(Sources);
        if (operations is null || sources is null) return;

        start.RegisterOperationAction(call => Called(call, operations, sources), OperationKind.Invocation);
    }

    private static void Called(OperationAnalysisContext call, INamedTypeSymbol operations, INamedTypeSymbol sources)
    {
        var invocation = (IInvocationOperation)call.Operation;
        var method = invocation.TargetMethod;
        if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, operations)) return;

        var list = Held(invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0)?.Value);
        if (list?.Type is not { } type || !IsASourceOfText(type, sources)) return;

        call.ReportDiagnostic(Diagnostic.Create(OverAText, list.Syntax.GetLocation(), list.Syntax.ToString(), method.Name));
    }

    private static bool IsASourceOfText(ITypeSymbol type, INamedTypeSymbol sources)
        => IsOfText(type, sources) || type.AllInterfaces.Any(contract => IsOfText(contract, sources));

    private static bool IsOfText(ITypeSymbol type, INamedTypeSymbol sources)
        => type is INamedTypeSymbol { TypeArguments.Length: 1 } source
            && source.TypeArguments[0].SpecialType == SpecialType.System_String
            && SymbolEqualityComparer.Default.Equals(source.OriginalDefinition, sources);

    private static IOperation? Held(IOperation? value)
    {
        var held = value;
        while (held is IConversionOperation or IParenthesizedOperation)
            held = held is IConversionOperation converted ? converted.Operand : ((IParenthesizedOperation)held).Operand;
        return held;
    }
}
