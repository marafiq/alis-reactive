namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.RichTextEditor
{
    /// <summary>
    /// The activities director's Garden Club paragraph for the family newsletter, and last month's paragraph
    /// to start from.
    /// </summary>
    public sealed class RichTextEditorModel
    {
        /// <summary>This month's paragraph, as formatted HTML.</summary>
        public string? Paragraph { get; set; }

        /// <summary>Last month's paragraph, as formatted HTML.</summary>
        public string LastMonthParagraph { get; set; } = string.Empty;
    }

    /// <summary>The paragraph the director saves.</summary>
    public sealed class NewsletterParagraphSave
    {
        /// <summary>The paragraph as formatted HTML.</summary>
        public string Paragraph { get; set; } = string.Empty;
    }

    /// <summary>What the newsletter says about the saved paragraph.</summary>
    public sealed class NewsletterParagraphSaved
    {
        /// <summary>The paragraph saved, with its length in words.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
