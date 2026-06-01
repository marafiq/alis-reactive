using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

// Harness B — hand-rolled deterministic seeded PBT over the REAL Shape algebra.
// Lives in the SAME namespace + assembly as the linked real source, so internal
// factories (Shape.String, Shape.ArrayOf, ShapeContractCompatibility.*) are reachable
// directly — no reflection, no InternalsVisibleTo.
namespace Alis.Reactive.PlanModel;

/// <summary>A deterministic recursive Shape generator + hand shrinker.</summary>
internal sealed class ShapeGen
{
    private readonly Random _rng;
    internal ShapeGen(int seed) => _rng = new Random(seed);

    // Scalars usable anywhere. "none" excluded here because array-item / nullable-inner
    // reject none by construction; we inject none separately at legal positions.
    private static readonly Func<Shape>[] NonNoneScalars =
    {
        () => Shape.String, () => Shape.Number, () => Shape.Boolean,
        () => Shape.Date,   () => Shape.Raw,    () => Shape.Any,
    };

    private static readonly string[] FieldNames = { "a", "b", "c", "id", "name", "qty", "items" };

    /// <summary>Top-level shape may be none; nested positions that forbid none are guarded.</summary>
    internal Shape Next(int maxDepth)
    {
        // 1-in-8 top-level none.
        if (_rng.Next(8) == 0) return Shape.None;
        return NextNonNone(maxDepth);
    }

    private Shape NextNonNone(int depth)
    {
        // At depth 0 only scalars (no recursion budget left).
        if (depth <= 0) return NonNoneScalars[_rng.Next(NonNoneScalars.Length)]();

        int pick = _rng.Next(10);
        switch (pick)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return NonNoneScalars[_rng.Next(NonNoneScalars.Length)]();
            case 4:
            case 5:
                return Shape.ArrayOf(NextNonNone(depth - 1)); // item != none by construction
            case 6:
                return Shape.Nullable(NextNonNone(depth - 1)); // inner != none by construction
            case 7:
                return Shape.OpenObject();
            default:
                return NextClosedObject(depth);
        }
    }

    private Shape NextClosedObject(int depth)
    {
        int n = _rng.Next(0, 4); // 0..3 fields
        var dict = new Dictionary<string, Shape>(StringComparer.Ordinal);
        // shuffle field names, take n distinct
        var names = FieldNames.OrderBy(_ => _rng.Next()).Take(n).ToArray();
        foreach (var name in names)
        {
            // object fields MAY legally be none (ObjectOf only null-checks the dict).
            Shape fieldShape = _rng.Next(12) == 0 ? Shape.None : NextNonNone(depth - 1);
            dict[name] = fieldShape;
        }
        return Shape.ObjectOf(dict);
    }

    // ---- Shrinker: produce strictly-simpler candidates of a shape. ----
    // Order: replace composite by child, drop a field, swap scalar to a "smaller" scalar.
    internal static IEnumerable<Shape> Shrink(Shape s)
    {
        // composite -> child
        if (s.TryGetArrayItemShape(out var item)) { yield return item; }
        if (s.TryGetNullableInnerShape(out var inner)) { yield return inner; }
        if (s.TryGetObjectContract(out var contract) && !ReferenceEquals(contract, ShapeObjectContract.Open))
        {
            var fields = contract.Fields;
            // drop each field one at a time
            foreach (var key in fields.Keys.ToArray())
            {
                var reduced = new Dictionary<string, Shape>(StringComparer.Ordinal);
                foreach (var kv in fields) if (kv.Key != key) reduced[kv.Key] = kv.Value;
                yield return reduced.Count == 0 ? Shape.OpenObject() : Shape.ObjectOf(reduced);
            }
            // shrink each field value
            foreach (var key in fields.Keys.ToArray())
            {
                foreach (var sv in Shrink(fields[key]))
                {
                    if (sv.IsNone) continue; // keep object field non-none for stable shrink targets
                    var rep = new Dictionary<string, Shape>(StringComparer.Ordinal);
                    foreach (var kv in fields) rep[kv.Key] = kv.Key == key ? sv : kv.Value;
                    yield return Shape.ObjectOf(rep);
                }
            }
        }
        // scalar swaps toward the simplest scalar (string)
        switch (s.Kind)
        {
            case "number": case "boolean": case "date": case "raw": case "any":
                yield return Shape.String;
                break;
        }
    }
}

internal static class Program
{
    // ---- rendering ----
    internal static string Render(Shape s)
    {
        if (s.TryGetArrayItemShape(out var item)) return "array<" + Render(item) + ">";
        if (s.TryGetNullableInnerShape(out var inner)) return "nullable<" + Render(inner) + ">";
        if (s.TryGetObjectContract(out var c))
        {
            if (c.Fields.Count == 0) return c.AllowsAdditionalFields ? "object<open>" : "object{}";
            var sb = new StringBuilder("object{");
            bool first = true;
            foreach (var kv in c.Fields) { if (!first) sb.Append(", "); sb.Append(kv.Key).Append(':').Append(Render(kv.Value)); first = false; }
            if (c.AllowsAdditionalFields) sb.Append(", ...");
            sb.Append('}');
            return sb.ToString();
        }
        return s.Kind;
    }

