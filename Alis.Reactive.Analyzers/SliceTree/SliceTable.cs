using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Alis.Reactive.Analyzers.SliceTree
{
    internal sealed class SliceTable
    {
        private SliceTable(ImmutableDictionary<string, Slice> sliceOfFolder, ImmutableArray<Problem> problems)
        {
            SliceOfFolder = sliceOfFolder;
            Problems = problems;
        }

        public ImmutableDictionary<string, Slice> SliceOfFolder { get; }

        public ImmutableArray<Problem> Problems { get; }

        public static SliceTable Build(SourceText text, IReadOnlyCollection<string> folders)
        {
            var sliceOfFolder = new Dictionary<string, Slice>(StringComparer.Ordinal);
            var rowOfSlice = new Dictionary<string, int>(StringComparer.Ordinal);
            var problems = ImmutableArray.CreateBuilder<Problem>();

            foreach (var entry in Entries(text))
            {
                var matched = false;
                foreach (var (folder, sliceName) in Expand(entry.Text, folders))
                {
                    matched = true;
                    if (sliceOfFolder.TryGetValue(folder, out var other) && other.Name != sliceName)
                    {
                        problems.Add(new Problem($"folder '{Display(folder)}' falls under '{other.Name}' and '{sliceName}'", entry.Span));
                        continue;
                    }
                    if (rowOfSlice.TryGetValue(sliceName, out var otherRow) && otherRow != entry.Row)
                    {
                        problems.Add(new Problem($"slice '{sliceName}' is in two rows", entry.Span));
                        continue;
                    }
                    rowOfSlice[sliceName] = entry.Row;
                    sliceOfFolder[folder] = new Slice(sliceName, entry.Row);
                }
                if (!matched) problems.Add(new Problem($"entry '{entry.Text}' names no folder", entry.Span));
            }

            return new SliceTable(sliceOfFolder.ToImmutableDictionary(StringComparer.Ordinal), problems.ToImmutable());
        }

        public static string Display(string folder) => folder.Length == 0 ? "." : folder;

        private static IEnumerable<Entry> Entries(SourceText text)
        {
            var row = 0;
            foreach (var line in text.Lines)
            {
                var content = line.ToString();
                var any = false;
                var i = 0;
                while (i < content.Length)
                {
                    while (i < content.Length && char.IsWhiteSpace(content[i])) i++;
                    if (i == content.Length) break;
                    var start = i;
                    while (i < content.Length && !char.IsWhiteSpace(content[i])) i++;
                    any = true;
                    yield return new Entry(content.Substring(start, i - start), row, new TextSpan(line.Start + start, i - start));
                }
                if (any) row++;
            }
        }

        private static IEnumerable<(string Folder, string Slice)> Expand(string entry, IEnumerable<string> folders)
        {
            foreach (var folder in folders)
            {
                if (entry == ".")
                {
                    if (folder.Length == 0) yield return (folder, ".");
                }
                else if (entry == "*")
                {
                    if (folder.Length > 0) yield return (folder, folder.Split('/')[0]);
                }
                else if (entry.EndsWith("/*", StringComparison.Ordinal))
                {
                    var parent = entry.Substring(0, entry.Length - 2) + "/";
                    if (folder.StartsWith(parent, StringComparison.Ordinal))
                        yield return (folder, parent + folder.Substring(parent.Length).Split('/')[0]);
                }
                else if (folder == entry)
                {
                    yield return (folder, entry);
                }
            }
        }

        private sealed class Entry(string text, int row, TextSpan span)
        {
            public string Text { get; } = text;
            public int Row { get; } = row;
            public TextSpan Span { get; } = span;
        }
    }

    internal sealed class Slice(string name, int row)
    {
        public string Name { get; } = name;
        public int Row { get; } = row;
    }

    internal sealed class Problem(string message, TextSpan span)
    {
        public string Message { get; } = message;
        public TextSpan Span { get; } = span;
    }
}
