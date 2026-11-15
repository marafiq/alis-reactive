using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal sealed class MemberLambda
{
    private MemberLambda(LambdaExpression lambda, IReadOnlyList<Step> steps, MemberExpression member, Type declaring)
    {
        Lambda = lambda;
        Steps = steps;
        Member = member;
        Declaring = declaring;
    }

    internal LambdaExpression Lambda { get; }

    internal IReadOnlyList<Step> Steps { get; }

    internal MemberExpression Member { get; }

    internal Type Declaring { get; }

    internal bool EndsAtAPosition => Steps[Steps.Count - 1].IsPosition;

    internal string Scope
    {
        get
        {
            var named = ClassNames.InNamespace(Declaring).ToCharArray();
            for (var at = 0; at < named.Length; at++)
                if (!IsIdCharacter(named[at])) named[at] = '_';
            return new string(named);
        }
    }

    internal static MemberLambda Of(LambdaExpression lambda, string mvcText, string name, Type model)
    {
        var steps = StepsOf(lambda, mvcText) ?? throw Unbound(lambda);
        var member = NearestMember(lambda) ?? throw NamesNoMember(lambda, model);
        var declaring = WrittenMember.DeclaringClass(member) ?? throw NamesNoMember(lambda, model);
        if (ClassNames.IsInterface(declaring)) throw DeclaredByAnInterface(name, declaring);
        foreach (var step in steps)
        {
            if (step.Node is not MemberExpression access) continue;
            if (WrittenMember.DeclaringClass(access) is { } holder && Nullable.GetUnderlyingType(holder) is not null) throw DeclaredByANullable(lambda, access, holder, model);
        }

        return new MemberLambda(lambda, steps, member, declaring);
    }

    private static IReadOnlyList<Step>? StepsOf(LambdaExpression lambda, string mvcText)
    {
        var taken = new List<System.Linq.Expressions.Expression>();
        var node = lambda.Body;
        while (node != lambda.Parameters[0])
        {
            var holder = node switch
            {
                MemberExpression { Expression: { } member } => member,
                MethodCallExpression { Object: { } instance, Arguments.Count: 1 } => instance,
                BinaryExpression { NodeType: ExpressionType.ArrayIndex } position => position.Left,
                _ => null,
            };
            if (holder is null) return null;
            taken.Insert(0, node);
            node = holder;
        }

        var named = Named(mvcText);
        if (named.Count != taken.Count) return null;
        var steps = new List<Step>(taken.Count);
        for (var at = 0; at < taken.Count; at++) steps.Add(new Step(taken[at], named[at]));
        return steps;
    }

    internal static IReadOnlyList<(string Text, bool IsPosition)> Named(string mvcText)
    {
        var named = new List<(string, bool)>();
        var at = 0;
        while (at < mvcText.Length)
        {
            var position = mvcText[at] == '[';
            if (position || named.Count > 0) at++;
            var start = at;
            while (at < mvcText.Length && !EndsStep(mvcText[at], position)) at++;
            named.Add((mvcText.Substring(start, at - start), position));
            if (position) at++;
        }

        return named;
    }

    private static bool EndsStep(char character, bool position)
        => position ? character == ']' : character is '.' or '[';

    private static MemberExpression? NearestMember(LambdaExpression lambda)
    {
        var node = lambda.Body;
        while (node is not MemberExpression)
        {
            node = node switch
            {
                MethodCallExpression { Object: { } instance } => instance,
                BinaryExpression { NodeType: ExpressionType.ArrayIndex } position => position.Left,
                _ => null,
            };
            if (node is null) return null;
        }

        return (MemberExpression)node;
    }

    private static bool IsIdCharacter(char character)
        => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';

    private static InvalidOperationException NamesNoMember(LambdaExpression lambda, Type model)
        => new($"Input '{lambda}' names no member of {ClassNames.AsCSharpWritesIt(model)}: bind a member of the view's model, or render the member with Html.EditorFor from the view that holds it.");

    private static InvalidOperationException Unbound(LambdaExpression lambda)
    {
        var parameterUse = new ParameterUse(lambda.Parameters[0]);
        parameterUse.Visit(lambda.Body);
        if (!parameterUse.Found)
        {
            return new InvalidOperationException(
                "An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop.");
        }

        return new InvalidOperationException(
            $"'{lambda}' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    private static InvalidOperationException DeclaredByAnInterface(string name, Type declaring)
        => new($"Input '{name}' binds a member that {ClassNames.AsCSharpWritesIt(declaring)} declares. An input's id is scoped by the class that declares its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a base class, that declares it.");

    private static InvalidOperationException DeclaredByANullable(LambdaExpression lambda, MemberExpression member, Type declaring, Type model)
    {
        var nullable = ClassNames.AsCSharpWritesIt(declaring);
        var named = member.ToString().Substring(member.Expression!.ToString().Length + 1);
        return new InvalidOperationException(
            $"Input '{lambda}' binds {named}, which {nullable} declares, not a member of {ClassNames.AsCSharpWritesIt(model)}: bind '{lambda.Parameters[0]} => {member.Expression}', which takes a {nullable}.");
    }

    internal sealed class Step
    {
        internal Step(System.Linq.Expressions.Expression node, (string Text, bool IsPosition) named)
        {
            Node = node;
            Text = named.Text;
            IsPosition = named.IsPosition;
        }

        internal System.Linq.Expressions.Expression Node { get; }

        internal string Text { get; }

        internal bool IsPosition { get; }
    }

    private sealed class ParameterUse(ParameterExpression parameter) : ExpressionVisitor
    {
        internal bool Found { get; private set; }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            Found |= node == parameter;
            return node;
        }
    }
}
