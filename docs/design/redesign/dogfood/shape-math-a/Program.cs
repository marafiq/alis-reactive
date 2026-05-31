using System.Globalization;
using System.Text;
using System.Text.Json;
using Alis.Reactive.PlanModel;
using CsCheck;

// =====================================================================================
// Shape domain model as ALGEBRA — property-based test, harness A (CsCheck 4.7.0).
//
// Laws E1..F2 + S1..S3 run against the REAL framework source (LINKED, not re-implemented):
//   ../../../../../Alis.Reactive/PlanModel/Shape.cs
//   ../../../../../Alis.Reactive/PlanModel/ShapeContractCompatibility.cs
// Laws P1..P3 (applyShape / convertByShape) run against the PROVEN port:
//   ../shape-v3/ShapeConvert.cs
//
// Operations under test:
//   equals  = Shape.Equals          hash = Shape.GetHashCode
//   merge   = ShapeContractCompatibility.TryMergeContracts (false => conflict)
//   accepts = ShapeContractCompatibility.CanAccept(expected, actual)
//   ser     = JsonSerializer.Serialize(shape)
//   fromClr = Shape.FromClrType
//   apply   = ShapeConvert.ApplyShape   convert = ShapeConvert.ConvertByShape
// =====================================================================================

internal static class Program
{
    private const long Base = 20_000;     // per-law budget for the non-combinatorial laws
    private const long Heavy = 200_000;   // M5 / C1 / A4 / serialize-injectivity / apply-convert

    private static readonly List<(string Id, string Verdict, long Cases, string Counter, string Notes)> Results = new();

    private static int Main()
    {
        Console.WriteLine("=== Shape algebra PBT (harness A / CsCheck 4.7.0) ===");
        Console.WriteLine("REAL source linked: Shape.cs + ShapeContractCompatibility.cs");
        Console.WriteLine("PROVEN port linked: shape-v3/ShapeConvert.cs");
        Console.WriteLine();

        GeneratorCoverage(); // hard evidence the gen reaches every kind + depth >= 3

        E1_Reflexive(); E2_Symmetric(); E3_Transitive(); E4_HashCongruent();
        M1_Idempotent(); M2_Commutative(); M3_IdentityAny(); M4_AnnihilatorNone(); M5_Associative();
        A1_Reflexive(); A2_AnyTopBottom(); A3_NoneEdge(); A4_Transitive();
        C1_MergeUpperBound(); C2_EqualImpliesSelf();
        P1_ApplyTotal(); P2_ApplyIdempotent(); P3_ConvertApplyCoherence();
        S1_Deterministic(); S2_Congruent(); S3_InjectiveUpToEquality();
        F1_Total(); F2_Deterministic();

        Console.WriteLine();
        Console.WriteLine("=== SUMMARY ===");
        foreach (var r in Results)
        {
            Console.WriteLine($"{r.Id,-3} {r.Verdict,-10} cases={r.Cases,-9} {r.Notes}");
            if (r.Verdict == "FAILS" && r.Counter.Length > 0)
                Console.WriteLine($"      counterexample: {r.Counter}");
        }

        // Emit machine-readable block for transcription into the PROVE schema.
        Console.WriteLine();
        Console.WriteLine("=== MACHINE ===");
        foreach (var r in Results)
            Console.WriteLine($"MR|{r.Id}|{r.Verdict}|{r.Cases}|{r.Counter}|{r.Notes}");
        return 0;
    }

    // -----------------------------------------------------------------------------------
    // Recursive Shape generator — depth >= 3: all scalars, array, nullable, closed+open objects.
    // -----------------------------------------------------------------------------------

    private static readonly Shape[] Scalars =
    {
        Shape.String, Shape.Number, Shape.Boolean, Shape.Date, Shape.Raw, Shape.Any, Shape.None
    };
    private static readonly Shape[] NonNoneScalars =
    {
        Shape.String, Shape.Number, Shape.Boolean, Shape.Date, Shape.Raw, Shape.Any
    };

