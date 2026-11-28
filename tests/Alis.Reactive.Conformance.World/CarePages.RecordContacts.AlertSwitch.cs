namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string AlertPreferences = RecordContacts + "alert-preferences";

    public const string AlertPreferencesSave = AlertPreferences + "/save";

    public const string AlertPreferencesEnabled = AlertPreferences + "/enabled";

    public const string AlertPreferencesDayChannelReview = AlertPreferences + "?reviewChannel=true";

    public const string AlertPreferencesNightChannelReview = AlertPreferencesEnabled + "?reviewChannel=true";

    public const string AlertPreferencesFieldLabelReview = AlertPreferences + "?reviewChannel=true&explicitChannelLabel=false";

    public const string AlertPreferencesWardManaged = AlertPreferences + "/ward-managed";

    public const string AlertPreferencesReplacement = AlertPreferences + "/replacement";

    public const string AlertPreferencesReplacementPaused = AlertPreferences + "/replacement-paused";

    public const string AlertPreferencesBrowserStorage = AlertPreferences + "/browser-storage";

    public const string AlertPreferencesSavedOnBrowser = AlertPreferencesBrowserStorage + "/saved";

    public const string AlertPreferencesLocked = AlertPreferences + "/locked";

    public const string AlertPreferencesCompact = AlertPreferences + "/compact";

    public const string AlertPreferencesRightToLeft = AlertPreferences + "/right-to-left";

    public const string AlertSwitchPreview = AlertPreferences + "/preview";

    public const string AlertDestination = AlertPreferences + "/destination";
}
