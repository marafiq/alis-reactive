---- MODULE Run ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "EnclosingRunsOn", "AlwaysYields", "ChainHandlers", "AfterIfReadsBefore", "StopSkipsFinally",
            "StopRunsOn", "AbortRunsTheRoute", "AbortRunsUnloadedFinally", "UnwindRestartsFinally",
            "DefaultFinallyFirst", "DisposeSkipsRelease", "DisposeAwaitsFinally"}

ASSUME Mutant \in Mutants

Stmt(k, y, n, l, r, f, d) == [k |-> k, yes |-> y, no |-> n, loading |-> l, route |-> r, finally |-> f, defaults |-> d]

Leaf(k) == Stmt(k, <<>>, <<>>, <<>>, <<>>, <<>>, FALSE)

Leaves == {Leaf("write"), Leaf("await"), Leaf("stop")}

Short == {<<>>} \cup {<<s>> : s \in Leaves}

Quiet == {<<>>, <<Leaf("write")>>, <<Leaf("await")>>, <<Leaf("stop")>>}

InnerLoading == {<<>>, <<Leaf("await")>>, <<Leaf("stop")>>}

Inner == Leaves
    \cup {Stmt("if", y, n, <<>>, <<>>, <<>>, FALSE) : y \in Short, n \in Short}
    \cup {Stmt("request", <<>>, <<>>, l, r, f, d) : l \in InnerLoading, r \in Short, f \in Quiet, d \in BOOLEAN}

Body == {<<>>} \cup {<<s>> : s \in Inner} \cup {<<s, t>> : s \in Inner, t \in Leaves}

Outer == {Stmt("if", y, n, <<>>, <<>>, <<>>, FALSE) : y \in Body, n \in Short}
    \cup {Stmt("request", <<>>, <<>>, l, r, f, d) : l \in Short, r \in Body, f \in Quiet, d \in BOOLEAN}

Programs == {<<s, Leaf("write")>> : s \in Outer}

Seconds == {<<Leaf("write")>>, <<Leaf("await"), Leaf("write")>>}

DefaultFinally == <<Leaf("await"), Leaf("write")>>

Handlers == {1, 2}

NoStmt == Leaf("none")

FinallyRoles == {"finally", "defaultFinally"}

FirstRole == IF Mutant = "DefaultFinallyFirst" THEN "defaultFinally" ELSE "finally"

Frame(k, body, path, role, node, state) ==
    [k |-> k, body |-> body, i |-> 1, path |-> path, role |-> role, state |-> state, abrupt |-> FALSE, node |-> node,
     loadedOwn |-> FALSE, loadedDefaults |-> FALSE, holds |-> FALSE]

BlockFrame(body, path, role) == Frame("block", body, path, role, NoStmt, "none")

RequestFrame(node, path) == Frame("request", <<>>, path, "none", node, IF node.defaults THEN "defaultLoading" ELSE "loading")

VARIABLES prog, defaultLoading, stack, status, turn, active, writes, awaitsRan, stopped, ranAfterStop, ranAfterAbort,
          ranWhilePending, pending, loaded, finallies, held, aborted, cutDefaultFinally, abortNesting, deepest, innerStopped

vars == <<prog, defaultLoading, stack, status, turn, active, writes, awaitsRan, stopped, ranAfterStop, ranAfterAbort,
          ranWhilePending, pending, loaded, finallies, held, aborted, cutDefaultFinally, abortNesting, deepest, innerStopped>>

RECURSIVE MarkFrom(_, _, _, _), MarkStmt(_, _, _)

MarkFrom(b, i, p, f) ==
    IF i > Len(b) THEN [marked |-> {}, flag |-> f]
    ELSE LET s == MarkStmt(b[i], p \o <<i>>, f)
             rest == MarkFrom(b, i + 1, p, s.flag)
         IN [marked |-> s.marked \cup rest.marked, flag |-> rest.flag]