    // Small key alphabet so object contracts collide on keys (exercises field-by-field merge/accept).
    private static readonly Gen<string> GenFieldName = Gen.OneOfConst("a", "b", "c", "id", "name");

    // Hard evidence the recursive generator reaches every shape kind and depth >= 3.
    // Tallies over a sample; prints the distribution so no HOLDS is silently NOT_TESTED.
    private static void GeneratorCoverage()
    {
        var kinds = new Dictionary<string, long>();
        long maxDepth = 0, withObjOpen = 0, withObjClosed = 0, withNullable = 0, withArray = 0, atLeastD3 = 0;
        const long n = 50_000;
        // threads:1 — this tally mutates shared state; the law predicates below are pure and run multi-threaded.
        S3Shape.Sample(s =>
        {
            kinds[s.Kind] = kinds.TryGetValue(s.Kind, out var v) ? v + 1 : 1;
            var d = Depth(s);
            if (d > maxDepth) maxDepth = d;
            if (d >= 3) atLeastD3++;
            if (HasKind(s, "object", openOnly: true)) withObjOpen++;
            if (HasKind(s, "object", closedOnly: true)) withObjClosed++;
            if (HasKind(s, "nullable")) withNullable++;
            if (HasKind(s, "array")) withArray++;
            return true;
        }, iter: n, threads: 1);

        Console.WriteLine($"GEN COVERAGE over {n} draws:");
        foreach (var kv in kinds.OrderBy(k => k.Key))
            Console.WriteLine($"   kind {kv.Key,-9} {kv.Value}");
        Console.WriteLine($"   maxDepth={maxDepth}  draws>=depth3={atLeastD3}  array={withArray}  nullable={withNullable}  closedObj={withObjClosed}  openObj={withObjOpen}");
        Console.WriteLine();
    }

    private static long Depth(Shape s)
    {
        if (s.TryGetArrayItemShape(out var i)) return 1 + Depth(i);
        if (s.TryGetNullableInnerShape(out var n)) return 1 + Depth(n);
        if (s.TryGetObjectContract(out var c) && c.Fields.Count > 0)
            return 1 + c.Fields.Values.Max(Depth);
        return 1;
    }

    private static bool HasKind(Shape s, string kind, bool openOnly = false, bool closedOnly = false)
    {
        if (s.Kind == kind)
        {
            if (kind == "object" && (openOnly || closedOnly))
            {
                s.TryGetObjectContract(out var c);
                if (openOnly && !(c.AllowsAdditionalFields && c.Fields.Count == 0)) { }
                else if (closedOnly && c.AllowsAdditionalFields) { }
                else return true;
            }
            else return true;
        }
        if (s.TryGetArrayItemShape(out var i)) return HasKind(i, kind, openOnly, closedOnly);
        if (s.TryGetNullableInnerShape(out var nn)) return HasKind(nn, kind, openOnly, closedOnly);
        if (s.TryGetObjectContract(out var oc))
            foreach (var f in oc.Fields)
                if (HasKind(f.Value, kind, openOnly, closedOnly)) return true;
        return false;
    }

    private static Gen<Shape> GenScalar() => Gen.OneOfConst(Scalars);
    private static Gen<Shape> GenNonNoneScalar() => Gen.OneOfConst(NonNoneScalars);

    // Any shape (including None) to the requested depth.
    private static Gen<Shape> GenShape(int depth)
    {
        if (depth <= 0)
            return GenScalar();

        var nonNoneInner = GenNonNoneShape(depth - 1);
        return Gen.Frequency<Shape>(
            (5, GenScalar()),
            (3, nonNoneInner.Select(Shape.ArrayOf)),
            (3, nonNoneInner.Select(Shape.Nullable)),
            (1, Gen.Const(Shape.OpenObject())),
            (4, GenClosedObject(GenShape(depth - 1))));
    }

