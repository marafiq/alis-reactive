using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

const string World = "001";

if (args.Length != 3)
{
    Console.Error.WriteLine("usage: dotnet run scripts/windows-zones.cs -- <CLDR's windowsZones.xml> <its lock> <the table to write>");
    Console.Error.WriteLine("Reads CLDR's windowsZones.xml, whose SHA-256 the lock pins, and writes the table that names each Windows time zone id's default IANA id, the row of territory 001, as the net48 host's C#.");
    return 2;
}

var (source, lockFile, table) = (args[0], args[1], args[2]);
var missing = new[] { source, lockFile }.Where(path => !File.Exists(path)).ToList();
if (missing.Count > 0)
{
    Console.Error.WriteLine("[windows-zones] no file at " + string.Join(", none at ", missing) + ".");
    return 2;
}

using var pin = JsonDocument.Parse(File.ReadAllText(lockFile));
var release = pin.RootElement.GetProperty("release").GetString()!;
var pinned = pin.RootElement.GetProperty("sha256").GetString()!;
var found = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(source)));
if (found != pinned)
{
    Console.Error.WriteLine($"[windows-zones] {source} has SHA-256 {found}, and {lockFile} pins {pinned} for {release}: take the file from the lock's url, or move the lock with the file.");
    return 1;
}

using var reader = XmlReader.Create(source, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null });
var rows = XDocument.Load(reader).Descendants("mapZone")
    .Select(row => (Windows: row.Attribute("other")!.Value, Territory: row.Attribute("territory")!.Value, Iana: row.Attribute("type")!.Value))
    .ToList();
var defaults = rows.Where(row => row.Territory == World).ToList();
var problems = defaults.GroupBy(row => row.Windows, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1)
    .Select(group => $"'{group.Key}' has {group.Count()} rows of territory {World}")
    .Concat(defaults.Where(row => row.Iana.Length == 0 || row.Iana.Contains(' ')).Select(row => $"'{row.Windows}' names '{row.Iana}' under territory {World}, which is not one IANA id"))
    .Concat(rows.Select(row => row.Windows).Distinct(StringComparer.OrdinalIgnoreCase).Except(defaults.Select(row => row.Windows), StringComparer.OrdinalIgnoreCase)
        .Select(windows => $"'{windows}' has no row of territory {World}"))
    .Concat(defaults.SelectMany(row => new[] { row.Windows, row.Iana }).Where(id => id.Any(letter => letter is < ' ' or > '~' or '"' or '\\'))
        .Select(id => $"'{id}' holds a character a C# text of the table does not write as it is"))
    .ToList();
if (problems.Count > 0 || defaults.Count == 0)
{
    Console.Error.WriteLine("[windows-zones] " + (defaults.Count == 0 ? $"{source} holds no row of territory {World}" : string.Join("; ", problems)) + ".");
    return 1;
}

var written = new StringBuilder()
    .Append("#if NET48\n")
    .Append("using System;\n")
    .Append("using System.Collections.Generic;\n")
    .Append('\n')
    .Append("namespace Alis.Reactive.PathsAndWire;\n")
    .Append('\n')
    .Append("internal static class WindowsZones\n")
    .Append("{\n")
    .Append($"    internal const string Release = \"{release}\";\n")
    .Append('\n')
    .Append("    private static readonly Dictionary<string, string> IanaIds = new(StringComparer.OrdinalIgnoreCase)\n")
    .Append("    {\n");
foreach (var row in defaults.OrderBy(row => row.Windows, StringComparer.Ordinal))
    written.Append($"        [\"{row.Windows}\"] = \"{row.Iana}\",\n");
written
    .Append("    };\n")
    .Append('\n')
    .Append("    internal static bool Names(string windowsId, out string ianaId) => IanaIds.TryGetValue(windowsId, out ianaId);\n")
    .Append("}\n")
    .Append("#endif\n");
File.WriteAllText(table, written.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine($"[windows-zones] {table}: {defaults.Count} Windows time zone ids of CLDR {release}, each with its IANA id of territory {World}.");
return 0;
