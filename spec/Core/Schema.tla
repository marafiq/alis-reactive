---- MODULE Schema ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "TodaysShapeSwitches", "ConversionOperatorKept", "EachInARead", "AsOfAny", "PatternReadInBrowser",
            "NullableOfNullable", "EmptySet", "BindingLeavesItsBlock", "DecimalTextComparedAsText", "SelectFieldUnchecked"}

ASSUME Mutant \in Mutants

StatementKinds == {"set", "do", "let", "if", "all", "request", "stop", "listen"}
ExpressionKinds == {"lit", "obj", "ref", "get", "data", "as", "call", "fn", "answer", "record", "list", "op", "signal"}
AbsenceKinds == {"start", "nothing", "unbound"}
ShapeKinds == {"string", "number", "boolean", "date", "instant", "file", "any", "array", "nullable", "object"}
PathStepKinds == {"each"}
LocatorKinds == {"element", "plugin"}
ListeningKinds == {"method", "assignment", "returned"}
ReleaseKinds == {"living", "held"}
PluginDataKinds == {"none", "hub", "model", "form", "stream"}
TimeZoneKinds == {"clock", "zone"}
TraceKinds == {"off", "on"}

Kinds == StatementKinds \cup ExpressionKinds \cup AbsenceKinds \cup ShapeKinds \cup PathStepKinds \cup LocatorKinds \cup ListeningKinds
         \cup ReleaseKinds \cup PluginDataKinds \cup TimeZoneKinds \cup TraceKinds

Handlers == [k \in Kinds |->
    IF k = "none" THEN {}
    ELSE IF k \in ShapeKinds /\ Mutant = "TodaysShapeSwitches" THEN {"convert", "bound"}
    ELSE IF k \in StatementKinds THEN {"execute"}
    ELSE IF k \in ExpressionKinds THEN {"evaluate"}
    ELSE IF k \in ShapeKinds THEN {"convert"}
    ELSE IF k \in PathStepKinds THEN {"formWalk"}
    ELSE IF k \in LocatorKinds THEN {"driverOf"}
    ELSE IF k \in ListeningKinds THEN {"listen"}
    ELSE IF k \in ReleaseKinds THEN {"release"}
    ELSE IF k \in PluginDataKinds THEN {"namedPlugin"}
    ELSE IF k \in AbsenceKinds THEN {"absent"}
    ELSE IF k \in TimeZoneKinds THEN {"zone"}
    ELSE {"trace"}]

Operators == {"eq", "neq", "lt", "lte", "gt", "gte", "and", "or", "not", "add", "sub", "mul", "mod", "neg", "coalesce",
              "cond", "nullableValue", "empty", "matches", "toArray", "finite", "compareNumbers", "compareOrdinal",
              "compareBooleans", "compareCulture", "merge", "trim", "trimStart", "trimEnd", "upperInvariant", "lowerInvariant",
              "upper", "lower", "upperTurkic", "lowerTurkic", "contains", "startsWith", "endsWith", "nullOrEmpty", "includes",
              "sumInt", "sumDouble", "sumDecimal", "eqDecimal", "nonNull"}

OperatorSwitch == Operators

WalkSteps == {"member", "index"}

Converting == ShapeKinds \ {"any"}

Leaf(k) == [k |-> k, c |-> <<>>, p |-> <<>>, s |-> "string", o |-> "eq", h |-> "value"]

DecimalText == [Leaf("data") EXCEPT !.h = "decimalText"]

FieldHoldingNoNull == [Leaf("data") EXCEPT !.h = "holdsNoNull"]

Leaves == {Leaf("lit"), Leaf("obj"), DecimalText, FieldHoldingNoNull}

Paths == {<<"member">>, <<"each">>}

WrittenOps == {"eq", "eqDecimal", "matches", "toNumber"}

RECURSIVE ExprsTo(_)
ExprsTo(d) ==
    IF d = 0 THEN Leaves
    ELSE LET below == ExprsTo(d - 1) IN
        below
        \cup {[k |-> "get", c |-> <<e>>, p |-> path, s |-> "string", o |-> "eq", h |-> "value"] : e \in below, path \in Paths}
        \cup {[k |-> "as", c |-> <<e>>, p |-> <<>>, s |-> shape, o |-> "eq", h |-> "value"] : e \in below, shape \in {"number", "any"}}
        \cup {[k |-> "op", c |-> <<e, x>>, p |-> <<>>, s |-> "string", o |-> op, h |-> "value"] : e \in below, x \in below, op \in WrittenOps}
        \cup {[k |-> "op", c |-> <<e>>, p |-> <<>>, s |-> "string", o |-> "nonNull", h |-> "value"] : e \in below}
        \cup {[k |-> "fn", c |-> <<e>>, p |-> <<>>, s |-> "string", o |-> "eq", h |-> "value"] : e \in below}

