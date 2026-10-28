namespace Alis.Reactive.Framework.App.Requests;

/// <summary>The page that sends every kind of request.</summary>
public sealed class RequestsPage;

public sealed class ResidentQuery
{
    public int Id { get; set; }

    public string? Wing { get; set; }
}

public sealed class NoteRequest
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public string? Title { get; set; }

    public int Tally { get; set; }

    public decimal Amount { get; set; }
}

public sealed class TotalRoute
{
    public decimal Total { get; set; }
}

public sealed class RefuseQuery
{
    public int Status { get; set; }
}

public sealed class SecondQuery
{
    public string From { get; set; } = "";
}

public sealed class SlowQuery
{
    public int Delay { get; set; }

    public string Name { get; set; } = "";
}

/// <summary>What an endpoint received, in its own words.</summary>
public sealed class Echo
{
    public string Summary { get; set; } = "";
}

/// <summary>Why an endpoint refused.</summary>
public sealed class Refusal
{
    public string Reason { get; set; } = "";
}

public sealed class FirstAnswer
{
    public string Next { get; set; } = "";
}
