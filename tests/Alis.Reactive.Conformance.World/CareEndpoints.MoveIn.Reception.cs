namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    public const string MoveInReception = CarePages.MoveIn + "/reception";
    public const string MoveInHistory = CarePages.MoveIn + "/history";
}

public sealed partial class MoveInChange
{
    public string? BarredVisitor { get; set; }
}

public static partial class MoveInChanges
{
    public static Seed ReceptionBars(string visitor) => new() { MoveIn = new MoveInChange { BarredVisitor = visitor } };
}
