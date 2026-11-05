#if NET48
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Alis.Reactive
{
    /// <summary>
    /// Versioned URLs for the files an MVC 5 app serves from its own folders, as <c>asp-append-version</c>
    /// writes them on .NET: the packages' browser files under <c>Content\{PackageId}\</c> and the app's own.
    /// </summary>
    public static class UrlVersionExtensions
    {
        private static readonly ConcurrentDictionary<string, (DateTime Written, string Version)> Versions =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The URL of the file at <paramref name="contentPath"/> with the file's content hash as <c>v</c>. The URL a
        /// browser caches changes exactly when the file does, so the first page after a package upgrade fetches the
        /// package's new files, and an unchanged file stays cached.
        /// </summary>
        /// <remarks>
        /// <code>
        /// &lt;script src="@Url.AppendVersion("~/Content/AlisReactive/js/alis-reactive.js")"&gt;&lt;/script&gt;
        /// </code>
        /// The hash is the file's SHA-256 in URL-safe base64, as ASP.NET Core writes it, read once per change of the
        /// file's last write time.
        /// </remarks>
        /// <param name="url">The view's URL helper.</param>
        /// <param name="contentPath">The file's app-relative path, starting with <c>~/</c>.</param>
        /// <exception cref="InvalidOperationException">No file is at <paramref name="contentPath"/>.</exception>
        public static string AppendVersion(this UrlHelper url, string contentPath)
        {
            var physicalPath = HostingEnvironment.MapPath(contentPath);
            if (physicalPath == null || !File.Exists(physicalPath))
            {
                throw new InvalidOperationException(
                    $"No file is at {contentPath} ({physicalPath}). A package's build copies its browser files into " +
                    "Content\\{PackageId}\\; build the app before it serves a page.");
            }

            var written = File.GetLastWriteTimeUtc(physicalPath);
            var version = Versions.AddOrUpdate(
                physicalPath,
                _ => (written, Hash(physicalPath)),
                (_, known) => known.Written == written ? known : (written, Hash(physicalPath))).Version;
            return url.Content(contentPath) + "?v=" + version;
        }

        private static string Hash(string physicalPath)
        {
            using (var sha = SHA256.Create())
            using (var file = File.OpenRead(physicalPath))
                return Convert.ToBase64String(sha.ComputeHash(file)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
#endif
