namespace Alis.Reactive
{
    /// <summary>
    /// The browser's <c>File</c>, as an uploader hands it to a page: what the page can read about a file
    /// the person chose. A form-data request sends the file itself.
    /// </summary>
    public sealed class BrowserFile
    {
        /// <summary>The file's name, as the person's computer names it.</summary>
        public string Name { get; set; } = "";

        /// <summary>The file's size in bytes.</summary>
        public long Size { get; set; }

        /// <summary>The file's media type, for example <c>image/png</c>; empty when the browser does not know it.</summary>
        public string Type { get; set; } = "";

        /// <summary>When the file was last changed, in milliseconds since 1970.</summary>
        public long LastModified { get; set; }
    }
}
