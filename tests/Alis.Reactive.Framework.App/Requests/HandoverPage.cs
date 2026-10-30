namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// The night nurse hands the shift over: a post to an endpoint that checks the antiforgery token, sent with the
/// token the page was given as the header ASP.NET Core reads it from.
/// </summary>
public sealed class HandoverPage
{
    public HandoverToken Token { get; set; }
}

/// <summary>The handover the endpoint binds.</summary>
public sealed class HandoverNote
{
    public string? Note { get; set; }
}

/// <summary>Whether the page's request carries the antiforgery token it was given.</summary>
public enum HandoverToken
{
    /// <summary>The request sends the token as the <c>RequestVerificationToken</c> header.</summary>
    Sent,

    /// <summary>The request sends no token.</summary>
    LeftOut,
}
