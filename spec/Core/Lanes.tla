---- MODULE Lanes ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "DisposeLeavesRunsGoing", "ReplaceKeepsOld", "SkipStartsAnyway", "AllSettledEarly",
            "BranchStopStopsOthers", "ReleaseBeforeFinally", "JoinAtTheBranchAwait"}

ASSUME Mutant \in Mutants

Runs == {"first", "second"}
Branches == {"one", "two"}

VARIABLES policy, owner, pc, aborted, routeRan, finallyRan, replaced, skipped, ranWhileDisposed, branch,
          stoppedItself, allSettled, parallelDone, released, finallyAfterRelease, tail, tailPending,
          routeAfterReplace, tailed

trigger == <<policy, owner, pc, aborted, routeRan, finallyRan, replaced, skipped, ranWhileDisposed, tail, tailPending,
             routeAfterReplace>>
parallel == <<branch, stoppedItself, allSettled, parallelDone, tailed>>

vars == <<trigger, parallel, released, finallyAfterRelease>>

Init ==
    /\ policy \in {"overlap", "replace", "skip"}
    /\ owner = "live"
    /\ pc = [r \in Runs |-> "idle"]
    /\ aborted = [r \in Runs |-> FALSE]
    /\ routeRan = [r \in Runs |-> FALSE]
    /\ finallyRan = [r \in Runs |-> 0]
    /\ replaced = FALSE
    /\ skipped = FALSE
    /\ ranWhileDisposed = FALSE
    /\ branch = [b \in Branches |-> "inflight"]
    /\ stoppedItself = [b \in Branches |-> FALSE]
    /\ allSettled = 0
    /\ parallelDone = FALSE
    /\ released = FALSE
    /\ finallyAfterRelease = FALSE
    /\ tail \in [Runs -> BOOLEAN]
    /\ tailPending = [r \in Runs |-> FALSE]
    /\ routeAfterReplace = FALSE
    /\ tailed = [b \in Branches |-> FALSE]

InFlight(r) == pc[r] \in {"inflight", "tail"}

FireFirst ==
    /\ pc["first"] = "idle"
    /\ owner = "live"
    /\ pc' = [pc EXCEPT !["first"] = "inflight"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, ranWhileDisposed, tail, tailPending,
                   routeAfterReplace, parallel, released, finallyAfterRelease>>

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
    /\ UNCHANGED <<policy, owner, routeRan, finallyRan, ranWhileDisposed, tail, tailPending, routeAfterReplace, parallel,
                   released, finallyAfterRelease>>

Respond(r) ==
    /\ pc[r] = "inflight"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ pc' = [pc EXCEPT ![r] = "route"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, ranWhileDisposed, tail, tailPending,
                   routeAfterReplace, parallel, released, finallyAfterRelease>>

Route(r) ==
    /\ pc[r] = "route"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ routeRan' = [routeRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (replaced /\ r = "first"))
    /\ tailPending' = [tailPending EXCEPT ![r] = tail[r]]
    /\ pc' = [pc EXCEPT ![r] = IF tail[r] THEN "tail" ELSE "finally"]
    /\ UNCHANGED <<policy, owner, aborted, finallyRan, replaced, skipped, tail, parallel, released, finallyAfterRelease>>

SettleTheTail(r) ==
    /\ tailPending[r]
    /\ ~aborted[r]
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ tailPending' = [tailPending EXCEPT ![r] = FALSE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (replaced /\ r = "first"))
    /\ pc' = [pc EXCEPT ![r] = IF pc[r] = "tail" THEN "finally" ELSE pc[r]]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, tail, parallel, released,
                   finallyAfterRelease>>

Finally(r) ==
    /\ pc[r] = "finally"
    /\ finallyRan' = [finallyRan EXCEPT ![r] = @ + 1]
    /\ finallyAfterRelease' = (finallyAfterRelease \/ released)
    /\ pc' = [pc EXCEPT ![r] = IF aborted[r] \/ owner = "disposed" THEN "ended" ELSE "after"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, replaced, skipped, ranWhileDisposed, tail, tailPending,
                   routeAfterReplace, parallel, released>>

