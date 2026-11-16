using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.Validation;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.TriggersAndPipelines;

internal sealed class RequestWriter<TModel> : IDefaultRoutes
{
    private readonly BlockWriter _writer;
    private readonly RequestPartsWriter _parts;
    private readonly RequestMethod _method;
    private readonly RefExpression _argument;
    private readonly RefExpression _reply;
    private readonly bool _enteredAfterAwait;
    private readonly BlockWriter.Place _place;
    private readonly RequestDefaults _defaults;
    private readonly bool _sentByADefault;
    private readonly Func<RequestWriter<TModel>, IReadOnlyList<Statement>> _defaultCheck;
    private Layer _defaultLayer = new([], []);
    private readonly List<(StatusRoute Route, bool ShowsNotice)> _defaultStatuses = [];
    private readonly List<(OtherwiseRoute Route, bool ShowsNotice)> _defaultOtherwise = [];
    private readonly List<StatusRoute> _statuses = [];
    private IReadOnlyList<Statement> _loading = [];
    private IReadOnlyList<Statement> _finally = [];
    private IReadOnlyList<Statement> _chained = [];
    private Route _success = new(SuccessBody.None, []);
    private OtherwiseRoute _otherwise = new(OtherwiseBody.None, []);
    private bool _hasLoading;
    private bool _hasFinally;
    private bool _hasOtherwise;
    private IReadOnlyList<Statement> _check = [];
    private bool _namesItsCheck;
    private (RefExpression Body, RefExpression Failures)? _checkNames;
    private int _stage;
    private bool _placesAtTheNotice;
    private bool _noDefaults;
    private bool _noRetry;
    private BusyChoice _busy = BusyChoice.Derived;
    private IReadOnlyList<string> _busyIds = [];

    private RequestWriter(BlockWriter writer, RequestPartsWriter parts, RequestMethod method, Func<RequestWriter<TModel>, IReadOnlyList<Statement>> defaultCheck)
    {
        _writer = writer;
        _parts = parts;
        _method = method;
        _argument = writer.NextBinding("argument");
        _reply = writer.RequestReply();
        _enteredAfterAwait = writer.PathAwaited;
        _defaults = writer.Content.Services.GetRequiredService<RequestDefaults>();
        _sentByADefault = RequestScopes.SentByADefault(writer);
        _defaultCheck = defaultCheck;
        _place = writer.Placed(Written(), Ended);
    }

    private enum BusyChoice
    {
        Derived,
        Targets,
        Page,
        None,
    }

    internal static RequestWriter<TModel> Open<TRequest>(BlockWriter writer, string url, RequestMethod method, Action<QueryParts<TRequest>> parts)
        => Open(writer, url, method, request => parts(new QueryParts<TRequest>(request, 0)), DefaultCheck<TRequest>);

    internal static RequestWriter<TModel> Open<TRequest>(BlockWriter writer, string url, RequestMethod method, Action<BodyParts<TRequest>> parts)
        => Open(writer, url, method, request => parts(new BodyParts<TRequest>(request, 0)), DefaultCheck<TRequest>);

    private static RequestWriter<TModel> Open(BlockWriter writer, string url, RequestMethod method, Action<RequestPartsWriter> parts, Func<RequestWriter<TModel>, IReadOnlyList<Statement>> defaultCheck)
    {
        if (writer.Content.Antiforgery.Choice == AntiforgeryChoice.NoRequestsCall) throw new InvalidOperationException(RequestHost.NoRequestsCall(method.Text + " " + url));
        var request = new RequestPartsWriter(writer, url);
        parts(request);
        request.Complete();
        return new RequestWriter<TModel>(writer, request, method, defaultCheck);
    }

    internal int Validate<TRequest, TValidator>(int stage, string ruleSet)
        where TValidator : ReactiveValidator<TRequest>
    {
        Continue(stage);
        NamesItsCheck();
        _check = RequestChecks.Named<TRequest, TValidator>(_writer, CheckParts, ruleSet);
        return Changed();
    }

    internal int NoValidation(int stage)
    {
        Continue(stage);
        NamesItsCheck();
        _check = [];
        return Changed();
    }

    internal int Success<TReply>(int stage, SuccessBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        _success = new Route(kind, Route(reply, body));
        return Changed();
    }

    internal int Status<TReply>(int stage, int status, ErrorBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        if (status is < 300 or > 599)
            throw new InvalidOperationException($"OnError status {status} is outside 300 to 599; name an error status in that range.");
        if (_statuses.Any(route => route.Status == status))
            throw new InvalidOperationException($"OnError already names status {status}; write one route for that status.");
        _statuses.Add(new StatusRoute(status, kind, Route(reply, body)));
        return Changed();
    }

