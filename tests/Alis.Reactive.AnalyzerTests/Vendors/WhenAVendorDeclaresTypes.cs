using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorDeclaresTypes
{
    [AcceptedCase]
    public async Task modules_and_nested_names_keep_each_declaration()
    {
        using var program = new VendorProgram("""
            declare module 'care' { export interface Gauge { value: number; } }
            declare namespace Ward { interface Gauge { value: string; } }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tcare:Gauge@1\tcare\tGauge\tinterface\tGauge"));
        Assert.That(table, Does.Contain("type\tapi:Ward.Gauge@1\tapi\tWard.Gauge\tinterface\tWard.Gauge"));
    }

    [AcceptedCase]
    public async Task aliases_and_enums_are_types_of_their_module()
    {
        using var program = new VendorProgram("export type Label = string; export enum State { Ready, Running }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tapi:Label@1\tapi\tLabel\talias\tLabel"));
        Assert.That(table, Does.Contain("type\tapi:State@1\tapi\tState\tenum\tState"));
    }

    [RefusedCase]
    public async Task two_files_that_declare_one_name_keep_two_declarations()
    {
        using var program = new VendorProgram("import './node_modules/care/a'; import './node_modules/care/b';");
        program.Write("node_modules/care/a.d.ts", "export interface Gauge { first: number; }");
        program.Write("node_modules/care/b.d.ts", "export interface Gauge { second: string; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tcare:Gauge@1\tcare\tGauge\tinterface\tGauge"));
        Assert.That(table, Does.Contain("type\tcare:Gauge@2\tcare\tGauge\tinterface\tGauge"));
        Assert.That(table, Does.Contain("member\tcare:Gauge@2.second\tcare:Gauge@2\tsecond\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task merged_declarations_keep_each_own_overload_separate()
    {
        using var program = new VendorProgram("import './node_modules/care/a'; import './node_modules/care/b';");
        program.Write("node_modules/care/a.d.ts", "interface Gauge { read(value: string): string; }");
        program.Write("node_modules/care/b.d.ts", "interface Gauge { read(value: number): number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tcare:Gauge@1.read/s0/returns/0\tsignature\tcare:Gauge@1.read/s0\treturns\t0\tother\t\tstring"));
        Assert.That(table, Does.Contain("holds\tcare:Gauge@2.read/s1/returns/0\tsignature\tcare:Gauge@2.read/s1\treturns\t0\tother\t\tnumber"));
        Assert.That(table, Does.Contain("signature\tcare:Gauge@2.read/s1\tmember\tcare:Gauge@2.read\t1\tnone\tfalse\tfalse"));
    }

    [AcceptedCase]
    public async Task an_own_member_is_separate_from_the_base_that_declares_its_other_member()
    {
        using var program = new VendorProgram("export interface Base { inherited: number; } export interface Gauge extends Base { own: string; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("base\tapi:Gauge@1\t0\tapi:Base@1"));
        Assert.That(table, Does.Contain("member\tapi:Base@1.inherited\tapi:Base@1\tinherited\tproperty\tfalse\tfalse\tfalse\tfalse"));
        Assert.That(table, Does.Not.Contain("member\tapi:Gauge@1.inherited\tapi:Gauge@1\tinherited\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task a_generic_base_is_read_at_its_heritage_type_and_flattened()
    {
        using var program = new VendorProgram("export declare class Base<T> { value: T; again(): this; } export declare class Gauge extends Base<string> {}");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("base\tapi:Gauge@1\t0\tapi:Base#1"));
        Assert.That(table, Does.Contain("type\tapi:Base#1\tapi\tBase#1\tlisted\tBase<string>"));
        Assert.That(table, Does.Contain("holds\tapi:Base#1.again/s0/returns/0\tsignature\tapi:Base#1.again/s0\treturns\t0\tobject\tapi:Base#1\t"));
        Assert.That(table, Does.Contain("holds\tapi:Base#1.value/read/0\tmember\tapi:Base#1.value\tread\t0\tother\t\tstring"));
    }

    [RefusedCase]
    public async Task a_free_type_parameter_is_kept_as_text()
    {
        using var program = new VendorProgram("export interface Box<T> { value: T; } export interface Gauge { read<T>(): Box<T>; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s0/returns/0\tsignature\tapi:Gauge@1.read/s0\treturns\t0\tother\t\tBox<T>"));
        Assert.That(table, Does.Contain("holds\tapi:Box@1.value/read/0\tmember\tapi:Box@1.value\tread\t0\tother\t\tT"));
    }

    [AcceptedCase]
    public async Task an_object_literal_has_its_own_members_and_display_text()
    {
        using var program = new VendorProgram("export interface Gauge { value: { count: number }; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tapi:%7B%7D#1\tapi\t{}#1\tlisted\t{ count: number; }"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.value/read/0\tmember\tapi:Gauge@1.value\tread\t0\tobject\tapi:%7B%7D#1\t"));
        Assert.That(table, Does.Contain("member\tapi:%7B%7D#1.count\tapi:%7B%7D#1\tcount\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [AcceptedCase]
    public async Task an_array_names_its_element_and_an_index_names_its_value()
    {
        using var program = new VendorProgram("""
            declare global { interface Array<T> { [index: number]: T; } }
            export interface Item { name: string; }
            export interface Gauge { items: Item[]; [name: string]: Item[]; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.items/read/0\tmember\tapi:Gauge@1.items\tread\t0\titems\tapi:Item@1\t"));
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.%5Bindex%5D\tapi:Gauge@1\t[index]\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [AcceptedCase]
    public async Task escaped_text_cannot_add_a_column_or_a_line()
    {
        using var program = new VendorProgram("export interface Gauge { 'a\\tb\\nc\\r\\\\': number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.a%09b%0Ac%0D%5C\tapi:Gauge@1\ta\\tb\\nc\\r\\\\\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [AcceptedCase]
    public async Task root_file_order_does_not_change_the_table()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        program.Write("other.ts", "export interface Other { value: string; }");
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"noLib":true,"types":[]},"files":["api.ts","other.ts"]}""");
        var first = await program.Extract();
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"noLib":true,"types":[]},"files":["other.ts","api.ts"]}""");
        var second = await program.Extract();
        Assert.That(second, Is.EqualTo(first));
        Assert.That(second[0], Is.EqualTo("format\t1"));
        Assert.That(second[1..], Is.Ordered.Using<string>(StringComparer.Ordinal));
    }

    [RefusedCase]
    public async Task a_consumer_compiler_resolution_failure_does_not_use_the_scripts_compiler()
    {
        using var program = new VendorProgram("export interface Gauge { value: number; }");
        program.Write("node_modules/typescript/package.json", """{"name":"typescript","main":"missing-compiler.cjs"}""");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await program.Extract());
        Assert.That(exception!.Message, Does.Contain("missing-compiler.cjs"));
        Assert.That(File.Exists(program.Table), Is.False);
    }
}
