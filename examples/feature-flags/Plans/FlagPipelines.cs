using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Fusion.AppLevel;
using FeatureFlags.Models;

namespace FeatureFlags.Plans;

/// <summary>
/// Pipeline fragments the flag screens repeat. Each one is ordinary DSL; they live here
/// so every input on the form reacts the same way.
/// </summary>
public static class FlagPipelines
{
    /// <summary>
    /// Any change on the form: show the save bar, then ask the server what the draft
    /// means. The whole form goes up; "Who has it" and the save summary come back as HTML.
    /// New flags also refresh the headline and the code snippets, since their name and key
    /// are still being typed.
    /// </summary>
    public static void RefreshDraft(PipelineBuilder<FlagEditModel> p, bool isNew)
    {
        p.Element(FlagIds.SaveBar).Show();

        if (isNew)
        {
            p.Parallel(WhoHasIt, SaveSummary, DevSnippets, Headline);
            return;
        }

        p.Parallel(WhoHasIt, SaveSummary);
    }

    /// <summary>Recompute "Who has it" only, with one extra field (the site just tapped).</summary>
    public static void PickSite(PipelineBuilder<FlagEditModel> p, string siteId)
    {
        p.Post(FlagRoutes.WhoHasIt)
            .Gather(g => g.IncludeAll().Static("PickSite", siteId))
            .Response(r => r.OnSuccess(s => s.Into(FlagIds.WhoHasItSlot)));
    }

    /// <summary>Load one flag into the detail pane; on phones the pane slides in.</summary>
    public static void OpenFlag<TModel>(PipelineBuilder<TModel> p, string flagId) where TModel : class
    {
        p.Get(FlagRoutes.Detail)
            .Gather(g => g.Static("id", flagId))
            .Response(r => r.OnSuccess(s =>
            {
                s.Into(FlagIds.DetailSlot);
                s.Element(FlagIds.Shell).AddClass(FlagIds.ShowDetail);
            }));
    }

    /// <summary>Tell the list to reload for an app (after a save, a create, or opening a twin).</summary>
    public static void ListChanged<TModel>(PipelineBuilder<TModel> p, string app) where TModel : class
    {
        p.Dispatch(FlagEvents.FlagsChanged, new FlagsChanged { App = app });
    }

    public static void Toast<TModel>(PipelineBuilder<TModel> p, string title, string content) where TModel : class
    {
        p.Component<FusionToast>()
            .SetTitle(title)
            .SetContent(content)
            .Success()
            .ShowCloseButton()
            .Show();
    }

    private static void WhoHasIt(HttpRequestBuilder<FlagEditModel> request) =>
        request.Post(FlagRoutes.WhoHasIt)
            .Gather(g => g.IncludeAll())
            .Response(r => r.OnSuccess(s => s.Into(FlagIds.WhoHasItSlot)));

    private static void SaveSummary(HttpRequestBuilder<FlagEditModel> request) =>
        request.Post(FlagRoutes.SaveSummary)
            .Gather(g => g.IncludeAll())
            .Response(r => r.OnSuccess(s => s.Into(FlagIds.SaveSummarySlot)));

    private static void Headline(HttpRequestBuilder<FlagEditModel> request) =>
        request.Post(FlagRoutes.Headline)
            .Gather(g => g.IncludeAll())
            .Response(r => r.OnSuccess(s => s.Into(FlagIds.HeadlineSlot)));

    private static void DevSnippets(HttpRequestBuilder<FlagEditModel> request) =>
        request.Post(FlagRoutes.DevSnippets)
            .Gather(g => g.IncludeAll())
            .Response(r => r.OnSuccess(s => s.Into(FlagIds.DevSnippetsSlot)));
}

/// <summary>Copies text with the browser clipboard. The DSL has no clipboard reaction, so this is a plugin.</summary>
public sealed class ClipboardPlugin : Plugin
{
    public ClipboardPlugin()
        : base("clipboard")
    {
        WriteText = Command("writeText").Arg<string>();
    }

    public PluginCommand WriteText { get; }
}