MarkStmt(s, p, f) ==
    CASE s.k = "write" -> [marked |-> IF f THEN {p} ELSE {}, flag |-> f]
      [] s.k = "await" -> [marked |-> {}, flag |-> TRUE]
      [] s.k = "stop" -> [marked |-> {}, flag |-> f]
      [] s.k = "if" ->
           LET y == MarkFrom(s.yes, 1, p \o <<1>>, f)
               n == MarkFrom(s.no, 1, p \o <<2>>, f)
           IN [marked |-> y.marked \cup n.marked, flag |-> IF Mutant = "AfterIfReadsBefore" THEN f ELSE y.flag \/ n.flag]
      [] OTHER ->
           LET d == MarkFrom(IF s.defaults THEN defaultLoading ELSE <<>>, 1, p \o <<4>>, f)
               l == MarkFrom(s.loading, 1, p \o <<1>>, d.flag)
               r == MarkFrom(s.route, 1, p \o <<2>>, TRUE)
               z == MarkFrom(s.finally, 1, p \o <<3>>, TRUE)
               y == MarkFrom(IF s.defaults THEN DefaultFinally ELSE <<>>, 1, p \o <<5>>, TRUE)
           IN [marked |-> d.marked \cup l.marked \cup r.marked \cup z.marked \cup y.marked, flag |-> TRUE]

Marked(program) == MarkFrom(program, 1, <<>>, FALSE).marked

Init ==
    /\ prog \in {[h \in Handlers |-> IF h = 1 THEN first ELSE second] : first \in Programs, second \in Seconds}
    /\ defaultLoading \in {<<>>, <<Leaf("await")>>}
    /\ stack = [h \in Handlers |-> <<>>]
    /\ status = [h \in Handlers |-> "idle"]
    /\ turn = "idle"
    /\ active = 0
    /\ writes = {}
    /\ awaitsRan = [h \in Handlers |-> 0]
    /\ stopped = [h \in Handlers |-> FALSE]
    /\ ranAfterStop = [h \in Handlers |-> FALSE]
    /\ ranAfterAbort = FALSE
    /\ ranWhilePending = [h \in Handlers |-> FALSE]
    /\ pending = [h \in Handlers |-> FALSE]
    /\ loaded = [h \in Handlers |-> {}]
    /\ finallies = [h \in Handlers |-> <<>>]
    /\ held = [h \in Handlers |-> 0]
    /\ aborted = "no"
    /\ cutDefaultFinally = FALSE
    /\ abortNesting = 0
    /\ deepest = 0
    /\ innerStopped = FALSE

InTurn == turn \in {"h1", "h2"}

Top(h) == stack[h][Len(stack[h])]

Below(h) == SubSeq(stack[h], 1, Len(stack[h]) - 1)

Unwinding(s) == \E j \in 1..Len(s) : s[j].role \in FinallyRoles \/ s[j].abrupt

Abrupt(s) == [j \in 1..Len(s) |-> IF s[j].k = "request" THEN [s[j] EXCEPT !.abrupt = TRUE] ELSE s[j]]

RECURSIVE Unwound(_, _)

Unwound(s, byAbort) ==
    IF s = <<>> THEN <<>>
    ELSE LET t == s[Len(s)]
             rest == SubSeq(s, 1, Len(s) - 1)
             at(state) == Append(rest, [t EXCEPT !.state = state, !.abrupt = TRUE])
         IN IF t.k = "block" \/ t.state = "defaultLoadingRunning" THEN Unwound(rest, byAbort)
            ELSE CASE t.state \in {"loadingRunning", "route", "routeRunning"} ->
                        IF Mutant = "StopSkipsFinally" /\ ~byAbort THEN at("after")
                        ELSE Append(rest, [t EXCEPT !.state = "finally", !.abrupt = TRUE,
                                              !.loadedOwn = @ \/ (byAbort /\ t.state = "loadingRunning"
                                                                   /\ Mutant = "AbortRunsUnloadedFinally")])
                   [] t.state = "finallyRunning" -> at(IF Mutant = "UnwindRestartsFinally" THEN "finally" ELSE "defaultFinally")
                   [] OTHER -> at("after")

