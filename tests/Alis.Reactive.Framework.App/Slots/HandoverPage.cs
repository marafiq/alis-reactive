namespace Alis.Reactive.Framework.App.Slots;

/// <summary>
/// The shift handover, opened with the night shift's draft rendered inside the container the page clears.
/// Opened <see cref="Afresh"/>, the page clears the draft as it starts.
/// </summary>
public sealed class HandoverPage
{
    public bool Afresh { get; set; }
}

/// <summary>The night shift's draft: a partial of its own model, with a ready handler of its own.</summary>
public sealed class HandoverDraft;
