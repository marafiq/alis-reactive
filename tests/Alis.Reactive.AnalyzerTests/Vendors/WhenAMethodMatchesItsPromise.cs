using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAMethodMatchesItsPromise
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Method = Type + "member\tapi:Gauge@1.read\tapi:Gauge@1\tread\tmethod\tfalse\tfalse\tfalse\ttrue\n";
    private const string Sync = Method + "signature\tread/s0\tmember\tapi:Gauge@1.read\t0\tnone\tfalse\tfalse\n";
    private const string Async = Method + "signature\tread/s0\tmember\tapi:Gauge@1.read\t0\tpromise\tfalse\tfalse\n";

    [RefusedCase]
    public Task void_method_refuses_async()
        => InGauge("public void Read() => {|#0:Method|}();", Async, "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task void_methodasync_accepts_async()
        => InGauge("public void Read() => MethodAsync();", Async);

    [RefusedCase]
    public Task void_methodasync_refuses_sync()
        => InGauge("public void Read() => {|#0:MethodAsync|}();", Sync, "Declaration member 'Gauge.Read' uses MethodAsync for 'read', whose vendor signatures return no promise; use Method");

    [AcceptedCase]
    public Task void_method_accepts_sync()
        => InGauge("public void Read() => Method();", Sync);

    [RefusedCase]
    public Task value_method_refuses_async()
        => InGauge("public MethodResult<int> Read() => {|#0:Method|}<int>();", Async, "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task value_methodasync_accepts_async()
        => InGauge("public MethodResult<int> Read() => MethodAsync<int>();", Async);

    [RefusedCase]
    public Task value_methodasync_refuses_sync()
        => InGauge("public MethodResult<int> Read() => {|#0:MethodAsync|}<int>();", Sync, "Declaration member 'Gauge.Read' uses MethodAsync for 'read', whose vendor signatures return no promise; use Method");

    [AcceptedCase]
    public Task value_method_accepts_sync()
        => InGauge("public MethodResult<int> Read() => Method<int>();", Sync);

    [RefusedCase]
    public Task void_with_arguments_method_refuses_async()
        => InGauge("public void Read() => {|#0:Method|}([]);", Async, "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task void_with_arguments_methodasync_accepts_async()
        => InGauge("public void Read() => MethodAsync([]);", Async);

    [RefusedCase]
    public Task void_with_arguments_methodasync_refuses_sync()
        => InGauge("public void Read() => {|#0:MethodAsync|}([]);", Sync, "Declaration member 'Gauge.Read' uses MethodAsync for 'read', whose vendor signatures return no promise; use Method");

    [AcceptedCase]
    public Task void_with_arguments_method_accepts_sync()
        => InGauge("public void Read() => Method([]);", Sync);

    [RefusedCase]
    public Task value_with_arguments_method_refuses_async()
        => InGauge("public MethodResult<int> Read() => {|#0:Method|}<int>([]);", Async, "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task value_with_arguments_methodasync_accepts_async()
        => InGauge("public MethodResult<int> Read() => MethodAsync<int>([]);", Async);

    [RefusedCase]
    public Task value_with_arguments_methodasync_refuses_sync()
        => InGauge("public MethodResult<int> Read() => {|#0:MethodAsync|}<int>([]);", Sync, "Declaration member 'Gauge.Read' uses MethodAsync for 'read', whose vendor signatures return no promise; use Method");

    [AcceptedCase]
    public Task value_with_arguments_method_accepts_sync()
        => InGauge("public MethodResult<int> Read() => Method<int>([]);", Sync);

    [RefusedCase]
    public Task a_mixed_return_still_requires_awaiting()
        => InGauge("public void Read() => {|#0:Method|}();", Async.Replace("\tpromise\t", "\tmixed\t"), "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task a_mixed_return_uses_the_async_door()
        => InGauge("public void Read() => MethodAsync();", Async.Replace("\tpromise\t", "\tmixed\t"));

    [RefusedCase]
    public Task a_later_promised_overload_is_not_ignored()
        => InGauge("public void Read() => {|#0:Method|}();", Sync + "signature\tread/s1\tmember\tapi:Gauge@1.read\t1\tpromise\tfalse\tfalse\n", "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task overloads_with_a_promise_use_the_async_door()
        => InGauge("public void Read() => MethodAsync();", Sync + "signature\tread/s1\tmember\tapi:Gauge@1.read\t1\tpromise\tfalse\tfalse\n");

    [RefusedCase]
    public Task a_callable_property_can_return_a_promise()
        => InGauge("public void Read() => {|#0:Method|}();", Async.Replace("\tmethod\t", "\tproperty\t"), "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task a_callable_property_can_be_awaited()
        => InGauge("public void Read() => MethodAsync();", Async.Replace("\tmethod\t", "\tproperty\t"));

    [AcceptedCase]
    public Task a_property_door_is_not_a_method_promise()
        => InGauge("public ReadOnly<int> Read => ReadOnly<int>();", Async);

    [AcceptedCase]
    public Task an_absent_member_is_left_to_alis016()
        => InGauge("public void Read() => MethodAsync();", Type);

    [AcceptedCase]
    public Task a_noncallable_property_is_left_to_alis018()
        => InGauge("public void Read() => MethodAsync();", Sync.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\n", "\tfalse\n"));

    [AcceptedCase]
    public Task no_table_means_no_promise_check()
        => InGauge("public void Read() => Method();", null);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public void Read() => Method(name: {|CS0103:Unknown|});", Async);

    [AcceptedCase]
    public Task a_missing_signature_does_not_prove_a_return_mode()
        => InGauge("public void Read() => MethodAsync();", Method);

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_method_doors()
    {
        var test = new Case<ALIS017MethodMatchesPromise>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public void Read() { } }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Async));
        return test.RunAsync();
    }

    [RefusedCase]
    public Task a_partial_method_is_reported_once_at_its_definition()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial void {|#0:Read|}(); } public sealed partial class Gauge { public partial void Read() => Method(); }", Async, "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [RefusedCase]
    public Task an_inherited_method_keeps_its_promise()
        => InGauge("public void Read() => {|#0:Method|}();", Type + Async.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n", "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");

    [AcceptedCase]
    public Task a_partial_method_can_await_its_promise()
        => Source("""[Mirrors("api", "Gauge")] public sealed partial class Gauge : JsObject { public partial void Read(); } public sealed partial class Gauge { public partial void Read() => MethodAsync(); }""", Async);

    [RefusedCase]
    public async Task a_promise_from_the_extractor_refuses_the_sync_door()
    {
        using var program = new VendorProgram("interface Promise<T> { then(done: (value: T) => unknown): unknown; } export interface Gauge { read(): Promise<number>; }");
        var table = await program.Extract();
        await InGauge("public void Read() => {|#0:Method|}();", string.Join("\n", table), "Declaration member 'Gauge.Read' uses Method for 'read', whose vendor signatures return a promise; use MethodAsync");
    }

    [AcceptedCase]
    public async Task a_promise_from_the_extractor_accepts_the_async_door()
    {
        using var program = new VendorProgram("interface Promise<T> { then(done: (value: T) => unknown): unknown; } export interface Gauge { read(): Promise<number>; }");
        var table = await program.Extract();
        await InGauge("public void Read() => MethodAsync();", string.Join("\n", table));
    }

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS017MethodMatchesPromise>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(ALIS017MethodMatchesPromise.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
