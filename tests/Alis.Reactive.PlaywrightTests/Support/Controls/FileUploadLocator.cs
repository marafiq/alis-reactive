using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class FileUploadLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal FileUploadLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    internal ILocator Input => _page.Locator($"#{_componentId}");
    internal ILocator Wrapper => Input.Locator("xpath=ancestor::*[contains(@class,'e-upload')][1]");

    internal async Task AttachFiles(params FilePayload[] files)
    {
        await Input.SetInputFilesAsync(files);
    }
}
