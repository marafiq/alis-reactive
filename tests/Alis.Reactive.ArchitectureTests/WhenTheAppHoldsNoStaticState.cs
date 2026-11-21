using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Alis.Reactive.Conformance.App;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenTheAppHoldsNoStaticState
{
    private const string InItsWorld =
        "what a request changes lives in its world, a value that never changes is a constant or a read-only static of a type nothing assigns, "
        + "and a list that never changes is a read-only view or an immutable collection of such values";

    private const BindingFlags EveryStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    private const BindingFlags EveryInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly Type[] ReadOnlyViews =
        [typeof(IEnumerable), typeof(IEnumerable<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>), typeof(IReadOnlyDictionary<,>), typeof(IReadOnlySet<>)];

    private static readonly Lazy<IReadOnlyList<MetadataReference>> ThePlatform = new(
        () => ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly)).ToList());

    [Test]
    public void no_static_of_the_conformance_app_holds_what_a_request_can_change()
    {
        var app = typeof(Program).Assembly;

        Assert.That(TypesWrittenIn(app), Is.Not.Empty, "The conformance app holds no type, so the rule read nothing");
        Assert.That(Problems(app), Is.Empty, "A static of the conformance app is shared by every world, so one test's request changes what another test reads.");
    }

    [RefusedCase]
    public void a_static_a_request_can_assign_is_refused_by_its_name()
        => Assert.That(
            Problems(Built("""
                namespace Ward;

                internal static class Admissions
                {
                    internal static int Admitted;

                    internal static string? LastAdmitted { get; set; }

                    internal static event System.Action? Changed;

                    internal static void Admit(string resident)
                    {
                        Admitted++;
                        LastAdmitted = resident;
                        Changed?.Invoke();
                    }
                }
                """)),
            Is.EqualTo(new[]
            {
                "Ward.Admissions.Admitted is a static a request can assign; " + InItsWorld,
                "Ward.Admissions.Changed is a static a request can assign; " + InItsWorld,
                "Ward.Admissions.LastAdmitted is a static a request can assign; " + InItsWorld,
            }));

    [RefusedCase]
    public void a_read_only_static_that_holds_a_collection_a_request_can_change_is_refused_with_the_collection()
        => Assert.That(
            Problems(Built("""
                using System.Collections.Concurrent;
                using System.Collections.Generic;
                using System.Collections.Immutable;

                namespace Ward;

                internal static class Register
                {
                    internal static readonly Dictionary<string, string> ByBed = new();

                    internal static readonly ConcurrentQueue<string> Calls = new();

                    internal static readonly string[] Wings = ["North", "South"];

                    internal static readonly IReadOnlyDictionary<string, List<string>> ByWing = new Dictionary<string, List<string>>();

                    internal static readonly ImmutableArray<string[]> Rounds = [];

                    internal static List<string> Residents { get; } = [];
                }
                """)),
            Is.EqualTo(new[]
            {
                "Ward.Register.ByBed is a static that holds what a request can change, since a Dictionary can be changed; " + InItsWorld,
                "Ward.Register.ByWing is a static that holds what a request can change, since a List can be changed; " + InItsWorld,
                "Ward.Register.Calls is a static that holds what a request can change, since a ConcurrentQueue can be changed; " + InItsWorld,
                "Ward.Register.Residents is a static that holds what a request can change, since a List can be changed; " + InItsWorld,
                "Ward.Register.Rounds is a static that holds what a request can change, since an array's items can be assigned; " + InItsWorld,
                "Ward.Register.Wings is a static that holds what a request can change, since an array's items can be assigned; " + InItsWorld,
            }));

    [RefusedCase]
    public void a_read_only_static_of_the_apps_own_type_that_a_request_can_change_is_refused_with_the_member()
        => Assert.That(
            Problems(Built("""
                using System.Collections.Generic;

                namespace Ward;

                internal sealed class Resident
                {
                    public string Name { get; set; } = "";
                }

                internal sealed class Round(int hour)
                {
                    internal int Next() => hour++;
                }

                internal sealed class Store
                {
                    private readonly List<string> _all = [];

                    internal int Count => _all.Count;
                }

                internal static class Register
                {
                    internal static readonly Resident OnCall = new();

                    internal static readonly IReadOnlyList<Resident> Seeded = [];

                    internal static readonly Round Night = new(22);

                    internal static readonly Store All = new();
                }
                """)),
            Is.EqualTo(new[]
            {
                "Ward.Register.All is a static that holds what a request can change, since a List can be changed; " + InItsWorld,
                "Ward.Register.Night is a static that holds what a request can change, since Round.hour can be assigned; " + InItsWorld,
                "Ward.Register.OnCall is a static that holds what a request can change, since Resident.Name can be assigned; " + InItsWorld,
                "Ward.Register.Seeded is a static that holds what a request can change, since Resident.Name can be assigned; " + InItsWorld,
            }));

    [AcceptedCase]
    public void a_constant_a_read_only_static_nothing_assigns_and_what_an_instance_holds_are_accepted()
        => Assert.That(
            Problems(Built("""
                using System;
                using System.Collections.Concurrent;
                using System.Collections.Frozen;
                using System.Collections.Generic;
                using System.Collections.Immutable;
                using System.Linq;

                namespace Ward;

                internal enum Floor
                {
                    Ground,
                    First,
                }

                internal record struct Bed(int Number, Floor On);

                internal sealed record Wing(string Name, int Beds);

                internal sealed class Vendor(string name)
                {
                    public string Name { get; } = name;
                }

                internal sealed class Shift
                {
                    private readonly Shift? _next;

                    internal Shift(Shift? next) => _next = next;

                    internal bool IsLast => _next is null;
                }

                internal sealed class Worlds
                {
                    private readonly ConcurrentDictionary<string, List<string>> _byKey = new();
                    private int _opened;

                    internal int Open(string key) => _byKey.TryAdd(key, []) ? ++_opened : _opened;
                }

                internal static class Register
                {
                    internal const string Home = "Maple Court";

                    internal static readonly string Greeting = string.Concat("Good ", "morning");

                    internal static readonly TimeSpan Round = TimeSpan.FromHours(2);

                    internal static readonly IReadOnlyList<string> Wings = ["North", "South"];

                    internal static readonly IReadOnlyDictionary<string, Wing> ByName = new Dictionary<string, Wing> { ["North"] = new("North", 12) };

                    internal static readonly ImmutableArray<Wing> All = [new("North", 12)];

                    internal static readonly Bed Nearest = new(1, Floor.Ground);

                    internal static readonly IReadOnlyList<Bed> Free = [new(2, Floor.First)];

                    internal static readonly FrozenSet<string> Closed = FrozenSet.ToFrozenSet(["East"]);

                    internal static readonly Shift Night = new(new Shift(null));

                    internal static Vendor BoardVendor { get; } = new("board");

                    internal static List<string> Fresh => ["North"];

                    internal static int Longest(IEnumerable<string> names) => names.Max(name => name.Length);
                }
                """)),
            Is.Empty);

    private static List<string> Problems(Assembly app)
        => TypesWrittenIn(app)
            .SelectMany(type => type.GetFields(EveryStatic))
            .Where(field => !field.IsLiteral)
            .Select(field => (Name: $"{field.DeclaringType!.FullName}.{NameOf(field)}", Refusal: Refusal(field, app)))
            .Where(read => read.Refusal.Length > 0)
            .Select(read => $"{read.Name} is {read.Refusal}; {InItsWorld}")
            .Order(StringComparer.Ordinal)
            .ToList();

    private static List<Type> TypesWrittenIn(Assembly app) => app.GetTypes().Where(type => !IsTheCompilers(type)).ToList();

    private static bool IsTheCompilers(Type type)
        => type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) || (type.DeclaringType is { } holder && IsTheCompilers(holder));

    private static string Refusal(FieldInfo held, Assembly app)
    {
        if (!held.IsInitOnly) return "a static a request can assign";

        var changes = WhatChanges(held.FieldType, app, []);
        return changes.Length == 0 ? "" : $"a static that holds what a request can change, since {changes}";
    }

    private static string WhatChanges(Type held, Assembly app, HashSet<Type> read)
    {
        if (held.IsArray) return "an array's items can be assigned";
        if (!read.Add(held)) return "";
        if (held.Assembly == app) return WhatChangesInTheApps(held, app, read);
        if (held == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(held)) return "";
        if (!IsReadOnly(held)) return $"a {NameOf(held)} can be changed";

        return held.GetGenericArguments().Select(item => WhatChanges(item, app, read)).FirstOrDefault(changes => changes.Length > 0, "");
    }

    private static string WhatChangesInTheApps(Type own, Assembly app, HashSet<Type> read)
    {
        foreach (var member in own.GetFields(EveryInstance))
        {
            var isAssignedThroughAReference = !member.IsInitOnly && !own.IsValueType;
            if (isAssignedThroughAReference) return $"{NameOf(own)}.{NameOf(member)} can be assigned";

            var changes = WhatChanges(member.FieldType, app, read);
            if (changes.Length > 0) return changes;
        }

        return "";
    }

    private static bool IsReadOnly(Type collection)
        => collection.Namespace is "System.Collections.Immutable" or "System.Collections.Frozen"
           || (collection.IsInterface && ReadOnlyViews.Contains(collection.IsGenericType ? collection.GetGenericTypeDefinition() : collection));

    private static string NameOf(FieldInfo field) => field.Name.StartsWith('<') ? field.Name[1..field.Name.IndexOf('>')] : field.Name;

    private static string NameOf(Type type) => type.Name.Split('`')[0];

    private static Assembly Built(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Ward",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14))],
            ThePlatform.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        using var image = new MemoryStream();
        var emitted = compilation.Emit(image);
        if (!emitted.Success)
            throw new InvalidOperationException("The case's text does not compile: " + string.Join("; ", emitted.Diagnostics.Where(found => found.Severity == DiagnosticSeverity.Error)));

        image.Position = 0;
        return new AssemblyLoadContext("a case of " + nameof(WhenTheAppHoldsNoStaticState), isCollectible: true).LoadFromStream(image);
    }
}
