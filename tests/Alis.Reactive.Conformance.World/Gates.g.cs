namespace Alis.Reactive.Conformance.World;

public enum CallbackPropertyGate
{
    Step,
    Script,
    Freeze,
    Restart,
    LoadA,
    LoadB,
    UnloadA,
    UnloadB,
    Raise,
}

public enum CompositionGate
{
    Unload,
    Begin,
    Stop,
    Answer,
    Route,
    DefaultFinally,
}

public enum LanesGate
{
    FireFirst,
    FireSecond,
    Dispose,
    Release,
    AllSettled,
    Respond,
    Route,
    SettleTheTail,
    Finally,
    After,
    Settle,
}

public enum OwnershipGate
{
    LoadA,
    UnloadA,
    LoadB,
    UnloadB,
    HostDestroys,
    ServerHangsUp,
    Retry,
    SendsSlotInput,
    ShowsFailures,
    Raise,
    UnsubscribeThrows,
    ListenA,
    Collect,
    HostDestroysListened,
}

public enum RealtimeGate
{
    BeginPage,
    Message,
    Retry,
    Unload,
    Answer,
    DefaultFinally,
    Connect,
    Drop,
    Reconnected,
    GiveUp,
}

public enum RequestGate
{
    Check,
    DefaultLoading,
    Loading,
    Guard,
    Attempt,
    Route,
    Finally,
    DefaultFinally,
    Chained,
    After,
}

public enum RunGate
{
    Fire,
    FirstReturns,
    SecondReturns,
    VendorReads,
    Step,
    Settle,
    Abort,
    CutInItsStep,
}

public enum TracingGate
{
    Trigger,
    Attempt,
    Respond,
    Route,
    Abort,
}
