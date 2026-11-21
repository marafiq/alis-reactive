using System;
using System.Collections.Concurrent;
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

    private static readonly DiagnosticDescriptor Unmarked = new(
        Id, "A flags registration names a marked enum",
        "Flags<{0}>() names {0} as a flags enum, and {0} has no [Flags], so FluentValidation and System.Text.Json read it as one value; mark {0} [Flags], or take the call out",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Overlapping = new(
        Id, "Flags values are disjoint or nested",
        "Flags<{0}>() names {0}, whose values {1} ({2}) and {3} ({4}) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OutsideRange = new(
        Id, "Flags values fit exact nonnegative browser integers",
        "Flags<{0}>() names {0}, whose value {1} is {2}, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TypeParameter = new(
        Id, "A flags registration names a concrete enum",
        "Flags<{0}>() names a type parameter, so the build cannot read its flags values; name a concrete [Flags] enum at startup",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnregisteredRule = new(
        Id, "A flags rule names its enum at startup",
        "IsInEnum reads {0}, which [Flags] marks and the startup does not name, as one value, where FluentValidation reads it as flags; name it with Flags<{0}>() at startup",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor UnregisteredSource = new(
        Id, "A request source names its flags enum at startup",
        "'{0}' sends {1}, which [Flags] marks and the startup does not name, so a combination its source holds stops the request; name it with Flags<{1}>() at startup",
        Category, DiagnosticSeverity.Error, isEnabledByDefault: true, customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Unmarked, Overlapping, OutsideRange, TypeParameter, UnregisteredRule, UnregisteredSource);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var builder = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Validation.ReactiveFluentValidationBuilder");
        var services = start.Compilation.GetTypeByMetadataName("Alis.Reactive.RenderingAndDelivery.ValidationServices");
        var rules = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Validation.RuleBuilderExtensions");
        var flags = start.Compilation.GetTypeByMetadataName("System.FlagsAttribute");
        var source = start.Compilation.GetTypeByMetadataName("Alis.Reactive.ValuesAndSources.ITypedSource`1");
        var input = start.Compilation.GetTypeByMetadataName("Alis.Reactive.TriggersAndPipelines.BoundInput`1");
        var expression = start.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
        if (builder is null || services is null || rules is null || flags is null || source is null || input is null || expression is null) return;

        var registrations = Methods(builder, "Flags");
        var startups = Methods(services, "AddReactiveFluentValidation");
        var checks = Methods(rules, "IsInEnum");
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

        var named = new ConcurrentDictionary<ITypeSymbol, bool>(SymbolEqualityComparer.Default);
        var started = new ConcurrentBag<bool>();
        var uses = new ConcurrentBag<(INamedTypeSymbol Enum, Location At, string Member)>();
        start.RegisterOperationAction(context =>
        {
            var call = (IInvocationOperation)context.Operation;
            var method = (call.TargetMethod.ReducedFrom ?? call.TargetMethod).OriginalDefinition;
            if (call.DescendantsAndSelf().Any(part => part.Kind == OperationKind.Invalid || part.Type?.TypeKind == TypeKind.Error)) return;
            if (startups.Contains(method, SymbolEqualityComparer.Default)) started.Add(true);
            if (registrations.Contains(method, SymbolEqualityComparer.Default))
            {
                var type = call.TargetMethod.TypeArguments[0];
                if (type.TypeKind == TypeKind.Error) return;
                if (type is ITypeParameterSymbol)
                {
                    context.ReportDiagnostic(Diagnostic.Create(TypeParameter, TypeArgument(call), type.Name));
                    return;
                }
                if (type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } concrete) return;
                named.TryAdd(concrete, true);
                CheckRegistration(context, call, concrete, flags);
                return;
            }

            if (checks.Contains(method, SymbolEqualityComparer.Default))
            {
                if (EnumOf(call.TargetMethod.TypeArguments.Last()) is { } type && Marked(type, flags))
                    uses.Add((type, MethodName(call), ""));
                return;
            }

            if (!requestDoors.Contains(method)) return;
            var member = call.Arguments.FirstOrDefault(argument => argument.Parameter is { } parameter && Is(parameter.Type, expression));
            if (member?.Parameter?.Type is not INamedTypeSymbol memberExpression || memberExpression.TypeArguments[0] is not INamedTypeSymbol lambda) return;
            if (lambda.DelegateInvokeMethod is not { } invoke || EnumOf(invoke.ReturnType) is not { } sent || !Marked(sent, flags)) return;
            uses.Add((sent, member.Value.Syntax.GetLocation(), member.Value.Syntax.ToString()));
        }, OperationKind.Invocation);

        start.RegisterCompilationEndAction(end =>
        {
            if (started.IsEmpty) return;
            foreach (var use in uses)
            {
                end.CancellationToken.ThrowIfCancellationRequested();
                if (named.ContainsKey(use.Enum)) continue;
                var name = use.Enum.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                end.ReportDiagnostic(use.Member.Length == 0
                    ? Diagnostic.Create(UnregisteredRule, use.At, name)
                    : Diagnostic.Create(UnregisteredSource, use.At, use.Member, name));
            }
        });
    }

    private static ImmutableArray<IMethodSymbol> Methods(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IMethodSymbol>().ToImmutableArray();

    private static bool Is(ITypeSymbol type, INamedTypeSymbol definition)
        => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition);

    private static INamedTypeSymbol? EnumOf(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable) type = nullable.TypeArguments[0];
        return type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumeration ? enumeration : null;
    }

    private static bool Marked(INamedTypeSymbol type, INamedTypeSymbol flags)
        => type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, flags));

    private static void CheckRegistration(OperationAnalysisContext context, IInvocationOperation call, INamedTypeSymbol type, INamedTypeSymbol flags)
    {
        var at = TypeArgument(call);
        var name = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        if (!Marked(type, flags))
        {
            context.ReportDiagnostic(Diagnostic.Create(Unmarked, at, name));
            return;
        }

        var fields = type.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsImplicitlyDeclared).ToArray();
        if (fields.Any(field => !field.HasConstantValue)) return;
        var values = new List<(string Name, ulong Value)>();
        foreach (var field in fields)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var number = Convert.ToDecimal(field.ConstantValue, CultureInfo.InvariantCulture);
            if (number < 0 || number > 9007199254740991m)
            {
                context.ReportDiagnostic(Diagnostic.Create(OutsideRange, at, name, field.Name, number.ToString(CultureInfo.InvariantCulture)));
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
                context.ReportDiagnostic(Diagnostic.Create(Overlapping, at, name, left.Name, left.Value.ToString(CultureInfo.InvariantCulture), right.Name, right.Value.ToString(CultureInfo.InvariantCulture)));
                return;
            }
        }
    }

    private static Location TypeArgument(IInvocationOperation call)
        => CallName(call) is GenericNameSyntax generic ? generic.TypeArgumentList.Arguments[0].GetLocation() : call.Syntax.GetLocation();

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
