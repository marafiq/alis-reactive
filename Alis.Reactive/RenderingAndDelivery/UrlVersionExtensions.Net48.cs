#if NET48
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The address of a file an MVC 5 app serves from its own folders, with the file's content hash, as
/// <c>asp-append-version</c> writes it on ASP.NET Core: the packages' browser files under <c>Content\{PackageId}\</c>
/// and the app's own.
/// </summary>
public static class UrlVersionExtensions
{
    private static readonly ConcurrentDictionary<string, (DateTime Written, string Version)> Versions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Writes the address of the file at <paramref name="appRelativeUrl"/> with the file's content hash as <c>v</c>.</summary>
    /// <remarks>
    /// <c>&lt;script src="@Url.AppendVersion("~/Content/AlisReactive/js/alis-reactive.js")"&gt;&lt;/script&gt;</c>.
    /// The address changes exactly when the file's bytes do, so the first page after a package upgrade fetches the
    /// package's new files and an unchanged file stays cached. The hash is the file's SHA-256 in URL-safe base64, as
    /// ASP.NET Core writes it, read again only when the file's last write time changes.
    /// </remarks>
    /// <param name="url">The view's URL helper.</param>
    /// <param name="appRelativeUrl">The file's address in the app, starting with <c>~/</c>.</param>
    /// <returns>The file's address with its content hash.</returns>
    /// <exception cref="InvalidOperationException">No file is at <paramref name="appRelativeUrl"/>: build the app before it serves a page.</exception>
    public static string AppendVersion(this UrlHelper url, string appRelativeUrl)
    {
        var physicalPath = HostingEnvironment.MapPath(appRelativeUrl);
        if (physicalPath is null || !File.Exists(physicalPath))
        {
            throw new InvalidOperationException(
                $"No file is at {appRelativeUrl} ({physicalPath}). A package's build copies its browser files into " +
                "Content\\{PackageId}\\; build the app before it serves a page.");
        }

        var written = File.GetLastWriteTimeUtc(physicalPath);
        var version = Versions.AddOrUpdate(
            physicalPath,
            path => (written, HashOf(path)),
            (path, known) => known.Written == written ? known : (written, HashOf(path))).Version;
        return url.Content(appRelativeUrl) + "?v=" + version;
    }

    private static string HashOf(string physicalPath)
    {
        using var sha = SHA256.Create();
        using var file = File.OpenRead(physicalPath);
        return Convert.ToBase64String(sha.ComputeHash(file)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
#endif
