---- MODULE Lanes ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "DisposeLeavesRunsGoing", "AbortSkipsFinally", "ReplaceKeepsOld", "SkipStartsAnyway",
            "AllSettledEarly", "BranchStopStopsOthers", "FinallyRunsTwice",
            "PayloadWriteAfterAwait", "ReleaseBeforeFinally"}

ASSUME Mutant \in Mutants

Runs == {"first", "second"}
Branches == {"one", "two"}
Steps == {"write", "await", "read"}
Handlers == [1..3 -> Steps]

VARIABLES policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped,
          ranWhileDisposed, branch, stoppedItself, allSettled, parallelDone, released, finallyAfterRelease,
          handler

vars == <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped,
          ranWhileDisposed, branch, stoppedItself, allSettled, parallelDone, released, finallyAfterRelease,
          handler>>

trigger == <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed>>
parallel == <<branch, stoppedItself, allSettled, parallelDone>>

Init ==
    /\ policy \in {"overlap", "replace", "skip"}
    /\ owner = "live"
    /\ pc = [r \in Runs |-> "idle"]
    /\ aborted = [r \in Runs |-> FALSE]
    /\ routeRan = [r \in Runs |-> FALSE]
    /\ finallyRan = [r \in Runs |-> 0]
    /\ afterRan = [r \in Runs |-> FALSE]
    /\ replaced = FALSE
    /\ skipped = FALSE
    /\ ranWhileDisposed = FALSE
    /\ branch = [b \in Branches |-> "inflight"]
    /\ stoppedItself = [b \in Branches |-> FALSE]
    /\ allSettled = 0
    /\ parallelDone = FALSE
    /\ released = FALSE
    /\ finallyAfterRelease = FALSE
    /\ handler \in Handlers

InFlight(r) == pc[r] = "inflight"

FireFirst ==
    /\ pc["first"] = "idle"
    /\ owner = "live"
    /\ pc' = [pc EXCEPT !["first"] = "inflight"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler>>

FireSecond ==
    /\ pc["second"] = "idle"
    /\ pc["first"] # "idle"
    /\ owner = "live"
    /\ CASE policy = "skip" /\ InFlight("first") /\ Mutant # "SkipStartsAnyway" ->
              /\ pc' = [pc EXCEPT !["second"] = "skipped"]
              /\ skipped' = TRUE
              /\ UNCHANGED <<aborted, replaced>>
         [] policy = "replace" /\ InFlight("first") ->
              /\ replaced' = TRUE
              /\ IF Mutant = "ReplaceKeepsOld"
                   THEN pc' = [pc EXCEPT !["second"] = "inflight"] /\ UNCHANGED aborted
                   ELSE /\ pc' = [pc EXCEPT !["second"] = "inflight", !["first"] = "finally"]
                        /\ aborted' = [aborted EXCEPT !["first"] = TRUE]
              /\ UNCHANGED skipped
         [] OTHER ->
              /\ pc' = [pc EXCEPT !["second"] = "inflight"]
              /\ skipped' = (policy = "skip" /\ InFlight("first"))
              /\ UNCHANGED <<aborted, replaced>>
    /\ UNCHANGED <<policy, owner, routeRan, finallyRan, afterRan, ranWhileDisposed, parallel, released, finallyAfterRelease, handler>>

Respond(r) ==
    /\ InFlight(r)
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ pc' = [pc EXCEPT ![r] = "route"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler>>

Route(r) ==
    /\ pc[r] = "route"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ routeRan' = [routeRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ pc' = [pc EXCEPT ![r] = "finally"]
    /\ UNCHANGED <<policy, owner, aborted, finallyRan, afterRan, replaced, skipped, parallel, released, finallyAfterRelease, handler>>

Finally(r) ==
    /\ pc[r] = "finally"
    /\ finallyRan' = [finallyRan EXCEPT ![r] = @ + 1]
    /\ finallyAfterRelease' = (finallyAfterRelease \/ released)
    /\ pc' = [pc EXCEPT ![r] = IF Mutant = "FinallyRunsTwice" /\ finallyRan[r] = 0 THEN "finally"
                               ELSE IF aborted[r] \/ owner = "disposed" THEN "ended" ELSE "after"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, handler>>

