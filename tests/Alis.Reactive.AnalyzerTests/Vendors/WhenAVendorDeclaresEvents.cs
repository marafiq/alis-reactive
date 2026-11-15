using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorDeclaresEvents
{
    [AcceptedCase]
    public async Task each_literal_event_reaches_only_its_signatures_listener()
    {
        using var program = new VendorProgram("""
            export interface Gauge {
                on(name: 'alarm', listener: (count: number) => void): void;
                on(name: 'silenced', listener: (reason: string) => void): void;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("event\tapi:Gauge@1.on\tapi:Gauge@1.on/s0\tname\talarm\tapi:function#1/s0"));
        Assert.That(table, Does.Contain("event\tapi:Gauge@1.on\tapi:Gauge@1.on/s1\tname\tsilenced\tapi:function#2/s0"));
        Assert.That(table, Does.Contain("holds\tapi:function#2/s0/p0/value/0\tparameter\tapi:function#2/s0/p0\tvalue\t0\tother\t\tstring"));
    }

    [RefusedCase]
    public async Task a_string_parameter_names_no_event()
    {
        using var program = new VendorProgram("export interface Gauge { on(name: string, listener: () => void): void; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.on\tapi:Gauge@1\ton\tmethod\tfalse\tfalse\tfalse\ttrue"));
        Assert.That(table, Has.None.StartsWith("event\t"));
    }

    [AcceptedCase]
    public async Task an_event_map_is_referenced_with_its_bases()
    {
        using var program = new VendorProgram("""
            export interface Base { alarm: number; }
            export interface Events extends Base { silenced: string; }
            export interface Gauge { on<K extends keyof Events>(name: K, listener: (value: Events[K]) => void): void; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("base\tapi:Events@1\t0\tapi:Base@1"));
        Assert.That(table, Does.Contain("event\tapi:Gauge@1.on\tapi:Gauge@1.on/s0\tkeys\tapi:Events@1\tapi:function#1/s0"));
    }

    [RefusedCase]
    public async Task an_iterator_names_its_yield_and_excludes_its_final_return()
    {
        using var program = new VendorProgram("""
            declare global { interface SymbolConstructor { readonly iterator: unique symbol; } var Symbol: SymbolConstructor; }
            export interface Item { name: string; }
            export interface Gauge {
                [Symbol.iterator](): { next(): { done?: false; value: Item } | { done: true; value: string } };
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.%5BSymbol.iterator%5D\tapi:Gauge@1\t[Symbol.iterator]\tmethod\tfalse\tfalse\tfalse\ttrue"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.%5BSymbol.iterator%5D/s0/yields/0\tsignature\tapi:Gauge@1.%5BSymbol.iterator%5D/s0\tyields\t0\tobject\tapi:Item@1\t"));
        Assert.That(table, Does.Not.Contain("holds\tapi:Gauge@1.%5BSymbol.iterator%5D/s0/yields/1\tsignature\tapi:Gauge@1.%5BSymbol.iterator%5D/s0\tyields\t1\tother\t\tstring"));
    }

    [AcceptedCase]
    public async Task a_library_augmentation_is_an_own_member_of_the_type_it_augments()
    {
        using var program = new VendorProgram("export type Collection = HTMLCollection;");
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"lib":["dom","dom.iterable","es2022"],"types":[]},"files":["api.ts"]}""");
        var table = await program.Extract();
        Assert.That(Array.Find(table, line => line.StartsWith("member\tlib.dom:HTMLCollectionBase@1.%5BSymbol.iterator%5D\t", StringComparison.Ordinal)), Is.EqualTo("member\tlib.dom:HTMLCollectionBase@1.%5BSymbol.iterator%5D\tlib.dom:HTMLCollectionBase@1\t[Symbol.iterator]\tmethod\tfalse\tfalse\tfalse\ttrue"));
        Assert.That(Array.Find(table, line => line.StartsWith("base\tlib.dom:HTMLCollection@1\t0\t", StringComparison.Ordinal)), Is.EqualTo("base\tlib.dom:HTMLCollection@1\t0\tlib.dom:HTMLCollectionBase@1"));
    }

    [AcceptedCase]
    public async Task a_type_without_an_iterator_keeps_only_its_own_members()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse"));
        Assert.That(table, Has.None.Contains("[Symbol.iterator]"));
    }
}
