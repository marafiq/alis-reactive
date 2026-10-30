namespace Alis.Reactive.Framework.App.Requests;

/// <summary>The resident directory, opened for the wing and the wings its address names.</summary>
public sealed class DirectoryPage;

/// <summary>The query the directory's address carries, as its action binds it with <c>[FromQuery]</c>.</summary>
public sealed class DirectoryQuery
{
    public string? Wing { get; set; }

    public string[] Wings { get; set; } = [];
}