    internal static string RenderVal(object? v)
    {
        switch (v)
        {
            case null: return "null";
            case bool b: return b ? "true" : "false";
            case double d:
                if (double.IsNaN(d)) return "NaN";
                if (double.IsPositiveInfinity(d)) return "Infinity";
                if (double.IsNegativeInfinity(d)) return "-Infinity";
                return d.ToString("R", CultureInfo.InvariantCulture);
            case string str: return "\"" + str + "\"";
            case JsDate jd: return "Date(" + jd.EpochMs.ToString("R", CultureInfo.InvariantCulture) + ")";
            case IReadOnlyList<object?> arr: return "[" + string.Join(",", arr.Select(RenderVal)) + "]";
            case IReadOnlyDictionary<string, object?> obj:
                return "{" + string.Join(",", obj.Select(kv => kv.Key + ":" + RenderVal(kv.Value))) + "}";
            default: return v.ToString() ?? "?";
        }
    }

    // structural value equality for P2/P3 (compares the JS-modelled CLR values)
    internal static bool ValEq(object? a, object? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        if (a is double da && b is double db)
            return (double.IsNaN(da) && double.IsNaN(db)) || da.Equals(db);
        if (a is bool ba && b is bool bb) return ba == bb;
        if (a is string sa && b is string sb) return sa == sb;
        if (a is JsDate ja && b is JsDate jb)
            return (double.IsNaN(ja.EpochMs) && double.IsNaN(jb.EpochMs)) || ja.EpochMs.Equals(jb.EpochMs);
        if (a is IReadOnlyList<object?> la && b is IReadOnlyList<object?> lb)
        {
            if (la.Count != lb.Count) return false;
            for (int i = 0; i < la.Count; i++) if (!ValEq(la[i], lb[i])) return false;
            return true;
        }
        if (a is IReadOnlyDictionary<string, object?> ma && b is IReadOnlyDictionary<string, object?> mb)
        {
            if (ma.Count != mb.Count) return false;
            foreach (var kv in ma)
            {
                if (!mb.TryGetValue(kv.Key, out var ov)) return false;
                if (!ValEq(kv.Value, ov)) return false;
            }
            return true;
        }
        // mixed double-vs-other never equal
        return a.GetType() == b.GetType() && a.Equals(b);
    }

    // ---- exhaustive small-shape enumeration (depth <= 2) for the algebra laws ----
    private static List<Shape> EnumerateDepth(int depth)
    {
        var atoms = new List<Shape>
        {
            Shape.String, Shape.Number, Shape.Boolean, Shape.Date,
            Shape.Raw, Shape.Any, Shape.None, Shape.OpenObject(),
        };
        if (depth <= 0) return atoms;

        var smaller = EnumerateDepth(depth - 1);
        var result = new List<Shape>(atoms);

        // array<s> for s != none from the smaller set
        foreach (var s in smaller)
            if (!s.IsNone) result.Add(Shape.ArrayOf(s));
        // nullable<s> for s != none
        foreach (var s in smaller)
            if (!s.IsNone) result.Add(Shape.Nullable(s));
        // closed objects with one field over a small field shape set (incl none as field value)
        var fieldShapes = new List<Shape> { Shape.String, Shape.Number, Shape.None };
        if (depth >= 2) { fieldShapes.Add(Shape.Nullable(Shape.String)); fieldShapes.Add(Shape.ArrayOf(Shape.Number)); }
        foreach (var fs in fieldShapes)
            result.Add(Shape.ObjectOf(new Dictionary<string, Shape> { { "a", fs } }));
        // two-field closed object (one combination) for richer object accept/merge cases
        result.Add(Shape.ObjectOf(new Dictionary<string, Shape> { { "a", Shape.String }, { "b", Shape.Number } }));
        result.Add(Shape.ObjectOf(new Dictionary<string, Shape> { { "a", Shape.String } }));
        // closed empty object
        result.Add(Shape.ObjectOf(new Dictionary<string, Shape>()));

        // de-dup by structural equality
        var distinct = new List<Shape>();
        foreach (var s in result)
            if (!distinct.Any(x => x.Equals(s))) distinct.Add(s);
        return distinct;
    }

    // ---- merge helper that returns nullable merged ----
    private static (bool ok, Shape? merged) Merge(Shape a, Shape b)
    {
        bool ok = ShapeContractCompatibility.TryMergeContracts(a, b, out var m);
        return (ok, m);
    }

    // ---- value generator for P-laws ----
    private static object? GenValue(Random rng, int depth)
    {
        int pick = rng.Next(depth <= 0 ? 7 : 9);
        switch (pick)
        {
            case 0: return null;
            case 1: return rng.Next(2) == 0;
            case 2:
            {
                int k = rng.Next(8);
                return k switch
                {
                    0 => 0d, 1 => 1d, 2 => -1d, 3 => 42d, 4 => 3.5d,
                    5 => double.NaN, 6 => double.PositiveInfinity, _ => 1234567890123d,
                };
            }
            case 3:
            {
                string[] strs = { "", "hello", "true", "false", "0", "42", "3.5", "2020-01-15", "2020-01-15T10:30:00Z", "not a date", "  " };
                return strs[rng.Next(strs.Length)];
            }
            case 4: return new JsDate(rng.Next(2) == 0 ? 0d : 1579084200000d);
            case 5: return new JsDate(double.NaN);
            case 6:
            {
                // array
                int n = rng.Next(0, 3);
                var arr = new object?[n];
                for (int i = 0; i < n; i++) arr[i] = GenValue(rng, depth - 1);
                return arr;
            }
            case 7:
            {
                // plain object
                int n = rng.Next(0, 3);
                var d = new Dictionary<string, object?>();
                string[] keys = { "a", "b", "c", "name", "qty" };
                foreach (var key in keys.OrderBy(_ => rng.Next()).Take(n))
                    d[key] = GenValue(rng, depth - 1);
                return d;
            }
            default:
                // nested array of objects for depth coverage
                return new object?[] { new Dictionary<string, object?> { { "a", 1d }, { "b", "x" } }, null, "z" };
        }
    }

