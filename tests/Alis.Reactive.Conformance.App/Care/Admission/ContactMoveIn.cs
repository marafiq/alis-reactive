using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ContactMoveIn
{
    public string? Phone { get; set; }

    public ContactMoveInKin NextOfKin { get; set; } = new();

    public string? Email { get; set; }

    public string Bound() => $"phone {Phone ?? "none"}, next of kin {NextOfKin.Name ?? "none"} and email {Email ?? "none"}";
}

public sealed class ContactMoveInKin
{
    public string? Name { get; set; }
}

public static class ContactMoveInEndpoints
{
    public const string MoveIns = CarePages.ContactMoveIn + "/move-ins";
}

public static class ContactMoveIns
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
