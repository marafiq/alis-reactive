using Alis.Reactive.PlanModel;
using System.Reflection;

Console.WriteLine("=== INDEPENDENT REPRO against REAL linked source ===\n");

// M3: witness s = None
{
    var (ok1, m1) = Ops.Merge(S.Any, S.None);
    var (ok2, m2) = Ops.Merge(S.None, S.Any);
    Console.WriteLine($"[M3] s=None : merge(Any,None) -> ok={ok1} merged={m1}  | merge(None,Any) -> ok={ok2} merged={m2}");
    var holds = ok1 && m1 == "none" && ok2 && m2 == "none";
    Console.WriteLine($"      LAW BROKEN: {!holds}\n");
}

// A2: witness s = None
{
    var l = Ops.Acc(S.Any, S.None);
    var r = Ops.Acc(S.None, S.Any);
    Console.WriteLine($"[A2] s=None : accepts(Any,None)={l}  accepts(None,Any)={r}");
    Console.WriteLine($"      LAW BROKEN: {!(l && r)}\n");
}

// M5: a=nullable<array<string>>, b=array<string>, c=array<any>
{
    var a = S.Nul(S.Arr(S.Str));
    var b = S.Arr(S.Str);
    var c = S.Arr(S.Any);
    var (okAB, mAB) = Ops.Merge(a, b);
    var (okBC, mBC) = Ops.Merge(b, c);
    Console.WriteLine($"[M5] a=nullable<array<string>> b=array<string> c=array<any>");
    Console.WriteLine($"      a*b -> ok={okAB} = {mAB}   ;  b*c -> ok={okBC} = {mBC}");
    if (okAB && okBC)
    {
        var (okL, mL) = Ops.Merge(S.Nul(S.Arr(S.Str)), c);   // (a*b)*c with a*b = nullable<array<string>>
        var (okR, mR) = Ops.Merge(a, S.Arr(S.Str));          // a*(b*c) with b*c = array<string>
        Console.WriteLine($"      (a*b)*c -> ok={okL} = {mL}");
        Console.WriteLine($"      a*(b*c) -> ok={okR} = {mR}");
        Console.WriteLine($"      LAW BROKEN: {okL != okR || (okL && mL != mR)}\n");
    }
}

// M5 (latest shrunk witness): a=boolean, b=nullable<boolean>, c=nullable<nullable<boolean>>
{
    var a = S.Bool;
    var b = S.Nul(S.Bool);
    var c = S.Nul(S.Nul(S.Bool));
    Ops.Assoc("M5'", a, b, c, "a=boolean b=nullable<boolean> c=nullable<nullable<boolean>>");
}

// A4: a=string, b=any, c=boolean (and a=string,b=any,c=number — both shrink targets)
{
    var ab = Ops.Acc(S.Str, S.Any);
    var bc1 = Ops.Acc(S.Any, S.Bool);
    var ac1 = Ops.Acc(S.Str, S.Bool);
    var bc2 = Ops.Acc(S.Any, S.Num);
    var ac2 = Ops.Acc(S.Str, S.Num);
    Console.WriteLine($"[A4] a=string b=any : accepts(a,b)={ab}");
    Console.WriteLine($"      c=boolean: accepts(b,c)={bc1} accepts(a,c)={ac1} => BROKEN {ab && bc1 && !ac1}");
    Console.WriteLine($"      c=number : accepts(b,c)={bc2} accepts(a,c)={ac2} => BROKEN {ab && bc2 && !ac2}\n");
}

// C1 (latest shrunk witness): a=array<array<object{b:raw}>>, b=array<array<object{}>>
{
    var inner = S.Obj(new() { { "b", S.Raw } });
    var a = S.Arr(S.Arr(inner));
    var b = S.Arr(S.Arr(S.Obj(new())));   // object{} = closed object with zero fields
    var (ok, m, merged) = Ops.MergeShape(a, b);
    Console.WriteLine($"[C1] a=array<array<object{{b:raw}}>> b=array<array<object{{}}>>");
    Console.WriteLine($"      merge -> ok={ok} merged={m}");
    if (ok && merged != null)
    {
        var accA = Ops.Acc(merged, a);
        var accB = Ops.Acc(merged, b);
        Console.WriteLine($"      accepts(m,a)={accA}  accepts(m,b)={accB}");
        Console.WriteLine($"      LAW BROKEN: {!(accA && accB)}\n");
    }
}

static class S
{
    static readonly Type T = typeof(Shape);
    static object Inv(string n, params object[] a) =>
        T.GetMethod(n, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, a)!;
    static Shape Fld(string n) =>
        (Shape)T.GetField(n, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
    public static Shape Str => Fld("String");
    public static Shape Num => Fld("Number");
    public static Shape Bool => Fld("Boolean");
    public static Shape Date => Fld("Date");
    public static Shape Raw => Fld("Raw");
    public static Shape Any => Fld("Any");
    public static Shape None => Fld("None");
    public static Shape Arr(Shape i) => (Shape)Inv("ArrayOf", i);
    public static Shape Nul(Shape i) => (Shape)Inv("Nullable", i);
    public static Shape Obj(Dictionary<string, Shape> f) => (Shape)Inv("ObjectOf", f);
}

static class Ops
{
    static readonly Type T = typeof(ShapeContractCompatibility);

    public static (bool ok, string merged) Merge(Shape a, Shape b)
    {
        var (ok, m, _) = MergeShape(a, b);
        return (ok, m);
    }

    public static (bool ok, string merged, Shape? shape) MergeShape(Shape a, Shape b)
    {
        var args = new object?[] { a, b, null };
        var ok = (bool)T.GetMethod("TryMergeContracts", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, args)!;
        var shape = ok ? (Shape)args[2]! : null;
        return (ok, ok ? shape!.DescribeContract() : "CONFLICT", shape);
    }

    public static bool Acc(Shape e, Shape a) =>
        (bool)T.GetMethod("CanAccept", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object?[] { e, a })!;

    public static void Assoc(string id, Shape a, Shape b, Shape c, string label)
    {
        var (okAB, _, ab) = MergeShape(a, b);
        var (okBC, _, bc) = MergeShape(b, c);
        Console.WriteLine($"[{id}] {label}");
        Console.WriteLine($"      a*b ok={okAB} ; b*c ok={okBC}");
        if (!okAB || !okBC) { Console.WriteLine("      (premise needs both inner merges defined)\n"); return; }
        var (okL, mL, _) = MergeShape(ab!, c);
        var (okR, mR, _) = MergeShape(a, bc!);
        Console.WriteLine($"      (a*b)*c -> ok={okL} = {mL}");
        Console.WriteLine($"      a*(b*c) -> ok={okR} = {mR}");
        Console.WriteLine($"      LAW BROKEN: {okL != okR || (okL && mL != mR)}\n");
    }
}
