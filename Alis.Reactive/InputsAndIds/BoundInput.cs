using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.InputsAndIds;

internal sealed partial class BoundInput
{
    private const string MessageOf = "_message";

    private readonly IReadOnlyList<PathStep> _path;
    private readonly Shape _shape;
    private readonly LitExpression _blank;
    private readonly Type _member;
    private readonly PlanContent _content;

    private BoundInput(string id, string name, IReadOnlyList<PathStep> path, Shape shape, LitExpression blank, Type member, PlanContent content, bool joinsTheBody)
    {
        Id = id;
        Name = name;
        _path = path;
        _shape = shape;
        _blank = blank;
        _member = member;
        _content = content;
        JoinsTheBody = joinsTheBody;
    }

    internal string Id { get; }

    internal string Name { get; }

    internal string MessageId => Id + MessageOf;

    internal bool BindsAList => _shape is ArrayShape;

    internal bool JoinsTheBody { get; }

    internal static BoundInput Of(MemberLambda lambda, MvcNames names, PlanContent content, Func<JsonElement> positionStart)
    {
        var json = content.Json;
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
            if (WrittenMember.LeftOutByItsClass(access, app)) return InNoBody(lambda, names, content, positionStart());
            var member = WrittenMember.Of(access, holder, lambda.Lambda);
            sinceTheItem.Add(WrittenMember.Of(access, live.GetTypeInfo(holder.Type), lambda.Lambda));
            path.Add(new MemberStep(NameOf(member, step.Text)));
        }

        var bindsAListItemThatIsAList = path[path.Count - 1] is IndexStep && app.GetTypeInfo(lambda.Lambda.Body.Type).IsList(out _);
        if (bindsAListItemThatIsAList) throw AListItemThatIsAList(names.Name);
        var blank = lambda.EndsAtAPosition ? positionStart() : StartOf(lambda, sinceTheItem);
        var shape = InputShapes.Of(live.GetTypeInfo(lambda.Lambda.Body.Type), blank, names.Name, json);
        if (shape is NumberShape && blank.ValueKind == JsonValueKind.String)
        {
            using var number = JsonDocument.Parse(blank.GetString()!);
            blank = number.RootElement.Clone();
        }

        return new BoundInput(lambda.Scope + "__" + names.IdOfTheName, names.Name, path, shape, new LitExpression(blank), lambda.Lambda.Body.Type, content, true);
    }

    private static BoundInput InNoBody(MemberLambda lambda, MvcNames names, PlanContent content, JsonElement blank)
    {
        var member = lambda.Lambda.Body.Type;
        var shape = InputShapes.Of(content.Json.LiveForm.GetTypeInfo(member), blank, names.Name, content.Json);
        return new(lambda.Scope + "__" + names.IdOfTheName, names.Name, [], shape, new LitExpression(blank), member, content, false);
    }

    internal InputBinding Binding(ComponentInput input, string invalidClass, bool hidden)
        => new(
            Name,
            _path,
            new AsExpression(ReadOf(input), new NullableShape(ConvertedShape(input))),
            _blank,
            Sent(),
            input.Committed,
            invalidClass,
            hidden ? "" : MessageId);

    private Shape ConvertedShape(ComponentInput input)
        => InputShapes.HoldsAnEnum(_shape) ? EnumInputRows.Of(_shape, _member, _content.Json.LiveForm, input, Name) : InputShapes.From(_shape, input.Text);

    private RecordExpression Sent()
    {
        var sendsFiles = _shape is FileShape or ArrayShape { Item: FileShape };
        if (sendsFiles) return new ContractRecord<FilesInputSent>().Set(sent => sent.Kind, "files").Written;
        var held = HeldForm.Live(_content.Json, _content.RendersUnder);
        var form = WireRecords.Of(SentForms.SourceForm(_member, held, _content, Name));
        var whenNothing = SentForms.Absence(_member);
        var whenBlank = WhenBlankOf(whenNothing);
        if (!InputShapes.HoldsAnEnum(_shape))
            return new ContractRecord<FieldInputSent>().Set(sent => sent.Kind, "field").Set(sent => sent.Form, form).Set(sent => sent.WhenNothing, whenNothing).Set(sent => sent.WhenBlank, whenBlank).Written;
        return new ContractRecord<EnumInputSent>().Set(sent => sent.Kind, "enum").Set(sent => sent.Form, form)
            .Set(sent => sent.Json, WireRecords.Of(SentForms.JsonFormOf(_member, held, _content, Name)))
            .Set(sent => sent.WhenNothing, whenNothing).Set(sent => sent.WhenBlank, whenBlank).Written;
    }

    private WhenBlank WhenBlankOf(WhenNothing whenNothing)
    {
        var unwritable = Unwritable(_shape, _blank.Value);
        var leftOutWhileBlank = unwritable || (_blank.Value.ValueKind == JsonValueKind.Null && whenNothing == WhenNothing.Leave);
        if (!leftOutWhileBlank) return WhenBlank.Send;
        var underAListIndex = _path.Any(step => step is IndexStep);
        if (underAListIndex) return WhenBlank.LeaveUnlessAlone;
        return unwritable ? WhenBlank.Leave : WhenBlank.Send;
    }

    private static bool Unwritable(Shape shape, JsonElement blank) => blank.ValueKind != JsonValueKind.Null && shape switch
    {
        DateShape or InstantShape => true,
        EnumShape enumeration => !enumeration.Values.Any(row => EnumRows.WrittenAlike(row.Live, blank)),
        NullableShape nullable => Unwritable(nullable.Inner, blank),
        ArrayShape array => blank.ValueKind == JsonValueKind.Array && blank.EnumerateArray().Any(item => Unwritable(array.Item, item)),
        _ => false,
    };

    private Schema.Expression ReadOf(ComponentInput input)
    {
        var readsTheFirstObject = input.ListsObjects && !BindsAList;
        return readsTheFirstObject ? new GetExpression([new IndexStep(0)], PathMembers.Optional, input.Read) : input.Read;
    }

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
