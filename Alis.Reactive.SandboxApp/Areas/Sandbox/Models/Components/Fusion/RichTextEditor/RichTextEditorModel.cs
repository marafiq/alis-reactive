namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.RichTextEditor
{
    public sealed class RichTextEditorModel
    {
        public string? Paragraph { get; set; }

        public string LastMonthParagraph { get; set; } = string.Empty;
    }

    public sealed class NewsletterParagraphSave
    {
        public string Paragraph { get; set; } = string.Empty;
    }

    public sealed class NewsletterParagraphSaved
    {
        public string Message { get; set; } = string.Empty;
    }
}
