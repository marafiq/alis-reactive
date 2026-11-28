using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

internal sealed class VendorProgram : IDisposable
{
    internal static string Root
    {
        get
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Alis.Reactive.slnx"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("The repository was not found.");
        }
    }

    internal string Folder { get; } = Path.Combine(Root, "tests", "Alis.Reactive.AnalyzerTests", "obj", "vendor-cases", Guid.NewGuid().ToString("N"));
    internal string Table => Path.Combine(Folder, "vendor-contract.tsv");

    internal VendorProgram(string source)
    {
        Write("tsconfig.json", """{"compilerOptions":{"strict":true,"noLib":true,"types":[],"target":"ES2022"},"files":["api.ts"]}""");
        Write("api.ts", source);
    }

    internal void Write(string file, string text)
    {
        var path = Path.Combine(Folder, file);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    internal async Task<string[]> Extract()
    {
        await Run("node", Path.Combine(Root, "Alis.Reactive.Analyzers", "Vendors", "vendor-contract.mjs"), Path.Combine(Folder, "tsconfig.json"), Table);
        return await File.ReadAllLinesAsync(Table);
    }

    internal Task Inputs()
    {
        return Run("node", Path.Combine(Root, "Alis.Reactive.Analyzers", "Vendors", "vendor-contract.mjs"), Path.Combine(Folder, "tsconfig.json"), Path.Combine(Folder, "inputs.sha256"), "--inputs");
    }

    internal Task Build()
    {
        return Run("dotnet", "build", Path.Combine(Folder, "Consumer.csproj"), "--no-restore", "-m:1", "--disable-build-servers", "-p:UseSharedCompilation=false", "-v:minimal");
    }

    internal async Task<string> Run(string program, params string[] arguments)
    {
        var start = new ProcessStartInfo(program, arguments)
        {
            WorkingDirectory = Folder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {program}.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var text = await output + await error;
        if (process.ExitCode != 0) throw new InvalidOperationException($"{program} exited {process.ExitCode}: {text}");
        return text;
    }

    public void Dispose()
    {
        Directory.Delete(Folder, true);
    }
}
