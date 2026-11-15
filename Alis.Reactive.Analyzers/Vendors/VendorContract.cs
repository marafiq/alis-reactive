using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

internal sealed class VendorContract
{
    private readonly ImmutableDictionary<(string Module, string Name), int> _types;

    private VendorContract(ImmutableDictionary<(string Module, string Name), int> types) => _types = types;

    internal int Count(string module, string name) => _types.TryGetValue((module, name), out var count) ? count : 0;

    internal static bool IsTable(string path)
    {
        var normal = path.Replace('\\', '/');
        return string.Equals(normal.Substring(normal.LastIndexOf('/') + 1), "vendor-contract.tsv", StringComparison.Ordinal);
    }

    internal static VendorContract? Read(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var types = ImmutableDictionary.CreateBuilder<(string Module, string Name), int>();
        var read = false;
        foreach (var file in options.AdditionalFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsTable(file.Path)) continue;
            var text = file.GetText(cancellationToken);
            if (text is null || text.Lines[0].ToString() != "format\t1") continue;
            read = true;
            foreach (var line in text.Lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var written = line.ToString();
                if (!written.StartsWith("type\t", StringComparison.Ordinal)) continue;
                var columns = written.Split('\t');
                if (columns.Length != 6 || columns[4] is not ("class" or "interface" or "alias" or "function" or "enum")) continue;
                var key = (Unescape(columns[2]), Unescape(columns[3]));
                types.TryGetValue(key, out var count);
                types[key] = count + 1;
            }
        }

        return read ? new VendorContract(types.ToImmutable()) : null;
    }

    private static string Unescape(string text)
    {
        if (text.IndexOf('\\') < 0) return text;
        var result = new StringBuilder(text.Length);
        for (var at = 0; at < text.Length; at++)
        {
            var character = text[at];
            if (character == '\\' && at + 1 < text.Length)
            {
                character = text[++at] switch
                {
                    't' => '\t',
                    'r' => '\r',
                    'n' => '\n',
                    var escaped => escaped,
                };
            }

            result.Append(character);
        }

        return result.ToString();
    }
}