    // ===================================================================
    // Verdict accumulator
    // ===================================================================
    private sealed class Law
    {
        public string Id = "";
        public int Cases;
        public bool Failed;
        public string Counterexample = "";
        public string Notes = "";
    }

    private static readonly List<Law> Results = new();

    private static Law Begin(string id) { var l = new Law { Id = id }; Results.Add(l); return l; }

    private static void Main()
    {
        if (Environment.GetEnvironmentVariable("VERIFY") == "1") { VerifyWitnesses(); return; }

        const int RandomCases = 12000; // > 10000 per law
        const int MaxDepth = 4;        // gives nesting depth >= 3 routinely

        var exhaustive = EnumerateDepth(2);
        Console.Error.WriteLine($"[harness] exhaustive depth<=2 shape count: {exhaustive.Count}");

        // pre-generate a deterministic shared pool for random algebra laws
        var poolGen = new ShapeGen(0xA11CE);
        var pool = new List<Shape>();
        for (int i = 0; i < 4000; i++) pool.Add(poolGen.Next(MaxDepth));

        RunEquivalence(pool, exhaustive, RandomCases);
        RunMerge(pool, exhaustive, RandomCases);
        RunAccept(pool, exhaustive, RandomCases);
        RunCoherence(pool, exhaustive, RandomCases);
        RunApplyConvert(RandomCases, MaxDepth);
        RunSerialize(pool, exhaustive, RandomCases);
        RunFromClr();

        // emit machine-readable JSON for the orchestrator to lift into the schema
        var report = new
        {
            laws = Results.Select(l => new
            {
                id = l.Id,
                verdict = l.Failed ? "FAILS" : "HOLDS",
                casesChecked = l.Cases,
                counterexample = l.Counterexample,
                notes = l.Notes,
            }),
        };
        Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    // ===================================================================
    // Direct witness reproduction — paste-into-REPL-grade proof that each
    // FAILS verdict reproduces against the linked REAL source, and that the
    // None-corner failures (M3,A2) are None-ONLY (no other s breaks them).
    // ===================================================================
    private static void VerifyWitnesses()
    {
        bool Acc(Shape e, Shape a) => ShapeContractCompatibility.CanAccept(e, a);

        Console.WriteLine("### M3 witness — merge(Any, None) and merge(None, Any)");
        var (m3a_ok, m3a) = Merge(Shape.Any, Shape.None);
        var (m3b_ok, m3b) = Merge(Shape.None, Shape.Any);
        Console.WriteLine($"  merge(Any,None): ok={m3a_ok} res={(m3a == null ? "null/CONFLICT" : Render(m3a))}  (M3 expects = None)");
        Console.WriteLine($"  merge(None,Any): ok={m3b_ok} res={(m3b == null ? "null/CONFLICT" : Render(m3b))}  (M3 expects = None)");
        // is None the ONLY s that breaks merge(Any,s)=s ?
        var ex2 = EnumerateDepth(2);
        int m3OtherBreaks = 0; string m3FirstOther = "";
        foreach (var s in ex2)
        {
            if (s.IsNone) continue;
            var (oa, ma) = Merge(Shape.Any, s);
            var (ob, mb) = Merge(s, Shape.Any);
            if (!oa || !ma!.Equals(s) || !ob || !mb!.Equals(s)) { m3OtherBreaks++; if (m3FirstOther == "") m3FirstOther = Render(s); }
        }
        Console.WriteLine($"  non-None shapes (depth<=2) breaking M3: {m3OtherBreaks} (first: {m3FirstOther}) => None is the sole violator: {m3OtherBreaks == 0}");

        Console.WriteLine("\n### A2 witness — accepts(Any, None) and accepts(None, Any)");
        Console.WriteLine($"  accepts(Any,None)={Acc(Shape.Any, Shape.None)} (A2 expects true)");
        Console.WriteLine($"  accepts(None,Any)={Acc(Shape.None, Shape.Any)} (A2 expects true)");
        int a2OtherBreaks = 0; string a2FirstOther = "";
        foreach (var s in ex2)
        {
            if (s.IsNone) continue;
            if (!Acc(Shape.Any, s) || !Acc(s, Shape.Any)) { a2OtherBreaks++; if (a2FirstOther == "") a2FirstOther = Render(s); }
        }
        Console.WriteLine($"  non-None shapes (depth<=2) breaking A2: {a2OtherBreaks} (first: {a2FirstOther}) => None is the sole violator: {a2OtherBreaks == 0}");

        Console.WriteLine("\n### A4 witness — a=string b=any c=number (transitivity, EXPECTED to fail)");
        Console.WriteLine($"  accepts(string,any)={Acc(Shape.String, Shape.Any)}");
        Console.WriteLine($"  accepts(any,number)={Acc(Shape.Any, Shape.Number)}");
        Console.WriteLine($"  accepts(string,number)={Acc(Shape.String, Shape.Number)}  => transitivity broken: {Acc(Shape.String, Shape.Any) && Acc(Shape.Any, Shape.Number) && !Acc(Shape.String, Shape.Number)}");

        Console.WriteLine("\n### C1 witness — MINIMAL object pair (array wrapper stripped)");
        var oa1 = Shape.ObjectOf(new Dictionary<string, Shape> { { "a", Shape.String } });
        var ob1 = Shape.ObjectOf(new Dictionary<string, Shape> { { "a", Shape.String }, { "b", Shape.Number } });
        var (cok, cm) = Merge(oa1, ob1);
        Console.WriteLine($"  a=object{{a:string}}  b=object{{a:string,b:number}}");
        Console.WriteLine($"  merge(a,b): ok={cok} m={(cm == null ? "CONFLICT" : Render(cm))}");
        if (cok)
            Console.WriteLine($"  accepts(m,a)={Acc(cm!, oa1)}  accepts(m,b)={Acc(cm!, ob1)}  => C1 (both must be true) broken: {!(Acc(cm!, oa1) && Acc(cm!, ob1))}");
        // SHRUNK minimal witness reported by the harness: disjoint single fields.
        var disjA = Shape.ObjectOf(new Dictionary<string, Shape> { { "a", Shape.String } });
        var disjB = Shape.ObjectOf(new Dictionary<string, Shape> { { "b", Shape.String } });
        var (cok2, cm2) = Merge(disjA, disjB);
        Console.WriteLine($"  --- SHRUNK minimal: a=object{{a:string}}  b=object{{b:string}}");
        Console.WriteLine($"  merge(a,b): ok={cok2} m={(cm2 == null ? "CONFLICT" : Render(cm2))}");
        if (cok2)
            Console.WriteLine($"  accepts(m,a)={Acc(cm2!, disjA)}  accepts(m,b)={Acc(cm2!, disjB)}  => C1 broken (merge accepts NEITHER input): {!(Acc(cm2!, disjA) && Acc(cm2!, disjB))}");
    }

    // helper: random pairs/triples from pool + a few from exhaustive
    private static IEnumerable<Shape> AllProbe(List<Shape> pool, List<Shape> exhaustive)
    {
        foreach (var s in exhaustive) yield return s;
        foreach (var s in pool) yield return s;
    }

    // ---------------- EQUIVALENCE ----------------
    private static void RunEquivalence(List<Shape> pool, List<Shape> ex, int n)
    {
        var e1 = Begin("E1"); var e2 = Begin("E2"); var e3 = Begin("E3"); var e4 = Begin("E4");
        var rng = new Random(11);

        // E1 reflexive — over exhaustive + pool
        foreach (var s in AllProbe(pool, ex))
        {
            e1.Cases++;
            if (!s.Equals(s)) { e1.Failed = true; e1.Counterexample = "s=" + Render(s) + " : s.Equals(s)==false"; break; }
        }

        // build a pool with deliberate duplicates so a=b actually fires
        var dupPool = new List<Shape>(pool);
        foreach (var s in ex) dupPool.Add(s);
        // add structural clones so equality hits true
        foreach (var s in ex.Take(40)) dupPool.Add(Clone(s));

        // E2 symmetric, E3 transitive, E4 hash-congruent
        for (int i = 0; i < n; i++)
        {
            var a = dupPool[rng.Next(dupPool.Count)];
            var b = dupPool[rng.Next(dupPool.Count)];
            var c = dupPool[rng.Next(dupPool.Count)];

            e2.Cases++;
            if (a.Equals(b) != b.Equals(a)) { e2.Failed = true; e2.Counterexample = $"a={Render(a)} b={Render(b)}"; }

            e3.Cases++;
            if (a.Equals(b) && b.Equals(c) && !a.Equals(c))
            { e3.Failed = true; e3.Counterexample = $"a={Render(a)} b={Render(b)} c={Render(c)}"; }

            e4.Cases++;
            if (a.Equals(b) && a.GetHashCode() != b.GetHashCode())
            { e4.Failed = true; e4.Counterexample = $"a={Render(a)} b={Render(b)} hashA={a.GetHashCode()} hashB={b.GetHashCode()}"; }
        }

        // exhaustive pairwise for E2/E3/E4 (small set squared)
        for (int i = 0; i < ex.Count; i++)
        for (int j = 0; j < ex.Count; j++)
        {
            var a = ex[i]; var b = ex[j];
            e2.Cases++;
            if (a.Equals(b) != b.Equals(a)) { e2.Failed = true; e2.Counterexample = $"a={Render(a)} b={Render(b)}"; }
            e4.Cases++;
            if (a.Equals(b) && a.GetHashCode() != b.GetHashCode())
            { e4.Failed = true; e4.Counterexample = $"a={Render(a)} b={Render(b)}"; }
        }
        e1.Notes = "reflexive over all probes";
        e2.Notes = "with structural clones to force a=b true";
        e3.Notes = "with structural clones to force chains";
        e4.Notes = "equal-implies-equal-hash";
    }

    private static Shape Clone(Shape s)
    {
        if (s.TryGetArrayItemShape(out var it)) return Shape.ArrayOf(Clone(it));
        if (s.TryGetNullableInnerShape(out var inn)) return Shape.Nullable(Clone(inn));
        if (s.TryGetObjectContract(out var c))
        {
            if (c.AllowsAdditionalFields && c.Fields.Count == 0) return Shape.OpenObject();
            var d = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var kv in c.Fields) d[kv.Key] = Clone(kv.Value);
            return Shape.ObjectOf(d);
        }
        return s.Kind switch
        {
            "string" => Shape.String, "number" => Shape.Number, "boolean" => Shape.Boolean,
            "date" => Shape.Date, "raw" => Shape.Raw, "any" => Shape.Any, "none" => Shape.None,
            _ => s,
        };
    }

