namespace FeatureFlags.Plans;

/// <summary>Every URL the flag screens request. Controller routes and plans share these.</summary>
public static class FlagRoutes
{
    public const string Index = "/";
    public const string List = "/flags/list";
    public const string Detail = "/flags/detail";
    public const string New = "/flags/new";
    public const string WhoHasIt = "/flags/who-has-it";
    public const string SaveSummary = "/flags/save-summary";
    public const string KeyField = "/flags/key-field";
    public const string DevSnippets = "/flags/dev-snippets";
    public const string Save = "/flags/save";
    public const string Create = "/flags/create";
    public const string Check = "/flags/check";
    public const string CheckResult = "/flags/check/result";
}

/// <summary>
/// Developer-chosen element IDs. Partial containers double as the slot each partial
/// is injected into.
/// </summary>
public static class FlagIds
{
    public const string Shell = "ff-shell";
    public const string ShowDetail = "is-detail";

    public const string ListSlot = "flag-list";
    public const string DetailSlot = "flag-detail";

    public const string Form = "flag-form";
    public const string WhoHasItSlot = "who-has-it";
    public const string SaveSummarySlot = "save-summary";
    public const string KeyFieldSlot = "key-field";
    public const string DevSnippetsSlot = "dev-snippets";
    public const string SaveBar = "save-bar";
    public const string PickFields = "pick-fields";
    public const string EveryoneNote = "everyone-note";
    public const string HistoryMore = "history-more";
    public const string HistoryToggle = "history-toggle";
    public const string DevPanel = "dev-panel";
    public const string DetailsPanel = "details-panel";
    public const string Open = "is-open";

    public const string CheckResultSlot = "check-result";
    public const string DrawerContent = "alis-drawer-content";
    public const string DrawerTitle = "alis-drawer-title";
}

/// <summary>Document events the flag screens dispatch to each other.</summary>
public static class FlagEvents
{
    /// <summary>A flag was saved, created or opened in another app: refresh the list for that app.</summary>
    public const string FlagsChanged = "flags-changed";
}
