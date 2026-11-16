using System.Collections.Concurrent;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldData
{
    private const string Anywhere = "";

    private readonly ConcurrentQueue<string> _residents = new();
    private readonly ConcurrentQueue<string> _carers = new();
    private readonly ConcurrentDictionary<(string Gate, string At), List<WorldGate>> _gates = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<int>> _answers = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<CollectedSpan> _spans = new();
    private readonly ConcurrentQueue<HeardAttempt> _attempts = new();
    private readonly ConcurrentQueue<string> _secondOriginLog = new();
    private volatile string _word = "";
    private volatile string _componentText = "Alpha";
    private volatile string _componentOuterText = "Gamma";
    private volatile string _componentReadOnlyText = "RO-2026-11-10";
    private volatile string _componentCurrentItem = "One";
    private volatile int _heldNamedItems = 4;
    private volatile int _heldTotal = 14;
    private volatile int _componentOuterCount = 8;
    private volatile int _componentActiveItems = 3;
    private volatile int _componentCount = 3;
    private volatile string? _componentRecordDetail;
    private volatile ComponentVersion _componentVersion = ComponentVersion.Current;
    private volatile string _notesGroup = "Alpha";
    private volatile int _moveStatus = StatusCodes.Status200OK;
    private volatile bool _codeRefuses;
    private volatile int _heldMatchingItems = 1;
    private volatile bool _textRefuses;
    private volatile bool _heldCountRefuses;
    private volatile bool _settleRefuses;
    private volatile bool _sessionSignedOut;
    private volatile bool _countRefuses;
    private volatile bool _itemRefuses;
    private volatile bool _bedsDropTheirConnection;
    private volatile bool _componentHoldsNoRecords;
    private volatile string? _componentStartsPlace;
    private volatile string? _componentBetaConstructor;
    private volatile int _recordCount = 125;
    private volatile bool _checkAnswer = true;
    private volatile int _bindingsBetaCount;
    private volatile string? _bindingsBetaStarted;
    private volatile string? _bindingStarted;
    private volatile bool _bindingsWhole;
    private volatile bool _renderUnrecorded;

    internal IReadOnlyCollection<string> Residents => _residents;

    internal IReadOnlyCollection<string> Carers => _carers;

    internal string Word => _word;

    internal string ComponentText => _componentText;

    internal string ComponentOuterText => _componentOuterText;

    internal string ComponentReadOnlyText => _componentReadOnlyText;

    internal string ComponentCurrentItem => _componentCurrentItem;

    internal int HeldNamedItems => _heldNamedItems;

    internal int HeldTotal => _heldTotal;

    internal int ComponentOuterCount => _componentOuterCount;

    internal int ComponentActiveItems => _componentActiveItems;

    internal int ComponentCount => _componentCount;

    internal string? ComponentRecordDetail => _componentRecordDetail;

    internal ComponentVersion ComponentVersion => _componentVersion;

    internal string NotesGroup => _notesGroup;

    internal int MoveStatus => _moveStatus;

    internal bool CodeRefuses => _codeRefuses;

    internal int HeldMatchingItems => _heldMatchingItems;

    internal bool TextRefuses => _textRefuses;

    internal bool HeldCountRefuses => _heldCountRefuses;

    internal bool SettleRefuses => _settleRefuses;

    internal bool SessionSignedOut => _sessionSignedOut;

    internal bool CountRefuses => _countRefuses;

    internal bool ItemRefuses => _itemRefuses;

    internal bool BedsDropTheirConnection => _bedsDropTheirConnection;

    internal bool ComponentHoldsNoRecords => _componentHoldsNoRecords;

    internal string? ComponentStartsPlace => _componentStartsPlace;

    internal string? ComponentBetaConstructor => _componentBetaConstructor;

    internal int RecordCount => _recordCount;

    internal bool CheckAnswer => _checkAnswer;

    internal int BindingsBetaCount => _bindingsBetaCount;

    internal string? BindingsBetaStarted => _bindingsBetaStarted;

    internal string? BindingStarted => _bindingStarted;

    internal bool BindingsWhole => _bindingsWhole;

    internal bool RenderUnrecorded => _renderUnrecorded;

    internal IReadOnlyCollection<CollectedSpan> Spans => _spans;

    internal IReadOnlyCollection<HeardAttempt> Attempts => _attempts;

    internal IReadOnlyCollection<string> SecondOriginLog => _secondOriginLog;

    internal HostOrders HostOrders { get; } = new();

    internal void Take(Seed seed)
    {
        foreach (var resident in seed.Residents) _residents.Enqueue(resident);
        foreach (var carer in seed.Carers) _carers.Enqueue(carer);
        foreach (var (endpoint, statuses) in seed.Answers) _answers[endpoint] = new ConcurrentQueue<int>(statuses);
        if (seed.Word is { } word) _word = word;
        if (seed.ComponentText is { } text) _componentText = text;
        if (seed.ComponentOuterText is { } outerText) _componentOuterText = outerText;
        if (seed.ComponentReadOnlyText is { } readOnlyText) _componentReadOnlyText = readOnlyText;
        if (seed.ComponentCurrentItem is { } currentItem) _componentCurrentItem = currentItem;
        if (seed.HeldNamedItems is { } heldNamedItems) _heldNamedItems = heldNamedItems;
        if (seed.HeldTotal is { } heldTotal) _heldTotal = heldTotal;
        if (seed.ComponentOuterCount is { } outerCount) _componentOuterCount = outerCount;
        if (seed.ComponentActiveItems is { } activeItems) _componentActiveItems = activeItems;
        if (seed.ComponentCount is { } count) _componentCount = count;
        if (seed.ComponentRecordDetail is { } recordDetail) _componentRecordDetail = recordDetail;
        if (seed.ComponentVersion is { } version) _componentVersion = version;
        if (seed.NotesGroup is { } notesGroup) _notesGroup = notesGroup;
        if (seed.MoveStatus is { } moveStatus) _moveStatus = moveStatus;
        if (seed.CodeRefuses is { } codeRefuses) _codeRefuses = codeRefuses;
        if (seed.HeldMatchingItems is { } heldMatchingItems) _heldMatchingItems = heldMatchingItems;
        if (seed.TextRefuses is { } textRefuses) _textRefuses = textRefuses;
        if (seed.HeldCountRefuses is { } heldCountRefuses) _heldCountRefuses = heldCountRefuses;
        if (seed.SettleRefuses is { } settleRefuses) _settleRefuses = settleRefuses;
        if (seed.SessionSignedOut is { } sessionSignedOut) _sessionSignedOut = sessionSignedOut;
        if (seed.CountRefuses is { } countRefuses) _countRefuses = countRefuses;
        if (seed.ItemRefuses is { } itemRefuses) _itemRefuses = itemRefuses;
        if (seed.BedsDropTheirConnection is { } bedsDropTheirConnection) _bedsDropTheirConnection = bedsDropTheirConnection;
        if (seed.ComponentHoldsNoRecords is { } holdsNoRecords) _componentHoldsNoRecords = holdsNoRecords;
        if (seed.ComponentStartsPlace is { } startsPlace) _componentStartsPlace = startsPlace;
        if (seed.ComponentBetaConstructor is { } betaConstructor) _componentBetaConstructor = betaConstructor;
        if (seed.RecordCount is { } recordCount) _recordCount = recordCount;
        if (seed.CheckAnswer is { } checkAnswer) _checkAnswer = checkAnswer;
        if (seed.BindingsBetaCount is { } betaCount) _bindingsBetaCount = betaCount;
        if (seed.BindingsBetaStarted is { } betaStarted) _bindingsBetaStarted = betaStarted;
        if (seed.BindingStarted is { } bindingStarted) _bindingStarted = bindingStarted;
        if (seed.BindingsWhole is { } bindingsWhole) _bindingsWhole = bindingsWhole;
        if (seed.RenderUnrecorded is { } renderUnrecorded) _renderUnrecorded = renderUnrecorded;
    }

    internal void Collected(CollectedSpan span) => _spans.Enqueue(span);

    internal void Heard(string? traceparent, string? baggage) => _attempts.Enqueue(new HeardAttempt(traceparent, baggage));

    internal void SecondOriginReceived(string line) => _secondOriginLog.Enqueue(line);

    internal int NextAnswer(string endpoint) => _answers.TryGetValue(endpoint, out var statuses) && statuses.TryDequeue(out var status) ? status : StatusCodes.Status200OK;

    internal GateOrder Arm(GateOrder order)
    {
        var holds = _gates.GetOrAdd((order.Gate, order.At), _ => []);
        lock (holds)
        {
            holds.Add(new WorldGate());
            return new GateOrder { Gate = order.Gate, At = order.At, Hold = holds.Count };
        }
    }

    internal Task Reach(string gate, string at) => ParkAt(gate, at) ?? ParkAt(gate, Anywhere) ?? Task.CompletedTask;

    internal Task? Held(GateOrder order) => HoldOf(order)?.Held;

    internal bool Release(GateOrder order)
    {
        if (HoldOf(order) is not { IsParked: true } held) return false;

        held.Release();
        return true;
    }

    internal IList<string> End()
    {
        var holds = _gates.SelectMany(gate => HoldsOf(gate.Value).Select(hold => (gate.Key.Gate, gate.Key.At, Hold: hold))).ToList();
        var parked = holds.Where(armed => armed.Hold.IsParked).Select(armed => Named(armed.Gate, armed.At)).Order(StringComparer.Ordinal).ToList();
        foreach (var armed in holds) armed.Hold.EndWithItsWorld();
        HostOrders.End();
        return parked;
    }

    internal static string Named(string gate, string at) => at == Anywhere ? gate : $"{gate} at {at}";

    private static WorldGate[] HoldsOf(List<WorldGate> holds)
    {
        lock (holds) return [.. holds];
    }

    private Task? ParkAt(string gate, string at)
    {
        if (!_gates.TryGetValue((gate, at), out var holds)) return null;

        lock (holds) return holds.Find(hold => !hold.Held.IsCompleted)?.Park();
    }

    private WorldGate? HoldOf(GateOrder order)
    {
        if (!_gates.TryGetValue((order.Gate, order.At), out var holds)) return null;

        lock (holds) return holds.ElementAtOrDefault(order.Hold - 1);
    }
}