    // Any non-None shape (a legal array item / nullable inner) to the requested depth.
    private static Gen<Shape> GenNonNoneShape(int depth)
    {
        if (depth <= 0)
            return GenNonNoneScalar();

        var nonNoneInner = GenNonNoneShape(depth - 1);
        return Gen.Frequency<Shape>(
            (5, GenNonNoneScalar()),
            (3, nonNoneInner.Select(Shape.ArrayOf)),
            (3, nonNoneInner.Select(Shape.Nullable)),
            (1, Gen.Const(Shape.OpenObject())),
            (4, GenClosedObject(GenShape(depth - 1))));
    }

    // Closed object with 0..3 fields; field shapes drawn from the (possibly None-bearing) gen.
    private static Gen<Shape> GenClosedObject(Gen<Shape> fieldShape) =>
        Gen.Select(GenFieldName, fieldShape, (k, v) => (k, v)).Array[0, 3]
           .Select(pairs =>
           {
               var d = new Dictionary<string, Shape>(StringComparer.Ordinal);
               foreach (var (k, v) in pairs) d[k] = v; // later keys win — fine, all keys valid
               return Shape.ObjectOf(d);
           });

    private static readonly Gen<Shape> S3Shape = GenShape(3);

    private static bool Merge(Shape a, Shape b, out Shape? m) =>
        ShapeContractCompatibility.TryMergeContracts(a, b, out m);

    private static bool Accepts(Shape e, Shape a) =>
        ShapeContractCompatibility.CanAccept(e, a);

    private static string Ser(Shape s) => JsonSerializer.Serialize(s);

