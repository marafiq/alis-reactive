namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    internal OwnerDeskPage OwnerDeskOf(string draftId) => Desk.OwnerDeskOf(draftId);

    internal string PayerContactOf(string draftId) => Desk.PayerContactOf(draftId);
}
