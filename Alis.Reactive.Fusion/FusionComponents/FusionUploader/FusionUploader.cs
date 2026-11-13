using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A file uploader: EJ2's Uploader, which lists the files a person chose, sent with the form and not uploaded on
/// its own.
/// </summary>
/// <remarks>
/// An uploader bound to a file member is an input: a form sends its <see cref="Files"/> and a check reads them.
/// <see cref="FilesChanged"/> is raised once the list holds a change, so a handler of it reads the files listed.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "Uploader")]
public sealed class FusionUploader : FusionComponent, IInputComponent
{
    /// <summary>Gets the files a person chose, as the browser holds them; a file the app listed reads nothing.</summary>
    public ReadOnly<IReadOnlyList<BrowserFile>> Files => throw new NotImplementedException();

    /// <summary>Gets the files the uploader lists.</summary>
    /// <returns>The files, each as the uploader describes it.</returns>
    public LiveObjects<FileInfo> GetFilesData() => ReturnedObjects<FileInfo>();

    /// <summary>Removes every file from the uploader's list.</summary>
    public void ClearAll() => Method();

    /// <summary>Gets the event raised when a person chooses or drops files, before the uploader lists them.</summary>
    public Event<SelectedEventArgs> Selected => Event<SelectedEventArgs>();

    /// <summary>Gets the event raised once the uploader's list holds the files a person chose or removed.</summary>
    public Event<FilesChangedEventArgs> FilesChanged => Event<FilesChangedEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: FilesChanged);

    /// <summary>
    /// A file the uploader lists.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FileInfo")]
    public sealed class FileInfo : JsObject
    {
        /// <summary>Gets the file's name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the file's size, in bytes.</summary>
        public ReadOnly<long> Size => ReadOnly<long>();

        /// <summary>Gets the file's media type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets the file's status in the uploader's list.</summary>
        public ReadOnly<string> Status => ReadOnly<string>();

        /// <summary>Gets the file as the browser holds it, for a file a person chose; a file the app listed reads nothing.</summary>
        public BrowserFile RawFile => LiveObject<BrowserFile>(nullable: true);
    }

    /// <summary>
    /// What a listener of <see cref="Selected"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "SelectedEventArgs")]
    public sealed class SelectedEventArgs : JsObject
    {
        /// <summary>Gets the files the person chose or dropped.</summary>
        public LiveObjects<FileInfo> FilesData => LiveObjects<FileInfo>();

        /// <summary>Gets whether the uploader leaves the files out of its list; write true to leave them out.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="FilesChanged"/> receives.
    /// </summary>
    public sealed class FilesChangedEventArgs : JsObject
    {
        /// <summary>Gets the files the uploader lists after the change.</summary>
        public LiveObjects<FileInfo> FilesData => LiveObjects<FileInfo>();
    }
}
