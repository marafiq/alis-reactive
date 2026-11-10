using Microsoft.CodeAnalysis;

sealed record SurfaceParameter(string Name, string Type, string Kind, bool CallerMemberName);

sealed record SurfaceMember(
    string Assembly,
    string Receiver,
    string ReceiverName,
    string DeclaringType,
    string Member,
    string MemberKind,
    string Access,
    bool Static,
    IReadOnlyList<SurfaceParameter> Parameters,
    string Result,
    string Id);

static class Surface
{
    static readonly HashSet<string> SourceTypes = ["Alis.Reactive.Values.ITypedSource`1", "Alis.Reactive.Values.TypedSource`1", "Alis.Reactive.Values.Arg`1"];

    static readonly Dictionary<string, string> StandsIn = new()
    {
        ["Alis.Reactive.Pipelines.PipelineVerbs<TModel>"] = "Alis.Reactive.Pipelines.Pipeline<TModel>",
        ["Alis.Reactive.Hosting.PlanVerbs<TModel>"] = "Alis.Reactive.Hosting.ReactivePlan<TModel>",
    };

    public static IReadOnlyList<SurfaceMember> Read(IEnumerable<IAssemblySymbol> assemblies)
    {
        var members = new List<SurfaceMember>();
        foreach (var assembly in assemblies)
            foreach (var type in Types(assembly.GlobalNamespace).Where(IsFramework))
                foreach (var member in type.GetMembers().Where(IsExposed))
                    if (Describe(assembly, type, member) is { } described)
                        members.Add(described);
        return members;
    }

    public static string Receiver(ITypeSymbol type)
    {
        var display = Definition(type).ToDisplayString();
        return StandsIn.TryGetValue(display, out var stood) ? stood : display;
    }

    static string Family(string display) => display.Split('<')[0];

    public static string ShortName(string receiver)
    {
        var withoutArguments = receiver.Split('<')[0];
        return withoutArguments[(withoutArguments.LastIndexOf('.') + 1)..];
    }

    public static bool IsFrameworkType(ITypeSymbol type) =>
        Definition(type) is INamedTypeSymbol named
        && named.ContainingNamespace.ToDisplayString().StartsWith("Alis.Reactive", StringComparison.Ordinal);

    public static ITypeSymbol Definition(ITypeSymbol type) => type is INamedTypeSymbol named ? named.OriginalDefinition : type;

    static SurfaceMember? Describe(IAssemblySymbol assembly, INamedTypeSymbol type, ISymbol member)
    {
        var access = member.DeclaredAccessibility == Accessibility.Public ? "public" : "protected";
        var declaring = type.OriginalDefinition.ToDisplayString();
        switch (member)
        {
            case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
            {
                var extended = method.IsExtensionMethod ? method.Parameters[0].Type : type.IsExtension ? type.ExtensionParameter!.Type : type;
                var receiver = Receiver(extended);
                var parameters = method.IsExtensionMethod ? method.Parameters.Skip(1) : method.Parameters;
                return new SurfaceMember(assembly.Name, receiver, ShortName(receiver), declaring, method.Name, "method", access,
                    method.IsStatic && !method.IsExtensionMethod && !type.IsExtension,
                    parameters.Select(Parameter).ToList(), Result(method.ReturnType, extended), method.GetDocumentationCommentId() ?? method.Name);
            }
            case IPropertySymbol property:
            {
                var extended = type.IsExtension ? type.ExtensionParameter!.Type : type;
                var receiver = Receiver(extended);
                return new SurfaceMember(assembly.Name, receiver, ShortName(receiver), declaring, property.Name, "property", access,
                    property.IsStatic && !type.IsExtension, property.Parameters.Select(Parameter).ToList(), Result(property.Type, extended),
                    property.GetDocumentationCommentId() ?? property.Name);
            }
            case IFieldSymbol field:
            {
                var receiver = Receiver(type);
                return new SurfaceMember(assembly.Name, receiver, ShortName(receiver), declaring, field.Name, "field", access,
                    field.IsStatic, [], Result(field.Type, type), field.GetDocumentationCommentId() ?? field.Name);
            }
            case IEventSymbol @event:
            {
                var receiver = Receiver(type);
                return new SurfaceMember(assembly.Name, receiver, ShortName(receiver), declaring, @event.Name, "event", access,
                    @event.IsStatic, [], Result(@event.Type, type), @event.GetDocumentationCommentId() ?? @event.Name);
            }
            default:
                return null;
        }
    }

    static SurfaceParameter Parameter(IParameterSymbol parameter) =>
        new(parameter.Name, parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), Kind(parameter.Type),
            parameter.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.CallerMemberNameAttribute"));

    public static string Kind(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array) return Kind(array.ElementType) + "[]";
        if (type is ITypeParameterSymbol) return "source";
        if (type.SpecialType == SpecialType.System_String) return "string";
        if (type.SpecialType == SpecialType.System_Object) return "object";
        if (type is INamedTypeSymbol named)
        {
            var definition = Metadata(named);
            if (definition == "System.Type") return "type";
            if (definition == "System.Linq.Expressions.Expression`1") return "path";
            if (SourceTypes.Contains(definition)) return "source";
            if (named.TypeKind == TypeKind.Delegate) return "callback" + named.DelegateInvokeMethod!.Parameters.Length;
            if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return Kind(named.TypeArguments[0]) + "?";
            if (named.SpecialType != SpecialType.None) return named.ToDisplayString();
            return named.OriginalDefinition.Name;
        }
        return type.ToDisplayString();
    }

    static string Result(ITypeSymbol type, ITypeSymbol receiver)
    {
        if (type.SpecialType == SpecialType.System_Void) return "void";
        if (Family(Receiver(type)) == Family(Receiver(receiver))) return "chain";
        if (IsSource(type)) return "source";
        return Kind(type);
    }

    static bool IsSource(ITypeSymbol type) =>
        type is INamedTypeSymbol named
        && named.AllInterfaces.Append(named).Any(candidate => Metadata(candidate) == "Alis.Reactive.Values.ITypedSource`1");

    public static string Metadata(INamedTypeSymbol type) =>
        type.ContainingType is { } outer ? Metadata(outer) + "+" + type.MetadataName : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;

    static bool IsExposed(ISymbol member) =>
        member.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal
        && !member.IsOverride
        && !member.IsImplicitlyDeclared
        && member is not IMethodSymbol { MethodKind: not MethodKind.Ordinary };

    static bool IsFramework(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
            if (current.DeclaredAccessibility != Accessibility.Public && !current.IsExtension) return false;
        return type.ContainingNamespace.ToDisplayString().StartsWith("Alis.Reactive", StringComparison.Ordinal);
    }

    static IEnumerable<INamedTypeSymbol> Types(INamespaceSymbol space)
    {
        foreach (var type in space.GetTypeMembers())
            foreach (var nested in Nested(type))
                yield return nested;
        foreach (var child in space.GetNamespaceMembers())
            foreach (var type in Types(child))
                yield return type;
    }

    static IEnumerable<INamedTypeSymbol> Nested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var inner in type.GetTypeMembers())
            foreach (var nested in Nested(inner))
                yield return nested;
    }
}
