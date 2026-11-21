using System;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The page's notice, such as a toast: a step shows a text as a success, an error, a warning or information.
/// </summary>
/// <remarks>
/// The notice is whatever component the app registers for it. A page whose app registers none stops the pipeline
/// that reaches it.
/// </remarks>
public sealed class Notice
{
    private const string Contract = "notice";
    private const string NoKey = "";
    private const string ShowMethod = "show";

    private readonly BlockWriter _writer;
    private readonly Action _shown;

    internal Notice(BlockWriter writer, Action shown)
    {
        _writer = writer;
        _shown = shown;
    }

    /// <summary>Shows a text as a success.</summary>
    /// <param name="text">The text to show.</param>
    public void Success(string text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "success");

    /// <summary>Shows what a source reads when the step runs, as a success.</summary>
    /// <remarks>A source that reads nothing shows an empty text.</remarks>
    /// <param name="text">The source of the text to show.</param>
    public void Success(ITypedSource<string?> text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "success");

    /// <summary>Shows a text as an error.</summary>
    /// <param name="text">The text to show.</param>
    public void Error(string text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "error");

    /// <summary>Shows what a source reads when the step runs, as an error.</summary>
    /// <remarks>A source that reads nothing shows an empty text.</remarks>
    /// <param name="text">The source of the text to show.</param>
    public void Error(ITypedSource<string?> text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "error");

    /// <summary>Shows a text as a warning.</summary>
    /// <param name="text">The text to show.</param>
    public void Warning(string text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "warning");

    /// <summary>Shows what a source reads when the step runs, as a warning.</summary>
    /// <remarks>A source that reads nothing shows an empty text.</remarks>
    /// <param name="text">The source of the text to show.</param>
    public void Warning(ITypedSource<string?> text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "warning");

    /// <summary>Shows a text as information.</summary>
    /// <param name="text">The text to show.</param>
    public void Info(string text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "info");

    /// <summary>Shows what a source reads when the step runs, as information.</summary>
    /// <remarks>A source that reads nothing shows an empty text.</remarks>
    /// <param name="text">The source of the text to show.</param>
    public void Info(ITypedSource<string?> text) => Shown(TextForms.Part(text ?? throw new ArgumentNullException(nameof(text))), "info");

    private void Shown(Expression text, string kind)
    {
        var notice = _writer.Content.ReachesPlugin(new PluginLocator(Contract, NoKey, VendorListening.OfAnEventTarget, NonePluginData.Instance));
        _writer.Write(new DoStatement(false, new CallExpression([new MemberStep(ShowMethod)], PathMembers.Required, new ObjExpression(notice), [text, Literals.Text(kind)])));
        _shown();
    }
}
