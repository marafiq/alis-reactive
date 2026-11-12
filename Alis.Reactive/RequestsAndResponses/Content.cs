using System;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The kinds of body a route reads. A route names one, and the browser reads the answer's body as that kind.
/// </summary>
/// <remarks>
/// A success whose body is not of the kind its route names runs the request's catch-all route.
/// </remarks>
public static class Content
{
    /// <summary>Gets the kind of a route with no data: the body is not read.</summary>
    public static NoContent None => throw new NotImplementedException();

    /// <summary>Gets the kind of a body of any <c>text/*</c> type, read as text.</summary>
    public static ErrorContent<string> Text => throw new NotImplementedException();

    /// <summary>Gets the kind of a body of <c>text/html</c> or <c>application/xhtml+xml</c>, which <c>Into</c> loads into an element.</summary>
    public static ErrorContent<Markup> Html => throw new NotImplementedException();

    /// <summary>Gets the kind of a body of any media type that the browser saves as a download.</summary>
    public static SuccessContent<ResponseFile> File => throw new NotImplementedException();

    /// <summary>Makes the kind of a body of <c>application/json</c>, or of a type ending in <c>+json</c>, read as <typeparamref name="TBody"/>.</summary>
    /// <typeparam name="TBody">The type the body is read as.</typeparam>
    /// <returns>The kind, which a success route and an error route take.</returns>
    public static ErrorContent<TBody> Json<TBody>() => throw new NotImplementedException();
}

/// <summary>
/// The kind of a route with no data.
/// </summary>
public sealed class NoContent
{
    private NoContent()
    {
    }
}

/// <summary>
/// A kind of body a success route reads.
/// </summary>
/// <typeparam name="TBody">The type the body is read as.</typeparam>
public abstract class SuccessContent<TBody>
{
    private protected SuccessContent()
    {
    }
}

/// <summary>
/// A kind of body an error route reads, and a success route too.
/// </summary>
/// <typeparam name="TBody">The type the body is read as.</typeparam>
public sealed class ErrorContent<TBody> : SuccessContent<TBody>
{
    private ErrorContent()
    {
    }
}

/// <summary>
/// The file a route saved as the browser's download.
/// </summary>
public sealed class ResponseFile
{
    /// <summary>Gets the name the download was saved under: the answer's <c>Content-Disposition</c> name, else the last segment of the request's path.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the file's size in bytes.</summary>
    public long Size { get; init; }

    /// <summary>Gets the file's media type.</summary>
    public string Type { get; init; } = "";
}
