# FusionAiAssistView MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `AIAssistView`
MVC builder: `AIAssistViewBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 37 |
| JS members with matching builder method | 32 |
| JS members without matching builder method | 4 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActiveView` | `System.Int32` |
| `AttachmentRemoved` | `System.String` |
| `AttachmentSettings` | `Syncfusion.EJ2.InteractiveChat.AIAssistViewAttachmentSettings` |
| `AttachmentUploadFailure` | `System.String` |
| `AttachmentUploadSuccess` | `System.String` |
| `BannerTemplate` | `System.String` |
| `BeforeAttachmentUpload` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `EnableAttachments` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FooterTemplate` | `System.String` |
| `FooterToolbarSettings` | `Syncfusion.EJ2.InteractiveChat.AIAssistViewFooterToolbarSettings` |
| `Height` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Locale` | `System.String` |
| `Prompt` | `System.String` |
| `PromptChanged` | `System.String` |
| `PromptIconCss` | `System.String` |
| `PromptItemTemplate` | `System.String` |
| `PromptPlaceholder` | `System.String` |
| `PromptRequest` | `System.String` |
| `Prompts` | `System.Object` |
| `PromptSuggestionItemTemplate` | `System.String` |
| `PromptSuggestions` | `System.String[]` |
| `PromptSuggestionsHeader` | `System.String` |
| `PromptToolbarSettings` | `Syncfusion.EJ2.InteractiveChat.AIAssistViewPromptToolbarSettings` |
| `ResponseIconCss` | `System.String` |
| `ResponseItemTemplate` | `System.String` |
| `ResponseToolbarSettings` | `Syncfusion.EJ2.InteractiveChat.AIAssistViewResponseToolbarSettings` |
| `ShowClearButton` | `System.Boolean` |
| `ShowHeader` | `System.Boolean` |
| `StopRespondingClick` | `System.String` |
| `ToolbarSettings` | `Syncfusion.EJ2.InteractiveChat.AIAssistViewToolbarSettings` |
| `Views` | `System.Collections.Generic.List{Syncfusion.EJ2.InteractiveChat.AIAssistViewView}` |
| `Width` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `activeView` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `addPromptResponse` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `attachmentRemoved` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `attachmentSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `attachmentUploadFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `attachmentUploadSuccess` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `bannerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeAttachmentUpload` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `enableAttachments` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `executePrompt` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `footerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `footerToolbarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `prompt` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `promptIconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptItemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptRequest` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `prompts` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptSuggestionItemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptSuggestions` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptSuggestionsHeader` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `promptToolbarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `responseIconCss` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `responseItemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `responseToolbarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `scrollToBottom` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showHeader` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `stopRespondingClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `toolbarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `views` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
