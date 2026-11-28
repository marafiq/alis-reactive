using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS011NamespaceIsTheFirstFolder : DiagnosticAnalyzer
{
    public const string Id = "ALIS011";

    private const string NoNamespace = "<global namespace>";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "A file's namespace is its project's root namespace and its first folder",
        "'{0}' is in namespace '{1}'; a file's namespace is its project's root namespace and its first folder, '{2}'",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Rule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var table = SliceTable.Read(start.Options, start.CancellationToken);
        if (table is null || start.Compilation.AssemblyName is not { } root) return;

        start.RegisterSyntaxTreeAction(tree => ReadNamespaces(tree, table, root));
    }

    private static void ReadNamespaces(SyntaxTreeAnalysisContext context, SliceTable table, string root)
    {
        var placement = table.Place(context.Tree);
        if (!placement.IsRead) return;

        var file = table.PathFromTheTable(context.Tree);
        var folder = placement.Folder;
        var expected = folder == SliceTable.Beside ? root : root + "." + folder.Split('/')[0];
        var unit = (CompilationUnitSyntax)context.Tree.GetRoot(context.CancellationToken);

        if (unit.Members.FirstOrDefault(IsType) is { } outside)
            context.ReportDiagnostic(Diagnostic.Create(Rule, NameOf(outside).GetLocation(), file, NoNamespace, expected));

        foreach (var declared in unit.DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<BaseNamespaceDeclarationSyntax>())
        {
            if (!declared.Members.Any(IsType)) continue;
            var found = string.Join(".", declared.AncestorsAndSelf().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(part => Written(part.Name)));
            if (found != expected) context.ReportDiagnostic(Diagnostic.Create(Rule, declared.Name.GetLocation(), file, found, expected));
        }
    }

    private static bool IsType(MemberDeclarationSyntax member) => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax;

    private static SyntaxToken NameOf(MemberDeclarationSyntax type) => type switch
    {
        BaseTypeDeclarationSyntax declared => declared.Identifier,
        DelegateDeclarationSyntax declared => declared.Identifier,
        _ => type.GetFirstToken(),
    };

    private static string Written(NameSyntax name) => name switch
    {
        QualifiedNameSyntax qualified => Written(qualified.Left) + "." + Written(qualified.Right),
        SimpleNameSyntax simple => simple.Identifier.ValueText,
        _ => name.ToString(),
    };
}
