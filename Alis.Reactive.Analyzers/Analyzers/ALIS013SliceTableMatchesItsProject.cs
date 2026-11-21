using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS013SliceTableMatchesItsProject : DiagnosticAnalyzer
{
    public const string Id = "ALIS013";

    private const string Title = "SliceTree.txt and its project agree";
    private const string Category = "Alis.Reactive.Analyzers";
    private const string Opening = "SliceTree.txt and the package disagree: ";

    private static readonly DiagnosticDescriptor EntryNamesNoFolder = Live("entry '{0}' names no folder");
    private static readonly DiagnosticDescriptor FolderUnderTwoSlices = Live("folder '{0}' falls under '{1}' and '{2}'");
    private static readonly DiagnosticDescriptor SliceInTwoRows = Live("slice '{0}' is in two rows");
    private static readonly DiagnosticDescriptor FolderHasNoRow = Live("folder '{0}' has no row");
    private static readonly DiagnosticDescriptor TypeInTwoSlices = Live("'{0}' is declared in slices '{1}' and '{2}'");

    private static readonly DiagnosticDescriptor NoTable = new(
        Id,
        Title,
        Opening + "'{0}' is built with AlisSliceTree on and holds no SliceTree.txt",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules
        = ImmutableArray.Create(EntryNamesNoFolder, FolderUnderTwoSlices, SliceInTwoRows, FolderHasNoRow, TypeInTwoSlices, NoTable);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static DiagnosticDescriptor Live(string message)
        => new(Id, Title, Opening + message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static void Start(CompilationStartAnalysisContext start)
    {
        var table = SliceTable.Read(start.Options, start.CancellationToken);
        if (table is null)
        {
            if (SliceTable.IsOn(start.Options)) start.RegisterCompilationEndAction(ReportNoTable);
            return;
        }

        start.RegisterAdditionalFileAction(file => ReadEntries(file, table));
        start.RegisterSyntaxTreeAction(tree => ReadFolder(tree, table));
        start.RegisterSymbolAction(symbol => ReadParts(symbol, table), SymbolKind.NamedType);
    }

    private static void ReportNoTable(CompilationAnalysisContext end)
        => end.ReportDiagnostic(Diagnostic.Create(NoTable, Location.None, end.Compilation.AssemblyName));

    private static void ReadEntries(AdditionalFileAnalysisContext context, SliceTable table)
    {
        if (!ReferenceEquals(context.AdditionalFile, table.File)) return;

        var rowOfSlice = new Dictionary<string, int>(StringComparer.Ordinal);
        var named = new HashSet<SliceEntry>();
        var contested = new HashSet<string>(StringComparer.Ordinal);
        var inTwoRows = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var placement = table.Place(tree);
            if (!placement.IsRead) continue;

            foreach (var entry in table.Entries)
            {
                if (table.SliceOf(entry, tree) is not { } slice) continue;
                named.Add(entry);
                if (!rowOfSlice.TryGetValue(slice, out var row)) rowOfSlice.Add(slice, entry.Row);
                else if (row != entry.Row && inTwoRows.Add(slice))
                    context.ReportDiagnostic(Diagnostic.Create(SliceInTwoRows, table.LocationOf(entry), slice));
            }

            if (placement.First is { } first && placement.Second is { } second && contested.Add(placement.Folder))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    FolderUnderTwoSlices, table.LocationOf(second), placement.Folder, table.SliceOf(first, tree), table.SliceOf(second, tree)));
            }
        }

        foreach (var entry in table.Entries.Where(entry => !named.Contains(entry)))
            context.ReportDiagnostic(Diagnostic.Create(EntryNamesNoFolder, table.LocationOf(entry), entry.Text));
    }

    private static void ReadFolder(SyntaxTreeAnalysisContext context, SliceTable table)
    {
        var placement = table.Place(context.Tree);
        if (!placement.IsRead || placement.Slice is not null || placement.First is not null) return;

        var first = context.Tree.GetRoot(context.CancellationToken).GetFirstToken(includeZeroWidth: true);
        context.ReportDiagnostic(Diagnostic.Create(FolderHasNoRow, first.GetLocation(), placement.Folder));
    }

    private static void ReadParts(SymbolAnalysisContext context, SliceTable table)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.IsImplicitlyDeclared || type.DeclaringSyntaxReferences.Length < 2) return;

        Location? at = null;
        var slices = new List<string>();
        foreach (var location in type.Locations.OrderBy(location => location.SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(location => location.SourceSpan.Start))
        {
            if (location.SourceTree is not { } tree || table.Place(tree).Slice is not { } slice) continue;
            at ??= location;
            if (!slices.Contains(slice.Name, StringComparer.Ordinal)) slices.Add(slice.Name);
        }

        if (at is not null && slices.Count > 1)
            context.ReportDiagnostic(Diagnostic.Create(TypeInTwoSlices, at, type.ToDisplayString(), slices[0], slices[1]));
    }
}
