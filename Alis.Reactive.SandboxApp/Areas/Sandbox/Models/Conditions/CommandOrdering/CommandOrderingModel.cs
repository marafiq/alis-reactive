namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CommandOrdering;

public class CommandOrderingModel
{
    public string CareLevel { get; set; } = "";
}

public class CommandOrderingPayload
{
    public string Level { get; set; } = "";
}

/// <summary>The document events the CommandOrdering page raises and hears.</summary>
public static class CommandOrderingEvents
{
    public static readonly AppEvent<CommandOrderingPayload> OrderingTest = AppEvent.Declare<CommandOrderingPayload>();
}