Stopped(s) == IF Mutant = "StopRunsOn" THEN SubSeq(s, 1, Len(s) - 1) ELSE Unwound(s, FALSE)

Following(role) ==
    CASE role = "defaultLoading" -> "loading"
      [] role = "loading" -> "attempt"
      [] role = "route" -> "finally"
      [] role = FirstRole -> "defaultFinally"
      [] OTHER -> "after"

Ended(f, rest) ==
    IF rest = <<>> \/ f.role = "branch" THEN rest
    ELSE LET r == rest[Len(rest)]
         IN Append(SubSeq(rest, 1, Len(rest) - 1),
                   [r EXCEPT !.state = Following(f.role), !.loadedOwn = @ \/ f.role = "loading",
                             !.loadedDefaults = @ \/ f.role = "defaultLoading"])

Settled(h, s) ==
    /\ stack' = [stack EXCEPT ![h] = s]
    /\ IF s = <<>> THEN status' = [status EXCEPT ![h] = "ended"] /\ active' = 0 ELSE UNCHANGED <<status, active>>

Ran(h) ==
    /\ ranAfterStop' = [ranAfterStop EXCEPT ![h] = @ \/ (stopped[h] /\ ~Unwinding(stack[h]))]
    /\ ranAfterAbort' = (ranAfterAbort \/ (h = 1 /\ aborted # "no" /\ ~Unwinding(stack[h])))
    /\ ranWhilePending' = [ranWhilePending EXCEPT ![h] = @ \/ pending[h]]

Wait(h, s) ==
    IF Mutant = "EnclosingRunsOn" /\ Len(s) > 1
      THEN /\ stack' = [stack EXCEPT ![h] = SubSeq(s, 1, Len(s) - 1)]
           /\ pending' = [pending EXCEPT ![h] = TRUE]
           /\ UNCHANGED <<status, active>>
      ELSE /\ stack' = [stack EXCEPT ![h] = s]
           /\ status' = [status EXCEPT ![h] = "waiting"]
           /\ active' = 0
           /\ UNCHANGED pending

Push(h, s, frame) ==
    IF Mutant = "AlwaysYields"
      THEN /\ stack' = [stack EXCEPT ![h] = Append(s, frame)]
           /\ status' = [status EXCEPT ![h] = "waiting"]
           /\ active' = 0
      ELSE /\ stack' = [stack EXCEPT ![h] = Append(s, frame)]
           /\ UNCHANGED <<status, active>>

