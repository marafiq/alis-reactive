---- MODULE Lanes ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "DisposeLeavesRunsGoing", "ReplaceKeepsOld", "SkipStartsAnyway", "AllSettledEarly",
            "BranchStopStopsOthers", "ReleaseBeforeFinally", "JoinAtTheBranchAwait"}

ASSUME Mutant \in Mutants

Runs == {"first", "second"}
Branches == {"one", "two"}

VARIABLES policy, owner, pc, completion, routeRan, finallyRan, decided, ranWhileDisposed, branch,
          branchCompletion, stoppedItself, allSettled, parallelDone, released, finallyAfterRelease, tail, tailPending,
          routeAfterReplace, tailed

trigger == <<policy, owner, pc, completion, routeRan, finallyRan, decided, ranWhileDisposed, tail, tailPending,
             routeAfterReplace>>
parallel == <<branch, branchCompletion, stoppedItself, allSettled, parallelDone, tailed>>

vars == <<trigger, parallel, released, finallyAfterRelease>>

Init ==
    /\ policy \in {"overlap", "replace", "skip"}
    /\ owner = "live"
    /\ pc = [r \in Runs |-> "idle"]
    /\ completion = [r \in Runs |-> "none"]
    /\ routeRan = [r \in Runs |-> FALSE]
    /\ finallyRan = [r \in Runs |-> 0]
    /\ decided = "none"
    /\ ranWhileDisposed = FALSE
    /\ branch = [b \in Branches |-> "attempt"]
    /\ branchCompletion = [b \in Branches |-> "none"]
    /\ stoppedItself = [b \in Branches |-> FALSE]
    /\ allSettled = 0
    /\ parallelDone = FALSE
    /\ released = FALSE
    /\ finallyAfterRelease = FALSE
    /\ tail \in [Runs -> BOOLEAN]
    /\ tailPending = [r \in Runs |-> FALSE]
    /\ routeAfterReplace = FALSE
    /\ tailed = [b \in Branches |-> FALSE]

InFlight(r) == pc[r] = "attempt" \/ (pc[r] = "route" /\ tailPending[r])

FireFirst ==
    /\ pc["first"] = "idle"
    /\ owner = "live"
    /\ pc' = [pc EXCEPT !["first"] = "attempt"]
    /\ UNCHANGED <<policy, owner, completion, routeRan, finallyRan, decided, ranWhileDisposed, tail, tailPending,
                   routeAfterReplace, parallel, released, finallyAfterRelease>>

FireSecond ==
    /\ decided = "none"
    /\ pc["first"] # "idle"
    /\ owner = "live"
    /\ CASE policy = "skip" /\ InFlight("first") /\ Mutant # "SkipStartsAnyway" ->
              /\ decided' = "skip"
              /\ UNCHANGED <<pc, completion>>
         [] policy = "replace" /\ InFlight("first") ->
              /\ decided' = "replace"
              /\ IF Mutant = "ReplaceKeepsOld"
                   THEN pc' = [pc EXCEPT !["second"] = "attempt"] /\ UNCHANGED completion
                   ELSE /\ pc' = [pc EXCEPT !["second"] = "attempt", !["first"] = "finally"]
                        /\ completion' = [completion EXCEPT !["first"] = "aborted"]
         [] OTHER ->
              /\ pc' = [pc EXCEPT !["second"] = "attempt"]
              /\ decided' = IF policy = "skip" /\ InFlight("first") THEN "skip" ELSE "overlap"
              /\ UNCHANGED completion
    /\ UNCHANGED <<policy, owner, routeRan, finallyRan, ranWhileDisposed, tail, tailPending, routeAfterReplace, parallel,
                   released, finallyAfterRelease>>

Respond(r) ==
    /\ pc[r] = "attempt"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ pc' = [pc EXCEPT ![r] = "route"]
    /\ UNCHANGED <<policy, owner, completion, routeRan, finallyRan, decided, ranWhileDisposed, tail, tailPending,
                   routeAfterReplace, parallel, released, finallyAfterRelease>>

Route(r) ==
    /\ pc[r] = "route"
    /\ ~tailPending[r]
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ routeRan' = [routeRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (decided = "replace" /\ r = "first"))
    /\ tailPending' = [tailPending EXCEPT ![r] = tail[r]]
    /\ pc' = [pc EXCEPT ![r] = IF tail[r] THEN "route" ELSE "finally"]
    /\ completion' = IF tail[r] THEN completion ELSE [completion EXCEPT ![r] = "completed"]
    /\ UNCHANGED <<policy, owner, finallyRan, decided, tail, parallel, released, finallyAfterRelease>>

SettleTheTail(r) ==
    /\ pc[r] = "route"
    /\ tailPending[r]
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ tailPending' = [tailPending EXCEPT ![r] = FALSE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (decided = "replace" /\ r = "first"))
    /\ pc' = [pc EXCEPT ![r] = "finally"]
    /\ completion' = [completion EXCEPT ![r] = "completed"]
    /\ UNCHANGED <<policy, owner, routeRan, finallyRan, decided, tail, parallel, released, finallyAfterRelease>>

