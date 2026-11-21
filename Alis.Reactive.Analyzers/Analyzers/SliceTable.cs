using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Alis.Reactive.Analyzers.Analyzers;

internal sealed class Slice
{
    internal Slice(string name, int row)
    {
        Name = name;
        Row = row;
    }

    internal string Name { get; }

    internal int Row { get; }
}

internal sealed class SliceEntry
{
    internal SliceEntry(string text, int row, TextSpan span)
    {
        Text = text;
        Row = row;
        Span = span;
    }

    internal string Text { get; }

    internal int Row { get; }

    internal TextSpan Span { get; }
}

internal readonly struct Placement
{
    private Placement(bool isRead, string folder, Slice? slice, SliceEntry? first, SliceEntry? second)
    {
        IsRead = isRead;
        Folder = folder;
        Slice = slice;
        First = first;
        Second = second;
    }

    internal bool IsRead { get; }

    internal string Folder { get; }

    internal Slice? Slice { get; }

    internal SliceEntry? First { get; }

    internal SliceEntry? Second { get; }

    internal static Placement NotRead => new(false, "", null, null, null);

    internal static Placement In(string folder, Slice slice, SliceEntry entry) => new(true, folder, slice, entry, null);

    internal static Placement Rowless(string folder) => new(true, folder, null, null, null);

    internal static Placement Contested(string folder, SliceEntry first, SliceEntry second) => new(true, folder, null, first, second);
}

internal sealed class SliceTable
{
    internal const string FileName = "SliceTree.txt";
    internal const string Beside = ".";

    private const string BuildProperty = "build_property.AlisSliceTree";
    private const string EachFolder = "*";
    private const string EachFolderUnder = "/*";

    private readonly string _folder;

    private SliceTable(AdditionalText file, SourceText text, string folder, ImmutableArray<SliceEntry> entries)
    {
        File = file;
        Text = text;
        _folder = folder;
        Entries = entries;
    }

    internal AdditionalText File { get; }

    internal SourceText Text { get; }

    internal ImmutableArray<SliceEntry> Entries { get; }

    internal static bool IsOn(AnalyzerOptions options)
        => options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(BuildProperty, out var value)
           && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    internal static SliceTable? Read(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        AdditionalText? file = null;
        foreach (var candidate in options.AdditionalFiles)
        {
            if (!IsTable(candidate.Path)) continue;
            if (file is null || string.CompareOrdinal(candidate.Path, file.Path) < 0) file = candidate;
        }

        var text = file?.GetText(cancellationToken);
        if (file is null || text is null) return null;

        var entries = ImmutableArray.CreateBuilder<SliceEntry>();
        var row = 0;
        foreach (var line in text.Lines)
        {
            var written = line.ToString();
            var rowHasEntry = false;
            var at = 0;
            while (at < written.Length)
            {
                if (char.IsWhiteSpace(written[at]))
                {
                    at++;
                    continue;
                }

                var start = at;
                while (at < written.Length && !char.IsWhiteSpace(written[at])) at++;
                if (!rowHasEntry)
                {
                    rowHasEntry = true;
                    row++;
                }

                entries.Add(new SliceEntry(written.Substring(start, at - start), row, new TextSpan(line.Start + start, at - start)));
            }
        }

        var path = Normal(file.Path);
        return new SliceTable(file, text, path.Substring(0, path.LastIndexOf('/') + 1), entries.ToImmutable());
    }

    internal Location LocationOf(SliceEntry entry) => Location.Create(File.Path, entry.Span, Text.Lines.GetLinePositionSpan(entry.Span));

    internal string PathFromTheTable(SyntaxTree tree)
    {
        var path = Normal(tree.FilePath);
        return path.StartsWith(_folder, StringComparison.Ordinal) ? path.Substring(_folder.Length) : path;
    }

    internal Placement Place(SyntaxTree tree)
    {
        var path = Normal(tree.FilePath);
        if (!path.StartsWith(_folder, StringComparison.Ordinal)) return Placement.NotRead;

        var last = path.LastIndexOf('/');
        var folder = last < _folder.Length ? "" : path.Substring(_folder.Length, last - _folder.Length);
        var first = folder.Length == 0 ? "" : FirstFolder(folder);
        if (first is "bin" or "obj") return Placement.NotRead;

        SliceEntry? found = null;
        string? slice = null;
        foreach (var entry in Entries)
        {
            var named = SliceOf(entry, folder, first);
            if (named is null) continue;
            if (found is null)
            {
                found = entry;
                slice = named;
            }
            else if (!string.Equals(named, slice, StringComparison.Ordinal))
            {
                return Placement.Contested(Display(folder), found, entry);
            }
        }

        return found is null ? Placement.Rowless(Display(folder)) : Placement.In(Display(folder), new Slice(slice!, found.Row), found);
    }

    internal string? SliceOf(SliceEntry entry, SyntaxTree tree)
    {
        var path = Normal(tree.FilePath);
        if (!path.StartsWith(_folder, StringComparison.Ordinal)) return null;
        var last = path.LastIndexOf('/');
        var folder = last < _folder.Length ? "" : path.Substring(_folder.Length, last - _folder.Length);
        var first = folder.Length == 0 ? "" : FirstFolder(folder);
        return first is "bin" or "obj" ? null : SliceOf(entry, folder, first);
    }

    private static string? SliceOf(SliceEntry entry, string folder, string first)
    {
        var written = entry.Text;
        if (folder.Length == 0) return written == Beside ? Beside : null;
        if (written == Beside) return null;
        if (written == EachFolder) return first;
        if (!written.EndsWith(EachFolderUnder, StringComparison.Ordinal)) return string.Equals(written, folder, StringComparison.Ordinal) ? written : null;

        var parent = written.Substring(0, written.Length - EachFolder.Length);
        if (!folder.StartsWith(parent, StringComparison.Ordinal)) return null;
        var next = folder.IndexOf('/', parent.Length);
        return next < 0 ? folder : folder.Substring(0, next);
    }

    private static string FirstFolder(string folder)
    {
        var end = folder.IndexOf('/');
        return end < 0 ? folder : folder.Substring(0, end);
    }

    private static string Display(string folder) => folder.Length == 0 ? Beside : folder;

    private static bool IsTable(string path)
    {
        var normal = Normal(path);
        return string.Equals(normal.Substring(normal.LastIndexOf('/') + 1), FileName, StringComparison.Ordinal);
    }

    private static string Normal(string path) => path.Replace('\\', '/');
}