Statement(h) ==
    LET f == Top(h)
        s == f.body[f.i]
        p == f.path \o <<f.i>>
        moved == Append(Below(h), [f EXCEPT !.i = @ + 1])
    IN /\ Ran(h)
       /\ CASE s.k = "write" ->
                 /\ writes' = writes \cup {[h |-> h, path |-> p, role |-> f.role, inTurn |-> InTurn]}
                 /\ Settled(h, moved)
                 /\ UNCHANGED <<awaitsRan, stopped, pending, loaded, finallies, held, cutDefaultFinally, innerStopped>>
            [] s.k = "await" ->
                 /\ awaitsRan' = [awaitsRan EXCEPT ![h] = @ + 1]
                 /\ IF h = 1 /\ aborted = "dispose" /\ f.role \in FinallyRoles /\ Mutant # "DisposeAwaitsFinally"
                      THEN /\ Settled(h, Unwound(moved, FALSE))
                           /\ cutDefaultFinally' = (cutDefaultFinally \/ f.role = "defaultFinally")
                           /\ UNCHANGED pending
                      ELSE /\ Wait(h, moved)
                           /\ UNCHANGED cutDefaultFinally
                 /\ UNCHANGED <<writes, stopped, loaded, finallies, held, innerStopped>>
            [] s.k = "stop" ->
                 /\ stopped' = [stopped EXCEPT ![h] = TRUE]
                 /\ innerStopped' = (innerStopped \/ (Len(p) = 5 /\ p[2] = 2 /\ p[4] = 2))
                 /\ Settled(h, Stopped(moved))
                 /\ UNCHANGED <<writes, awaitsRan, pending, loaded, finallies, held, cutDefaultFinally>>
            [] s.k = "if" ->
                 /\ \E c \in {1, 2} : Push(h, moved, BlockFrame(IF c = 1 THEN s.yes ELSE s.no, p \o <<c>>, "branch"))
                 /\ UNCHANGED <<writes, awaitsRan, stopped, pending, loaded, finallies, held, cutDefaultFinally, innerStopped>>
            [] OTHER ->
                 /\ Push(h, moved, RequestFrame(s, p))
                 /\ UNCHANGED <<writes, awaitsRan, stopped, pending, loaded, finallies, held, cutDefaultFinally, innerStopped>>
       /\ deepest' = IF s.k = "await" /\ Len(p) > deepest THEN Len(p) ELSE deepest

BlockEnds(h) ==
    LET f == Top(h)
        rest == Below(h)
    IN /\ Ran(h)
       /\ Settled(h, Ended(f, rest))
       /\ loaded' = IF f.role \in {"loading", "defaultLoading"}
                      THEN [loaded EXCEPT ![h] = @ \cup {[node |-> rest[Len(rest)].path,
                                                          layer |-> IF f.role = "loading" THEN "own" ELSE "defaults"]}]
                      ELSE loaded
       /\ UNCHANGED <<writes, awaitsRan, stopped, pending, finallies, held, cutDefaultFinally, deepest, innerStopped>>

RequestStep(h) ==
    LET r == Top(h)
        below == Below(h)
        opened(state, body, role, n) ==
            Push(h, Append(below, [r EXCEPT !.state = state]), BlockFrame(body, r.path \o <<n>>, role))
        layer == IF (r.state = "finally") = (FirstRole = "finally") THEN "own" ELSE "defaults"
        layerLoaded == IF layer = "own" THEN r.loadedOwn ELSE r.loadedDefaults
    IN /\ Ran(h)
       /\ CASE r.state = "defaultLoading" ->
                 /\ opened("defaultLoadingRunning", defaultLoading, "defaultLoading", 4)
                 /\ UNCHANGED <<awaitsRan, finallies, held, deepest, pending>>
            [] r.state = "loading" ->
                 /\ opened("loadingRunning", r.node.loading, "loading", 1)
                 /\ UNCHANGED <<awaitsRan, finallies, held, deepest, pending>>
            [] r.state = "attempt" ->
                 /\ awaitsRan' = [awaitsRan EXCEPT ![h] = @ + 1]
                 /\ held' = [held EXCEPT ![h] = @ + (IF r.node.defaults THEN 1 ELSE 0)]
                 /\ Wait(h, Append(below, [r EXCEPT !.state = "route", !.holds = r.node.defaults]))
                 /\ deepest' = IF Len(r.path) > deepest THEN Len(r.path) ELSE deepest
                 /\ UNCHANGED finallies
            [] r.state = "route" ->
                 /\ opened("routeRunning", r.node.route, "route", 2)
                 /\ UNCHANGED <<awaitsRan, finallies, held, deepest, pending>>
            [] r.state \in {"finally", "defaultFinally"} ->
                 /\ IF layerLoaded
                      THEN /\ opened(IF r.state = "finally" THEN "finallyRunning" ELSE "defaultFinallyRunning",
                                     IF layer = "own" THEN r.node.finally ELSE DefaultFinally,
                                     IF layer = "own" THEN "finally" ELSE "defaultFinally",
                                     IF layer = "own" THEN 3 ELSE 5)
                           /\ finallies' = [finallies EXCEPT ![h] = Append(@, [node |-> r.path, layer |-> layer])]
                      ELSE /\ Settled(h, Append(below, [r EXCEPT !.state = IF r.state = "finally" THEN "defaultFinally" ELSE "after"]))
                           /\ UNCHANGED finallies
                 /\ UNCHANGED <<awaitsRan, held, deepest, pending>>
            [] OTHER ->
                 /\ held' = [held EXCEPT ![h] = @ - (IF r.holds /\ ~(Mutant = "DisposeSkipsRelease" /\ aborted = "dispose")
                                                      THEN 1 ELSE 0)]
                 /\ Settled(h, IF r.abrupt THEN Unwound(below, FALSE) ELSE below)
                 /\ UNCHANGED <<awaitsRan, finallies, deepest, pending>>
       /\ UNCHANGED <<writes, stopped, loaded, cutDefaultFinally, innerStopped>>

