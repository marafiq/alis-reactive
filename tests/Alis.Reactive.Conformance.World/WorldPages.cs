namespace Alis.Reactive.Conformance.World;

public static class WorldPages
{
    public const string CultureSlip = "/world/culture-slip";
    public const string CultureSlipUnderTheGermanPhonebook = CultureSlip + "?culture=de-DE_phoneb";
    public const string CultureSlipUnderGerman = CultureSlip + "?culture=de-DE";
    public const string CultureSlipUnderAnUnservedCulture = CultureSlip + "?culture=fr-FR";
    public const string Records = "/world/records";
    public const string Word = "/world/word";
    public const string Words = "/world/words";
    public const string WordDesk = "/world/word-desk";
    public const string WordSlip = "/world/word-slip";
    public const string ZoneSlip = "/world/zone-slip";
    public const string Collector = "/world/collector";
    public const string Echo = "/world/echo";
    public const string SecondOriginLog = "/world/second-origin/log";
}
