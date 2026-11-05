namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.HttpPipeline.FamilyCall;

public class FamilyCallModel
{
}

public sealed record FamilyPortalAnswer(PortalResident Resident);

public sealed record PortalResident(string Name, string Room, PortalContact EmergencyContact);

public sealed record PortalContact(string Name, string Relationship, string Phone);
