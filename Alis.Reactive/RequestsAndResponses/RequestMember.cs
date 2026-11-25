using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RequestMember
{
    private readonly IReadOnlyList<MemberLambda.Step> _steps;

    private RequestMember(LambdaExpression lambda, string name, IReadOnlyList<MemberLambda.Step> steps, IReadOnlyList<PathStep> path)
    {
        Lambda = lambda;
        Name = name;
        _steps = steps;
        Path = path;
    }

    internal LambdaExpression Lambda { get; }

    internal string Name { get; }

    internal bool TopLevel => _steps.Count == 1 && !_steps[0].IsPosition;

    internal IReadOnlyList<PathStep> Path { get; }

    internal static RequestMember Of<TRequest, TMember>(Expression<Func<TRequest, TMember>> member, JsonSerializerOptions app)
    {
        RefuseAKeyWrittenAsNoName(member.Body, app);
        var name = MvcText.Of(member);
        var steps = MemberLambda.StepsOf(member, name);
        if (steps is null || steps.Count == 0) throw NamesNoMember(member);
        return new RequestMember(member, name, steps, [.. steps.Select(step => StepOf(member, step, app))]);
    }

    internal IEnumerable<string> ItemsPastAPosition()
    {
        var written = "";
        for (var at = 0; at < _steps.Count; at++)
        {
            var step = _steps[at];
            var text = step.IsPosition ? "[" + step.Text + "]" : step.Text;
            written += step.IsPosition || at == 0 ? text : "." + text;
            if (Path[at] is IndexStep) yield return written;
        }
    }

    internal static bool Holds(string holder, string member)
        => member.Equals(holder, StringComparison.OrdinalIgnoreCase) || member.StartsWith(holder + ".", StringComparison.OrdinalIgnoreCase) ||
            member.StartsWith(holder + "[", StringComparison.OrdinalIgnoreCase);

    internal IReadOnlyList<JsonStep> BodySteps(JsonSerializerOptions body)
    {
        var steps = new List<JsonStep>(_steps.Count);
        for (var at = 0; at < _steps.Count; at++)
        {
            var step = _steps[at];
            if (step.IsPosition)
            {
                steps.Add(new JsonStep(step.Text, Path[at] is IndexStep));
                continue;
            }

            var access = (MemberExpression)step.Node;
            steps.Add(new JsonStep(MemberPath.BodyName(WrittenMember.Of(access, body.GetTypeInfo(access.Expression!.Type), Lambda), step.Text), false));
        }

        return steps;
    }

    private static PathStep StepOf(LambdaExpression member, MemberLambda.Step step, JsonSerializerOptions app)
    {
        if (!step.IsPosition) return new MemberStep(step.Text);
        var holder = app.GetTypeInfo(HolderOf(step.Node).Type);
        if (holder.IsList(out _)) return new IndexStep(int.Parse(step.Text, NumberStyles.None, CultureInfo.InvariantCulture));
        if (holder.IsDictionary(out _, out _)) return new MemberStep(step.Text);
        throw NamesNoMember(member);
    }

    private static void RefuseAKeyWrittenAsNoName(System.Linq.Expressions.Expression read, JsonSerializerOptions app)
    {
        var node = read;
        while (node is not null)
        {
            if (node is MethodCallExpression { Object: { } holder, Arguments.Count: 1 } && app.GetTypeInfo(holder.Type).IsDictionary(out var key, out _))
            {
                if (key.Kind != JsonTypeInfoKind.None)
                    throw new InvalidOperationException($"A dictionary keyed by {ClassNames.AsCSharpWritesIt(key.Type)} cannot be sent: System.Text.Json writes no property name for its keys; key it by text or a number.");
                DictionaryKeys.RefuseAKeyNamedApart(key);
            }

            node = node switch
            {
                MemberExpression access => access.Expression,
                MethodCallExpression call => call.Object,
                BinaryExpression { NodeType: ExpressionType.ArrayIndex } position => position.Left,
                _ => null,
            };
        }
    }

    private static System.Linq.Expressions.Expression HolderOf(System.Linq.Expressions.Expression position)
        => position is MethodCallExpression call ? call.Object! : ((BinaryExpression)position).Left;

    private static InvalidOperationException NamesNoMember(LambdaExpression member)
        => new($"'{member}' names no request member; name a member path, such as r => r.Address.Ward.");
}