After(r) ==
    /\ pc[r] = "after"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, tail, tailPending, routeAfterReplace,
                   parallel, released, finallyAfterRelease>>

EndOnDispose(s) ==
    CASE s \in {"inflight", "route", "tail"} -> "finally"
      [] s = "after" -> "ended"
      [] OTHER -> s

Dispose ==
    /\ owner = "live"
    /\ owner' = "disposed"
    /\ IF Mutant = "DisposeLeavesRunsGoing"
         THEN UNCHANGED <<pc, aborted>>
         ELSE /\ pc' = [r \in Runs |-> EndOnDispose(pc[r])]
              /\ aborted' = [r \in Runs |-> aborted[r] \/ pc[r] \in {"inflight", "route", "tail"}]
    /\ UNCHANGED <<policy, routeRan, finallyRan, replaced, skipped, ranWhileDisposed, tail, tailPending, routeAfterReplace,
                   parallel, released, finallyAfterRelease>>

Settle(b) ==
    /\ branch[b] \in {"inflight", "tail"}
    /\ \E outcome \in (IF branch[b] = "inflight" THEN {"settled", "stopped", "tail"} ELSE {"settled", "stopped"}) :
         /\ branch' = IF outcome = "stopped" /\ Mutant = "BranchStopStopsOthers"
                        THEN [c \in Branches |-> IF c = b \/ branch[c] \in {"inflight", "tail"} THEN "stopped" ELSE branch[c]]
                        ELSE [branch EXCEPT ![b] = outcome]
         /\ stoppedItself' = [stoppedItself EXCEPT ![b] = (outcome = "stopped")]
         /\ tailed' = [tailed EXCEPT ![b] = @ \/ outcome = "tail"]
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, allSettled, parallelDone>>

Release ==
    /\ owner = "disposed"
    /\ ~released
    /\ \A r \in Runs : pc[r] # "finally" \/ Mutant = "ReleaseBeforeFinally"
    /\ released' = TRUE
    /\ UNCHANGED <<trigger, parallel, finallyAfterRelease>>

AllSettled ==
    /\ ~parallelDone
    /\ CASE Mutant = "AllSettledEarly" -> \E b \in Branches : branch[b] # "inflight"
         [] Mutant = "JoinAtTheBranchAwait" -> \A b \in Branches : branch[b] # "inflight"
         [] OTHER -> \A b \in Branches : branch[b] \in {"settled", "stopped"}
    /\ allSettled' = allSettled + 1
    /\ parallelDone' = TRUE
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, branch, stoppedItself, tailed>>

Next ==
    \/ FireFirst \/ FireSecond \/ Dispose \/ Release \/ AllSettled
    \/ \E r \in Runs : Respond(r) \/ Route(r) \/ SettleTheTail(r) \/ Finally(r) \/ After(r)
    \/ \E b \in Branches : Settle(b)

Spec == Init /\ [][Next]_vars

FinallyRunsBeforeRelease == ~finallyAfterRelease

DisposedRunsNothingMore == ~ranWhileDisposed

ReplacedRunRunsNoRoute == ~routeAfterReplace

SkippedTriggerStartsNothing == skipped => pc["second"] = "skipped"

AllSettledAfterEveryRequest == allSettled > 0 => (allSettled = 1 /\ \A b \in Branches : branch[b] # "inflight")

BranchStopEndsOnlyItself == \A b \in Branches : branch[b] = "stopped" => stoppedItself[b]

JoinAfterEveryBranchTail == parallelDone => \A b \in Branches : branch[b] # "tail"

ReachReplaced == ~(replaced /\ pc["first"] = "ended" /\ finallyRan["first"] = 1 /\ routeRan["second"])

ReachDisposedInFlight == ~(owner = "disposed" /\ aborted["first"] /\ finallyRan["first"] = 1)

ReachAllSettledAfterAStop == ~(parallelDone /\ \E b \in Branches : branch[b] = "stopped")

ReachJoinAfterABranchTail == ~(parallelDone /\ \E b \in Branches : tailed[b])

ReachSkippedInTheTail == ~(skipped /\ pc["first"] = "tail")

====
