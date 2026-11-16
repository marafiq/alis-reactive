using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAPagesDoorsAreRead
{
    private const string Reader = "scripts/page-doors.cs";
    private const string Said = "[page-doors] ";

    private const string TheFramework = """
        namespace Alis.Reactive
        {
            public static class Loose
            {
                public static string Go() => throw new System.NotImplementedException();
            }
        }

        namespace Alis.Reactive.Wards
        {
            public static class Ward
            {
                public static string Open() => throw new System.NotImplementedException();
            }
        }

        namespace Alis.Reactive.Rooms
        {
            public static class Room
            {
                public static string Label() => "Room 4";

                public static string Clean() => throw new System.NotImplementedException();
            }
        }

        namespace Alis.Reactive.Events
        {
            public static class Bell
            {
                public static string Declare() => throw new System.NotImplementedException();
            }
        }
        """;

    private const string AnEastWard = """
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Razor.Hosting;

        [assembly: RazorCompiledItem(typeof(Care.App.East), "mvc.1.0.view", "/Wards/East.cshtml")]

        namespace Care.App
        {
            public sealed class EastController : Controller
            {
                [HttpGet("/wards/east")]
                public IActionResult East() => View("/Wards/East.cshtml");
            }

            public sealed class East
            {
                public string Render() => Alis.Reactive.Wards.Ward.Open() + Alis.Reactive.Rooms.Room.Label();
            }
        }
        """;

    private const string AnEastWardWithItsRooms = """
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Razor.Hosting;

        [assembly: RazorCompiledItem(typeof(Care.App.East), "mvc.1.0.view", "/Wards/East.cshtml")]
        [assembly: RazorCompiledItem(typeof(Care.App.Four), "mvc.1.0.view", "/Rooms/Four.cshtml")]
        [assembly: RazorCompiledItem(typeof(Care.App.Five), "mvc.1.0.view", "/Rooms/Five.cshtml")]

        namespace Care.App
        {
            public sealed class EastController : Controller
            {
                [HttpGet("/wards/east")]
                public IActionResult East() => View("/Wards/East.cshtml");

                [HttpGet("/rooms/four")]
                public IActionResult Four() => View("/Rooms/Four.cshtml");

                [HttpGet("/rooms/five")]
                public IActionResult Five() => View("/Rooms/Five.cshtml");
            }

            public sealed class East
            {
                private readonly System.Text.StringBuilder _html = new();

                public string Render()
                {
                    _html.Append("<a href=\"");
                    _html.Append("/rooms/four");
                    _html.Append(">Open room four</a>");
                    _html.Append("<a href=\"");
                    _html.Append("/rooms/five");
                    _html.Append(">Open room five</a>");
                    return _html + Alis.Reactive.Wards.Ward.Open();
                }
            }

            public sealed class Four
            {
                public string Render() => Alis.Reactive.Rooms.Room.Clean();
            }

            public sealed class Five
            {
                public string Render() => Alis.Reactive.Events.Bell.Declare();
            }
        }
        """;

    private static readonly Lazy<IReadOnlyList<MetadataReference>> ThePlatform = new(
        () => ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Where(assembly => Path.GetDirectoryName(assembly) != Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory))
            .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly)).ToList());

    [RefusedCase]
    public void a_test_whose_pages_no_route_of_the_app_answers_is_refused()
        => Assert.That(Read(AnEastWard, Naming("/wards/west")), Is.EqualTo(
            "exit 1\n[page-doors] M1.F1 X1.1 0: its test names no route an action of the app answers a GET on, so no page of it is read\n"));

    [RefusedCase]
    public void an_action_that_renders_a_view_by_no_path_is_refused()
        => Assert.That(Read(AnEastWard.Replace("    public sealed class East\n", """
                public sealed class WestController : Controller
                {
                    [HttpGet("/wards/west")]
                    public IActionResult West() => View();
                }

                public sealed class East

            """, StringComparison.Ordinal), Naming("/wards/east")), Is.EqualTo(
            "exit 1\n[page-doors] Care.App.WestController.West renders a view it names by no path\n"));

    [RefusedCase]
    public void a_page_an_action_names_that_the_app_does_not_compile_is_refused()
        => Assert.That(Read(AnEastWard.Replace("=> View(\"/Wards/East.cshtml\")", "=> View(\"/Wards/North.cshtml\")", StringComparison.Ordinal), Naming("/wards/east")), Is.EqualTo(
            "exit 1\n[page-doors] M1.F1 X1.1 0: its pages name /Wards/North.cshtml, no view the app compiles\n"));

    [RefusedCase]
    public void a_link_the_test_names_with_no_route_written_before_it_is_refused()
        => Assert.That(Read(AnEastWardWithItsRooms.Replace("_html.Append(\"/rooms/four\");", "_html.Append(RoomFour);", StringComparison.Ordinal)
                .Replace("public string Render()\n", "private static string RoomFour => \"/rooms/four\";\n\n        public string Render()\n", StringComparison.Ordinal),
            Naming("/wards/east", "Open room four")), Is.EqualTo(
            "exit 1\n[page-doors] M1.F1 X1.1 0: its test names the link 'Open room four' of /Wards/East.cshtml, whose address is written by no route before it, so the page it opens is not read\n"));

    [RefusedCase]
    public void a_door_not_built_in_the_frameworks_root_namespace_is_refused()
        => Assert.That(Read(AnEastWard.Replace("Alis.Reactive.Wards.Ward.Open()", "Alis.Reactive.Loose.Go()", StringComparison.Ordinal), Naming("/wards/east")), Is.EqualTo(
            "exit 1\n[page-doors] Alis.Reactive.Loose.Go throws System.NotImplementedException in Alis.Reactive's root namespace, which names no module\n"));

    [RefusedCase]
    public void an_app_built_against_another_framework_is_refused()
        => Assert.That(Read(AnEastWard.Replace("Alis.Reactive.Wards.Ward.Open()", "Alis.Reactive.Wards.Ward.Close()", StringComparison.Ordinal), Naming("/wards/east"),
                TheFramework.Replace("public static string Open()", "public static string Close() => \"closed\";\n\n        public static string Open()", StringComparison.Ordinal)), Is.EqualTo(
            "exit 1\n[page-doors] Alis.Reactive.Wards.Ward in Alis.Reactive declares no Close of the signature the app calls: the app is built against another Alis.Reactive, so build the solution again\n"));

    [AcceptedCase]
    public void a_page_lands_the_module_of_each_door_it_calls_that_is_not_built()
        => Assert.That(Read(AnEastWard, Naming("/wards/east")), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Wards"
                ],
                "views": [
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    [AcceptedCase]
    public void a_test_under_a_fresh_apps_host_is_read_by_none_of_the_apps_pages()
        => Assert.That(Read(AnEastWard, Naming("/wards/east") + """

           namespace NUnit.Framework
           {
               [System.AttributeUsage(System.AttributeTargets.Class)]
               public sealed class CategoryAttribute(string name) : System.Attribute
               {
                   public string Name { get; } = name;
               }
           }

           namespace Care.Tests
           {
               [NUnit.Framework.Category("consumerNet10")]
               public sealed class WhenAFreshAppOpens
               {
                   [Alis.Reactive.Conformance.World.FailsFor("M19.F1 M19.X5 0")]
                   public string[] the_home_page_boots() => ["/"];
               }
           }
           """), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Wards"
                ],
                "views": [
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    [AcceptedCase]
    public void a_link_the_test_names_opens_its_page_and_a_link_it_does_not_name_opens_none()
        => Assert.That(Read(AnEastWardWithItsRooms, Naming("/wards/east", "Open room four")), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Rooms",
                  "Wards"
                ],
                "views": [
                  "/Rooms/Four.cshtml",
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    [AcceptedCase]
    public void a_page_a_route_template_and_a_query_open_lands_its_doors()
        => Assert.That(Read(AnEastWard.Replace("[HttpGet(\"/wards/east\")]\n        public IActionResult East()", "[HttpGet(\"/wards/{ward}\")]\n        public IActionResult East(string ward)", StringComparison.Ordinal),
            Naming("/wards/East?shift=night")), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Wards"
                ],
                "views": [
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    [AcceptedCase]
    public void a_door_a_static_field_starts_with_lands_with_the_page_that_reads_the_field()
        => Assert.That(Read(AnEastWard.Replace(
                "public string Render() => Alis.Reactive.Wards.Ward.Open() + Alis.Reactive.Rooms.Room.Label();",
                "public string Render() => Bells.Rung;\n    }\n\n    public static class Bells\n    {\n        public static readonly string Rung = Alis.Reactive.Events.Bell.Declare();",
                StringComparison.Ordinal), Naming("/wards/east")), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Events"
                ],
                "views": [
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    [AcceptedCase]
    public void a_member_the_framework_can_call_lands_and_one_no_page_calls_lands_nothing()
        => Assert.That(Read(AnEastWard.Replace(
                "public string Render() => Alis.Reactive.Wards.Ward.Open() + Alis.Reactive.Rooms.Room.Label();",
                "public string Render() => Shown(new Rota());\n\n        private static string Shown(object rota) => rota.ToString()!;\n    }\n\n    public sealed class Rota\n    {\n        public override string ToString() => Alis.Reactive.Wards.Ward.Open();\n\n        public string Unread() => Alis.Reactive.Rooms.Room.Clean();",
                StringComparison.Ordinal), Naming("/wards/east")), Is.EqualTo(
            """
            exit 0
            {
              "M1.F1 X1.1 0": {
                "modules": [
                  "Wards"
                ],
                "views": [
                  "/Wards/East.cshtml"
                ]
              }
            }

            """));

    private static string Naming(params string[] named)
        => """
           namespace Alis.Reactive.Conformance.World
           {
               [System.AttributeUsage(System.AttributeTargets.Method)]
               public sealed class FailsForAttribute(string promise) : System.Attribute
               {
                   public string Promise { get; } = promise;
               }
           }

           namespace Care.Tests
           {
               public sealed class WhenAWardOpens
               {
                   [Alis.Reactive.Conformance.World.FailsFor("M1.F1 X1.1 0")]
                   public string[] the_ward_opens() => [NAMED];
               }
           }
           """.Replace("NAMED", string.Join(", ", named.Select(text => "\"" + text + "\"")), StringComparison.Ordinal);

    private static string Read(string app, string tests, string builtAgainst = TheFramework)
    {
        using var folder = new WrittenFolder();
        var appPath = folder.Write("bin/Care.App.dll", Built("Care.App", app, [MetadataReference.CreateFromImage(Built("Alis.Reactive", builtAgainst, []))]));
        folder.Write("bin/Alis.Reactive.dll", Built("Alis.Reactive", TheFramework, []));
        folder.Write("bin/Alis.Reactive.Native.dll", Built("Alis.Reactive.Native", "namespace Alis.Reactive.Native.Beds { public static class Bed { } }", []));
        folder.Write("bin/Alis.Reactive.Fusion.dll", Built("Alis.Reactive.Fusion", "namespace Alis.Reactive.Fusion.Charts { public static class Chart { } }", []));
        var testsPath = folder.Write("tests/Care.Tests.dll", Built("Care.Tests", tests, []));
        var table = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(appPath))!, "page-doors.json");
        var run = Run("dotnet", ["run", Reader, "-p:RestorePackagesWithLockFile=false", "--", appPath, testsPath, table]);
        var refusals = run.Errors.Split('\n').Where(line => line.StartsWith(Said, StringComparison.Ordinal)).Select(line => line + "\n");
        var written = File.Exists(table) ? File.ReadAllText(table) : "";
        var read = $"exit {run.ExitCode}\n" + string.Concat(refusals) + written;
        return run.ExitCode is 0 or 1 ? read : read + run.Output + run.Errors;
    }

    private static byte[] Built(string name, string source, IReadOnlyList<MetadataReference> references)
    {
        var compilation = CSharpCompilation.Create(
            name,
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14))],
            [.. ThePlatform.Value, .. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, nullableContextOptions: NullableContextOptions.Enable));
        using var image = new MemoryStream();
        var emitted = compilation.Emit(image);
        if (!emitted.Success)
            throw new InvalidOperationException($"The case's {name} does not compile: " + string.Join("; ", emitted.Diagnostics.Where(found => found.Severity == DiagnosticSeverity.Error)));

        return image.ToArray();
    }

    private static (string Output, string Errors, int ExitCode) Run(string program, string[] arguments)
    {
        var start = new ProcessStartInfo(program, arguments)
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var started = Process.Start(start) ?? throw new InvalidOperationException(program + " could not be started");
        var output = started.StandardOutput.ReadToEndAsync();
        var errors = started.StandardError.ReadToEndAsync();
        started.WaitForExit();
        return (output.Result, errors.Result, started.ExitCode);
    }
}
