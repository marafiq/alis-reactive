namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string AdmissionClinical = Area + "admission-clinical/";

    public const string SmartPasteReferral = AdmissionClinical + "smart-paste-referral";

    public const string SmartPasteReferralSlot = SmartPasteReferral + "/slot";

    public const string SmartPasteReferralFields = SmartPasteReferral + "/fields";

    public const string SmartPasteAssist = "/intake/smart-paste";

    public const string AllergyIntake = AdmissionClinical + "allergy-intake";

    public const string WardAllergies = AdmissionClinical + "ward-allergies";

    public const string AllergyHintGiven = AdmissionClinical + "allergy-hint/given";

    public const string AllergyHintUnset = AdmissionClinical + "allergy-hint/unset";

    public const string HintChange = AdmissionClinical + "hint-change";

    public const string AllergyField = AdmissionClinical + "allergy-field";

    public const string FieldChange = AdmissionClinical + "field-change";

    public const string AllergyList = AdmissionClinical + "allergy-list";

    public const string AllergyListOnTheSouthWard = AdmissionClinical + "allergy-list?ward=South";

    public const string AllergyListOnTheNorthWard = AdmissionClinical + "allergy-list?ward=North";

    public const string AllergySelectionEmpty = AdmissionClinical + "allergy-selection/empty";

    public const string AllergySelectionOfLatex = AdmissionClinical + "allergy-selection/latex";

    public const string AllergySource = AdmissionClinical + "allergy-source";

    public const string AllergyTagging = AdmissionClinical + "allergy-tagging";

    public const string PeanutCheck = AdmissionClinical + "peanut-check";

    public const string AllergyOffsetWritten = AdmissionClinical + "allergy-offset/written";

    public const string AllergyOffsetRead = AdmissionClinical + "allergy-offset/read";

    public const string AllergySaveOfTwo = AdmissionClinical + "allergy-save/two";

    public const string AllergySaveOfNone = AdmissionClinical + "allergy-save/none";

    public const string AllergySearch = AdmissionClinical + "allergy-search";

    public const string AllergyCard = AdmissionClinical + "allergy-card";

    public const string AllergyWatch = AdmissionClinical + "allergy-watch";

    public const string AllergyRemoval = AdmissionClinical + "allergy-removal";

    public const string IntakeCheck = AdmissionClinical + "intake-check";

    public const string GuardianAllergiesOfLatex = AdmissionClinical + "guardian-allergies/latex";

    public const string GuardianAllergiesUnrecorded = AdmissionClinical + "guardian-allergies/unrecorded";

    public const string MedicationIntake = AdmissionClinical + "medication-intake";

    public const string WardMedications = AdmissionClinical + "ward-medications";

    public const string AllergyTags = AdmissionClinical + "allergy-tags";

    public const string WardPlacement = AdmissionClinical + "ward-placement";

    public const string HomeWards = AdmissionClinical + "home-wards";

    public const string BedAllocation = AdmissionClinical + "bed-allocation";

    public const string OpenBeds = AdmissionClinical + "open-beds";

    public const string PhotoConsentGiven = AdmissionClinical + "photo-consent/given";

    public const string PhotoConsentWithheld = AdmissionClinical + "photo-consent/withheld";

    public const string ArrivalDayKnown = AdmissionClinical + "arrival-day/known";

    public const string ArrivalDayOpen = AdmissionClinical + "arrival-day/open";

    public const string RespiteStayBooked = AdmissionClinical + "respite-stay/booked";

    public const string RespiteStayOpen = AdmissionClinical + "respite-stay/open";

    public const string FirstVisitBooked = AdmissionClinical + "first-visit/booked";

    public const string FirstVisitOpen = AdmissionClinical + "first-visit/open";

    public const string CarePlanUpload = AdmissionClinical + "care-plan-upload";

    public const string CarePlanDraft = AdmissionClinical + "care-plan-draft";

    public const string CarePlanFiling = AdmissionClinical + "care-plan-filing";

    public const string RoomType = AdmissionClinical + "room-type";

    public const string AdmissionStepsAtMedications = AdmissionClinical + "admission-steps/medications";

    public const string AdmissionStepsAtRoom = AdmissionClinical + "admission-steps/room";

    public const string CommunicationPreferences = AdmissionClinical + "communication-preferences";

    public const string LanguageIntake = AdmissionClinical + "language-intake";

    public const string ResidentPreview = AdmissionClinical + "resident-preview";

    public const string ResidentPreviewInCamelCase = AdmissionClinical + "resident-preview/camel-case";

    public const string AllergyLabels = AdmissionClinical + "allergy-labels";

    public const string AllergyLabelsReadRaw = AdmissionClinical + "allergy-labels/raw";

    public const string AllergySeverities = AdmissionClinical + "allergy-severities";

    public const string AllergySeveritiesOfAModeratePenicillin = AdmissionClinical + "allergy-severities/moderate";
}
