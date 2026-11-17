using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal sealed partial class BoundInput
{
    private const string MessageOf = "_message";

    private readonly IReadOnlyList<PathStep> _path;
    private readonly Shape _shape;
    private readonly Schema.Expression _blank;

    private BoundInput(string id, string name, IReadOnlyList<PathStep> path, Shape shape, Schema.Expression blank, bool joinsTheBody)
    {
        Id = id;
        Name = name;
        _path = path;
        _shape = shape;
        _blank = blank;
        JoinsTheBody = joinsTheBody;
    }

    internal string Id { get; }

    internal string Name { get; }

    internal string MessageId => Id + MessageOf;

    internal bool BindsAList => _shape is ArrayShape;

    internal bool JoinsTheBody { get; }

    internal static BoundInput Of(MemberLambda lambda, MvcNames names, AppJson json, Func<JsonElement> positionStart)
    {
        var app = json.App;
        var live = json.LiveForm;
        var path = new List<PathStep>(PrefixSteps(names.Prefix, app));
        var sinceTheItem = new List<WrittenMember>();
        foreach (var step in lambda.Steps)
        {
            var holder = app.GetTypeInfo(HolderOf(step.Node).Type);
            if (step.IsPosition)
            {
                path.Add(PositionIn(lambda, step, holder));
                sinceTheItem.Clear();
                continue;
            }

            var access = (MemberExpression)step.Node;
            if (holder.Kind != JsonTypeInfoKind.Object) throw NoBodyPath(lambda, step.Text, WrittenMember.DeclaringClass(access), holder);
            if (WrittenMember.LeftOutByItsClass(access, app)) return InNoBody(lambda, names, live, positionStart());
            var member = WrittenMember.Of(access, holder, lambda.Lambda);
            sinceTheItem.Add(WrittenMember.Of(access, live.GetTypeInfo(holder.Type), lambda.Lambda));
            path.Add(new MemberStep(NameOf(member, step.Text)));
        }

        var bindsAListItemThatIsAList = path[path.Count - 1] is IndexStep && app.GetTypeInfo(lambda.Lambda.Body.Type).IsList(out _);
        if (bindsAListItemThatIsAList) throw AListItemThatIsAList(names.Name);
        var blank = lambda.EndsAtAPosition ? positionStart() : StartOf(lambda, sinceTheItem);
        var shape = InputShapes.Of(live.GetTypeInfo(lambda.Lambda.Body.Type), blank);
        if (shape is NumberShape && blank.ValueKind == JsonValueKind.String)
        {
            using var number = JsonDocument.Parse(blank.GetString()!);
            blank = number.RootElement.Clone();
        }

        return new BoundInput(lambda.Scope + "__" + names.IdOfTheName, names.Name, path, shape, new LitExpression(blank), true);
    }

    private static BoundInput InNoBody(MemberLambda lambda, MvcNames names, JsonSerializerOptions live, JsonElement blank)
        => new(lambda.Scope + "__" + names.IdOfTheName, names.Name, [], InputShapes.Of(live.GetTypeInfo(lambda.Lambda.Body.Type), blank), new LitExpression(blank), false);

    internal InputBinding Binding(ComponentInput input, string invalidClass, bool hidden)
        => new(
            Name,
            _path,
            new OpExpression(Operator.Coalesce, [new AsExpression(input.Read, new NullableShape(InputShapes.From(_shape, input.Text))), _blank]),
            input.Committed,
            invalidClass,
            hidden ? "" : MessageId);

    private static partial string NameOf(WrittenMember member, string clrName);

    private static partial string PrefixMemberName(string clrName, JsonSerializerOptions app);

    private static IEnumerable<PathStep> PrefixSteps(string prefix, JsonSerializerOptions app)
    {
        foreach (var (text, isPosition) in MemberLambda.Named(prefix))
        {
            var isListPosition = isPosition && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);
            if (isListPosition) yield return new IndexStep(int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture));
            else yield return new MemberStep(isPosition ? text : PrefixMemberName(text, app));
        }
    }

    private static PathStep PositionIn(MemberLambda lambda, MemberLambda.Step step, JsonTypeInfo holder) => holder.Kind switch
    {
        JsonTypeInfoKind.Enumerable => new IndexStep(int.Parse(step.Text, NumberStyles.None, CultureInfo.InvariantCulture)),
        JsonTypeInfoKind.Dictionary => EntryOf(step, holder),
        _ => throw NoBodyPath(lambda, "[" + step.Text + "]", holder.Type, holder),
    };

    private static MemberStep EntryOf(MemberLambda.Step step, JsonTypeInfo holder)
    {
        if (holder.IsDictionary(out var key, out _)) DictionaryKeys.RefuseAKeyNamedApart(key);
        return new MemberStep(step.Text);
    }

    private static System.Linq.Expressions.Expression HolderOf(System.Linq.Expressions.Expression step) => step switch
    {
        MemberExpression { Expression: { } holder } => holder,
        MethodCallExpression { Object: { } holder } => holder,
        BinaryExpression position => position.Left,
        _ => throw new InvalidOperationException($"'{step}' is no step MVC names."),
    };

    private static JsonElement StartOf(MemberLambda lambda, IReadOnlyList<WrittenMember> sinceTheItem)
    {
        if (DataStarts.StartOf(sinceTheItem) is StartAbsence start) return start.Value;
        throw new InvalidOperationException(
            $"Input '{lambda.Lambda}' binds a member the app's JSON options make only from the data the endpoint is sent: an abstract class, or a class they bind by the type the data names. A blank input has no start its plan can carry: bind a member of a class they make from an empty object.");
    }

    private static InvalidOperationException AListItemThatIsAList(string name)
        => new($"Input '{name}' binds a list that is an item of a list, and a list that holds no items sends no field in a query or form data, which ends the outer list at its position on both hosts, so the endpoint binds none of the items after it; bind each item of the inner list, as '{name}[0]', or send the outer list whole in a JSON body.");

    private static InvalidOperationException NoBodyPath(MemberLambda lambda, string member, Type? declaring, JsonTypeInfo holder)
    {
        var holderName = ClassNames.AsCSharpWritesIt(holder.Type);
        var declarer = declaring is null ? holderName : ClassNames.AsCSharpWritesIt(declaring);
        return new InvalidOperationException(
            $"Input '{lambda.Lambda}' binds {member}, which {declarer} declares, and the app's JSON options write {holderName} as {WrittenAs(holder)}, so the input has no body path: bind a member of a class they write as an object.");
    }

    private static string WrittenAs(JsonTypeInfo holder) => holder.Kind switch
    {
        JsonTypeInfoKind.Enumerable => "a list",
        JsonTypeInfoKind.Dictionary => "a dictionary",
        JsonTypeInfoKind.Object => "an object",
        _ => "one value",
    };
}
