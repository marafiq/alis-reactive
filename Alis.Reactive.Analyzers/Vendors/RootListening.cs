using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Vendors;

internal sealed class RootListening
{
    private RootListening(string declaration, string module, string type, string name, Location location, bool returned, VendorContract.Member? member)
    {
        Declaration = declaration;
        Module = module;
        Type = type;
        Name = name;
        Location = location;
        Returned = returned;
        Member = member;
    }

    internal string Declaration { get; }
    internal string Module { get; }
    internal string Type { get; }
    internal string Name { get; }
    internal Location Location { get; }
    internal bool Returned { get; }
    internal VendorContract.Member? Member { get; }

    internal static void Register(CompilationStartAnalysisContext start, Action<SymbolAnalysisContext, RootListening> read, Action<OperationAnalysisContext, RootMemberDoor>? callback = null)
    {
        var component = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Vendors.Component");
        var vendor = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Vendors.ComponentVendor");
        var plugin = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Plugins.Plugin");
        var evented = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.EventedObject");
        var listening = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.EventListening");
        var raised = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.Event`1");
        var mirrors = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.MirrorsAttribute");
        if (component is null || vendor is null || plugin is null || evented is null || listening is null || raised is null || mirrors is null) return;
        var table = VendorContract.Read(start.Options, start.CancellationToken);
        if (table is null) return;
        var reader = new Reader(start.Compilation, component, vendor, plugin, evented, listening);
        start.RegisterSymbolAction(context =>
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!Types(type).SelectMany(current => current.GetMembers()).OfType<IPropertySymbol>().Any(property => property.Type is INamedTypeSymbol named && Types(named).Any(current => SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, raised)))) return;
            var attribute = type.GetAttributes().FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, mirrors));
            if (attribute is null || attribute.ConstructorArguments.Length != 2 || attribute.ConstructorArguments[0].Value is not string module || attribute.ConstructorArguments[1].Value is not string name) return;
            foreach (var definition in reader.Read(type, context.CancellationToken))
            {
                foreach (var member in definition.Members)
                {
                    if (table.TryMember(module, name, member.Name, context.CancellationToken, out var contract))
                        read(context, new RootListening(type.Name, module, name, member.Name, member.Location, definition.Returned, contract));
                }
            }
        }, SymbolKind.NamedType);
        if (callback is not null)
        {
            RootMemberDoor.Register(start, (context, door) =>
            {
                if (door.IsEvent && reader.Read(door.Owner, context.CancellationToken).Any(definition => definition.Callback)) callback(context, door);
            });
        }
    }

    private static IEnumerable<INamedTypeSymbol> Types(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType) yield return current;
    }

    private sealed class Definition(bool callback, bool returned, params (string Name, Location Location)[] members)
    {
        internal bool Callback { get; } = callback;
        internal bool Returned { get; } = returned;
        internal IReadOnlyList<(string Name, Location Location)> Members { get; } = members;
    }

    private sealed class Reader(Compilation compilation, INamedTypeSymbol component, INamedTypeSymbol vendor, INamedTypeSymbol plugin, INamedTypeSymbol evented, INamedTypeSymbol listening)
    {
        internal IEnumerable<Definition> Read(INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            var bases = Types(type).ToArray();
            if (bases.Any(current => SymbolEqualityComparer.Default.Equals(current, component)))
            {
                var property = Override(bases, component, "Vendor");
                if (PropertyValue(property, cancellationToken) is { } value && ReadValue(value, true, cancellationToken) is { } definition) yield return definition;
                yield break;
            }

            if (bases.Any(current => SymbolEqualityComparer.Default.Equals(current, plugin)))
            {
                foreach (var current in bases.TakeWhile(current => !SymbolEqualityComparer.Default.Equals(current, plugin)))
                {
                    foreach (var reference in current.DeclaringSyntaxReferences)
                    {
                        var syntax = reference.GetSyntax(cancellationToken);
                        if (syntax is not TypeDeclarationSyntax declaration || !compilation.ContainsSyntaxTree(syntax.SyntaxTree)) continue;
                        var initializers = declaration.Members.OfType<ConstructorDeclarationSyntax>().Select(constructor => constructor.Initializer).OfType<SyntaxNode>()
                            .Concat(declaration.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>() ?? []);
                        foreach (var initializer in initializers)
                        {
                            var operation = compilation.GetSemanticModel(initializer.SyntaxTree).GetOperation(initializer, cancellationToken);
                            if (operation is not IInvocationOperation call || !SymbolEqualityComparer.Default.Equals(call.TargetMethod.ContainingType, plugin) || Invalid(call)) continue;
                            var argument = call.Arguments.FirstOrDefault(argument => SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, listening));
                            if (argument is not null)
                            {
                                if (ReadValue(argument.Value, false, cancellationToken) is { } definition) yield return definition;
                            }
                            else
                            {
                                var location = initializer is ConstructorInitializerSyntax constructor ? constructor.ThisOrBaseKeyword.GetLocation()
                                    : ((PrimaryConstructorBaseTypeSyntax)initializer).Type.GetLocation();
                                yield return EventTarget(location);
                            }
                        }
                    }
                }

                yield break;
            }

            var own = Override(bases, evented, "Listening");
            if (PropertyValue(own, cancellationToken) is { } events && ReadValue(events, false, cancellationToken) is { } heard) yield return heard;
        }

        private static IPropertySymbol? Override(IEnumerable<INamedTypeSymbol> types, INamedTypeSymbol owner, string name)
        {
            var original = owner.GetMembers(name).OfType<IPropertySymbol>().Single();
            foreach (var property in types.SelectMany(type => type.GetMembers(name)).OfType<IPropertySymbol>())
            {
                for (var current = property.OverriddenProperty; current is not null; current = current.OverriddenProperty)
                    if (SymbolEqualityComparer.Default.Equals(current, original)) return property;
            }

            return null;
        }

        private IOperation? PropertyValue(IPropertySymbol? property, CancellationToken cancellationToken)
        {
            property = property?.PartialImplementationPart ?? property;
            if (property?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not PropertyDeclarationSyntax syntax || !compilation.ContainsSyntaxTree(syntax.SyntaxTree)) return null;
            var getter = syntax.AccessorList?.Accessors.FirstOrDefault(accessor => accessor.Keyword.ValueText == "get");
            var expression = syntax.Initializer?.Value ?? syntax.ExpressionBody?.Expression ?? getter?.ExpressionBody?.Expression
                ?? (getter?.Body?.Statements is { Count: 1 } statements && statements[0] is ReturnStatementSyntax returned ? returned.Expression : null);
            return expression is null ? null : compilation.GetSemanticModel(expression.SyntaxTree).GetOperation(expression, cancellationToken);
        }

        private Definition? ReadValue(IOperation value, bool isVendor, CancellationToken cancellationToken)
        {
            while (value is IConversionOperation conversion)
            {
                if (!conversion.Conversion.Exists) return null;
                value = conversion.Operand;
            }

            if (Invalid(value)) return null;
            if (value is IPropertyReferenceOperation property)
            {
                if (isVendor && SymbolEqualityComparer.Default.Equals(property.Property.ContainingType, vendor) && property.Property.Name == "Native") return EventTarget(value.Syntax is MemberAccessExpressionSyntax access ? access.Name.GetLocation() : value.Syntax.GetLocation());
                if (!isVendor && SymbolEqualityComparer.Default.Equals(property.Property.ContainingType, listening))
                {
                    return property.Property.Name switch
                    {
                        "EventTarget" => EventTarget(value.Syntax is MemberAccessExpressionSyntax access ? access.Name.GetLocation() : value.Syntax.GetLocation()),
                        "CallbackProperties" => new Definition(true, false),
                        _ => null,
                    };
                }

                if (!isVendor || !property.Property.IsStatic || property.Property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not PropertyDeclarationSyntax { Initializer.Value: { } initializer } || !compilation.ContainsSyntaxTree(initializer.SyntaxTree)) return null;
                value = compilation.GetSemanticModel(initializer.SyntaxTree).GetOperation(initializer, cancellationToken)!;
            }
            else if (isVendor && value is IFieldReferenceOperation { Field.IsStatic: true } field)
            {
                if (field.Field.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not VariableDeclaratorSyntax { Initializer.Value: { } expression } || !compilation.ContainsSyntaxTree(expression.SyntaxTree)) return null;
                value = compilation.GetSemanticModel(expression.SyntaxTree).GetOperation(expression, cancellationToken)!;
            }

            if (value is not IInvocationOperation call || Invalid(call) || !SymbolEqualityComparer.Default.Equals(call.TargetMethod.ContainingType, isVendor ? vendor : listening)) return null;
            var method = call.TargetMethod.Name;
            if (isVendor && method == "DefineCallbackProperties") return new Definition(true, false);
            if (isVendor ? method is not ("Define" or "DefineReturnedUnsubscriber") : method is not ("Methods" or "ReturnedUnsubscriber")) return null;
            var subscribe = Name(call, "subscribe");
            if (subscribe is null) return null;
            if (method is "DefineReturnedUnsubscriber" or "ReturnedUnsubscriber") return new Definition(false, true, subscribe.Value);
            var unsubscribe = Name(call, "unsubscribe");
            return unsubscribe is null ? null : new Definition(false, false, subscribe.Value, unsubscribe.Value);
        }

        private static (string Name, Location Location)? Name(IInvocationOperation call, string name)
        {
            var argument = call.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == name);
            return argument?.Value.ConstantValue is { HasValue: true, Value: string text } ? (text, argument.Value.Syntax.GetLocation()) : null;
        }

        private static bool Invalid(IOperation operation)
            => operation.DescendantsAndSelf().Any(part => part is IInvalidOperation || part.Type?.TypeKind == TypeKind.Error);

        private static Definition EventTarget(Location location) => new(false, false, ("addEventListener", location), ("removeEventListener", location));
    }
}
