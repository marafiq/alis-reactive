using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.Fusion.FusionTemplates;

internal static class TemplateReads
{
    internal static string Text(IReadOnlyList<string> path) => Guarded(path, "${alisFusionText(data." + Path(path) + ")}");

    internal static string Url(IReadOnlyList<string> path) => Guarded(path, "${alisFusionUrl(data." + Path(path) + ")}");

    internal static string Guarded(IReadOnlyList<string> path, string content)
        => path.Count == 1 ? content : "${if(" + Holders(path) + ")}" + content + "${/if}";

    internal static string Holders(IReadOnlyList<string> path)
        => string.Join("&&", Enumerable.Range(1, path.Count - 1).Select(count => Path(path.Take(count))));

    internal static string Path(IEnumerable<string> names) => string.Join(".", names);
}
