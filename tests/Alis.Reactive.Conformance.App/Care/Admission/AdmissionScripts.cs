namespace Alis.Reactive.Conformance.App.Care;

public interface IScriptedAdmissionPage
{
    IReadOnlyList<string> Scripts { get; }
}

public static class AdmissionScripts
{
    private const string Root = "/js/care/admission/";

    public const string MoveInBell = Root + "move-in-bell.js";
}