    internal int Otherwise<TReply>(int stage, OtherwiseBody kind, TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Continue(stage);
        if (_hasOtherwise) throw new InvalidOperationException("OnError already has a catch-all route; write one catch-all for the request.");
        _hasOtherwise = true;
        _otherwise = new OtherwiseRoute(kind, Route(reply, body));
        return Changed();
    }

    internal int Loading(int stage, Action<Pipeline<TModel>> body)
    {
        Continue(stage);
        if (_hasLoading) throw new InvalidOperationException("WhileLoading is already written for this request; put its steps in one block.");
        _hasLoading = true;
        _loading = _writer.Branch(_enteredAfterAwait, () => body(new Pipeline<TModel>(_writer))).Block;
        return Changed();
    }

    internal int Finally(int stage, Action<Pipeline<TModel>> body)
    {
        Continue(stage);
        if (_hasFinally) throw new InvalidOperationException("Finally is already written for this request; put its steps in one block.");
        _hasFinally = true;
        _finally = _writer.Branch(true, () => body(new Pipeline<TModel>(_writer))).Block;
        return Changed();
    }

    internal void Chained<TReply>(int stage, TReply reply, Action<TReply, Pipeline<TModel>> body)
    {
        Continue(stage);
        _chained = _writer.Branch(true, () => body(reply, new Pipeline<TModel>(_writer))).Block;
        Changed();
    }

    internal int NoDefaults(int stage)
    {
        Continue(stage);
        if (_busy is BusyChoice.Targets or BusyChoice.Page) throw BusyWithNoDefaults();
        _noDefaults = true;
        return Changed();
    }

