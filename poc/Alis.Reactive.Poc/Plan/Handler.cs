using System.Collections.Generic;

namespace Alis.Reactive.Plan;

internal sealed class Handler
{
    internal Handler(string on, IReadOnlyList<PathStep> path, string @event, IReadOnlyList<PathStep> payload, Policy policy, IReadOnlyList<Statement> body)
    {
        On = on;
        Path = path;
        Event = @event;
        Payload = payload;
        Policy = policy;
        Do = body;
    }

    public string On { get; }

    public IReadOnlyList<PathStep> Path { get; }

    public string Event { get; }

    public IReadOnlyList<PathStep> Payload { get; }

    public Policy Policy { get; }

    public IReadOnlyList<Statement> Do { get; }
}
