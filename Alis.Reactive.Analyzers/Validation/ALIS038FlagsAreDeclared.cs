using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Validation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS038FlagsAreDeclared : DiagnosticAnalyzer
{
    public const string Id = "ALIS038";
    private const string Category = "Alis.Reactive.Analyzers";
    private const string DisjointOrNested = "Flags values are disjoint or nested";
    private const string ExactAndNonnegative = "Flags values fit exact nonnegative browser integers";

    private static readonly DiagnosticDescriptor OverlappingRule = new(
        Id, DisjointOrNested,
        "IsInEnum reads {0}, whose values {1} ({2}) and {3} ({4}) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OverlappingSource = new(
        Id, DisjointOrNested,
        "'{0}' sends {1}, whose values {2} ({3}) and {4} ({5}) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OutsideRangeRule = new(
        Id, ExactAndNonnegative,
        "IsInEnum reads {0}, whose value {1} is {2}, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OutsideRangeSource = new(
        Id, ExactAndNonnegative,
        "'{0}' sends {1}, whose value {2} is {3}, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(OverlappingRule, OverlappingSource, OutsideRangeRule, OutsideRangeSource);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var rules = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Validation.RuleBuilderExtensions");
        var flags = start.Compilation.GetTypeByMetadataName("System.FlagsAttribute");
        var source = start.Compilation.GetTypeByMetadataName("Alis.Reactive.ValuesAndSources.ITypedSource`1");
        var input = start.Compilation.GetTypeByMetadataName("Alis.Reactive.TriggersAndPipelines.BoundInput`1");
        var expression = start.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
        if (rules is null || flags is null || source is null || input is null || expression is null) return;

        var checks = rules.GetMembers("IsInEnum").OfType<IMethodSymbol>().ToImmutableArray();
        var requestDoors = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        foreach (var name in new[] { "QueryParts`1", "QueriedParts`1", "BodyParts`1", "BodiedParts`1", "WholeBodyParts`1", "JsonBody`1", "UrlEncodedBody`1", "MultipartBody`1", "CondensedRequests" })
        {
            var type = start.Compilation.GetTypeByMetadataName("Alis.Reactive.TriggersAndPipelines." + name);
            if (type is null) continue;
            foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.Parameters.Any(parameter => Is(parameter.Type, expression))
                    && method.Parameters.Any(parameter => Is(parameter.Type, source) || Is(parameter.Type, input)))
                {
                    requestDoors.Add(method);
                }
            }
        }

        start.RegisterOperationAction(context =>
        {
            var call = (IInvocationOperation)context.Operation;
            var method = (call.TargetMethod.ReducedFrom ?? call.TargetMethod).OriginalDefinition;
            if (checks.Contains(method, SymbolEqualityComparer.Default))
            {
                if (EnumOf(call.TargetMethod.TypeArguments.Last()) is { } checkedEnum && Marked(checkedEnum, flags))
                    CheckValues(context, checkedEnum, MethodName(call), OutsideRangeRule, OverlappingRule);
                return;
            }

            if (!requestDoors.Contains(method)) return;
            var member = call.Arguments.FirstOrDefault(argument => argument.Parameter is { } parameter && Is(parameter.Type, expression));
            if (member?.Parameter?.Type is not INamedTypeSymbol memberExpression || memberExpression.TypeArguments[0] is not INamedTypeSymbol lambda) return;
            if (lambda.DelegateInvokeMethod is not { } invoke || EnumOf(invoke.ReturnType) is not { } sent || !Marked(sent, flags)) return;
            CheckValues(context, sent, member.Value.Syntax.GetLocation(), OutsideRangeSource, OverlappingSource, member.Value.Syntax.ToString());
        }, OperationKind.Invocation);
    }

    private static bool Is(ITypeSymbol type, INamedTypeSymbol definition)
        => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition);

    private static INamedTypeSymbol? EnumOf(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable) type = nullable.TypeArguments[0];
        return type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumeration ? enumeration : null;
    }

    private static bool Marked(INamedTypeSymbol type, INamedTypeSymbol flags)
        => type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, flags));

    private static void CheckValues(OperationAnalysisContext context, INamedTypeSymbol type, Location at, DiagnosticDescriptor outside, DiagnosticDescriptor overlapping, params object[] use)
    {
        var name = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var fields = type.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsImplicitlyDeclared).ToArray();
        if (fields.Any(field => !field.HasConstantValue)) return;
        var values = new List<(string Name, ulong Value)>();
        foreach (var field in fields)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var number = Convert.ToDecimal(field.ConstantValue, CultureInfo.InvariantCulture);
            if (number < 0 || number > 9007199254740991m)
            {
                context.ReportDiagnostic(Diagnostic.Create(outside, at, use.Concat(new object[] { name, field.Name, number.ToString(CultureInfo.InvariantCulture) }).ToArray()));
                return;
            }
            values.Add((field.Name, (ulong)number));
        }

        for (var first = 0; first < values.Count; first++)
        {
            for (var second = first + 1; second < values.Count; second++)
            {
                var left = values[first];
                var right = values[second];
                var shared = left.Value & right.Value;
                if (shared == 0 || shared == left.Value || shared == right.Value) continue;
                context.ReportDiagnostic(Diagnostic.Create(overlapping, at, use.Concat(new object[] { name, left.Name, left.Value.ToString(CultureInfo.InvariantCulture), right.Name, right.Value.ToString(CultureInfo.InvariantCulture) }).ToArray()));
                return;
            }
        }
    }

    private static Location MethodName(IInvocationOperation call)
        => CallName(call) is { } name ? name.Identifier.GetLocation() : call.Syntax.GetLocation();

    private static SimpleNameSyntax? CallName(IInvocationOperation call)
        => call.Syntax is InvocationExpressionSyntax invocation ? invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name,
            MemberBindingExpressionSyntax binding => binding.Name,
            SimpleNameSyntax name => name,
            _ => null,
        } : null;
}