RequestStates == {"defaultLoading", "loading", "attempt", "route", "finally", "defaultFinally", "after"}

Step(h) ==
    /\ active = h
    /\ status[h] = "running"
    /\ stack[h] # <<>>
    /\ \/ Top(h).k = "block" /\ Top(h).i <= Len(Top(h).body) /\ Statement(h)
       \/ Top(h).k = "block" /\ Top(h).i > Len(Top(h).body) /\ BlockEnds(h)
       \/ Top(h).k = "request" /\ Top(h).state \in RequestStates /\ RequestStep(h)
    /\ UNCHANGED <<prog, defaultLoading, turn, aborted, abortNesting>>

quiet == <<prog, defaultLoading, writes, awaitsRan, stopped, ranAfterStop, ranAfterAbort, ranWhilePending, pending, loaded,
           finallies, held, aborted, cutDefaultFinally, abortNesting, deepest, innerStopped>>

Begin(h, t) ==
    /\ status' = [status EXCEPT ![h] = "running"]
    /\ stack' = [stack EXCEPT ![h] = <<BlockFrame(prog[h], <<>>, "top")>>]
    /\ active' = h
    /\ turn' = t

Fire ==
    /\ turn = "idle"
    /\ Begin(1, "h1")
    /\ UNCHANGED quiet

Returned(h) == status[h] \in {"waiting", "ended"} /\ active = 0

FirstReturns ==
    /\ turn = "h1"
    /\ Returned(1)
    /\ IF Mutant = "ChainHandlers" /\ status[1] # "ended"
         THEN turn' = "read" /\ UNCHANGED <<stack, status, active>>
         ELSE Begin(2, "h2")
    /\ UNCHANGED quiet

SecondReturns ==
    /\ turn = "h2"
    /\ Returned(2)
    /\ turn' = "read"
    /\ UNCHANGED <<stack, status, active>>
    /\ UNCHANGED quiet

VendorReads ==
    /\ turn = "read"
    /\ turn' = "done"
    /\ UNCHANGED <<stack, status, active>>
    /\ UNCHANGED quiet

ChainedSecond ==
    /\ Mutant = "ChainHandlers"
    /\ turn = "done"
    /\ status[1] = "ended"
    /\ status[2] = "idle"
    /\ active = 0
    /\ Begin(2, "done")
    /\ UNCHANGED quiet

Settle(h) ==
    /\ turn = "done"
    /\ active = 0
    /\ status[h] = "waiting"
    /\ status' = [status EXCEPT ![h] = "running"]
    /\ active' = h
    /\ UNCHANGED <<stack, turn>>
    /\ UNCHANGED quiet

