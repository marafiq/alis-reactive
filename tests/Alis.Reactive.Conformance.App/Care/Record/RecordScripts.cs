namespace Alis.Reactive.Conformance.App.Care;

public interface IScriptedRecordPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class RecordScripts
{
    private const string Root = "/js/care/record/";

    public const string CardRadio = Root + "card-radio.js";

    public const string CardRadioWithAStuckUnsubscribe = Root + "card-radio-with-a-stuck-unsubscribe.js";

    public const string CardTally = Root + "card-tally.js";

    public const string HallBell = Root + "hall-bell.js";

    public const string PageTally = Root + "page-tally.js";
}
