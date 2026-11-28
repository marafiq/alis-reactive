using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorDocumentsItsPayload
{
    [RefusedCase]
    public async Task an_unused_alias_argument_is_the_optional_first_parameter()
    {
        using var program = new VendorProgram("""
            export interface Changed { value: number; }
            export type Emit<T> = (arg?: any, later?: string) => void;
            export interface Gauge { changed: Emit<Changed>; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p0/value/0\tparameter\tapi:Gauge@1.changed/s0/p0\tvalue\t0\tnull\t\t"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p0/value/1\tparameter\tapi:Gauge@1.changed/s0/p0\tvalue\t1\tobject\tapi:Changed@1\t"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p1/value/1\tparameter\tapi:Gauge@1.changed/s0/p1\tvalue\t1\tother\t\tstring"));
    }

    [AcceptedCase]
    public async Task an_alias_with_a_typed_first_parameter_keeps_that_parameter()
    {
        using var program = new VendorProgram("""
            export interface Changed { value: number; }
            export type Emit<T> = (arg: string) => void;
            export interface Gauge { changed: Emit<Changed>; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p0/value/0\tparameter\tapi:Gauge@1.changed/s0/p0\tvalue\t0\tother\t\tstring"));
    }

    [AcceptedCase]
    public async Task an_alias_that_uses_its_parameter_keeps_its_resolved_function()
    {
        using var program = new VendorProgram("""
            export interface Changed { value: number; }
            export type Emit<T> = (arg: any, later: T) => void;
            export interface Gauge { changed: Emit<Changed>; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p0/value/0\tparameter\tapi:Gauge@1.changed/s0/p0\tvalue\t0\tother\t\tany"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.changed/s0/p1/value/0\tparameter\tapi:Gauge@1.changed/s0/p1\tvalue\t0\tobject\tapi:Changed@1\t"));
    }

    [RefusedCase]
    public async Task documented_null_adds_read_and_write_null_with_its_package_file_and_line()
    {
        using var program = new VendorProgram("import { Gauge } from '@care/gauge'; export type Value = Gauge;");
        program.Write("node_modules/@care/gauge/package.json", """{"name":"@care/gauge","types":"index.d.ts"}""");
        program.Write("node_modules/@care/gauge/index.d.ts", """
            export declare class Gauge {
                /** @default null */
                value: number;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\t%40care%2Fgauge:Gauge@1.value\t%40care%2Fgauge:Gauge@1\tvalue\tproperty\tfalse\ttrue\ttrue\tfalse"));
        Assert.That(table, Does.Contain("default-null\t%40care%2Fgauge:Gauge@1.value\t@care/gauge\tindex.d.ts\t2\ttrue\ttrue"));
        Assert.That(table, Does.Contain("holds\t%40care%2Fgauge:Gauge@1.value/read/0\tmember\t%40care%2Fgauge:Gauge@1.value\tread\t0\tnull\t\t"));
    }

    [AcceptedCase]
    public async Task a_non_null_default_adds_no_null()
    {
        using var program = new VendorProgram("""
            export interface Gauge {
                /** @default 0 */
                value: number;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task a_documented_readonly_member_adds_only_read_null()
    {
        using var program = new VendorProgram("""
            export interface Gauge {
                /** @default null */
                readonly value: number;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("default-null\tapi:Gauge@1.value\tapi\tapi.ts\t2\ttrue\tfalse"));
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\treadonly\tfalse\ttrue\tfalse\tfalse"));
    }
}
