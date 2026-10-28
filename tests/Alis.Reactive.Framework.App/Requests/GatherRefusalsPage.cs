namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A sign-in whose request makes one mistake a gather cannot send, so it is refused when it renders.</summary>
public sealed class GatherRefusalsPage
{
    public GatherMistake Mistake { get; set; }

    public string? Visitor { get; set; }

    public int Party { get; set; }

    public int? Room { get; set; }

    public SignInGuide Guide { get; set; } = new();

    public List<IFormFile>? CarePlan { get; set; }

    /// <summary>The shift a header would carry; the view was given none.</summary>
    public string? Shift { get; set; }
}

/// <summary>The mistakes a gather can make.</summary>
public enum GatherMistake
{
    NameAMemberTheRequestLacks,
    NameAMemberTheRequestsNestedClassLacks,
    SendANumberToText,
    SendAMaybeNumberToANumber,
    SendFilesToText,
    NameNoInput,
    GiveAHeaderNoValue,
    SetAMemberThenTheValueHoldingIt,
    SetAValueThenAMemberUnderIt,
}

/// <summary>The guide, as a request that knows only the guide's name.</summary>
public sealed class GuideRequest
{
    public GuideName? Guide { get; set; }
}

public sealed class GuideName
{
    public string? Name { get; set; }
}

/// <summary>The party, as text.</summary>
public sealed class PartyAsText
{
    public string? Party { get; set; }
}

/// <summary>The care plan, as text.</summary>
public sealed class CarePlanAsText
{
    public string? CarePlan { get; set; }
}

/// <summary>A room the request requires.</summary>
public sealed class RoomRequired
{
    public int Room { get; set; }
}
