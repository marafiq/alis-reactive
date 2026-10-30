namespace Alis.Reactive.Framework.App.Slots;

/// <summary>The pass desk: a slot the visitor's pass loads into, and the one step the page writes about it.</summary>
public sealed class PassDeskPage
{
    public PassDeskStep Step { get; set; }
}

/// <summary>A notice beside the pass desk, a partial of its own model.</summary>
public sealed class PassNotice;

/// <summary>What the pass desk's step writes: an element beside the slot, or the slot itself in three ways.</summary>
public enum PassDeskStep
{
    WriteBesideTheSlot,
    ClearTheSlotsHtml,
    WriteTheSlotsTextFromAPartial,
    WriteTheTextOfTheSlotItUnloads,
}