    // ---------------- MERGE ----------------
    private static void RunMerge(List<Shape> pool, List<Shape> ex, int n)
    {
        var m1 = Begin("M1"); var m2 = Begin("M2"); var m3 = Begin("M3"); var m4 = Begin("M4"); var m5 = Begin("M5");
        var rng = new Random(22);
        var probe = AllProbe(pool, ex).ToList();

        // M1 idempotent: merge(s,s) defined and = s
        foreach (var s in probe)
        {
            m1.Cases++;
            var (ok, mer) = Merge(s, s);
            if (!ok || !mer!.Equals(s))
            { if (TryShrinkM1(s, out var w)) { m1.Failed = true; m1.Counterexample = w; break; } }
        }

        // exhaustive pairwise M2/M3/M4/M5 portion uses ex; random adds depth
        void CheckPair(Shape a, Shape b)
        {
            m2.Cases++;
            var (okAB, mAB) = Merge(a, b);
            var (okBA, mBA) = Merge(b, a);
            if (okAB != okBA) { if (!m2.Failed) { m2.Failed = true; m2.Counterexample = $"a={Render(a)} b={Render(b)} : defined(a,b)={okAB} defined(b,a)={okBA}"; } }
            else if (okAB && !mAB!.Equals(mBA!)) { if (!m2.Failed) { m2.Failed = true; m2.Counterexample = $"a={Render(a)} b={Render(b)} : merge(a,b)={Render(mAB!)} merge(b,a)={Render(mBA!)}"; } }

            // M3 identity Any
            m3.Cases++;
            var (okAnyS, mAnyS) = Merge(Shape.Any, b);
            var (okSAny, mSAny) = Merge(b, Shape.Any);
            if (!okAnyS || !mAnyS!.Equals(b) || !okSAny || !mSAny!.Equals(b))
            { if (!m3.Failed) { m3.Failed = true; m3.Counterexample = $"s={Render(b)} merge(Any,s) ok={okAnyS} res={(mAnyS==null?"null":Render(mAnyS))} ; merge(s,Any) ok={okSAny} res={(mSAny==null?"null":Render(mSAny))}"; } }

            // M4 annihilator None (using b as s)
            m4.Cases++;
            CheckM4(b, m4);
        }

        foreach (var a in ex) foreach (var b in ex) CheckPair(a, b);
        for (int i = 0; i < n; i++) CheckPair(pool[rng.Next(pool.Count)], pool[rng.Next(pool.Count)]);

        // M5 associativity — random triples + exhaustive subset
        int m5NonVacuous = 0;
        void CheckTriple(Shape a, Shape b, Shape c)
        {
            m5.Cases++;
            var (okAB, mAB) = Merge(a, b);
            var (okBC, mBC) = Merge(b, c);
            if (!okAB || !okBC) return;
            var (okABc, mABc) = Merge(mAB!, c);
            var (okAbc, mAbc) = Merge(a, mBC!);
            if (!okABc || !okAbc) return; // both must be defined for the equality clause
            m5NonVacuous++;
            if (!mABc!.Equals(mAbc!))
            { if (!m5.Failed) { m5.Failed = true; m5.Counterexample = $"a={Render(a)} b={Render(b)} c={Render(c)} : (ab)c={Render(mABc!)} a(bc)={Render(mAbc!)}"; } }
        }

        // exhaustive triples over a small subset to bound cost
        var small = ex.Where(s => DepthOf(s) <= 1).Take(30).ToList();
        foreach (var a in small) foreach (var b in small) foreach (var c in small) CheckTriple(a, b, c);
        for (int i = 0; i < n; i++) CheckTriple(pool[rng.Next(pool.Count)], pool[rng.Next(pool.Count)], pool[rng.Next(pool.Count)]);

        m1.Notes = "merge(s,s)=s"; m2.Notes = "exhaustive pairs + random"; m3.Notes = "Any identity";
        m4.Notes = "None annihilator with s=None corner";
        m5.Notes = $"associativity when all four defined; {m5NonVacuous} non-vacuous triples checked";
    }

