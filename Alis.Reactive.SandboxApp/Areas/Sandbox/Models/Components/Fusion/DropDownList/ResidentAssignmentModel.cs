namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public sealed class ResidentAssignmentModel
{
    // Null means nobody is assigned; zero is not an assigned resident either.
    public int? ResidentId { get; set; }
    public int? BackupResidentId { get; set; }
    public string Wing { get; set; } = "East";
}

public sealed record AssignmentResident(int Id, string Name);
public sealed record AssignmentReceipt(string Message);
