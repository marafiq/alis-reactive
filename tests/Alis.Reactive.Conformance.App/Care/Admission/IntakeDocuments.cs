using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeDocuments
{
    public string? Name { get; set; }

    public List<IntakeDocument> Documents { get; set; } = [];
}

public sealed partial class IntakeDocument
{
    public string? Caption { get; set; }
}

public static class IntakeDocumentsEndpoints
{
    public const string Saves = CarePages.IntakeDocuments + "/saves";
}

public static class IntakeDocumentSaves
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
