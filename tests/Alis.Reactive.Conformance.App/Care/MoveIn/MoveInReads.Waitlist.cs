namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    internal WaitlistRecordPage WaitlistOf(string draftId) => Desk.WaitlistOnFile(draftId);
}