After(r) ==
    /\ pc[r] = "after"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ afterRan' = [afterRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, parallel, released, finallyAfterRelease, handler>>

EndOnDispose(s) ==
    CASE s \in {"inflight", "route"} -> IF Mutant = "AbortSkipsFinally" THEN "ended" ELSE "finally"
      [] s = "after" -> "ended"
      [] OTHER -> s

Dispose ==
    /\ owner = "live"
    /\ owner' = "disposed"
    /\ IF Mutant = "DisposeLeavesRunsGoing"
         THEN UNCHANGED <<pc, aborted>>
         ELSE /\ pc' = [r \in Runs |-> EndOnDispose(pc[r])]
              /\ aborted' = [r \in Runs |-> aborted[r] \/ pc[r] \in {"inflight", "route"}]
    /\ UNCHANGED <<policy, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler>>

Settle(b) ==
    /\ branch[b] = "inflight"
    /\ \E outcome \in {"settled", "stopped"} :
         /\ branch' = IF outcome = "stopped" /\ Mutant = "BranchStopStopsOthers"
                        THEN [c \in Branches |-> IF c = b \/ branch[c] = "inflight" THEN "stopped" ELSE branch[c]]
                        ELSE [branch EXCEPT ![b] = outcome]
         /\ stoppedItself' = [stoppedItself EXCEPT ![b] = (outcome = "stopped")]
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, handler, allSettled, parallelDone>>

Release ==
    /\ owner = "disposed"
    /\ ~released
    /\ \A r \in Runs : pc[r] # "finally" \/ Mutant = "ReleaseBeforeFinally"
    /\ released' = TRUE
    /\ UNCHANGED <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed,
                   parallel, finallyAfterRelease, handler>>

AllSettled ==
    /\ ~parallelDone
    /\ IF Mutant = "AllSettledEarly"
         THEN \E b \in Branches : branch[b] # "inflight"
         ELSE \A b \in Branches : branch[b] # "inflight"
    /\ allSettled' = allSettled + 1
    /\ parallelDone' = TRUE
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, handler, branch, stoppedItself>>

Next ==
    \/ FireFirst \/ FireSecond \/ Dispose \/ Release \/ AllSettled
    \/ \E r \in Runs : Respond(r) \/ Route(r) \/ Finally(r) \/ After(r)
    \/ \E b \in Branches : Settle(b)

Spec == Init /\ [][Next]_vars

FirstAwait(h) == IF \E i \in 1..3 : h[i] = "await" THEN CHOOSE i \in 1..3 : h[i] = "await" /\ \A j \in 1..(i - 1) : h[j] # "await" ELSE 4

Refused(h) == (\E i \in 1..3 : h[i] = "write" /\ i > FirstAwait(h)) /\ Mutant # "PayloadWriteAfterAwait"

VendorSees(h) == {i \in 1..3 : h[i] = "write" /\ i < FirstAwait(h)}

Writes(h) == {i \in 1..3 : h[i] = "write"}

PayloadWritesInTheVendorsTurn == ~Refused(handler) => Writes(handler) = VendorSees(handler)

FinallyRunsBeforeRelease == ~finallyAfterRelease

DisposedRunsNothingMore == ~ranWhileDisposed

FinallyRunsWhenAborted == \A r \in Runs : (aborted[r] /\ pc[r] = "ended") => finallyRan[r] >= 1

FinallyRunsOnce == \A r \in Runs : finallyRan[r] <= 1

ReplacedRunRunsNoRoute == replaced => ~routeRan["first"]

SkippedTriggerStartsNothing == skipped => pc["second"] = "skipped"

AllSettledAfterEveryRequest == allSettled > 0 => (allSettled = 1 /\ \A b \in Branches : branch[b] # "inflight")

BranchStopEndsOnlyItself == \A b \in Branches : branch[b] = "stopped" => stoppedItself[b]

ReachReplaced == ~(replaced /\ pc["first"] = "ended" /\ finallyRan["first"] = 1 /\ routeRan["second"])

ReachDisposedInFlight == ~(owner = "disposed" /\ aborted["first"] /\ finallyRan["first"] = 1)

ReachAllSettledAfterAStop == ~(parallelDone /\ \E b \in Branches : branch[b] = "stopped")

====