    private static bool TryShrinkM1(Shape s, out string witness)
    {
        // shrink to minimal s where merge(s,s) != s
        Shape cur = s;
        bool Fails(Shape x) { var (ok, m) = Merge(x, x); return !ok || !m!.Equals(x); }
        bool reduced = true;
        while (reduced)
        {
            reduced = false;
            foreach (var sm in ShapeGen.Shrink(cur))
                if (Fails(sm)) { cur = sm; reduced = true; break; }
        }
        var (ok2, m2) = Merge(cur, cur);
        witness = $"s={Render(cur)} : merge(s,s) {(ok2 ? "=" + Render(m2!) : "=CONFLICT")} (expected {Render(cur)})";
        return true;
    }

    private static void CheckM4(Shape s, Law m4)
    {
        if (s.IsNone)
        {
            var (ok, m) = Merge(Shape.None, Shape.None);
            if (!ok || !m!.Equals(Shape.None))
            { if (!m4.Failed) { m4.Failed = true; m4.Counterexample = $"merge(None,None) ok={ok} res={(m==null?"null":Render(m))} (expected None)"; } }
            return;
        }
        var (okNs, _) = Merge(Shape.None, s);
        var (okSn, _) = Merge(s, Shape.None);
        if (okNs || okSn)
        { if (!m4.Failed) { m4.Failed = true; m4.Counterexample = $"s={Render(s)} : merge(None,s) defined={okNs} merge(s,None) defined={okSn} (expected both conflict)"; } }
    }

