namespace Alis.Reactive.Conformance.World;

public static class WorldEndpoints
{
    public const string Ready = "/world/ready";
    public const string Seed = "/world/seed";
    public const string End = "/world/end";
    public const string ArmGate = "/world/gates/arm";
    public const string HeldGate = "/world/gates/held";
    public const string ReleaseGate = "/world/gates/release";
    public const string HostOrders = "/world/host/orders";
    public const string HostRestart = "/world/host/restart";
    public const string HostDone = "/world/host/done";
    public const string WordAnswer = "/world/word/answer";
    public const string WordAnswerAgain = "/world/word/answer-again";
    public const string WordDeskAnswer = "/world/word-desk/answer";
    public const string Settle = "/world/settle";
    public const string Count = "/world/count";
    public const string Item = "/world/item";
    public const string Text = "/world/text";
    public const string HeldCount = "/world/held-count";
    public const string Record = "/world/record";
    public const string Check = "/world/check";
    public const string SessionRenew = "/world/session/renew";
    public const string SessionFlush = "/world/session/flush";
    public const string CollectorTraces = "/world/collector/v1/traces";
    public const string CollectorLines = "/world/collector/lines";
    public const string PharmacyStock = "/world/pharmacy/stock";
    public const string FinallySettle = "/world/settle/finally";
    public const string StreamPush = "/world/streams/push";
    public const string StreamOpen = "/world/streams/open";
    public const string StreamDrop = "/world/streams/drop";
    public const string StreamNextHeld = "/world/streams/next-held";
    public const string StreamRefuse = "/world/streams/refuse";
    public const string StreamRefused = "/world/streams/refused";
    public const string StreamReopen = "/world/streams/reopen";
}
