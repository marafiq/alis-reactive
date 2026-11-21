using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS022MemberPresence : DiagnosticAnalyzer
{
    public const string Id = "ALIS022";

    private static readonly DiagnosticDescriptor NeedsOptional = new(
        Id,
        "A declaration follows its vendor member presence",
        "Declaration member '{0}' names optional vendor member '{1}'; declare optional: true",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NeedsRequired = new(
        Id,
        "A declaration follows its vendor member presence",
        "Declaration member '{0}' names required vendor member '{1}'; remove optional: true",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NeedsOptional, NeedsRequired);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start => RootMemberDoor.Register(start, Read));
    }

    private static void Read(OperationAnalysisContext context, RootMemberDoor door)
    {
        if (door.IsEvent) return;
        if (door.Vendor is not { } member) return;
        var optional = door.Flag("optional");
        if (optional.Value is not { } declared || declared == member.Optional) return;
        context.ReportDiagnostic(Diagnostic.Create(member.Optional ? NeedsOptional : NeedsRequired, optional.Location, door.Member, door.Name));
    }
}