    // ---------------- ACCEPT ----------------
    private static void RunAccept(List<Shape> pool, List<Shape> ex, int n)
    {
        var a1 = Begin("A1"); var a2 = Begin("A2"); var a3 = Begin("A3"); var a4 = Begin("A4");
        var rng = new Random(33);
        var probe = AllProbe(pool, ex).ToList();

        bool Acc(Shape e, Shape a) => ShapeContractCompatibility.CanAccept(e, a);

        // A1 reflexive
        foreach (var s in probe)
        {
            a1.Cases++;
            if (!Acc(s, s)) { a1.Failed = true; a1.Counterexample = $"s={Render(s)} : accepts(s,s)==false"; break; }
        }

        // A2 Any top+bottom ; A3 None edge
        foreach (var s in probe)
        {
            a2.Cases++;
            if (!Acc(Shape.Any, s) || !Acc(s, Shape.Any))
            { if (!a2.Failed) { a2.Failed = true; a2.Counterexample = $"s={Render(s)} accepts(Any,s)={Acc(Shape.Any, s)} accepts(s,Any)={Acc(s, Shape.Any)}"; } }

            a3.Cases++;
            if (s.IsNone) continue; // handled below
            bool isAny = s.Equals(Shape.Any);
            if (!isAny)
            {
                if (Acc(Shape.None, s) || Acc(s, Shape.None))
                { if (!a3.Failed) { a3.Failed = true; a3.Counterexample = $"s={Render(s)} accepts(None,s)={Acc(Shape.None, s)} accepts(s,None)={Acc(s, Shape.None)} (expected both false)"; } }
            }
        }
        // A3 None-None positive
        a3.Cases++;
        if (!Acc(Shape.None, Shape.None))
        { if (!a3.Failed) { a3.Failed = true; a3.Counterexample = "accepts(None,None)==false (expected true)"; } }

        // A4 transitive — EXPECTED TO FAIL. Search exhaustively first (most likely minimal), then random.
        bool foundA4 = false;
        Shape? wa = null, wb = null, wc = null;

        // exhaustive triples over the small set
        var small = ex.ToList();
        for (int i = 0; i < small.Count && !foundA4; i++)
        for (int j = 0; j < small.Count && !foundA4; j++)
        for (int k = 0; k < small.Count && !foundA4; k++)
        {
            a4.Cases++;
            var (x, y, z) = (small[i], small[j], small[k]);
            if (Acc(x, y) && Acc(y, z) && !Acc(x, z)) { foundA4 = true; wa = x; wb = y; wc = z; }
        }
        if (!foundA4)
        {
            for (int t = 0; t < n; t++)
            {
                a4.Cases++;
                var x = pool[rng.Next(pool.Count)]; var y = pool[rng.Next(pool.Count)]; var z = pool[rng.Next(pool.Count)];
                if (Acc(x, y) && Acc(y, z) && !Acc(x, z)) { foundA4 = true; wa = x; wb = y; wc = z; break; }
            }
        }
        if (foundA4)
        {
            // shrink the triple
            ShrinkA4(ref wa!, ref wb!, ref wc!, Acc);
            a4.Failed = true;
            a4.Counterexample = $"a={Render(wa!)} b={Render(wb!)} c={Render(wc!)} : accepts(a,b)={Acc(wa!, wb!)} accepts(b,c)={Acc(wb!, wc!)} accepts(a,c)={Acc(wa!, wc!)}";
        }
        a4.Notes = foundA4 ? "transitivity broken (expected)" : "no counterexample found in searched space";

        a1.Notes = "reflexive"; a2.Notes = "Any top+bottom"; a3.Notes = "None edge incl None-None=true";
    }

