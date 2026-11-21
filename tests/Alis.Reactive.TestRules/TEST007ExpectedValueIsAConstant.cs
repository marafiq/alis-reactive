using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST007ExpectedValueIsAConstant : DiagnosticAnalyzer
{
    public const string Id = "TEST007";

    private const string Title = "An expected value and what finds an element are constants";

    private static readonly DiagnosticDescriptor Computed = new(
        Id,
        Title,
        "'{0}' is given to '{1}' and is no constant; write the literal the promise states",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor BuiltBefore = new(
        Id,
        Title,
        "'{0}' is given to '{1}' and is built before the call; write the options where they are passed, each value a literal",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Computed, BuiltBefore);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        start.RegisterOperationAction(ReadAMember, OperationKind.Invocation);

        var assert = start.Compilation.GetTypeByMetadataName(TestCode.NUnitsAssert);
        var constraint = start.Compilation.GetTypeByMetadataName(TestCode.NUnitsConstraint);
        if (assert is null || constraint is null) return;

        start.RegisterOperationAction(called => ReadAnAssertion(called, assert, constraint), OperationKind.Invocation);
    }

    private static void ReadAMember(OperationAnalysisContext context)
    {
        var called = (IInvocationOperation)context.Operation;
        var member = called.TargetMethod;
        if (!TestCode.IsOfPlaywright(member)) return;

        var name = Taken.NameOf(member);
        if (!Taken.Finding.Contains(name) && !Taken.Expecting.Contains(name)) return;

        foreach (var given in called.Arguments)
        {
            Report(context, given.Value, member.Name);
            var written = TestCode.Written(given.Value);
            if (IsBuiltBeforeTheCall(written, given.Parameter))
                context.ReportDiagnostic(Diagnostic.Create(BuiltBefore, written.Syntax.GetLocation(), written.Syntax.ToString(), member.Name));
            if (written is not IObjectCreationOperation { Initializer: { } options }) continue;

            foreach (var set in options.Initializers.OfType<ISimpleAssignmentOperation>())
            {
                if (set.Target is IPropertyReferenceOperation option) Report(context, set.Value, option.Property.Name);
            }
        }
    }

    private static bool IsBuiltBeforeTheCall(IOperation written, IParameterSymbol? taking)
    {
        var takesOptions = taking?.Type is INamedTypeSymbol { TypeKind: TypeKind.Class } options && TestCode.IsOfPlaywright(options);
        if (!takesOptions || written.ConstantValue.HasValue) return false;

        return written is not IObjectCreationOperation { Arguments.Length: 0 };
    }

    private static void ReadAnAssertion(OperationAnalysisContext context, INamedTypeSymbol assert, INamedTypeSymbol constraint)
    {
        var called = (IInvocationOperation)context.Operation;
        if (!TestCode.IsAssertThat(called, assert) || TestCode.ConstraintOf(called, constraint) is not { } expression) return;

        foreach (var given in expression.DescendantsAndSelf().OfType<IArgumentOperation>())
        {
            if (given.Parameter?.ContainingSymbol is not { } taker) continue;
            Report(context, given.Value, taker is IMethodSymbol { MethodKind: MethodKind.Constructor } ? taker.ContainingType.Name : taker.Name);
        }
    }

    private static void Report(OperationAnalysisContext context, IOperation given, string taker)
    {
        foreach (var computed in WhatIsNoConstant(given))
            context.ReportDiagnostic(Diagnostic.Create(Computed, computed.Syntax.GetLocation(), computed.Syntax.ToString(), taker));
    }

    private static IEnumerable<IOperation> WhatIsNoConstant(IOperation given)
    {
        var value = TestCode.Written(given);
        if (value.ConstantValue.HasValue) return [];

        return value switch
        {
            ICollectionExpressionOperation listed => listed.Elements.SelectMany(element => WhatIsNoConstant(element is ISpreadOperation spread ? spread.Operand : element)),
            IArrayCreationOperation { Initializer: { } listed } => listed.ElementValues.SelectMany(WhatIsNoConstant),
            _ => IsRead(value.Type) ? [value] : [],
        };
    }

    private static bool IsRead(ITypeSymbol? type) => type switch
    {
        null => false,
        IArrayTypeSymbol array => IsAValue(array.ElementType),
        _ => IsAValue(type) || type.AllInterfaces.Concat([type]).OfType<INamedTypeSymbol>().Any(IsAListOfValues),
    };

    private static bool IsAListOfValues(INamedTypeSymbol type)
        => type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T && IsAValue(type.TypeArguments[0]);

    private static bool IsAValue(ITypeSymbol type)
    {
        var held = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable ? nullable.TypeArguments[0] : type;
        return held.TypeKind == TypeKind.Enum || held.SpecialType is SpecialType.System_String or SpecialType.System_Boolean
            or SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16
            or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
            or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;
    }
}
