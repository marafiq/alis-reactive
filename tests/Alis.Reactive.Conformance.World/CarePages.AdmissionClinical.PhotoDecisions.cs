namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string PhotoDecision = AdmissionClinical + "photo-decision";

    public const string PhotoDecisionPending = PhotoDecision + "?pending=true";

    public const string PhotoDecisionGranted = PhotoDecision + "?granted=true";

    public const string PhotoDecisionPendingGranted = PhotoDecision + "?pending=true&granted=true";

    public const string PhotoDecisionLocked = PhotoDecision + "?locked=true";

    public const string PhotoDecisionSave = PhotoDecision + "/save";

    public const string PhotoConsentSheet = AdmissionClinical + "photo-consent-sheet";

    public const string PhotoConsentSheetFieldLabel = PhotoConsentSheet + "?fieldLabelOnly=true";

    public const string PhotoConsentSheetPlain = PhotoConsentSheet + "?plain=true";

    public const string PhotoConsentSheetLarge = PhotoConsentSheet + "?largePrint=true";

    public const string PhotoConsentSheetArabic = PhotoConsentSheet + "?arabic=true";

    public const string PhotoConsentSheetHouse = PhotoConsentSheet + "?houseCaption=true";

    public const string PhotoReleaseFiling = AdmissionClinical + "photo-release-filing";

    public const string PhotoReleaseClinical = PhotoReleaseFiling + "?clinical=true";

    public const string PhotoReleaseDirectory = PhotoReleaseFiling + "?localNotice=true";

    public const string PhotoWorksheet = AdmissionClinical + "photo-worksheet";

    public const string PhotoWorksheetPreview = PhotoWorksheet + "?signed=true";

    public const string PhotoWorksheetReopened = PhotoWorksheet + "?reopen=true";
}