    private static void ShrinkA4(ref Shape a, ref Shape b, ref Shape c, Func<Shape, Shape, bool> Acc)
    {
        bool Fails(Shape x, Shape y, Shape z) => Acc(x, y) && Acc(y, z) && !Acc(x, z);
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var na in ShapeGen.Shrink(a)) if (Fails(na, b, c)) { a = na; changed = true; break; }
            if (changed) continue;
            foreach (var nb in ShapeGen.Shrink(b)) if (Fails(a, nb, c)) { b = nb; changed = true; break; }
            if (changed) continue;
            foreach (var nc in ShapeGen.Shrink(c)) if (Fails(a, b, nc)) { c = nc; changed = true; break; }
        }
    }

    // ---------------- COHERENCE ----------------
    private static void RunCoherence(List<Shape> pool, List<Shape> ex, int n)
    {
        var c1 = Begin("C1"); var c2 = Begin("C2");
        var rng = new Random(44);
        bool Acc(Shape e, Shape a) => ShapeContractCompatibility.CanAccept(e, a);

        Shape? c1wa = null, c1wb = null;
        void CheckC1(Shape a, Shape b)
        {
            c1.Cases++;
            var (ok, m) = Merge(a, b);
            if (!ok) return;
            if (!Acc(m!, a) || !Acc(m!, b))
            { if (!c1.Failed) { c1.Failed = true; c1wa = a; c1wb = b; } }
        }

        // C2 equal-implies-self
        void CheckC2(Shape a, Shape b)
        {
            c2.Cases++;
            if (a.Equals(b))
            {
                var (ok, m) = Merge(a, b);
                if (!ok || !m!.Equals(a))
                { if (!c2.Failed) { c2.Failed = true; c2.Counterexample = $"a={Render(a)} b={Render(b)} (equal) : merge {(ok ? "=" + Render(m!) : "=CONFLICT")} (expected {Render(a)})"; } }
            }
        }

        foreach (var a in ex) foreach (var b in ex) { CheckC1(a, b); CheckC2(a, b); }
        // C2 with forced equal clones
        foreach (var s in ex) { var cl = Clone(s); CheckC2(s, cl); }
        for (int i = 0; i < n; i++)
        {
            var a = pool[rng.Next(pool.Count)]; var b = pool[rng.Next(pool.Count)];
            CheckC1(a, b); CheckC2(a, b);
        }
        // if C1 failed, shrink the witness pair to minimal form
        if (c1.Failed && c1wa != null && c1wb != null)
        {
            ShrinkC1(ref c1wa, ref c1wb, Acc);
            var (ok, m) = Merge(c1wa, c1wb);
            c1.Counterexample = $"a={Render(c1wa)} b={Render(c1wb)} m={Render(m!)} : accepts(m,a)={Acc(m!, c1wa)} accepts(m,b)={Acc(m!, c1wb)} (both must be true)";
        }
        c1.Notes = "merge is upper bound under accepts"; c2.Notes = "a=b => merge(a,b)=a";
    }

    private static void ShrinkC1(ref Shape a, ref Shape b, Func<Shape, Shape, bool> Acc)
    {
        bool Fails(Shape x, Shape y)
        {
            var (ok, m) = Merge(x, y);
            return ok && (!Acc(m!, x) || !Acc(m!, y));
        }
        bool changed = true;
        while (changed)
        {
            changed = false;
            // coordinated: strip a common array wrapper from BOTH sides at once
            if (a.TryGetArrayItemShape(out var ia) && b.TryGetArrayItemShape(out var ib) && Fails(ia, ib))
            { a = ia; b = ib; changed = true; continue; }
            // coordinated: strip a common nullable wrapper from BOTH sides at once
            if (a.TryGetNullableInnerShape(out var na2) && b.TryGetNullableInnerShape(out var nb2) && Fails(na2, nb2))
            { a = na2; b = nb2; changed = true; continue; }
            // independent shrink of a
            foreach (var na in ShapeGen.Shrink(a)) if (Fails(na, b)) { a = na; changed = true; break; }
            if (changed) continue;
            // independent shrink of b
            foreach (var nb in ShapeGen.Shrink(b)) if (Fails(a, nb)) { b = nb; changed = true; break; }
        }
    }

    // ---------------- APPLY / CONVERT (via proven port) ----------------
    private static void RunApplyConvert(int n, int maxDepth)
    {
        var p1 = Begin("P1"); var p2 = Begin("P2"); var p3 = Begin("P3");
        var rng = new Random(55);
        var shapeGen = new ShapeGen(0xBEEF);
        int p3ConvertErr = 0, p3ConvertOk = 0, deepShapes = 0;

        for (int i = 0; i < n; i++)
        {
            var s = shapeGen.Next(maxDepth);
            var v = GenValue(rng, 3);
            if (DepthOf(s) >= 2) deepShapes++;

            // P1 total
            p1.Cases++;
            object? applied;
            try { applied = ShapeConvert.ApplyShape(v, s); }
            catch (Exception ex)
            {
                if (!p1.Failed) { p1.Failed = true; p1.Counterexample = $"v={RenderVal(v)} s={Render(s)} threw {ex.GetType().Name}: {ex.Message}"; }
                continue;
            }

            // P2 idempotent
            p2.Cases++;
            try
            {
                var twice = ShapeConvert.ApplyShape(applied, s);
                if (!ValEq(applied, twice))
                { if (!p2.Failed) { p2.Failed = true; p2.Counterexample = ShrinkP2(v, s, rng); } }
            }
            catch (Exception ex)
            { if (!p2.Failed) { p2.Failed = true; p2.Counterexample = $"v={RenderVal(v)} s={Render(s)} second apply threw {ex.GetType().Name}: {ex.Message}"; } }

            // P3 convert/apply coherence
            p3.Cases++;
            try
            {
                var conv = ShapeConvert.ConvertByShape(v, s);
                if (conv.Ok)
                {
                    p3ConvertOk++;
                    if (!ValEq(conv.Value, applied))
                    { if (!p3.Failed) { p3.Failed = true; p3.Counterexample = ShrinkP3(v, s); } }
                }
                else
                {
                    p3ConvertErr++;
                    if (!ValEq(applied, v))
                    { if (!p3.Failed) { p3.Failed = true; p3.Counterexample = ShrinkP3(v, s); } }
                }
            }
            catch (Exception ex)
            { if (!p3.Failed) { p3.Failed = true; p3.Counterexample = $"v={RenderVal(v)} s={Render(s)} convert threw {ex.GetType().Name}: {ex.Message}"; } }
        }
        p1.Notes = $"applyShape total (no throw); {deepShapes} shapes depth>=2";
        p2.Notes = "apply(apply(v,s),s)=apply(v,s)";
        p3.Notes = $"convert ok => apply=w ({p3ConvertOk} ok-cases); convert err => apply=v ({p3ConvertErr} err-cases) — both branches exercised";
    }

    private static string ShrinkP2(object? v, Shape s, Random rng)
    {
        bool Fails(object? vv, Shape ss)
        {
            try { var a = ShapeConvert.ApplyShape(vv, ss); var b = ShapeConvert.ApplyShape(a, ss); return !ValEq(a, b); }
            catch { return false; }
        }
        Shape cs = s;
        bool changed = true;
        while (changed) { changed = false; foreach (var ns in ShapeGen.Shrink(cs)) if (Fails(v, ns)) { cs = ns; changed = true; break; } }
        var a1 = ShapeConvert.ApplyShape(v, cs);
        var a2 = ShapeConvert.ApplyShape(a1, cs);
        return $"v={RenderVal(v)} s={Render(cs)} : apply(v,s)={RenderVal(a1)} apply(apply(v,s),s)={RenderVal(a2)}";
    }

    private static string ShrinkP3(object? v, Shape s)
    {
        bool Fails(object? vv, Shape ss)
        {
            try
            {
                var a = ShapeConvert.ApplyShape(vv, ss);
                var c = ShapeConvert.ConvertByShape(vv, ss);
                return c.Ok ? !ValEq(c.Value, a) : !ValEq(a, vv);
            }
            catch { return false; }
        }
        Shape cs = s;
        bool changed = true;
        while (changed) { changed = false; foreach (var ns in ShapeGen.Shrink(cs)) if (Fails(v, ns)) { cs = ns; changed = true; break; } }
        var ap = ShapeConvert.ApplyShape(v, cs);
        var cv = ShapeConvert.ConvertByShape(v, cs);
        return $"v={RenderVal(v)} s={Render(cs)} : convert.Ok={cv.Ok} convert.Value={(cv.Ok ? RenderVal(cv.Value) : "ERR:" + cv.Error)} apply={RenderVal(ap)}";
    }

    // ---------------- SERIALIZE ----------------
    private static void RunSerialize(List<Shape> pool, List<Shape> ex, int n)
    {
        var s1 = Begin("S1"); var s2 = Begin("S2"); var s3 = Begin("S3");
        var rng = new Random(66);
        var opts = new JsonSerializerOptions();
        string Ser(Shape s) => JsonSerializer.Serialize(s, opts);
        var probe = AllProbe(pool, ex).ToList();

        // S1 deterministic
        foreach (var s in probe)
        {
            s1.Cases++;
            if (Ser(s) != Ser(s)) { if (!s1.Failed) { s1.Failed = true; s1.Counterexample = $"s={Render(s)} not byte-stable"; } }
        }

        // S2 congruent (a=b => ser(a)=ser(b)) — force equals via clones
        var dup = new List<Shape>(probe);
        foreach (var s in ex) dup.Add(Clone(s));
        for (int i = 0; i < n; i++)
        {
            var a = dup[rng.Next(dup.Count)]; var b = dup[rng.Next(dup.Count)];
            s2.Cases++;
            if (a.Equals(b) && Ser(a) != Ser(b))
            { if (!s2.Failed) { s2.Failed = true; s2.Counterexample = $"a={Render(a)} b={Render(b)} ser(a)={Ser(a)} ser(b)={Ser(b)}"; } }
        }
        foreach (var s in ex) { var cl = Clone(s); s2.Cases++; if (s.Equals(cl) && Ser(s) != Ser(cl)) { if (!s2.Failed) { s2.Failed = true; s2.Counterexample = $"s={Render(s)} clone differs"; } } }

        // S3 injective-up-to-equality (ser(a)=ser(b) => a=b). Build serialize->shape buckets.
        var byteToShape = new Dictionary<string, Shape>();
        bool S3Check(Shape s)
        {
            s3.Cases++;
            var bytes = Ser(s);
            if (byteToShape.TryGetValue(bytes, out var prev))
            {
                if (!prev.Equals(s))
                { if (!s3.Failed) { s3.Failed = true; s3.Counterexample = $"distinct shapes share bytes: a={Render(prev)} b={Render(s)} ser={bytes}"; } return false; }
            }
            else byteToShape[bytes] = s;
            return true;
        }
        foreach (var s in ex) S3Check(s);
        foreach (var s in pool) S3Check(s);

        s1.Notes = "byte-stable across repeated calls";
        s2.Notes = "equal shapes serialize identically";
        s3.Notes = "no two distinct shapes share serialized bytes";
    }

    // ---------------- FROM-CLR ----------------
    private static void RunFromClr()
    {
        var f1 = Begin("F1"); var f2 = Begin("F2");

        var types = new List<Type>
        {
            typeof(string), typeof(bool), typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(float), typeof(double),
            typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly), typeof(TimeOnly),
            typeof(Guid), typeof(TimeSpan), typeof(object), typeof(StringComparison) /*enum*/,
            typeof(int?), typeof(bool?), typeof(DateTime?), typeof(double?), typeof(Guid?),
            typeof(int[]), typeof(string[]), typeof(double[][]), typeof(List<int>), typeof(List<string>),
            typeof(HashSet<int>), typeof(IEnumerable<int>), typeof(IReadOnlyList<string>), typeof(ICollection<double>),
            typeof(IList<bool>), typeof(ISet<int>), typeof(Dictionary<string, int>), typeof(IDictionary<string, int>),
            typeof(IReadOnlyDictionary<string, int>), typeof(List<List<int>>), typeof(int[,]),
            typeof(Nullable<>), typeof(List<>), typeof(Tuple<int, string>), typeof(KeyValuePair<string, int>),
            typeof(Uri), typeof(Version), typeof(System.IO.Stream), typeof(Action), typeof(Func<int>),
            typeof(decimal[]), typeof(List<DateTime>), typeof(Dictionary<int, List<string>>),
        };

        foreach (var t in types)
        {
            f1.Cases++;
            try
            {
                var a = Shape.FromClrType(t);
                if (a is null) { if (!f1.Failed) { f1.Failed = true; f1.Counterexample = $"FromClrType({t}) returned null"; } }
                f2.Cases++;
                var b = Shape.FromClrType(t);
                if (!a!.Equals(b)) { if (!f2.Failed) { f2.Failed = true; f2.Counterexample = $"FromClrType({t}) nondeterministic"; } }
            }
            catch (Exception ex)
            { if (!f1.Failed) { f1.Failed = true; f1.Counterexample = $"FromClrType({t}) threw {ex.GetType().Name}: {ex.Message}"; } }
        }
        f1.Notes = "diverse Type set incl scalars, nullable, arrays, generic collections, open generics, dicts, delegates";
        f2.Notes = "deterministic";
    }

    private static int DepthOf(Shape s)
    {
        if (s.TryGetArrayItemShape(out var it)) return 1 + DepthOf(it);
        if (s.TryGetNullableInnerShape(out var inn)) return 1 + DepthOf(inn);
        if (s.TryGetObjectContract(out var c) && c.Fields.Count > 0)
            return 1 + c.Fields.Values.Select(DepthOf).DefaultIfEmpty(0).Max();
        return 0;
    }
}