Abort(cause) ==
    /\ turn = "done"
    /\ aborted = "no"
    /\ status[1] = "waiting"
    /\ active = 0
    /\ LET s == stack[1]
           t == s[Len(s)]
           inFinally == t.k = "block" /\ t.role \in FinallyRoles
           keepsWaiting == inFinally /\ cause = "replace" /\ Mutant # "UnwindRestartsFinally"
           runsOn == ~inFinally /\ cause = "replace" /\ Mutant = "AbortRunsTheRoute"
           next == IF keepsWaiting THEN Abrupt(s) ELSE IF runsOn THEN s ELSE Unwound(s, TRUE)
       IN /\ stack' = [stack EXCEPT ![1] = next]
          /\ status' = [status EXCEPT ![1] = IF keepsWaiting THEN "waiting" ELSE IF next = <<>> THEN "ended" ELSE "running"]
          /\ active' = IF keepsWaiting \/ next = <<>> THEN 0 ELSE 1
          /\ cutDefaultFinally' = (cutDefaultFinally \/ (cause = "dispose" /\ inFinally /\ t.role = "defaultFinally"))
          /\ abortNesting' = Cardinality({j \in 1..Len(s) : s[j].k = "request"})
    /\ aborted' = cause
    /\ UNCHANGED <<prog, defaultLoading, turn, writes, awaitsRan, stopped, ranAfterStop, ranAfterAbort, ranWhilePending,
                   pending, loaded, finallies, held, deepest, innerStopped>>

Next ==
    \/ Fire \/ FirstReturns \/ SecondReturns \/ VendorReads \/ ChainedSecond
    \/ \E h \in Handlers : Step(h) \/ Settle(h)
    \/ \E cause \in {"replace", "dispose"} : Abort(cause)

Spec == Init /\ [][Next]_vars

Range(f) == {f[i] : i \in 1..Len(f)}

Encloses(outer, inner) == Len(outer) < Len(inner) /\ SubSeq(inner, 1, Len(outer)) = outer

WritesAfterTheTurnAreRefused == \A w \in writes : ~w.inTurn => w.path \in Marked(prog[w.h])

NothingRunsWhileItWaits == \A h \in Handlers : ~ranWhilePending[h]

SyncRunEndsInItsTurn == \A h \in Handlers : (turn \in {"read", "done"} /\ awaitsRan[h] = 0 /\ status[h] # "idle") => status[h] = "ended"

SecondHandlerStartsInTheTurn == turn \in {"read", "done"} => status[2] # "idle"

NothingRunsAfterAStop == \A h \in Handlers : ~ranAfterStop[h]

NothingRunsAfterAnAbort == ~ranAfterAbort

FinallyRunsWhenLoaded == \A h \in Handlers : status[h] = "ended" => loaded[h] \subseteq Range(finallies[h])

FinallyOnlyWhenLoaded == \A h \in Handlers : Range(finallies[h]) \subseteq loaded[h]

FinallyRunsOnce == \A h \in Handlers : Len(finallies[h]) = Cardinality(Range(finallies[h]))

InnerFinallyFirst == \A h \in Handlers : \A i, j \in 1..Len(finallies[h]) :
    i < j => /\ ~(finallies[h][i].node = finallies[h][j].node /\ finallies[h][i].layer = "defaults")
             /\ ~Encloses(finallies[h][i].node, finallies[h][j].node)

ReleasedWhenEnded == \A h \in Handlers : status[h] = "ended" => held[h] = 0

DisposedRunNeverWaits == aborted = "dispose" => status[1] # "waiting"

ReachAwaitDeepInARoute == deepest < 5

ReachStopInAnInnerRoute == ~(innerStopped /\ \E h \in Handlers : status[h] = "ended" /\ Len(finallies[h]) = 4)

ReachAbortInAnInnerRequest == ~(abortNesting = 2 /\ status[1] = "ended" /\ Len(finallies[1]) = 4)

ReachReleaseAfterACut == ~(cutDefaultFinally /\ status[1] = "ended")

ReachLoadingWriteAfterTheDefaultsAwait == ~(\E w \in writes : w.role = "loading" /\ Len(w.path) = 3 /\ ~w.inTurn)

====
