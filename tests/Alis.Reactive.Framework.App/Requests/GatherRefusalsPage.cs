namespace Alis.Reactive.Framework.App.Requests;

public sealed class GatherRefusalsPage
{
    public GatherMistake Mistake { get; set; }

    public string? Visitor { get; set; }

    public int Party { get; set; }

    public int? Room { get; set; }

    public SignInGuide Guide { get; set; } = new();

    public List<IFormFile>? CarePlan { get; set; }

    public string? Shift { get; set; }
}

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
    SetAMemberTwice,
    SendFormDataWithNothingGathered,
}

public sealed class GuideRequest
{
    public GuideName? Guide { get; set; }
}

public sealed class GuideName
{
    public string? Name { get; set; }
}

public sealed class PartyAsText
{
    public string? Party { get; set; }
}

public sealed class CarePlanAsText
{
    public string? CarePlan { get; set; }
}

public sealed class RoomRequired
{
    public int Room { get; set; }
}
