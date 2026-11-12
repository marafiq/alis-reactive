using System;
using Alis.Reactive.ValuesAndSources;

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
    private Notice()
    {
    }

    /// <summary>Shows a text as a success.</summary>
    /// <param name="text">The text to show.</param>
    public void Success(string text) => throw new NotImplementedException();

    /// <summary>Shows what a source reads when the step runs, as a success.</summary>
    /// <param name="text">The source of the text to show.</param>
    public void Success(ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Shows a text as an error.</summary>
    /// <param name="text">The text to show.</param>
    public void Error(string text) => throw new NotImplementedException();

    /// <summary>Shows what a source reads when the step runs, as an error.</summary>
    /// <param name="text">The source of the text to show.</param>
    public void Error(ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Shows a text as a warning.</summary>
    /// <param name="text">The text to show.</param>
    public void Warning(string text) => throw new NotImplementedException();

    /// <summary>Shows what a source reads when the step runs, as a warning.</summary>
    /// <param name="text">The source of the text to show.</param>
    public void Warning(ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Shows a text as information.</summary>
    /// <param name="text">The text to show.</param>
    public void Info(string text) => throw new NotImplementedException();

    /// <summary>Shows what a source reads when the step runs, as information.</summary>
    /// <param name="text">The source of the text to show.</param>
    public void Info(ITypedSource<string?> text) => throw new NotImplementedException();
}