    internal int Busy(int stage, string[] elementIds)
    {
        if (elementIds is null) throw new ArgumentNullException(nameof(elementIds));
        Continue(stage);
        Choose(elementIds.Length == 0 ? BusyChoice.Page : BusyChoice.Targets);
        if (_noDefaults) throw BusyWithNoDefaults();
        if (_sentByADefault)
            throw new InvalidOperationException("Busy says where the default busy indicator shows, and a request a default's block sends takes no defaults; take out Busy.");
        if (!_defaults.ShowsBusy)
            throw new InvalidOperationException("Busy says where the default busy indicator shows, and the app's defaults show none; call ShowBusy() in AddReactiveRequests, or take out Busy.");
        if (elementIds.Any(string.IsNullOrEmpty))
            throw new InvalidOperationException("Busy names an element by the id the view gave it, and one id is empty; name each element by its id.");
        var named = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in elementIds.Where(id => !named.Add(id)))
            throw new InvalidOperationException($"Busy names element '{id}' twice; name each element once.");
        _busyIds = elementIds;
        return Changed();
    }

    internal int NoBusy(int stage)
    {
        Continue(stage);
        Choose(BusyChoice.None);
        return Changed();
    }

    internal int NoRetry(int stage)
    {
        Continue(stage);
        _noRetry = true;
        return Changed();
    }

    IReadOnlyList<Statement> IDefaultRoutes.Route<TReply>(TReply reply, Action<TReply, Pipeline<NoModel>> body)
    {
        Reaches(reply);
        return _writer.Branch(true, () => body(reply, new Pipeline<NoModel>(_writer))).Block;
    }

    private Statement Ended()
    {
        if (TakesDefaults) _defaultLayer = RequestScopes.InDefaults(_writer, WriteDefaults);
        if (!_namesItsCheck) _check = _defaultCheck(this);
        return Written();
    }

    private Layer WriteDefaults()
    {
        var loading = _writer.Branch(_enteredAfterAwait, () => _defaults.LoadingBlock(new Pipeline<NoModel>(_writer))).Block;
        var @finally = _writer.Branch(true, () => _defaults.FinallyBlock(new Pipeline<NoModel>(_writer))).Block;
        foreach (var status in _defaults.Statuses)
        {
            var (block, showsNotice, _) = RequestScopes.InRoute(_writer, _reply, _parts.ContainerId, () => status.Write(this));
            _defaultStatuses.Add((new StatusRoute(status.Status, status.Kind, block), showsNotice));
        }

        foreach (var otherwise in _defaults.CatchAll)
        {
            var (block, showsNotice, _) = RequestScopes.InRoute(_writer, _reply, _parts.ContainerId, () => otherwise.Write(this));
            _defaultOtherwise.Add((new OtherwiseRoute(otherwise.Kind, block), showsNotice));
        }

        return new Layer(loading, @finally);
    }

    private IReadOnlyList<Statement> Route<TReply>(TReply reply, Action<TReply, Pipeline<TModel>> body) where TReply : JsObject
    {
        Reaches(reply);
        var (block, _, placesAtTheNotice) = RequestScopes.InRoute(_writer, _reply, _parts.ContainerId, () => _writer.Branch(true, () => body(reply, new Pipeline<TModel>(_writer))).Block);
        _placesAtTheNotice |= placesAtTheNotice;
        return block;
    }

    private void Reaches(JsObject reply)
    {
        reply.Reach(new Reached(_reply, _writer));
        if (reply is ISentRequestReply sent) sent.SentBy(_parts);
    }

    private static IReadOnlyList<Statement> DefaultCheck<TRequest>(RequestWriter<TModel> request) => RequestChecks.Default<TRequest>(request._writer, request.CheckParts);

    private RequestCheckParts CheckParts()
    {
        _checkNames ??= (_writer.NextBinding("checked"), _writer.NextBinding("failures"));
        return _parts.CheckParts(_argument, _method, _checkNames.Value);
    }

    private void NamesItsCheck()
    {
        if (_namesItsCheck) throw new InvalidOperationException("The request already names its check; write one Validate or NoValidation for the request.");
        _namesItsCheck = true;
    }

    private IReadOnlyList<Statement> Check() => _check;

    private void Choose(BusyChoice choice)
    {
        if (_busy != BusyChoice.Derived) throw new InvalidOperationException("A request says once where its busy indicator shows; write one Busy or NoBusy.");
        _busy = choice;
    }

    private static InvalidOperationException BusyWithNoDefaults()
        => new("Busy says where the default busy indicator shows, and this request takes no defaults; take out NoDefaults() or Busy.");

    private void Continue(int stage)
    {
        if (stage != _stage || !_writer.Replaced(_place, Written())) throw BrokenSentence();
        _stage++;
    }

    private int Changed()
    {
        if (!_writer.Replaced(_place, Written())) throw BrokenSentence();
        return _stage;
    }

    private RequestStatement Written()
        => new(_argument.Name, _reply.Name, _parts.Send(_method, _success.Kind, RequestHost.RespondedJson, SendDefaults()),
            Check(), Layers(), _success, Statuses(), CatchAll(), _chained);

    private bool TakesDefaults => !_sentByADefault && !_noDefaults;

    private bool ChecksAtTheNotice => _check.Count > 0 && !_parts.SendsAContainer;

    private Layer[] Layers() => TakesDefaults ? [_defaultLayer, new Layer(_loading, _finally)] : [new Layer(_loading, _finally)];

    private IEnumerable<(StatusRoute Route, bool ShowsNotice)> ComposedStatuses
        => TakesDefaults ? _defaultStatuses.Where(composed => _statuses.All(own => own.Status != composed.Route.Status)) : [];

    private IEnumerable<(OtherwiseRoute Route, bool ShowsNotice)> ComposedOtherwise
        => TakesDefaults && !_hasOtherwise ? _defaultOtherwise : [];

    private StatusRoute[] Statuses() => [.. _statuses, .. ComposedStatuses.Select(composed => composed.Route)];

    private OtherwiseRoute CatchAll() => ComposedOtherwise.Select(composed => composed.Route).FirstOrDefault() ?? _otherwise;

    private SendDefaults SendDefaults()
    {
        var namesTheNotice = _placesAtTheNotice || ChecksAtTheNotice || ComposedStatuses.Any(composed => composed.ShowsNotice) || ComposedOtherwise.Any(composed => composed.ShowsNotice);
        var contracts = new List<RequestContract>();
        if (HoldsBusy) contracts.Add(RequestContract.Busy);
        if (namesTheNotice) contracts.Add(RequestContract.Notice);
        return new SendDefaults(TakesDefaults ? _defaults.Headers : [], Retry(), BusyRecord(),
            new ListExpression([.. contracts.Select(contract => ContractRecord<HttpSend>.Literal(contract))]));
    }

    private RecordExpression Retry()
    {
        var retried = TakesDefaults && !_noRetry && _method != RequestMethod.Post;
        return retried
            ? new ContractRecord<StandardRetryPolicy>().Set(retry => retry.Kind, "standard").Set(retry => retry.Attempts, _defaults.Attempts)
                .Set(retry => retry.FirstWaitMilliseconds, _defaults.FirstWaitMilliseconds).Written
            : new ContractRecord<NoneRetryPolicy>().Set(retry => retry.Kind, "none").Written;
    }

    private bool HoldsBusy => TakesDefaults && _defaults.ShowsBusy && _busy != BusyChoice.None;

    private RecordExpression BusyRecord()
    {
        if (!HoldsBusy) return new ContractRecord<NoneBusy>().Set(busy => busy.Kind, "none").Written;
        var ids = BusyIds();
        if (ids.Count == 0) return new ContractRecord<PageBusy>().Set(busy => busy.Kind, "page").Written;
        return new ContractRecord<TargetsBusy>().Set(busy => busy.Kind, "targets").Set(busy => busy.Values, new ListExpression([.. ids.Select(id =>
            new ContractRecord<ElementBusyTarget>().Set(target => target.Kind, "element").Set(target => target.Id, id).Written)])).Written;
    }

    private IReadOnlyList<string> BusyIds() => _busy == BusyChoice.Page ? [] : _busy == BusyChoice.Targets ? _busyIds : WrittenBySuccess();

    private IReadOnlyList<string> WrittenBySuccess()
        => [.. _success.Do.Select(WrittenObject).OfType<ObjExpression>().Select(written => written.Id).Where(id => !id.StartsWith("$", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal)];

    private static Expression? WrittenObject(Statement statement) => statement switch
    {
        SetStatement set => Root(set.Target),
        DoStatement done => Root(done.Expr),
        LetStatement { Expr: ObjExpression element } => element,
        LetStatement { Expr: CallExpression call } => Root(call),
        _ => null,
    };

    private static Expression Root(Expression expression) => expression switch
    {
        GetExpression read => Root(read.Target),
        CallExpression call => Root(call.Target),
        _ => expression,
    };

    private static InvalidOperationException BrokenSentence()
        => new("A request's next door continues its current stage before any other step of its block; write the request in one sentence.");
}

