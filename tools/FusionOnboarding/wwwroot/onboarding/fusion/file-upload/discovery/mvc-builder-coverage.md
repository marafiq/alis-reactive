# FusionFileUpload MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Uploader`
MVC builder: `UploaderBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 41 |
| JS members with matching builder method | 37 |
| JS members without matching builder method | 15 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionComplete` | `System.String` |
| `AllowedExtensions` | `System.String` |
| `AsyncSettings` | `Syncfusion.EJ2.Inputs.UploaderAsyncSettings` |
| `AutoUpload` | `System.Boolean` |
| `BeforeRemove` | `System.String` |
| `BeforeUpload` | `System.String` |
| `Buttons` | `Syncfusion.EJ2.Inputs.UploaderButtonsProps` |
| `Canceling` | `System.String` |
| `Change` | `System.String` |
| `ChunkFailure` | `System.String` |
| `ChunkSuccess` | `System.String` |
| `ChunkUploading` | `System.String` |
| `Clearing` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DirectoryUpload` | `System.Boolean` |
| `DropArea` | `System.String` |
| `DropEffect` | `Syncfusion.EJ2.Inputs.DropEffect` |
| `Enabled` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Failure` | `System.String` |
| `FileListRendering` | `System.String` |
| `Files` | `System.Collections.Generic.List{Syncfusion.EJ2.Inputs.UploaderUploadedFiles}` |
| `HtmlAttributes` | `System.Object` |
| `Locale` | `System.String` |
| `MaxFileSize` | `System.Double` |
| `MinFileSize` | `System.Double` |
| `Multiple` | `System.Boolean` |
| `Pausing` | `System.String` |
| `Progress` | `System.String` |
| `Removing` | `System.String` |
| `Rendering` | `System.String` |
| `Resuming` | `System.String` |
| `Selected` | `System.String` |
| `SequentialUpload` | `System.Boolean` |
| `ShowFileList` | `System.Boolean` |
| `Success` | `System.String` |
| `Template` | `System.String` |
| `Uploading` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `allowedExtensions` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `asyncSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autoUpload` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeRemove` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeUpload` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `buttons` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `bytesToSize` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cancel` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `canceling` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `chunkFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `chunkSuccess` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `chunkUploading` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `clearAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `createFileList` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `directoryUpload` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dropArea` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dropEffect` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `failure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fileList` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `fileListRendering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `files` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filesData` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `getFilesData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `maxFileSize` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minFileSize` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `multiple` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pause` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pausing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `progress` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `remove` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rendering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resume` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resuming` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `retry` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `sequentialUpload` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showFileList` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `sortFileList` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `success` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `template` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `traverseFileTree` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `upload` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `uploading` | event | yes | candidate: typed event; payload and browser gesture proof required |