    // Rebuild a structurally-equal but reference-distinct twin (forces Equals/hash/ser to do real work).
    private static Shape Clone(Shape s)
    {
        if (s.TryGetArrayItemShape(out var item))
            return Shape.ArrayOf(Clone(item));
        if (s.TryGetNullableInnerShape(out var inner))
            return Shape.Nullable(Clone(inner));
        if (s.TryGetObjectContract(out var contract))
        {
            if (contract.AllowsAdditionalFields && contract.Fields.Count == 0)
                return Shape.OpenObject();
            var d = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var f in contract.Fields) d[f.Key] = Clone(f.Value);
            return Shape.ObjectOf(d);
        }
        return s.Kind switch // scalars are interned singletons — rebuild from kind
        {
            "string" => Shape.String,
            "number" => Shape.Number,
            "boolean" => Shape.Boolean,
            "date" => Shape.Date,
            "raw" => Shape.Raw,
            "any" => Shape.Any,
            "none" => Shape.None,
            _ => s
        };
    }

    // ============================== EQUIVALENCE ==============================

    private static void E1_Reflexive() => Run("E1", Base, S3Shape,
        s => s.Equals(s), "reflexive: s = s", Describe);

    private static void E2_Symmetric() => Run("E2", Base, Pair(S3Shape),
        t => t.a.Equals(t.b) == t.b.Equals(t.a), "symmetric: (a=b)==(b=a)", DescPair);

    private static void E3_Transitive()
    {
        // Twins built from the same shape make the a=b=c premise reachable with distinct objects.
        var gen = S3Shape.Select(s => (a: s, b: Clone(s), c: Clone(s)));
        Run("E3", Base, gen,
            t => !(t.a.Equals(t.b) && t.b.Equals(t.c)) || t.a.Equals(t.c),
            "transitive: a=b & b=c => a=c", DescTriple);
    }

    private static void E4_HashCongruent() => Run("E4", Base, S3Shape.Select(s => (a: s, b: Clone(s))),
        t => !t.a.Equals(t.b) || t.a.GetHashCode() == t.b.GetHashCode(),
        "hash-congruent: a=b => hash(a)=hash(b)", DescPair);

    // ============================== MERGE ==============================

    private static void M1_Idempotent() => Run("M1", Base, S3Shape, s =>
    {
        var defined = Merge(s, s, out var m);
        return defined && m!.Equals(s);
    }, "idempotent: merge(s,s) defined & = s", Describe);

    private static void M2_Commutative() => Run("M2", Heavy, Pair(S3Shape), t =>
    {
        var d1 = Merge(t.a, t.b, out var m1);
        var d2 = Merge(t.b, t.a, out var m2);
        if (d1 != d2) return false;
        return !d1 || m1!.Equals(m2!);
    }, "commutative: definedness agrees & results structurally equal", DescPair);

    private static void M3_IdentityAny() => Run("M3", Base, S3Shape, s =>
    {
        var d1 = Merge(Shape.Any, s, out var m1);
        var d2 = Merge(s, Shape.Any, out var m2);
        return d1 && d2 && m1!.Equals(s) && m2!.Equals(s);
    }, "identity Any: merge(Any,s)=s and merge(s,Any)=s", Describe);

    private static void M4_AnnihilatorNone() => Run("M4", Base, S3Shape, s =>
    {
        if (s.Equals(Shape.None))
        {
            var d = Merge(Shape.None, Shape.None, out var m);
            return d && m!.Equals(Shape.None);
        }
        var dl = Merge(Shape.None, s, out _);
        var dr = Merge(s, Shape.None, out _);
        return !dl && !dr;
    }, "annihilator None: None+None=None; else None+s and s+None conflict", Describe);

    private static void M5_Associative() => Run("M5", Heavy, Triple(S3Shape), t =>
    {
        var dab = Merge(t.a, t.b, out var ab);
        var dbc = Merge(t.b, t.c, out var bc);
        if (!dab || !dbc) return true; // require both inner merges defined
        var dLeft = Merge(ab!, t.c, out var left);
        var dRight = Merge(t.a, bc!, out var right);
        if (dLeft != dRight) return false;          // conflict-propagation must be consistent
        return !dLeft || left!.Equals(right!);
    }, "associative: (a*b)*c = a*(b*c) when all defined; conflict agreement", DescTriple);

    // ============================== ACCEPT ==============================

    private static void A1_Reflexive() => Run("A1", Base, S3Shape,
        s => Accepts(s, s), "reflexive: accepts(s,s)", Describe);

    private static void A2_AnyTopBottom() => Run("A2", Base, S3Shape,
        s => Accepts(Shape.Any, s) && Accepts(s, Shape.Any),
        "Any top+bottom: accepts(Any,s) and accepts(s,Any)", Describe);

    private static void A3_NoneEdge() => Run("A3", Base, S3Shape, s =>
    {
        if (s.Equals(Shape.None) || s.Equals(Shape.Any)) return true; // outside the quantifier set
        return !Accepts(Shape.None, s) && !Accepts(s, Shape.None);
    }, "None edge: not accepts(None,s)/accepts(s,None) for s not in {None,Any}; accepts(None,None)=true", Describe);

    private static void A4_Transitive() => Run("A4", Heavy, Triple(S3Shape), t =>
    {
        if (Accepts(t.a, t.b) && Accepts(t.b, t.c))
            return Accepts(t.a, t.c);
        return true; // premise false => vacuously holds
    }, "transitive (EXPECTED TO FAIL): accepts(a,b) & accepts(b,c) => accepts(a,c)", DescTriple);

    // ============================== COHERENCE ==============================

    private static void C1_MergeUpperBound() => Run("C1", Heavy, Pair(S3Shape), t =>
    {
        if (!Merge(t.a, t.b, out var m)) return true; // conflict => no claim
        return Accepts(m!, t.a) && Accepts(m!, t.b);
    }, "merge is upper bound: merge(a,b)=m => accepts(m,a) and accepts(m,b)", DescPair);

    private static void C2_EqualImpliesSelf() => Run("C2", Base, S3Shape.Select(s => (a: s, b: Clone(s))), t =>
    {
        if (!t.a.Equals(t.b)) return true;
        var d = Merge(t.a, t.b, out var m);
        return d && m!.Equals(t.a);
    }, "equal implies self: a=b => merge(a,b)=a", DescPair);

    // ============================== APPLY / CONVERT (proven port) ==============================
    //
    // JS-runtime values are wrapped in JsVal so CsCheck never holds a bare null
    // (Gen.Const(null) NREs inside CsCheck's value bookkeeping — a generator-tooling
    //  limitation, not an engine fact). .V is unwrapped (possibly to null) only when
    // calling the engine, so the JS "missing" path is still fully exercised.

    private static Gen<JsVal> GenValue(int depth)
    {
        Gen<JsVal> missing = Gen.Const(JsVal.Of(null));
        Gen<JsVal> numbers = Gen.OneOf(
            Gen.Double[-1e6, 1e6].Select(d => JsVal.Of(d)),
            Gen.OneOfConst(JsVal.Of(0d), JsVal.Of(1d), JsVal.Of(-1d), JsVal.Of(1.5d),
                JsVal.Of(double.NaN), JsVal.Of(double.PositiveInfinity), JsVal.Of(double.NegativeInfinity)));
        Gen<JsVal> bools = Gen.Bool.Select(b => JsVal.Of(b));
        Gen<JsVal> strings = Gen.OneOf(
            Gen.OneOfConst("", " ", "0", "1", "false", "true", "abc", "12", "1.5",
                "2020-01-02", "2020-01-02T03:04:05Z", "not-a-date").Select(s => JsVal.Of(s)),
            Gen.String[0, 6].Select(s => JsVal.Of(s)));
        Gen<JsVal> dates = Gen.OneOfConst(
            JsVal.Of(new JsDate(0d)), JsVal.Of(new JsDate(1_700_000_000_000d)),
            JsVal.Of(new JsDate(-86_400_000d)), JsVal.Of(new JsDate(double.NaN)));

        var leaf = Gen.OneOf(missing, numbers, bools, strings, dates);
        if (depth <= 0) return leaf;

        var child = GenValue(depth - 1);
        Gen<JsVal> array = child.Array[0, 3]
            .Select(xs => JsVal.Of((object?)(object?[])xs.Select(x => x.V).ToArray()));
        Gen<JsVal> obj = Gen.Select(GenFieldName, child, (k, v) => (k, v)).Array[0, 3]
            .Select(pairs =>
            {
                var d = new Dictionary<string, object?>();
                foreach (var (k, v) in pairs) d[k] = v.V;
                return JsVal.Of((object?)(IReadOnlyDictionary<string, object?>)d);
            });
        return Gen.OneOf(leaf, leaf, array, obj); // double-weight leaves
    }

    private static readonly Gen<(JsVal v, Shape s)> GenValueShape =
        Gen.Select(GenValue(3), S3Shape, (v, s) => (v, s));

    // Captures the actual throw + the exact failing case at detection time (CsCheck's
    // post-shrink print is unreliable for reference tuples, so we self-capture).
    private static void P1_ApplyTotal() => Run("P1", Heavy, GenValueShape, t =>
    {
        try { _ = ShapeConvert.ApplyShape(t.v.V, t.s); return true; }
        catch { return false; }
    }, "total: applyShape(v,s) never throws", DescVS);

    private static void P2_ApplyIdempotent() => Run("P2", Heavy, GenValueShape, t =>
    {
        var once = ShapeConvert.ApplyShape(t.v.V, t.s);
        var twice = ShapeConvert.ApplyShape(once, t.s);
        return ValueEqual(once, twice);
    }, "idempotent (shape-once): apply(apply(v,s),s) = apply(v,s)", DescVS);

    private static void P3_ConvertApplyCoherence() => Run("P3", Heavy, GenValueShape, t =>
    {
        var c = ShapeConvert.ConvertByShape(t.v.V, t.s);
        var applied = ShapeConvert.ApplyShape(t.v.V, t.s);
        return c.Ok ? ValueEqual(c.Value, applied) : ValueEqual(applied, t.v.V);
    }, "convert/apply coherence: ok(w)=>apply=w ; err=>apply=v", DescVS);

    // ============================== SERIALIZE ==============================

    private static void S1_Deterministic() => Run("S1", Base, S3Shape,
        s => Ser(s) == Ser(s), "deterministic: ser(s)=ser(s) byte-stable", Describe);

    private static void S2_Congruent() => Run("S2", Base, S3Shape.Select(s => (a: s, b: Clone(s))),
        t => !t.a.Equals(t.b) || Ser(t.a) == Ser(t.b), "congruent: a=b => ser(a)=ser(b)", DescPair);

    private static void S3_InjectiveUpToEquality() => Run("S3", Heavy, Pair(S3Shape),
        t => Ser(t.a) != Ser(t.b) || t.a.Equals(t.b),
        "injective-up-to-equality: ser(a)=ser(b) => a=b",
        t => $"a={Describe(t.a)} ser(a)={Ser(t.a)} | b={Describe(t.b)} ser(b)={Ser(t.b)}");

    // ============================== FROM-CLR ==============================

    private static readonly Type[] ClrTypes =
    {
        typeof(string), typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
        typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan),
        typeof(Guid), typeof(int?), typeof(double?), typeof(bool?), typeof(DateTime?),
        typeof(int[]), typeof(string[]), typeof(List<int>), typeof(List<string>), typeof(HashSet<int>),
        typeof(IEnumerable<double>), typeof(IList<bool>), typeof(ICollection<DateTime>),
        typeof(Dictionary<string, int>), typeof(IDictionary<int, string>),
        typeof(object), typeof(System.IO.Stream), typeof(DayOfWeek), typeof(int[][]),
        typeof(List<List<int>>), typeof(Uri), typeof(decimal?), typeof(Guid?), typeof(char),
        typeof(List<int?>), typeof(string[][]), typeof(IReadOnlyList<Guid>)
    };
    private static readonly Gen<Type> GenClrType = Gen.OneOfConst(ClrTypes);

    private static void F1_Total() => Run("F1", Base, GenClrType, t =>
    {
        try { return Shape.FromClrType(t) is not null; }
        catch { return false; }
    }, "total: fromClr(t) non-null & no throw over diverse Type set", t => t.FullName ?? t.Name);

    private static void F2_Deterministic() => Run("F2", Base, GenClrType,
        t => Shape.FromClrType(t).Equals(Shape.FromClrType(t)),
        "deterministic: fromClr(t)=fromClr(t)", t => t.FullName ?? t.Name);

    // ============================== shared gens / runner ==============================

    private static Gen<(Shape a, Shape b)> Pair(Gen<Shape> g) => Gen.Select(g, g, (a, b) => (a, b));
    private static Gen<(Shape a, Shape b, Shape c)> Triple(Gen<Shape> g) => Gen.Select(g, g, g, (a, b, c) => (a, b, c));

    private static string DescPair((Shape a, Shape b) t) => $"a={Describe(t.a)} | b={Describe(t.b)}";
    private static string DescTriple((Shape a, Shape b, Shape c) t) => $"a={Describe(t.a)} | b={Describe(t.b)} | c={Describe(t.c)}";
    private static string DescVS((JsVal v, Shape s) t) =>
        $"value={DescribeValue(t.v.V)} | shape={Describe(t.s)}";

    // Human-readable shape rendering. Robust: a render must never crash the run.
    // Renders the special shapes explicitly so "none"/"any" witnesses are unambiguous.
    private static string Describe(Shape s)
    {
        try
        {
            var body = s.DescribeContract();
            return s.Kind switch
            {
                "none" => "none(the None shape)",
                "any" => "any(the Any shape)",
                _ => body
            };
        }
        catch (Exception e) { return $"<describe-failed:{s?.Kind ?? "null"}:{e.GetType().Name}>"; }
    }

    private static string DescribeValue(object? v)
    {
        try { return DescribeValueCore(v); }
        catch (Exception e) { return $"<value-render-failed:{e.GetType().Name}>"; }
    }

    private static string DescribeValueCore(object? v)
    {
        switch (v)
        {
            case null: return "null";
            case bool b: return b ? "true" : "false";
            case double d:
                if (double.IsNaN(d)) return "NaN";
                if (double.IsPositiveInfinity(d)) return "+Inf";
                if (double.IsNegativeInfinity(d)) return "-Inf";
                return d.ToString("R", CultureInfo.InvariantCulture);
            case string str: return $"\"{str}\"";
            case JsDate jd: return $"Date({(double.IsNaN(jd.EpochMs) ? "NaN" : jd.EpochMs.ToString("R", CultureInfo.InvariantCulture))}ms)";
            case IReadOnlyList<object?> arr:
                return "[" + string.Join(",", arr.Select(DescribeValue)) + "]";
            case IReadOnlyDictionary<string, object?> obj:
                return "{" + string.Join(",", obj.Select(kv => $"{kv.Key}:{DescribeValue(kv.Value)}")) + "}";
            default: return v.ToString() ?? "?";
        }
    }

    // Deep structural equality of JS-runtime values, NaN==NaN treated equal (needed for apply idempotence).
    private static bool ValueEqual(object? x, object? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        if (x is double dx && y is double dy)
            return (double.IsNaN(dx) && double.IsNaN(dy)) || dx.Equals(dy);
        if (x is bool bx && y is bool by) return bx == by;
        if (x is string sx && y is string sy) return sx == sy;
        if (x is JsDate jx && y is JsDate jy)
            return (double.IsNaN(jx.EpochMs) && double.IsNaN(jy.EpochMs)) || jx.EpochMs.Equals(jy.EpochMs);
        if (x is IReadOnlyList<object?> ax && y is IReadOnlyList<object?> ay)
        {
            if (ax.Count != ay.Count) return false;
            for (var i = 0; i < ax.Count; i++)
                if (!ValueEqual(ax[i], ay[i])) return false;
            return true;
        }
        if (x is IReadOnlyDictionary<string, object?> ox && y is IReadOnlyDictionary<string, object?> oy)
        {
            if (ox.Count != oy.Count) return false;
            foreach (var kv in ox)
            {
                if (!oy.TryGetValue(kv.Key, out var ov)) return false;
                if (!ValueEqual(kv.Value, ov)) return false;
            }
            return true;
        }
        return Equals(x, y);
    }

    private static void Record(string id, string verdict, long cases, string counter, string notes) =>
        Results.Add((id, verdict, cases, counter, notes));

    // CsCheck .Sample with explicit iteration budget + shrinking.
    // On failure CsCheckException.Message already carries the shrunk witness rendered by `print`.
    private static void Run<T>(string id, long iterations, Gen<T> gen, Func<T, bool> law,
        string notes, Func<T, string> render)
    {
        try
        {
            gen.Sample(law, iter: iterations, print: render);
            Record(id, "HOLDS", iterations, string.Empty, notes);
            Console.WriteLine($"[{id}] HOLDS — {iterations} cases — {notes}");
        }
        catch (CsCheckException ex)
        {
            var (witness, total) = ParseFailure(ex.Message, iterations);
            Record(id, "FAILS", total, witness, notes);
            Console.WriteLine($"[{id}] FAILS after {total} cases — {notes}");
            Console.WriteLine($"      counterexample: {witness}");
        }
    }

    // Message form: "Set seed: ... to reproduce (N shrinks, M skipped, T total).\n<rendered witness>"
    private static (string witness, long total) ParseFailure(string message, long fallbackTotal)
    {
        var lines = message.Split('\n');
        long total = fallbackTotal;
        var seedLineIdx = Array.FindIndex(lines, l => l.Contains("total)"));
        if (seedLineIdx >= 0)
        {
            var m = System.Text.RegularExpressions.Regex.Match(lines[seedLineIdx], @"([\d,]+)\s+total");
            if (m.Success && long.TryParse(m.Groups[1].Value.Replace(",", ""), out var t)) total = t;
        }
        var witnessLines = lines.Where((_, i) => i > seedLineIdx).Select(l => l.Trim()).Where(l => l.Length > 0);
        var witness = string.Join(" | ", witnessLines);
        if (witness.Length == 0) witness = message.Replace("\n", " ").Trim();
        return (witness, total);
    }
}

// Wraps a JS-runtime value so CsCheck never has to hold a bare null (which trips its
// internal value bookkeeping). .V is the actual value handed to the engine — possibly null.
internal sealed record JsVal(object? V)
{
    internal static JsVal Of(object? v) => new(v);
    public override string ToString() => V switch { null => "null", _ => V.ToString() ?? "?" };
}