internal static class RequestScopes
{
    private const string FormObject = "$form";
    private const string FormPlugin = "alis.form";
    private const string NoKey = "";
    private const string ShowErrors = "showErrors";
    private const string NoticeSeparator = " ";

    private static readonly ConditionalWeakTable<BlockWriter, Scope> Scopes = new();

    internal static bool SentByADefault(BlockWriter writer) => Of(writer).DefaultDepth > 0;

    internal static TWritten InDefaults<TWritten>(BlockWriter writer, Func<TWritten> write)
    {
        var scope = Of(writer);
        scope.DefaultDepth++;
        try
        {
            return write();
        }
        finally
        {
            scope.DefaultDepth--;
        }
    }

    internal static (IReadOnlyList<Statement> Block, bool ShowsNotice, bool PlacesAtTheNotice) InRoute(BlockWriter writer, RefExpression reply, string? containerId,
        Func<IReadOnlyList<Statement>> write)
    {
        var routes = Of(writer).Routes;
        var route = new OpenRoute(reply, containerId);
        routes.Push(route);
        try
        {
            return (write(), route.ShowsNotice, route.PlacesAtTheNotice);
        }
        finally
        {
            routes.Pop();
        }
    }

    internal static void NoticeShown(BlockWriter writer)
    {
        foreach (var route in Of(writer).Routes) route.ShowsNotice = true;
    }

    internal static void ShowValidationErrors(BlockWriter writer, Expression problem)
    {
        var routes = Of(writer).Routes;
        if (routes.Count == 0)
        {
            throw new InvalidOperationException(
                "ShowValidationErrors shows a server's errors as a request's failures, and this step is in no route of a request; write it in the request's OnError or OnSuccess.");
        }

        var route = routes.Peek();
        writer.Content.Declares(FormObject, new PluginLocator(FormPlugin, NoKey, VendorListening.OfAnEventTarget, NonePluginData.Instance));
        var inputs = new GetExpression([new MemberStep(ContractRecord<HttpResponse>.Name(response => response.Inputs))], PathMembers.Required, route.Reply);
        writer.Write(new DoStatement(false, new CallExpression([new MemberStep(ShowErrors)], PathMembers.Required, new ObjExpression(FormObject), [problem, inputs, PlaceOf(route)])));
        if (route.ContainerId is not null) return;
        route.PlacesAtTheNotice = true;
        NoticeShown(writer);
    }

    private static RecordExpression PlaceOf(OpenRoute route) => route.ContainerId is null ? AtTheNotice : InTheSummary(route.ContainerId);

    private static RecordExpression InTheSummary(string containerId) => new ContractRecord<FormPlace>()
        .Set(place => place.Destination, new ContractRecord<SummaryFormDestination>().Set(destination => destination.Kind, "summary").Set(destination => destination.ContainerId, containerId).Written).Written;

    private static RecordExpression AtTheNotice => new ContractRecord<FormPlace>()
        .Set(place => place.Destination, new ContractRecord<NoticeFormDestination>().Set(destination => destination.Kind, "notice").Set(destination => destination.Separator, NoticeSeparator).Written).Written;

    private static Scope Of(BlockWriter writer) => Scopes.GetValue(writer, _ => new Scope());

    private sealed class Scope
    {
        internal int DefaultDepth { get; set; }

        internal Stack<OpenRoute> Routes { get; } = new();
    }

    private sealed class OpenRoute(RefExpression reply, string? containerId)
    {
        internal RefExpression Reply { get; } = reply;

        internal string? ContainerId { get; } = containerId;

        internal bool ShowsNotice { get; set; }

        internal bool PlacesAtTheNotice { get; set; }
    }
}
