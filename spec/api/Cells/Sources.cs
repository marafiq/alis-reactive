using Alis.Reactive.Conditions;
using Alis.Reactive.Lists;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;

namespace Cells;

public static class Sources
{
    public static TypedSource<string?> ComponentMember(CellBoard board) => board.Wing;

    public static TypedSource<string?> PluginMember(CellPlugin plugin) => plugin.Wing;

    public static TypedSource<string?> MethodResult(CellBoard board) => board.Describe();

    public static TypedSource<string?> MethodAsyncResult(CellBoard board) => board.DescribeLater();

    public static TypedSource<string?> HubInvokeResult() => NoDoor.Source("N21.1, N21.8: no hub door");

    public static TypedSource<string?> PageUrlMember(PipelineVerbs<CellView> verbs) => verbs.PageUrl<CellPage>().Read(page => page.Wing);

    public static TypedSource<string?> EventPayloadField(CellChanged changed) => changed.Wing;

    public static TypedSource<string?> PushedMessageField() => NoDoor.Source("N5.1, N21.8: no pushed message door");

    public static TypedSource<string?> ResponseBodyField(Response<CellRequest, CellReply> reply) => reply.Body.Read(body => body.Wing);

    public static TypedSource<string?> ContainerMember(RequestContainer<CellView> container) => container.Read(view => view.Wing);

    public static TypedSource<string?> ArrayOperationResult(Response<CellRequest, CellReply> reply) => reply.Body.Read(body => body.Residents).Find(resident => resident.Ward == "North", resident => resident.Wing);

    public static string Literal() => "North";

    public static TypedSource<string?> Object() => NoDoor.Source("N7.1: no Object door");

    public static TypedSource<string?> List() => NoDoor.Source("N7.2: no List door");

    public static TypedSource<string?> JoinedText() => NoDoor.Source("N7.7: no Concat door");

    public static TypedSource<string> IfNull(CellBoard board) => board.Wing.IfNull("none");

    public static TypedSource<string?> Choose(CellBoard board) => board.Full.Choose(board.Wing, "none");
}
