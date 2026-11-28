using System;

namespace Alis.Reactive.OwnershipAndState;

internal enum AntiforgeryChoice
{
    NoRequestsCall,
    NotChosen,
    NoToken,
    Header,
    Field,
}

internal sealed class PageAntiforgery
{
    private readonly Lazy<string> _token;

    internal PageAntiforgery(AntiforgeryChoice choice, string origin, Func<string> token)
    {
        Choice = choice;
        Origin = origin;
        _token = new Lazy<string>(token);
    }

    internal AntiforgeryChoice Choice { get; }

    internal string Origin { get; }

    internal string Token => _token.Value;
}
