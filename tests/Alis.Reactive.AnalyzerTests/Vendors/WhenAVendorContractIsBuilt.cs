using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorContractIsBuilt
{
    [RefusedCase]
    public async Task changing_only_typescript_replaces_the_table_before_the_second_compilation()
    {
        using var program = new VendorProgram("export interface Gauge { before: number; }");
        Project(program);
        await program.Build();
        var first = await File.ReadAllLinesAsync(Path.Combine(program.Folder, "obj", "Debug", "net10.0", "vendor-contract.tsv"));
        program.Write("api.ts", "export interface Gauge { after: string; }");
        await program.Build();
        var second = await File.ReadAllLinesAsync(Path.Combine(program.Folder, "obj", "Debug", "net10.0", "vendor-contract.tsv"));
        Assert.That(first, Does.Contain("member\tapi:Gauge@1.before\tapi:Gauge@1\tbefore\tproperty\tfalse\tfalse\tfalse\tfalse"));
        Assert.That(second, Does.Contain("member\tapi:Gauge@1.after\tapi:Gauge@1\tafter\tproperty\tfalse\tfalse\tfalse\tfalse"));
        Assert.That(second, Does.Not.Contain("member\tapi:Gauge@1.before\tapi:Gauge@1\tbefore\tproperty\tfalse\tfalse\tfalse\tfalse"));
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "compiled.txt")), Is.EqualTo("obj/Debug/net10.0/vendor-contract.tsv" + Environment.NewLine));
        Assert.That(await File.ReadAllLinesAsync(Path.Combine(program.Folder, "compiler-arguments.txt")), Does.Contain("/additionalfile:obj/Debug/net10.0/vendor-contract.tsv"));
    }

    [AcceptedCase]
    public async Task unchanged_inputs_keep_the_existing_table()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        Project(program);
        await program.Build();
        var path = Path.Combine(program.Folder, "obj", "Debug", "net10.0", "vendor-contract.tsv");
        var before = File.GetLastWriteTimeUtc(path);
        await program.Build();
        Assert.That(File.GetLastWriteTimeUtc(path), Is.EqualTo(before));
        Assert.That(await File.ReadAllLinesAsync(path), Does.Contain("type\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge"));
    }

    [RefusedCase]
    public async Task an_extended_config_changes_the_inputs()
    {
        using var program = new VendorProgram("export interface Gauge { value: number | null; }");
        program.Write("tsconfig.json", """{"extends":"./base.json","files":["api.ts"]}""");
        program.Write("base.json", """{"compilerOptions":{"strict":true,"noLib":true,"types":[]}}""");
        await program.Inputs();
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        program.Write("base.json", """{"compilerOptions":{"strict":false,"noLib":true,"types":[]}}""");
        await program.Inputs();
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(before));
    }

    [RefusedCase]
    public async Task a_resolved_declaration_changes_the_inputs_even_when_its_timestamp_is_preserved()
    {
        using var program = new VendorProgram("import { Gauge } from './vendor'; export type Api = Gauge;");
        program.Write("vendor.d.ts", "export interface Gauge { value: number; }");
        await program.Inputs();
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        var timestamp = File.GetLastWriteTimeUtc(Path.Combine(program.Folder, "vendor.d.ts"));
        program.Write("vendor.d.ts", "export interface Gauge { value: string; }");
        File.SetLastWriteTimeUtc(Path.Combine(program.Folder, "vendor.d.ts"), timestamp);
        await program.Inputs();
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(before));
    }

    [RefusedCase]
    public async Task the_lock_changes_the_inputs()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        program.Write("package-lock.json", "{\"lockfileVersion\":3}");
        await program.Inputs();
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        program.Write("package-lock.json", "{\"lockfileVersion\":2}");
        await program.Inputs();
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(before));
    }

    [RefusedCase]
    public async Task newly_included_files_change_the_inputs()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"noLib":true,"types":[]},"include":["*.ts"]}""");
        await program.Inputs();
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        program.Write("added.ts", "export interface Added { value: number; }");
        await program.Inputs();
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(before));
    }

    [RefusedCase]
    public async Task the_consumers_compiler_and_its_resolved_library_are_inputs()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        var source = Path.Combine(VendorProgram.Root, "node_modules", "typescript");
        program.Write("node_modules/typescript/package.json", await File.ReadAllTextAsync(Path.Combine(source, "package.json")));
        program.Write("node_modules/typescript/lib/typescript.js", await File.ReadAllTextAsync(Path.Combine(source, "lib", "typescript.js")));
        program.Write("node_modules/typescript/lib/lib.es5.d.ts", "interface LibraryGauge { value: number; }");
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"lib":["es5"],"types":[]},"files":["api.ts"]}""");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tlib.es5:LibraryGauge@1\tlib.es5\tLibraryGauge\tinterface\tLibraryGauge"));
        await program.Inputs();
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        program.Write("node_modules/typescript/lib/lib.es5.d.ts", "interface LibraryGauge { value: string; }");
        await program.Inputs();
        var library = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        Assert.That(library, Is.Not.EqualTo(before));
        await File.AppendAllTextAsync(Path.Combine(program.Folder, "node_modules", "typescript", "lib", "typescript.js"), "\n;");
        await program.Inputs();
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(library));
    }

    [RefusedCase]
    public async Task the_shipped_script_is_an_input()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        var script = Path.Combine(program.Folder, "vendor-contract.mjs");
        program.Write("vendor-contract.mjs", await File.ReadAllTextAsync(Path.Combine(VendorProgram.Root, "Alis.Reactive.Analyzers", "Vendors", "vendor-contract.mjs")));
        await program.Run("node", script, Path.Combine(program.Folder, "tsconfig.json"), Path.Combine(program.Folder, "inputs.sha256"), "--inputs");
        var before = await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256"));
        await File.AppendAllTextAsync(script, "\n;");
        await program.Run("node", script, Path.Combine(program.Folder, "tsconfig.json"), Path.Combine(program.Folder, "inputs.sha256"), "--inputs");
        Assert.That(await File.ReadAllTextAsync(Path.Combine(program.Folder, "inputs.sha256")), Is.Not.EqualTo(before));
    }

    private static void Project(VendorProgram program)
    {
        var target = SecurityElement.Escape(Path.Combine(VendorProgram.Root, "Alis.Reactive.Analyzers", "Vendors", "VendorContract.targets"));
        var assets = SecurityElement.Escape(Path.Combine(VendorProgram.Root, "tests", "Alis.Reactive.AnalyzerTests", "obj", "project.assets.json"));
        program.Write("Consumer.cs", "public sealed class Consumer;");
        program.Write("Consumer.csproj", $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <IsPackable>false</IsPackable>
                <ProvideCommandLineArgs>true</ProvideCommandLineArgs>
                <ProjectAssetsFile>{{assets}}</ProjectAssetsFile>
                <VendorContractTsConfig>tsconfig.json</VendorContractTsConfig>
              </PropertyGroup>
              <Import Project="{{target}}" />
              <Target Name="RecordCompilerInputs" AfterTargets="CoreCompile">
                <WriteLinesToFile File="compiled.txt" Lines="@(AdditionalFiles)" Overwrite="true" />
                <WriteLinesToFile File="compiler-arguments.txt" Lines="@(CscCommandLineArgs)" Overwrite="true" />
              </Target>
            </Project>
            """);
    }
}
