using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// A file the user chose in a file input, as the browser holds it.
/// </summary>
[Mirrors("lib.dom", "File")]
public sealed class BrowserFile : JsObject
{
    /// <summary>Gets the file's name.</summary>
    public ReadOnly<string> Name => throw new NotImplementedException();

    /// <summary>Gets the file's size in bytes.</summary>
    public ReadOnly<long> Size => throw new NotImplementedException();

    /// <summary>Gets the file's media type, empty when the browser knows none.</summary>
    public ReadOnly<string> Type => throw new NotImplementedException();

    /// <summary>Gets when the file was last modified, in milliseconds since the start of 1970, UTC.</summary>
    public ReadOnly<long> LastModified => throw new NotImplementedException();
}
