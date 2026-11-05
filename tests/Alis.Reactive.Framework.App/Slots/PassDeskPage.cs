namespace Alis.Reactive.Framework.App.Slots;

public sealed class PassDeskPage
{
    public PassDeskStep Step { get; set; }
}

public sealed class PassNotice;

public enum PassDeskStep
{
    WriteBesideTheSlot,
    ClearTheSlotsHtml,
    WriteTheSlotsTextFromAPartial,
    WriteTheTextOfTheSlotItUnloads,
}
