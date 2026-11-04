namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CommandOrdering;

public class CommandOrderingModel
{
    public string CareLevel { get; set; } = "";
}

public class CommandOrderingPayload
{
    public string Level { get; set; } = "";
}

public static class CommandOrderingEvents
{
    public static readonly AppEvent<CommandOrderingPayload> OrderingTest = AppEvent.Declare<CommandOrderingPayload>();
}
