using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Alis.Reactive.Analyzers.SliceTree
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SliceTreeAnalyzer : DiagnosticAnalyzer
    {
        public const string EdgeId = "ALIS010";
        public const string NamespaceId = "ALIS011";
        public const string SealedId = "ALIS012";
        public const string TableId = "ALIS013";
        public const string TableFileName = "SliceTree.txt";

        private static readonly DiagnosticDescriptor Edge = new DiagnosticDescriptor(
            EdgeId,
            title: "A slice uses only slices below it",
            messageFormat: "'{0}' uses '{1}' of slice '{2}', {3} in SliceTree.txt; a slice uses only slices in rows below it",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ADR-0004: a package is an inverted tree of concept slices, one per folder, each using only the slices in rows below it in the package's SliceTree.txt.");

        private static readonly DiagnosticDescriptor Namespace = new DiagnosticDescriptor(
            NamespaceId,
            title: "A file sits in its namespace",
            messageFormat: "'{0}' is in namespace '{1}'; expected '{2}': a file that declares a public type has the package namespace, any other file's namespace is its folder",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ADR-0004: every public type of a package is in the package's one namespace; an internal-only file's namespace is its folder.");

        private static readonly DiagnosticDescriptor Sealed = new DiagnosticDescriptor(
            SealedId,
            title: "A public class nothing derives from is sealed",
            messageFormat: "'{0}' is public and nothing in the package derives from it; seal it",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ADR-0004: a public class that no type in its package derives from is sealed; the open classes are the bases with a subclass beside them.");

        private static readonly DiagnosticDescriptor Table = new DiagnosticDescriptor(
            TableId,
            title: "The slice table and the package agree",
            messageFormat: "SliceTree.txt and the package disagree: {0}",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ADR-0004: each shipped package carries a SliceTree.txt whose rows name every folder once, and every entry names a folder.");

        private static readonly DiagnosticDescriptor MissingTable = new DiagnosticDescriptor(
            TableId,
            title: "The slice table and the package agree",
            messageFormat: "SliceTree.txt and the package disagree: {0}",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ADR-0004: each shipped package carries a SliceTree.txt whose rows name every folder once, and every entry names a folder.",
            customTags: WellKnownDiagnosticTags.CompilationEnd);

        private static readonly ImmutableArray<OperationKind> OperationKinds =
            Enum.GetValues(typeof(OperationKind))
                .Cast<OperationKind>()
                .Where(kind => kind != OperationKind.None && kind != OperationKind.Attribute)
                .Distinct()
                .ToImmutableArray();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Edge, Namespace, Sealed, Table, MissingTable);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(Start);
        }

        private static void Start(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;
            var assemblyName = compilation.AssemblyName ?? "";
            var table = context.Options.AdditionalFiles.FirstOrDefault(file => FileName(file.Path) == TableFileName);
            var text = table?.GetText(context.CancellationToken);
            if (table == null || text == null)
            {
                if (AnalyzerHelpers.ShippedAssemblies.Contains(assemblyName))
                    context.RegisterCompilationEndAction(end => end.ReportDiagnostic(
                        Diagnostic.Create(MissingTable, Location.None, $"'{assemblyName}' ships without a {TableFileName}")));
                return;
            }

            var root = DirectoryOf(table.Path);
            var folderOfPath = compilation.SyntaxTrees
                .Select(tree => (tree.FilePath, Folder: FolderOf(root, tree.FilePath)))
                .Where(file => file.Folder != null)
                .GroupBy(file => file.FilePath, StringComparer.Ordinal)
                .ToImmutableDictionary(group => group.Key, group => group.First().Folder!, StringComparer.Ordinal);
            var sliceTable = SliceTable.Build(text, folderOfPath.Values.Distinct(StringComparer.Ordinal).ToList());
            var run = new Run(compilation, assemblyName, root, table.Path, text, sliceTable, folderOfPath);

            context.RegisterAdditionalFileAction(run.ReportTableProblems);
            context.RegisterSyntaxTreeAction(run.AnalyzeTree);
            context.RegisterSymbolAction(run.AnalyzeNamedType, SymbolKind.NamedType);
            context.RegisterSymbolAction(run.AnalyzeMember, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event);
            context.RegisterOperationAction(run.AnalyzeOperation, OperationKinds);
        }

        private static string FileName(string path)
        {
            var normalized = path.Replace('\\', '/');
            return normalized.Substring(normalized.LastIndexOf('/') + 1);
        }

        private static string DirectoryOf(string path)
        {
            var normalized = path.Replace('\\', '/');
            var slash = normalized.LastIndexOf('/');
            return slash < 0 ? "" : normalized.Substring(0, slash + 1);
        }

        private static string? FolderOf(string root, string filePath)
        {
            var path = filePath.Replace('\\', '/');
            if (!path.StartsWith(root, StringComparison.Ordinal)) return null;
            var relative = path.Substring(root.Length);
            if (relative.StartsWith("bin/", StringComparison.Ordinal) || relative.StartsWith("obj/", StringComparison.Ordinal)) return null;
            var slash = relative.LastIndexOf('/');
            return slash < 0 ? "" : relative.Substring(0, slash);
        }

        private sealed class Run
        {
            private readonly Compilation compilation;
            private readonly string assemblyName;
            private readonly string root;
            private readonly string tablePath;
            private readonly SourceText tableText;
            private readonly SliceTable table;
            private readonly ImmutableDictionary<string, string> folderOfPath;
            private readonly ImmutableDictionary<string, Slice> sliceOfPath;
            private readonly Lazy<ImmutableHashSet<INamedTypeSymbol>> bases;

            public Run(Compilation compilation, string assemblyName, string root, string tablePath, SourceText tableText,
                SliceTable table, ImmutableDictionary<string, string> folderOfPath)
            {
                this.compilation = compilation;
                this.assemblyName = assemblyName;
                this.root = root;
                this.tablePath = tablePath;
                this.tableText = tableText;
                this.table = table;
                this.folderOfPath = folderOfPath;
                sliceOfPath = folderOfPath
                    .Where(file => table.SliceOfFolder.ContainsKey(file.Value))
                    .ToImmutableDictionary(file => file.Key, file => table.SliceOfFolder[file.Value], StringComparer.Ordinal);
                bases = new Lazy<ImmutableHashSet<INamedTypeSymbol>>(BaseTypes);
            }

            public void ReportTableProblems(AdditionalFileAnalysisContext context)
            {
                if (context.AdditionalFile.Path != tablePath) return;
                foreach (var problem in table.Problems)
                {
                    var location = Location.Create(tablePath, problem.Span, tableText.Lines.GetLinePositionSpan(problem.Span));
                    context.ReportDiagnostic(Diagnostic.Create(Table, location, problem.Message));
                }
            }

            public void AnalyzeTree(SyntaxTreeAnalysisContext context)
            {
                var path = context.Tree.FilePath;
                if (!folderOfPath.TryGetValue(path, out var folder)) return;
                var unit = context.Tree.GetRoot(context.CancellationToken);

                var namespaces = unit
                    .DescendantNodes(node => node is CompilationUnitSyntax || node is BaseNamespaceDeclarationSyntax)
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .ToList();
                var declaresPublic = namespaces.SelectMany(ns => ns.Members).Any(DeclaresPublicType);
                var expected = declaresPublic || folder.Length == 0 ? assemblyName : assemblyName + "." + folder.Replace('/', '.');
                var relative = path.Replace('\\', '/').Substring(root.Length);
                foreach (var ns in namespaces)
                {
                    var name = ns.Name.ToString();
                    if (name != expected)
                        context.ReportDiagnostic(Diagnostic.Create(Namespace, ns.Name.GetLocation(), relative, name, expected));
                }
                if (namespaces.Count == 0 && unit is CompilationUnitSyntax compilationUnit && compilationUnit.Members.Any(DeclaresType))
                {
                    var first = compilationUnit.Members.First(DeclaresType);
                    context.ReportDiagnostic(Diagnostic.Create(Namespace, Identifier(first).GetLocation(), relative, "the global namespace", expected));
                }

                if (!sliceOfPath.ContainsKey(path))
                    context.ReportDiagnostic(Diagnostic.Create(Table, unit.GetFirstToken().GetLocation(), $"folder '{SliceTable.Display(folder)}' has no row"));
            }

            public void AnalyzeNamedType(SymbolAnalysisContext context)
            {
                var type = (INamedTypeSymbol)context.Symbol;
                if (type.IsImplicitlyDeclared || type.DeclaringSyntaxReferences.IsEmpty || type.Locations.IsEmpty) return;
                var path = type.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;
                if (!folderOfPath.ContainsKey(path)) return;
                var at = type.Locations[0];

                var slices = type.DeclaringSyntaxReferences
                    .Select(part => sliceOfPath.TryGetValue(part.SyntaxTree.FilePath, out var slice) ? slice.Name : null)
                    .Where(name => name != null)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                if (slices.Count > 1)
                    context.ReportDiagnostic(Diagnostic.Create(Table, at, $"'{type.ToDisplayString()}' is declared in slices '{slices[0]}' and '{slices[1]}'"));

                var site = new Site(this, context.ReportDiagnostic, path, at);
                site.Type(type.BaseType);
                foreach (var implemented in type.Interfaces) site.Type(implemented);
                Constraints(site, type.TypeParameters);
                Attributes(site, type.GetAttributes(), context.CancellationToken);
                if (type.DelegateInvokeMethod is { } invoke)
                {
                    site.Type(invoke.ReturnType);
                    Parameters(site, invoke.Parameters, context.CancellationToken);
                }

                if (type.TypeKind == TypeKind.Class && type.DeclaredAccessibility == Accessibility.Public
                    && !type.IsSealed && !type.IsAbstract && !type.IsStatic && !bases.Value.Contains(type))
                    context.ReportDiagnostic(Diagnostic.Create(Sealed, at, type.ToDisplayString()));
            }

            public void AnalyzeMember(SymbolAnalysisContext context)
            {
                var symbol = context.Symbol;
                if (symbol.IsImplicitlyDeclared || symbol.DeclaringSyntaxReferences.IsEmpty || symbol.Locations.IsEmpty) return;
                if (symbol is IMethodSymbol { AssociatedSymbol: not null }) return;
                var path = symbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;
                var site = new Site(this, context.ReportDiagnostic, path, symbol.Locations[0]);
                Attributes(site, symbol.GetAttributes(), context.CancellationToken);
                switch (symbol)
                {
                    case IMethodSymbol method:
                        site.Type(method.ReturnType);
                        Parameters(site, method.Parameters, context.CancellationToken);
                        Constraints(site, method.TypeParameters);
                        Attributes(site, method.GetReturnTypeAttributes(), context.CancellationToken);
                        break;
                    case IPropertySymbol property:
                        site.Type(property.Type);
                        Parameters(site, property.Parameters, context.CancellationToken);
                        break;
                    case IFieldSymbol field:
                        site.Type(field.Type);
                        break;
                    case IEventSymbol @event:
                        site.Type(@event.Type);
                        break;
                }
            }

            public void AnalyzeOperation(OperationAnalysisContext context)
            {
                var operation = context.Operation;
                var site = new Site(this, context.ReportDiagnostic, operation.Syntax.SyntaxTree.FilePath, operation.Syntax.GetLocation());
                site.Type(operation.Type);
                switch (operation)
                {
                    case IInvocationOperation invocation: site.Member(invocation.TargetMethod); break;
                    case IObjectCreationOperation creation: site.Member(creation.Constructor); break;
                    case IMemberReferenceOperation reference: site.Member(reference.Member); break;
                    case IConversionOperation conversion: site.Member(conversion.OperatorMethod); break;
                    case IUnaryOperation unary: site.Member(unary.OperatorMethod); break;
                    case IBinaryOperation binary: site.Member(binary.OperatorMethod); break;
                    case ICompoundAssignmentOperation compound: site.Member(compound.OperatorMethod); break;
                    case IIncrementOrDecrementOperation increment: site.Member(increment.OperatorMethod); break;
                    case ITypeOfOperation typeOf: site.Type(typeOf.TypeOperand); break;
                    case IIsTypeOperation isType: site.Type(isType.TypeOperand); break;
                    case ISizeOfOperation sizeOf: site.Type(sizeOf.TypeOperand); break;
                    case IDeclarationPatternOperation declaration: site.Type(declaration.MatchedType); break;
                    case ITypePatternOperation typePattern: site.Type(typePattern.MatchedType); break;
                    case IRecursivePatternOperation recursive:
                        site.Type(recursive.MatchedType);
                        site.Member(recursive.DeconstructSymbol);
                        break;
                    case IVariableDeclaratorOperation declarator: site.Type(declarator.Symbol.Type); break;
                    case IAnonymousFunctionOperation lambda: Signature(site, lambda.Symbol); break;
                    case ILocalFunctionOperation local: Signature(site, local.Symbol); break;
                    case ICollectionExpressionOperation collection: site.Member(collection.ConstructMethod); break;
                }
            }

            public void Check(Action<Diagnostic> report, string fromPath, Location at, ISymbol definition)
            {
                if (!sliceOfPath.TryGetValue(fromPath, out var from)) return;
                var to = SliceOf(definition);
                if (to == null || to.Name == from.Name) return;
                if (to.Row > from.Row)
                    report(Diagnostic.Create(Edge, at, from.Name, definition.ToDisplayString(), to.Name, "a row above it"));
                else if (to.Row == from.Row)
                    report(Diagnostic.Create(Edge, at, from.Name, definition.ToDisplayString(), to.Name, "the same row"));
            }

            private Slice? SliceOf(ISymbol definition)
            {
                if (definition.ContainingAssembly == null
                    || !SymbolEqualityComparer.Default.Equals(definition.ContainingAssembly, compilation.Assembly))
                    return null;
                var declared = definition;
                while (declared.DeclaringSyntaxReferences.IsEmpty)
                {
                    declared = declared.ContainingSymbol;
                    if (declared == null || declared is INamespaceSymbol) return null;
                }
                return sliceOfPath.TryGetValue(declared.DeclaringSyntaxReferences[0].SyntaxTree.FilePath, out var slice) ? slice : null;
            }

            private ImmutableHashSet<INamedTypeSymbol> BaseTypes()
            {
                var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                foreach (var type in AllTypes(compilation.Assembly.GlobalNamespace))
                {
                    var baseType = type.BaseType?.OriginalDefinition;
                    if (baseType != null && SymbolEqualityComparer.Default.Equals(baseType.ContainingAssembly, compilation.Assembly))
                        builder.Add(baseType);
                }
                return builder.ToImmutable();
            }

            private static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol @namespace)
            {
                foreach (var type in @namespace.GetTypeMembers())
                    foreach (var nested in WithNested(type))
                        yield return nested;
                foreach (var child in @namespace.GetNamespaceMembers())
                    foreach (var type in AllTypes(child))
                        yield return type;
            }

            private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
            {
                yield return type;
                foreach (var nested in type.GetTypeMembers())
                    foreach (var inner in WithNested(nested))
                        yield return inner;
            }

            private static void Parameters(Site site, ImmutableArray<IParameterSymbol> parameters, System.Threading.CancellationToken cancellationToken)
            {
                foreach (var parameter in parameters)
                {
                    var at = parameter.Locations.IsEmpty ? site : site.At(parameter.Locations[0]);
                    at.Type(parameter.Type);
                    Attributes(at, parameter.GetAttributes(), cancellationToken);
                }
            }

            private static void Constraints(Site site, ImmutableArray<ITypeParameterSymbol> typeParameters)
            {
                foreach (var typeParameter in typeParameters)
                    foreach (var constraint in typeParameter.ConstraintTypes)
                        site.Type(constraint);
            }

            private static void Attributes(Site site, ImmutableArray<AttributeData> attributes, System.Threading.CancellationToken cancellationToken)
            {
                foreach (var attribute in attributes)
                {
                    var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken);
                    var at = syntax == null ? site : site.At(syntax.GetLocation());
                    at.Type(attribute.AttributeClass);
                    foreach (var argument in attribute.ConstructorArguments) Constant(at, argument);
                    foreach (var named in attribute.NamedArguments) Constant(at, named.Value);
                }
            }

            private static void Constant(Site site, TypedConstant constant)
            {
                if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol type) site.Type(type);
                else if (constant.Kind == TypedConstantKind.Array)
                    foreach (var value in constant.Values) Constant(site, value);
            }

            private static void Signature(Site site, IMethodSymbol method)
            {
                site.Type(method.ReturnType);
                foreach (var parameter in method.Parameters) site.Type(parameter.Type);
            }

            private static bool DeclaresPublicType(MemberDeclarationSyntax member) =>
                DeclaresType(member) && member.Modifiers.Any(SyntaxKind.PublicKeyword);

            private static bool DeclaresType(MemberDeclarationSyntax member) =>
                member is BaseTypeDeclarationSyntax || member is DelegateDeclarationSyntax;

            private static SyntaxToken Identifier(MemberDeclarationSyntax member) => member switch
            {
                BaseTypeDeclarationSyntax type => type.Identifier,
                DelegateDeclarationSyntax @delegate => @delegate.Identifier,
                _ => member.GetFirstToken(),
            };
        }

        private sealed class Site
        {
            private readonly Run run;
            private readonly Action<Diagnostic> report;
            private readonly string fromPath;
            private readonly Location at;
            private readonly HashSet<ISymbol> seen;

            public Site(Run run, Action<Diagnostic> report, string fromPath, Location at)
                : this(run, report, fromPath, at, new HashSet<ISymbol>(SymbolEqualityComparer.Default))
            {
            }

            private Site(Run run, Action<Diagnostic> report, string fromPath, Location at, HashSet<ISymbol> seen)
            {
                this.run = run;
                this.report = report;
                this.fromPath = fromPath;
                this.at = at;
                this.seen = seen;
            }

            public Site At(Location location) => new Site(run, report, fromPath, location, seen);

            public void Type(ITypeSymbol? type)
            {
                foreach (var named in Named(type)) Use(named);
            }

            public void Member(ISymbol? member)
            {
                if (member == null) return;
                Use(member);
                switch (member)
                {
                    case IMethodSymbol method:
                        foreach (var argument in method.TypeArguments) Type(argument);
                        Type(method.ReturnType);
                        foreach (var parameter in method.Parameters) Type(parameter.Type);
                        break;
                    case IPropertySymbol property:
                        Type(property.Type);
                        foreach (var parameter in property.Parameters) Type(parameter.Type);
                        break;
                    case IFieldSymbol field:
                        Type(field.Type);
                        break;
                    case IEventSymbol @event:
                        Type(@event.Type);
                        break;
                }
            }

            private void Use(ISymbol symbol)
            {
                var definition = symbol.OriginalDefinition;
                if (seen.Add(definition)) run.Check(report, fromPath, at, definition);
            }

            private static IEnumerable<INamedTypeSymbol> Named(ITypeSymbol? type)
            {
                switch (type)
                {
                    case null:
                    case IErrorTypeSymbol:
                    case ITypeParameterSymbol:
                    case IDynamicTypeSymbol:
                        yield break;
                    case IArrayTypeSymbol array:
                        foreach (var inner in Named(array.ElementType)) yield return inner;
                        yield break;
                    case IPointerTypeSymbol pointer:
                        foreach (var inner in Named(pointer.PointedAtType)) yield return inner;
                        yield break;
                    case IFunctionPointerTypeSymbol functionPointer:
                        foreach (var inner in Named(functionPointer.Signature.ReturnType)) yield return inner;
                        foreach (var parameter in functionPointer.Signature.Parameters)
                            foreach (var inner in Named(parameter.Type))
                                yield return inner;
                        yield break;
                    case INamedTypeSymbol named:
                        yield return named;
                        foreach (var argument in named.TypeArguments)
                            foreach (var inner in Named(argument))
                                yield return inner;
                        yield break;
                }
            }
        }
    }
}
