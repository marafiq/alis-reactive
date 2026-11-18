namespace Alis.Reactive.Conformance.App.Care;

public sealed class ReceiptPage
{
    public string Reference { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}

public sealed class ReceiptQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;
}
