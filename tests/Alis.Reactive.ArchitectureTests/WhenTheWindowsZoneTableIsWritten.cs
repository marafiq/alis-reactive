using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenTheWindowsZoneTableIsWritten
{
    private const string Writer = "scripts/windows-zones.cs";
    private const string Source = "spec/probes/cldr/windowsZones.xml.txt";
    private const string Pin = "spec/probes/cldr/windowsZones.lock";
    private const string Table = "Alis.Reactive/PathsAndWire/WindowsZones.g.Net48.cs";
    private const string Said = "[windows-zones] ";
    private const string SourceName = "windowsZones.xml.txt";
    private const string PinName = "windowsZones.lock";
    private const string TableName = "WindowsZones.g.Net48.cs";
    private const string ReleaseFortyEightsHash = "9cf3db6a31fb382fee21b70be6feba1e82766b0fcd06e6261fb7936a73e537ff";

    private const string TwoZones = """
        <?xml version="1.0" encoding="UTF-8" ?>
        <supplementalData>
            <windowsZones>
                <mapTimezones otherVersion="7e11800" typeVersion="2021a">
                    <mapZone other="India Standard Time" territory="001" type="Asia/Calcutta"/>
                    <mapZone other="India Standard Time" territory="IN" type="Asia/Calcutta"/>
                    <mapZone other="Eastern Standard Time" territory="001" type="America/New_York"/>
                    <mapZone other="Eastern Standard Time" territory="US" type="America/New_York America/Detroit"/>
                </mapTimezones>
            </windowsZones>
        </supplementalData>
        """;

    [Test]
    public void the_tracked_table_is_what_the_script_writes_from_the_pinned_source()
    {
        using var folder = new WrittenFolder();
        var source = folder.Write(SourceName, File.ReadAllBytes(Path.Combine(Repository.Root, Source)));
        var pin = folder.Write(PinName, File.ReadAllBytes(Path.Combine(Repository.Root, Pin)));

        Assert.That(Repository.TrackedFiles, Does.Contain(Table), "The net48 table is tracked, so a build on another machine compiles the same rows");
        Assert.That(Written(source, pin), Is.EqualTo("exit 0\n" + File.ReadAllText(Path.Combine(Repository.Root, Table))),
            "The tracked table differs from what " + Writer + " writes from " + Source + " at its pin: run it and keep what it writes.");
    }

    [RefusedCase]
    public void a_source_off_its_pin_is_refused_naming_both_hashes()
    {
        using var folder = new WrittenFolder();
        var source = folder.Write(SourceName, TwoZones);
        var pin = folder.Write(PinName, PinOf(ReleaseFortyEightsHash));

        Assert.That(Written(source, pin), Is.EqualTo(
            $"exit 1\n[windows-zones] {source} has SHA-256 {HashOf(TwoZones)}, and {pin} pins {ReleaseFortyEightsHash} for release-49: take the file from the lock's url, or move the lock with the file.\n"));
    }

    [RefusedCase]
    public void a_row_of_territory_001_that_names_two_iana_ids_is_refused()
    {
        var twoIds = TwoZones.Replace("territory=\"001\" type=\"Asia/Calcutta\"", "territory=\"001\" type=\"Asia/Calcutta Asia/Kolkata\"", StringComparison.Ordinal);

        Assert.That(WrittenOnItsPin(twoIds), Is.EqualTo(
            "exit 1\n[windows-zones] 'India Standard Time' names 'Asia/Calcutta Asia/Kolkata' under territory 001, which is not one IANA id.\n"));
    }

    [RefusedCase]
    public void a_windows_id_with_two_rows_of_territory_001_is_refused()
    {
        var twoRows = TwoZones.Replace(
            "<mapZone other=\"Eastern Standard Time\" territory=\"US\"",
            "<mapZone other=\"Eastern Standard Time\" territory=\"001\" type=\"America/Detroit\"/>\n<mapZone other=\"Eastern Standard Time\" territory=\"US\"",
            StringComparison.Ordinal);

        Assert.That(WrittenOnItsPin(twoRows), Is.EqualTo("exit 1\n[windows-zones] 'Eastern Standard Time' has 2 rows of territory 001.\n"));
    }

    [RefusedCase]
    public void a_windows_id_with_no_row_of_territory_001_is_refused()
    {
        var noDefault = TwoZones.Replace("<mapZone other=\"India Standard Time\" territory=\"001\" type=\"Asia/Calcutta\"/>", "", StringComparison.Ordinal);

        Assert.That(WrittenOnItsPin(noDefault), Is.EqualTo("exit 1\n[windows-zones] 'India Standard Time' has no row of territory 001.\n"));
    }

    [RefusedCase]
    public void an_id_holding_a_character_the_table_does_not_write_as_it_is_is_refused()
    {
        var backslash = TwoZones.Replace("other=\"India Standard Time\" territory=\"001\"", "other=\"India\\Standard Time\" territory=\"001\"", StringComparison.Ordinal)
            .Replace("other=\"India Standard Time\" territory=\"IN\"", "other=\"India\\Standard Time\" territory=\"IN\"", StringComparison.Ordinal);

        Assert.That(WrittenOnItsPin(backslash), Is.EqualTo(
            "exit 1\n[windows-zones] 'India\\Standard Time' holds a character a C# text of the table does not write as it is.\n"));
    }

    [RefusedCase]
    public void a_source_with_no_row_of_territory_001_is_refused()
    {
        using var folder = new WrittenFolder();
        const string NoZones = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<supplementalData><windowsZones><mapTimezones/></windowsZones></supplementalData>\n";
        var source = folder.Write(SourceName, NoZones);
        var pin = folder.Write(PinName, PinOf(HashOf(NoZones)));

        Assert.That(Written(source, pin), Is.EqualTo($"exit 1\n[windows-zones] {source} holds no row of territory 001.\n"));
    }

    [AcceptedCase]
    public void a_source_on_its_pin_writes_each_windows_id_with_its_iana_id_of_territory_001_in_ordinal_order()
        => Assert.That(WrittenOnItsPin(TwoZones), Is.EqualTo(
            """
            exit 0
            #if NET48
            using System;
            using System.Collections.Generic;

            namespace Alis.Reactive.PathsAndWire;

            internal static class WindowsZones
            {
                internal const string Release = "release-49";

                private static readonly Dictionary<string, string> IanaIds = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Eastern Standard Time"] = "America/New_York",
                    ["India Standard Time"] = "Asia/Calcutta",
                };

                internal static bool Names(string windowsId, out string ianaId) => IanaIds.TryGetValue(windowsId, out ianaId);
            }
            #endif

            """));

    private static string WrittenOnItsPin(string zones)
    {
        using var folder = new WrittenFolder();
        return Written(folder.Write(SourceName, zones), folder.Write(PinName, PinOf(HashOf(zones))));
    }

    private static string Written(string source, string pin)
    {
        var table = Path.Combine(Path.GetDirectoryName(source)!, TableName);
        var run = Run("dotnet", ["run", Writer, "-p:RestorePackagesWithLockFile=false", "--", source, pin, table]);
        var refusals = run.Errors.Split('\n').Where(line => line.StartsWith(Said, StringComparison.Ordinal)).Select(line => line + "\n");
        var written = File.Exists(table) ? File.ReadAllText(table) : "";
        var read = $"exit {run.ExitCode}\n" + string.Concat(refusals) + written;
        return run.ExitCode is 0 or 1 ? read : read + run.Output + run.Errors;
    }

    private static string PinOf(string sha256)
        => $$"""
           {
             "release": "release-49",
             "url": "https://raw.githubusercontent.com/unicode-org/cldr/96e54b273d2da4ec6fbad350ba19ff23fa24a588/common/supplemental/windowsZones.xml",
             "sha256": "{{sha256}}"
           }
           """;

    private static string HashOf(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

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
