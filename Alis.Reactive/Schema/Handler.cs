using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

internal sealed class Handler
{
    internal Handler(
        string on,
        IReadOnlyList<PathStep> path,
        string @event,
        IReadOnlyList<PathStep> payload,
        PathMembers members,
        Policy policy,
        IReadOnlyList<Statement> body)
    {
        On = on;
        Path = path;
        Event = @event;
        Payload = payload;
        Members = members;
        Policy = policy;
        Do = body;
    }

    public string On { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public string Event { get; }

    public IReadOnlyList<PathStep> Payload { get; }

    public PathMembers Members { get; }

    public Policy Policy { get; }

    public IReadOnlyList<Statement> Do { get; }
}

[JsonConverter(typeof(ChoiceConverter<Policy>))]
internal sealed class Policy : IChoice
{
    internal static readonly Policy Overlap = new("overlap");
    internal static readonly Policy Replace = new("replace");
    internal static readonly Policy Skip = new("skip");

    private Policy(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
