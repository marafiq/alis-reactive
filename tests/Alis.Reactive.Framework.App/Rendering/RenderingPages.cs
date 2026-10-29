namespace Alis.Reactive.Framework.App.Rendering;

/// <summary>A shift handed over to the next: the resident, and the notes the shift leaves.</summary>
public sealed class ShiftHandover
{
    public string? Resident { get; set; }

    public string? Notes { get; set; }
}

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class HandoverEcho
{
    public string Summary { get; set; } = "";
}
