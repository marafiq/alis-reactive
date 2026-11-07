using System;
using System.Linq.Expressions;
using System.Text;
using Alis.Reactive.Types;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Alis.Reactive.Inputs;

internal sealed class InputNames
{
#if NET48
    internal InputNames(string prefix)
    {
        Prefix = prefix;
    }
#else
    private readonly IHtmlHelper _html;
    private readonly ModelExpressionProvider _expressions;

    internal InputNames(string prefix, IHtmlHelper html, ModelExpressionProvider expressions)
    {
        Prefix = prefix;
        _html = html;
        _expressions = expressions;
    }
#endif

    internal string Prefix { get; }

    internal InputName Of<TModel, TValue>(Expression<Func<TModel, TValue>> member)
    {
        var text = ExpressionText(member);
        var bound = MemberTheModelReaches(member);
        var declaring = bound is null || text.Length == 0 ? null : JsonPath.DeclaringClassName(bound);
        if (declaring is null) throw Unbound(member);

        var name = new TemplateInfo { HtmlFieldPrefix = Prefix }.GetFullHtmlFieldName(text);
        return new InputName(name, Scope(declaring, member) + "__" + IdFromName(name));
    }

    internal void RefuseAPrefixTakenLate<TModel, TValue>(string prefix, Expression<Func<TModel, TValue>> member)
    {
        if (prefix == Prefix) return;
        throw new InvalidOperationException(
            $"The view's field prefix changed from {Described(Prefix)} to {Described(prefix)} after its plan started: MVC names input '{member}' under the new prefix and the plan under the one it started with. Give the view its prefix before its plan starts.");
    }

#if NET48
    private static string ExpressionText(LambdaExpression member) => ExpressionHelper.GetExpressionText(member);

    private static string IdFromName(string name) => HtmlHelper.GenerateIdFromName(name);
#else
    private string ExpressionText<TModel, TValue>(Expression<Func<TModel, TValue>> member) => _expressions.GetExpressionText(member);

    private string IdFromName(string name) => _html.GenerateIdFromName(name);
#endif

    private static InvalidOperationException Unbound(LambdaExpression member)
    {
        var parameterUse = new ParameterUse(member.Parameters[0]);
        parameterUse.Visit(member.Body);
        if (!parameterUse.Found)
        {
            return new InvalidOperationException(
                "An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop.");
        }

        return new InvalidOperationException(
            $"'{member}' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone.");
    }

    private static MemberExpression? MemberTheModelReaches(LambdaExpression member)
    {
        MemberExpression? bound = null;
        var node = member.Body;
        while (node != member.Parameters[0])
        {
            switch (node)
            {
                case MemberExpression { Expression: { } holder } access:
                    bound ??= access;
                    node = holder;
                    break;
                case MethodCallExpression { Object: { } holder, Arguments.Count: 1 }:
                    node = holder;
                    break;
                case BinaryExpression { NodeType: ExpressionType.ArrayIndex } position:
                    node = position.Left;
                    break;
                default:
                    return null;
            }
        }

        return bound;
    }

    private static string Scope(string declaringClass, LambdaExpression member)
    {
        var scope = new StringBuilder(declaringClass.Length);
        foreach (var character in declaringClass)
        {
            var carriesTypeArguments = character == '`';
            if (carriesTypeArguments)
            {
                throw new InvalidOperationException(
                    $"'{member}' binds a member that a generic class declares, whose full name carries its type arguments' assemblies and versions, so the id it scopes would change with them: bind a member a non-generic class declares.");
            }

            scope.Append(IsIdCharacter(character) ? character : '_');
        }

        return scope.ToString();
    }

    private static bool IsIdCharacter(char character)
        => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';

    private static string Described(string prefix) => prefix.Length == 0 ? "none" : $"'{prefix}'";
}

internal sealed class ParameterUse(ParameterExpression parameter) : ExpressionVisitor
{
    internal bool Found { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        Found |= node == parameter;
        return node;
    }
}

internal sealed class InputName
{
    internal InputName(string name, string id)
    {
        Name = name;
        Id = id;
    }

    internal string Name { get; }

    internal string Id { get; }
}
