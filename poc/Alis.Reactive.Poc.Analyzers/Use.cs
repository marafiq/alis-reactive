using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Poc.Analyzers;

internal sealed class Use
{
    private Use(Door door, string? javaScriptName, Location location, string declared, int? position, INamedTypeSymbol? reached, Typed? value, Typed? answer, bool? optional, bool? nullable)
    {
        Optional = optional;
        Nullable = nullable;
        Door = door;
        JavaScriptName = javaScriptName;
        Location = location;
        Declared = declared;
        Position = position;
        Reached = reached;
        Value = value;
        Answer = answer;
    }

    public Door Door { get; }

    public string? JavaScriptName { get; }

    public Location Location { get; }

    public string Declared { get; }

    public int? Position { get; }

    public INamedTypeSymbol? Reached { get; }

    public Typed? Value { get; }

    public Typed? Answer { get; }

    public bool? Optional { get; }

    public bool? Nullable { get; }

    public static Use Of(IInvocationOperation invocation, Door door, ISymbol member, INamedTypeSymbol holder)
    {
        var method = invocation.TargetMethod;
        var reached = door.Reach == Reach.None ? null : method.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
        var value = method.TypeArguments.Length == 1 && door.Reach == Reach.None ? Typed.Of(method, 0) : null;
        var answer = door.Reach == Reach.Callback && method.TypeArguments.Length == 2 ? Typed.Of(method, 1) : null;
        var position = door.Shape == DoorShape.Position ? Constant(invocation, "position") as int? : null;
        var name = CallerName(member);
        return new Use(door, NameOf(invocation, name), member.Locations[0], holder.Name + "." + name, position, reached, value, answer, Marked(invocation, "optional"), Marked(invocation, "nullable"));
    }

    public static string CallerName(ISymbol member) => member switch
    {
        IMethodSymbol { ExplicitInterfaceImplementations: { Length: > 0 } implemented } => implemented[0].Name,
        IPropertySymbol { ExplicitInterfaceImplementations: { Length: > 0 } implemented } => implemented[0].Name,
        _ => member.Name,
    };

    private static bool? Marked(IInvocationOperation invocation, string parameter)
    {
        var argument = invocation.Arguments.FirstOrDefault(each => each.Parameter?.Name == parameter);
        if (argument is null) return null;
        if (argument.ArgumentKind == ArgumentKind.DefaultValue) return false;
        return argument.Value.ConstantValue is { HasValue: true, Value: bool marked } ? marked : null;
    }

    private static object? Constant(IInvocationOperation invocation, string parameter)
    {
        var argument = invocation.Arguments.FirstOrDefault(each => each.Parameter?.Name == parameter);
        return argument?.Value.ConstantValue is { HasValue: true } constant ? constant.Value : null;
    }

    private static string? NameOf(IInvocationOperation invocation, string memberName)
    {
        var argument = invocation.Arguments.FirstOrDefault(each => each.Parameter?.Name == "name");
        if (argument is null || argument.ArgumentKind == ArgumentKind.DefaultValue) return CamelCase.Of(memberName);
        if (!argument.Value.ConstantValue.HasValue) return null;
        return argument.Value.ConstantValue.Value as string ?? CamelCase.Of(memberName);
    }
}

internal sealed class Typed
{
    private Typed(string shown, bool nullable)
    {
        Shown = shown;
        Nullable = nullable;
    }

    public string Shown { get; }

    public bool Nullable { get; }

    public static Typed? Of(IMethodSymbol method, int position)
    {
        var type = method.TypeArguments[position];
        if (type is ITypeParameterSymbol) return null;
        var nullable = type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            || method.TypeArgumentNullableAnnotations[position] == NullableAnnotation.Annotated;
        var bare = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } wrapped
            ? wrapped.TypeArguments[0]
            : type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        return new Typed(bare.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), nullable);
    }
}