RECURSIVE Nodes(_)
Nodes(e) == {e} \cup UNION {Nodes(e.c[i]) : i \in DOMAIN e.c}

RECURSIVE WriterAccepts(_)
WriterAccepts(e) ==
    /\ \A i \in DOMAIN e.c : WriterAccepts(e.c[i])
    /\ e.k = "get" => (\A i \in DOMAIN e.p : e.p[i] # "each" \/ Mutant = "EachInARead")
    /\ e.k = "as" => (e.s # "any" \/ Mutant = "AsOfAny")
    /\ e.k = "op" => (e.o \in Operators \/ Mutant = "ConversionOperatorKept")
    /\ (e.k = "op" /\ e.o = "matches") => (e.c[2].k = "lit" \/ Mutant = "PatternReadInBrowser")
    /\ (e.k = "op" /\ e.o \in {"eq", "neq"}) => ((\A i \in DOMAIN e.c : e.c[i].h # "decimalText") \/ Mutant = "DecimalTextComparedAsText")
    /\ (e.k = "op" /\ e.o = "eqDecimal") => \A i \in DOMAIN e.c : e.c[i].h = "decimalText"
    /\ e.k = "fn" => (e.c[1].h # "holdsNoNull" \/ Mutant = "SelectFieldUnchecked")
    /\ (e.k = "op" /\ e.o = "nonNull") => e.c[1].h = "holdsNoNull"

ShapeLeaves == {[k |-> k, c |-> <<>>] : k \in {"string", "file", "any"}}

RECURSIVE ShapesTo(_)
ShapesTo(d) ==
    IF d = 0 THEN ShapeLeaves
    ELSE LET below == ShapesTo(d - 1) IN
        below \cup {[k |-> w, c |-> <<s>>] : s \in below, w \in {"array", "nullable"}}

RECURSIVE ShapeNodes(_)
ShapeNodes(s) == {s} \cup UNION {ShapeNodes(s.c[i]) : i \in DOMAIN s.c}

RECURSIVE ShapeAccepted(_)
ShapeAccepted(s) ==
    /\ \A i \in DOMAIN s.c : ShapeAccepted(s.c[i])
    /\ s.k = "nullable" => (s.c[1].k # "nullable" \/ Mutant = "NullableOfNullable")

Depth == 2
MaxWrites == 5

NoExpr == Leaf("lit")
NoShape == [k |-> "string", c |-> <<>>]

VARIABLES plane, expr, shape, open, parent, log, writes

vars == <<plane, expr, shape, open, parent, log, writes>>

Init ==
    /\ \/ /\ plane = "expr"
          /\ expr \in {e \in ExprsTo(Depth) : WriterAccepts(e)}
          /\ shape = NoShape
       \/ /\ plane = "shape"
          /\ shape \in {s \in ShapesTo(Depth) : ShapeAccepted(s)}
          /\ expr = NoExpr
       \/ /\ plane = "statement"
          /\ expr = NoExpr
          /\ shape = NoShape
    /\ open = <<0>>
    /\ parent = <<>>
    /\ log = <<>>
    /\ writes = 0

Top == open[Len(open)]

Entry(k, n, len) == [k |-> k, b |-> Top, n |-> n, len |-> len]

InScope(name) == \E j \in DOMAIN log : log[j].k = "let" /\ log[j].n = name /\ \E i \in DOMAIN open : open[i] = log[j].b

BoundAnywhere(name) == \E j \in DOMAIN log : log[j].k = "let" /\ log[j].n = name

Write(entry) == log' = Append(log, entry) /\ writes' = writes + 1

CanWrite == plane = "statement" /\ writes < MaxWrites

Let == CanWrite /\ Write(Entry("let", "a", 0)) /\ UNCHANGED <<plane, expr, shape, open, parent>>

Ref == CanWrite /\ \E name \in {"a", "reply"} :
       /\ InScope(name) \/ (Mutant = "BindingLeavesItsBlock" /\ BoundAnywhere(name))
       /\ Write(Entry("ref", name, 0)) /\ UNCHANGED <<plane, expr, shape, open, parent>>

Set == CanWrite /\ \E len \in IF Mutant = "EmptySet" THEN {0, 1} ELSE {1} :
       Write(Entry("set", "-", len)) /\ UNCHANGED <<plane, expr, shape, open, parent>>

OpenBlock(k) ==
    /\ CanWrite
    /\ Len(open) <= Depth
    /\ Write(Entry(k, "-", 0))
    /\ parent' = Append(parent, Top)
    /\ open' = Append(open, Len(parent) + 1)
    /\ UNCHANGED <<plane, expr, shape>>

OpenIf == OpenBlock("if")

OpenAll == OpenBlock("all")

OpenRequest ==
    /\ CanWrite
    /\ Len(open) <= Depth
    /\ log' = log \o <<Entry("request", "-", 0), [k |-> "let", b |-> Len(parent) + 1, n |-> "reply", len |-> 0]>>
    /\ writes' = writes + 1
    /\ parent' = Append(parent, Top)
    /\ open' = Append(open, Len(parent) + 1)
    /\ UNCHANGED <<plane, expr, shape>>

Close == plane = "statement" /\ Len(open) > 1 /\ open' = SubSeq(open, 1, Len(open) - 1)
         /\ UNCHANGED <<plane, expr, shape, parent, log, writes>>

Next == Let \/ Ref \/ Set \/ OpenIf \/ OpenAll \/ OpenRequest \/ Close

Spec == Init /\ [][Next]_vars

RECURSIVE Encloses(_, _)
Encloses(outer, inner) == outer = inner \/ (inner # 0 /\ Encloses(outer, parent[inner]))

RECURSIVE BlockDepth(_)
BlockDepth(b) == IF b = 0 THEN 0 ELSE 1 + BlockDepth(parent[b])

KindsHandledOnce ==
    /\ \A k \in Kinds \ {"none"} : Cardinality(Handlers[k]) = 1
    /\ Handlers["none"] = {}
    /\ \A n \in Nodes(expr) : n.k \in Kinds
    /\ \A n \in ShapeNodes(shape) : n.k \in Kinds

OperatorsClosed == \A n \in Nodes(expr) : n.k = "op" => n.o \in OperatorSwitch

ReadsTakeNoEach == \A n \in Nodes(expr) : n.k = "get" => \A i \in DOMAIN n.p : n.p[i] \in WalkSteps

ConversionsConvert == \A n \in Nodes(expr) : n.k = "as" => n.s \in Converting

PatternsAreLiterals == \A n \in Nodes(expr) : (n.k = "op" /\ n.o = "matches") => n.c[2].k = "lit"

DecimalTextComparedByValue == \A n \in Nodes(expr) : (n.k = "op" /\ n.o \in {"eq", "neq"}) => \A i \in DOMAIN n.c : n.c[i].h # "decimalText"

SelectFieldHoldsItsType == \A n \in Nodes(expr) : n.k = "fn" => n.c[1].h # "holdsNoNull"

NullableWrapsOnce == \A n \in ShapeNodes(shape) : n.k = "nullable" => n.c[1].k # "nullable"

SetsWriteAMember == \A i \in DOMAIN log : log[i].k = "set" => log[i].len >= 1

RefsReadTheirBlock ==
    \A i \in DOMAIN log : log[i].k = "ref" =>
        \E j \in 1..(i - 1) : log[j].k = "let" /\ log[j].n = log[i].n /\ Encloses(log[j].b, log[i].b)

ReachNestedRead ==
    ~(\E i \in DOMAIN log : log[i].k = "ref" /\ BlockDepth(log[i].b) = 2
        /\ \E j \in 1..(i - 1) : log[j].k = "let" /\ log[j].b # log[i].b /\ Encloses(log[j].b, log[i].b))

ReachReplyInANestedBlock ==
    ~(\E i \in DOMAIN log : log[i].k = "ref" /\ log[i].n = "reply" /\ BlockDepth(log[i].b) = 2
        /\ \E j \in 1..(i - 1) : log[j].k = "let" /\ log[j].n = "reply" /\ log[j].b # log[i].b /\ Encloses(log[j].b, log[i].b))

ReachDeepRead == ~(plane = "expr" /\ expr.k = "as" /\ expr.c[1].k = "get" /\ expr.c[1].c[1].k = "obj")

ReachDecimalTextEquality == ~(plane = "expr" /\ expr.k = "op" /\ expr.o = "eqDecimal" /\ expr.c[1].h = "decimalText" /\ expr.c[2].h = "decimalText")

ReachSelectFieldStops == ~(plane = "expr" /\ expr.k = "fn" /\ expr.c[1].k = "op" /\ expr.c[1].o = "nonNull" /\ expr.c[1].c[1].h = "holdsNoNull")

====
