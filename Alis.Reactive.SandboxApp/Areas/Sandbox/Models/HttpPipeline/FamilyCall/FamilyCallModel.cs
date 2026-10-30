namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.HttpPipeline.FamilyCall;

/// <summary>
/// The night nurse's call to a resident's family after a fall. The page has no inputs: the nurse asks
/// the family portal for the resident's emergency contact and calls the number it gives.
/// </summary>
public class FamilyCallModel
{
}

/// <summary>
/// The family portal's answer, nested as the portal sends it: the resident, and within the resident the
/// emergency contact.
/// </summary>
public sealed record FamilyPortalAnswer(PortalResident Resident);

/// <summary>A resident as the family portal knows them.</summary>
public sealed record PortalResident(string Name, string Room, PortalContact EmergencyContact);

/// <summary>The person the community calls first, and how they are related to the resident.</summary>
public sealed record PortalContact(string Name, string Relationship, string Phone);
