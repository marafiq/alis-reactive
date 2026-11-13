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
}
