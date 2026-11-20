namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    public const string MoveInDocumentCard = CarePages.MoveIns + "document-card";
    public const string MoveInDocuments = CarePages.MoveIn + "/documents";
    public const string MoveInConsent = MoveInDocuments + "/DOC1";
    public const string MoveInPacketPreview = CarePages.MoveIns + "packet-preview";
    public const string MoveInPacket = CarePages.MoveIn + "/packet";
    public const string MoveInConsentContent = MoveInConsent + "/content";
}
