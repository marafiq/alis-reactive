using Alis.Reactive.Conditions;
using Alis.Reactive.Elements;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;

namespace Cells;

public static partial class Positions
{
    public static void PropertySet(CellBoard target, TypedSource<string?> value) => target.Note.Set(value);

    public static void WriteOnlySet(CellBoard target, TypedSource<string?> value) => target.Caption.Set(value);

    public static void ElementText(Pipeline<CellView> pipeline, TypedSource<string?> value) => pipeline.Element("note").Text.Set(value);

    public static void ElementHtml(Pipeline<CellView> pipeline, TypedSource<string?> value) => pipeline.Element("note").Html.Set(value);

    public static void ElementClass(Pipeline<CellView> pipeline, TypedSource<string?> value) => pipeline.Element("note").Classes.Add(value);

    public static void ConditionLeft(Pipeline<CellView> pipeline, TypedSource<string?> value) => pipeline.When(value.Eq("North"));

    public static void ConditionRight(Pipeline<CellView> pipeline, CellBoard target, TypedSource<string?> value) => pipeline.When(target.Wing.Eq(value));

    public static void TextTestOperand(TypedSource<string?> value) => NoDoor.Position("M3.N1, N7.3: no text test, MinLength, Between or membership operator on a pipeline condition");

    public static void MatchesPattern(TypedSource<string?> value) => NoDoor.Position("M3.N1: no Matches operator on a pipeline condition");

    public static void RequestMember(PipelineVerbs<CellView> verbs, TypedSource<string?> value) => verbs.Post<CellRequest>("/cells", request => request.Json(body => body.Set(r => r.Note, value)));

    public static void Header(PipelineVerbs<CellView> verbs, TypedSource<string?> value) => verbs.Post<CellRequest>("/cells", request => request.Header("X-Note", value));

    public static void DispatchPayloadMember(TypedSource<string?> value) => NoDoor.Position("M1.N4: no Dispatch door");

    public static void MethodArgument(CellBoard target, TypedSource<string?> value) => target.Mark(value);

    public static void PluginArgument(CellPlugin plugin, TypedSource<string?> value) => plugin.Mark(value);

    public static void HubArgument(TypedSource<string?> value) => NoDoor.Position("N21.1, N21.8: no hub door");
}
