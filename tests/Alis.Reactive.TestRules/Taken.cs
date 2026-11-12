using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Alis.Reactive.TestRules;

internal static class Taken
{
    internal const string WaitOnTime = "WaitForTimeoutAsync";
    internal const string PartialMatch = "ToContainTextAsync";

    internal static readonly ImmutableHashSet<string> Gestures = Members(
        "IPage.GotoAsync", "IPage.ReloadAsync", "IPage.GoBackAsync", "IPage.GoForwardAsync",
        "ILocator.ClickAsync", "ILocator.DblClickAsync", "ILocator.FillAsync", "ILocator.ClearAsync", "ILocator.PressAsync",
        "ILocator.PressSequentiallyAsync", "ILocator.SelectOptionAsync", "ILocator.CheckAsync", "ILocator.UncheckAsync",
        "ILocator.SetCheckedAsync", "ILocator.DragToAsync", "ILocator.SetInputFilesAsync", "ILocator.HoverAsync",
        "IKeyboard.PressAsync", "IMouse.MoveAsync");

    internal static readonly ImmutableHashSet<string> Finding = Members(
        "IPage.GetByRole", "IPage.GetByLabel", "IPage.GetByText", "IPage.Locator", "IPage.FrameLocator",
        "ILocator.GetByRole", "ILocator.GetByLabel", "ILocator.GetByText", "ILocator.Locator", "ILocator.FrameLocator",
        "IFrameLocator.GetByRole", "IFrameLocator.GetByLabel", "IFrameLocator.GetByText", "IFrameLocator.Locator", "IFrameLocator.FrameLocator",
        "ILocator.First", "ILocator.Last", "ILocator.Nth", "ILocator.Filter", "ILocator.And", "ILocator.Or", "ILocator.ContentFrame");

    internal static readonly ImmutableHashSet<string> TakingASelector = Members(
        "IPage.Locator", "IPage.FrameLocator", "ILocator.Locator", "ILocator.FrameLocator", "IFrameLocator.Locator", "IFrameLocator.FrameLocator");

    internal static readonly ImmutableHashSet<string> Expecting = Members(
        "Assertions.Expect",
        "ILocatorAssertions.ToHaveTextAsync", "ILocatorAssertions.ToHaveValueAsync", "ILocatorAssertions.ToHaveValuesAsync",
        "ILocatorAssertions.ToBeCheckedAsync", "ILocatorAssertions.ToBeVisibleAsync", "ILocatorAssertions.ToBeHiddenAsync",
        "ILocatorAssertions.ToBeFocusedAsync", "ILocatorAssertions.ToHaveClassAsync", "ILocatorAssertions.ToHaveAttributeAsync",
        "ILocatorAssertions.ToHaveIdAsync", "ILocatorAssertions.ToHaveCountAsync", "ILocatorAssertions.ToBeEmptyAsync", "ILocatorAssertions.Not",
        "IPageAssertions.ToHaveURLAsync", "IPageAssertions.Not");

    internal static readonly ImmutableHashSet<string> BesideThePage = Members(
        "IPage.RunAndWaitForRequestAsync", "IPage.RunAndWaitForDownloadAsync", "IPage.RunAndWaitForPopupAsync",
        "IRequest.Url", "IRequest.Method", "IRequest.PostData", "IRequest.Headers", "IRequest.HeaderValueAsync",
        "IConsoleMessage.Type", "IConsoleMessage.Text", "IDownload.SuggestedFilename");

    internal static readonly ImmutableHashSet<string> ReadBesideThePage = Members(
        "IRequest.Url", "IRequest.Method", "IRequest.PostData", "IRequest.Headers", "IRequest.HeaderValueAsync",
        "IConsoleMessage.Type", "IConsoleMessage.Text", "IDownload.SuggestedFilename");

    internal static readonly ImmutableHashSet<string> Ways = Members("IPage.Keyboard", "IPage.Mouse");

    internal static readonly ImmutableHashSet<string> ByTheHarness = Members(
        "Playwright.CreateAsync", "IPlaywright.Chromium", "IBrowserType.LaunchAsync", "IBrowser.NewContextAsync",
        "IBrowserContext.NewPageAsync", "IBrowserContext.AddCookiesAsync",
        "IBrowser.CloseAsync", "IBrowserContext.CloseAsync", "IPage.CloseAsync",
        "IPage.SetDefaultTimeout", "IPage.SetDefaultNavigationTimeout", "IBrowserContext.SetDefaultTimeout", "IBrowserContext.SetDefaultNavigationTimeout",
        "IPage.Request", "IPage.Console", "IPage.PageError", "IPage.Download", "IPage.Popup", "IPage.Close",
        "IPage.RequestGCAsync", "IPage.Clock",
        "IClock.InstallAsync", "IClock.PauseAtAsync", "IClock.FastForwardAsync", "IClock.RunForAsync", "IClock.ResumeAsync");

    internal static string NameOf(ISymbol member) => member.ContainingType.Name + "." + member.Name;

    private static ImmutableHashSet<string> Members(params string[] names) => ImmutableHashSet.Create(StringComparer.Ordinal, names);
}