Finally(r) ==
    /\ pc[r] = "finally"
    /\ finallyRan' = [finallyRan EXCEPT ![r] = @ + 1]
    /\ finallyAfterRelease' = (finallyAfterRelease \/ released)
    /\ pc' = [pc EXCEPT ![r] = IF completion[r] = "aborted" THEN "done" ELSE "after"]
    /\ UNCHANGED <<policy, owner, completion, routeRan, decided, ranWhileDisposed, tail, tailPending, routeAfterReplace,
                   parallel, released>>

After(r) ==
    /\ pc[r] = "after"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ UNCHANGED <<policy, owner, completion, routeRan, finallyRan, decided, tail, tailPending, routeAfterReplace,
                   parallel, released, finallyAfterRelease>>

Cut == {"attempt", "route", "finally", "after"}

EndOnDispose(s) ==
    CASE s \in {"attempt", "route"} -> "finally"
      [] s = "after" -> "done"
      [] OTHER -> s

Dispose ==
    /\ owner = "live"
    /\ owner' = "disposed"
    /\ IF Mutant = "DisposeLeavesRunsGoing"
         THEN UNCHANGED <<pc, completion>>
         ELSE /\ pc' = [r \in Runs |-> EndOnDispose(pc[r])]
              /\ completion' = [r \in Runs |-> IF pc[r] \in Cut THEN "aborted" ELSE completion[r]]
    /\ UNCHANGED <<policy, routeRan, finallyRan, decided, ranWhileDisposed, tail, tailPending, routeAfterReplace,
                   parallel, released, finallyAfterRelease>>

Settle(b) ==
    /\ branch[b] \in {"attempt", "route"}
    /\ \E how \in (IF branch[b] = "attempt" THEN {"completed", "stopped", "tail"} ELSE {"completed", "stopped"}) :
         LET stopsOthers == how = "stopped" /\ Mutant = "BranchStopStopsOthers"
             ends(c) == how # "tail" /\ (c = b \/ (stopsOthers /\ branch[c] \in {"attempt", "route"}))
         IN /\ branch' = [c \in Branches |-> IF c = b /\ how = "tail" THEN "route" ELSE IF ends(c) THEN "done" ELSE branch[c]]
            /\ branchCompletion' = [c \in Branches |-> IF ends(c) THEN how ELSE branchCompletion[c]]
            /\ stoppedItself' = [stoppedItself EXCEPT ![b] = (how = "stopped")]
            /\ tailed' = [tailed EXCEPT ![b] = @ \/ how = "tail"]
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, allSettled, parallelDone>>

Release ==
    /\ owner = "disposed"
    /\ ~released
    /\ \A r \in Runs : pc[r] # "finally" \/ Mutant = "ReleaseBeforeFinally"
    /\ released' = TRUE
    /\ UNCHANGED <<trigger, parallel, finallyAfterRelease>>

AllSettled ==
    /\ ~parallelDone
    /\ CASE Mutant = "AllSettledEarly" -> \E b \in Branches : branch[b] # "attempt"
         [] Mutant = "JoinAtTheBranchAwait" -> \A b \in Branches : branch[b] # "attempt"
         [] OTHER -> \A b \in Branches : branch[b] = "done"
    /\ allSettled' = allSettled + 1
    /\ parallelDone' = TRUE
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, branch, branchCompletion, stoppedItself, tailed>>

Next ==
    \/ FireFirst \/ FireSecond \/ Dispose \/ Release \/ AllSettled
    \/ \E r \in Runs : Respond(r) \/ Route(r) \/ SettleTheTail(r) \/ Finally(r) \/ After(r)
    \/ \E b \in Branches : Settle(b)

Spec == Init /\ [][Next]_vars

FinallyRunsBeforeRelease == ~finallyAfterRelease

DisposedRunsNothingMore == ~ranWhileDisposed

ReplacedRunRunsNoRoute == ~routeAfterReplace

SkippedTriggerStartsNothing == decided = "skip" => pc["second"] = "idle"

AllSettledAfterEveryRequest == allSettled > 0 => (allSettled = 1 /\ \A b \in Branches : branch[b] # "attempt")

BranchStopEndsOnlyItself == \A b \in Branches : branchCompletion[b] = "stopped" => stoppedItself[b]

JoinAfterEveryBranchTail == parallelDone => \A b \in Branches : branch[b] # "route"

ReachReplaced == ~(decided = "replace" /\ pc["first"] = "done" /\ completion["first"] = "aborted" /\ finallyRan["first"] = 1
                   /\ routeRan["second"])

ReachDisposedInFlight == ~(owner = "disposed" /\ completion["first"] = "aborted" /\ ~routeRan["first"] /\ finallyRan["first"] = 1)

ReachAllSettledAfterAStop == ~(parallelDone /\ \E b \in Branches : branchCompletion[b] = "stopped")

ReachJoinAfterABranchTail == ~(parallelDone /\ \E b \in Branches : tailed[b])

ReachSkippedInTheTail == ~(decided = "skip" /\ pc["first"] = "route" /\ tailPending["first"])

====
