using System;
using System.Linq;
using Alis.Reactive.Analyzers.TypeKnowledge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Vendors;

internal sealed class RootMemberDoor
{
    private RootMemberDoor(string member, string name, string module, string type, string door, Location nameLocation, Location doorLocation, VendorContract.Member? vendor)
    {
        Member = member;
        Name = name;
        Module = module;
        Type = type;
        Door = door;
        NameLocation = nameLocation;
        DoorLocation = doorLocation;
        Vendor = vendor;
    }

    internal string Member { get; }
    internal string Name { get; }
    internal string Module { get; }
    internal string Type { get; }
    internal string Door { get; }
    internal Location NameLocation { get; }
    internal Location DoorLocation { get; }
    internal VendorContract.Member? Vendor { get; }
    internal bool IsMethod => Door is "Method" or "MethodAsync";

    internal static void Register(CompilationStartAnalysisContext start, Action<OperationAnalysisContext, RootMemberDoor> read)
    {
        var declarations = start.Compilation.GetTypeByMetadataName(DeclarationDoor.Declarations);
        var mirrors = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.MirrorsAttribute");
        if (declarations is null || mirrors is null) return;
        var table = VendorContract.Read(start.Options, start.CancellationToken);
        if (table is null) return;
        start.RegisterOperationAction(context =>
        {
            if (Read(context, declarations, mirrors, table) is { } door) read(context, door);
        }, OperationKind.Invocation);
    }

    private static RootMemberDoor? Read(OperationAnalysisContext context, INamedTypeSymbol declarations, INamedTypeSymbol mirrors, VendorContract table)
    {
        var call = (IInvocationOperation)context.Operation;
        if (!DeclarationDoor.Is(call.TargetMethod, declarations) || call.TargetMethod.Name is not ("Property" or "ReadOnly" or "WriteOnly" or "Method" or "MethodAsync")) return null;
        if (call.Instance is not IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }) return null;
        if (context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.PropertyGet } author || author.AssociatedSymbol is IPropertySymbol { IsIndexer: true }) return null;
        IOperation body = call;
        while (body.Parent is IConversionOperation conversion) body = conversion;
        if (body.Parent is not (IReturnOperation or IExpressionStatementOperation) || body.Parent.Parent is not IBlockOperation { Operations.Length: 1, Parent: null or IMethodBodyOperation }) return null;
        if (call.DescendantsAndSelf().Any(operation => operation is IInvalidOperation || operation.Type?.TypeKind == TypeKind.Error)) return null;
        var attribute = author.ContainingType.GetAttributes().FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, mirrors));
        if (attribute is null || attribute.ConstructorArguments.Length != 2 || attribute.ConstructorArguments[0].Value is not string module || attribute.ConstructorArguments[1].Value is not string type) return null;
        var nameArgument = call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "name");
        var declaredBy = call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "declaredBy");
        if (nameArgument?.Value.ConstantValue is not { HasValue: true } constant || declaredBy is not { IsImplicit: true, Value.ConstantValue: { HasValue: true, Value: string declaredName } }) return null;
        var name = constant.Value as string ?? CamelCase(declaredName);
        if (!table.TryMember(module, type, name, context.CancellationToken, out var vendor)) return null;
        var member = author.AssociatedSymbol ?? author;
        ISymbol? definition = member switch
        {
            IMethodSymbol method => method.PartialDefinitionPart,
            IPropertySymbol property => property.PartialDefinitionPart,
            _ => null,
        };
        var written = (definition ?? member).DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
        var identifier = written switch
        {
            MethodDeclarationSyntax method => method.Identifier,
            PropertyDeclarationSyntax property => property.Identifier,
            _ => default,
        };
        if (identifier == default || call.Syntax is not InvocationExpressionSyntax invocation) return null;
        var calledName = invocation.Expression switch
        {
            SimpleNameSyntax simple => simple,
            MemberAccessExpressionSyntax access => access.Name,
            _ => null,
        };
        if (calledName is null) return null;
        var nameLocation = definition is not null || constant.Value is null ? identifier.GetLocation() : nameArgument.Value.Syntax.GetLocation();
        var doorLocation = definition is not null ? identifier.GetLocation() : calledName.Identifier.GetLocation();
        return new RootMemberDoor(DeclarationDoor.Named(member), name, module, type, call.TargetMethod.Name, nameLocation, doorLocation, vendor);
    }

    private static string CamelCase(string name)
    {
        if (name.Length == 0 || !char.IsUpper(name[0])) return name;
        var letters = name.ToCharArray();
        for (var at = 0; at < letters.Length; at++)
        {
            if (at > 0 && !char.IsUpper(letters[at])) break;
            if (at > 0 && at + 1 < letters.Length && !char.IsUpper(letters[at + 1])) break;
            letters[at] = char.ToLowerInvariant(letters[at]);
        }

        return new string(letters);
    }
}
