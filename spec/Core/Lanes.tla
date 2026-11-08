---- MODULE Lanes ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "DisposeLeavesRunsGoing", "AbortSkipsFinally", "ReplaceKeepsOld", "SkipStartsAnyway",
            "AllSettledEarly", "BranchStopStopsOthers", "FinallyRunsTwice",
            "PayloadWriteAfterAwait", "ReleaseBeforeFinally", "FinallyAtTheRoutesAwait", "OuterFinallyFirst",
            "JoinAtTheBranchAwait"}

ASSUME Mutant \in Mutants

Runs == {"first", "second"}
Branches == {"one", "two"}
Steps == {"write", "await", "read"}
Handlers == [1..3 -> Steps]

VARIABLES policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped,
          ranWhileDisposed, branch, stoppedItself, allSettled, parallelDone, released, finallyAfterRelease,
          handler, tail, tailPending, tailRan, finallies, followedBeforeTail, routeAfterReplace, tailed

tails == <<tail, tailPending, tailRan, finallies, followedBeforeTail, routeAfterReplace>>

vars == <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped,
          ranWhileDisposed, branch, stoppedItself, allSettled, parallelDone, released, finallyAfterRelease,
          handler, tails, tailed>>

trigger == <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed>>
parallel == <<branch, stoppedItself, allSettled, parallelDone, tailed>>

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
    /\ tail \in [Runs -> BOOLEAN]
    /\ tailPending = [r \in Runs |-> FALSE]
    /\ tailRan = [r \in Runs |-> FALSE]
    /\ finallies = [r \in Runs |-> <<>>]
    /\ followedBeforeTail = FALSE
    /\ routeAfterReplace = FALSE
    /\ tailed = [b \in Branches |-> FALSE]

InFlight(r) == pc[r] \in {"inflight", "tail"}

AbortedAt(s) == IF s = "tail" /\ Mutant # "OuterFinallyFirst" THEN "innerFinally" ELSE "finally"

FireFirst ==
    /\ pc["first"] = "idle"
    /\ owner = "live"
    /\ pc' = [pc EXCEPT !["first"] = "inflight"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler, tails>>

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
                   ELSE /\ pc' = [pc EXCEPT !["second"] = "inflight", !["first"] = AbortedAt(pc["first"])]
                        /\ aborted' = [aborted EXCEPT !["first"] = TRUE]
              /\ UNCHANGED skipped
         [] OTHER ->
              /\ pc' = [pc EXCEPT !["second"] = "inflight"]
              /\ skipped' = (policy = "skip" /\ InFlight("first"))
              /\ UNCHANGED <<aborted, replaced>>
    /\ UNCHANGED <<policy, owner, routeRan, finallyRan, afterRan, ranWhileDisposed, parallel, released, finallyAfterRelease, handler, tails>>

Respond(r) ==
    /\ pc[r] = "inflight"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ pc' = [pc EXCEPT ![r] = "route"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler, tails>>

Route(r) ==
    /\ pc[r] = "route"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ routeRan' = [routeRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (replaced /\ r = "first"))
    /\ tailPending' = [tailPending EXCEPT ![r] = tail[r]]
    /\ pc' = [pc EXCEPT ![r] = IF tail[r] /\ Mutant # "FinallyAtTheRoutesAwait" THEN "tail" ELSE "finally"]
    /\ UNCHANGED <<policy, owner, aborted, finallyRan, afterRan, replaced, skipped, parallel, released, finallyAfterRelease, handler,
                   tail, tailRan, finallies, followedBeforeTail>>

SettleTheTail(r) ==
    /\ tailPending[r]
    /\ ~aborted[r]
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ tailRan' = [tailRan EXCEPT ![r] = TRUE]
    /\ tailPending' = [tailPending EXCEPT ![r] = FALSE]
    /\ finallies' = [finallies EXCEPT ![r] = Append(@, "inner")]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ routeAfterReplace' = (routeAfterReplace \/ (replaced /\ r = "first"))
    /\ pc' = [pc EXCEPT ![r] = IF pc[r] = "tail" THEN "finally" ELSE pc[r]]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, parallel, released, finallyAfterRelease, handler,
                   tail, followedBeforeTail>>

InnerFinally(r) ==
    /\ pc[r] = "innerFinally"
    /\ finallies' = [finallies EXCEPT ![r] = Append(@, "inner")]
    /\ tailPending' = [tailPending EXCEPT ![r] = FALSE]
    /\ finallyAfterRelease' = (finallyAfterRelease \/ released)
    /\ pc' = [pc EXCEPT ![r] = IF finallyRan[r] > 0 THEN "ended" ELSE "finally"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, handler,
                   tail, tailRan, followedBeforeTail, routeAfterReplace>>

Finally(r) ==
    /\ pc[r] = "finally"
    /\ finallyRan' = [finallyRan EXCEPT ![r] = @ + 1]
    /\ finallies' = [finallies EXCEPT ![r] = Append(@, "outer")]
    /\ finallyAfterRelease' = (finallyAfterRelease \/ released)
    /\ followedBeforeTail' = (followedBeforeTail \/ (tailPending[r] /\ ~aborted[r]))
    /\ pc' = [pc EXCEPT ![r] = IF Mutant = "FinallyRunsTwice" /\ finallyRan[r] = 0 THEN "finally"
                               ELSE IF tailPending[r] /\ aborted[r] THEN "innerFinally"
                               ELSE IF aborted[r] \/ owner = "disposed" THEN "ended" ELSE "after"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, handler,
                   tail, tailPending, tailRan, routeAfterReplace>>

After(r) ==
    /\ pc[r] = "after"
    /\ owner = "live" \/ Mutant = "DisposeLeavesRunsGoing"
    /\ afterRan' = [afterRan EXCEPT ![r] = TRUE]
    /\ ranWhileDisposed' = (ranWhileDisposed \/ owner = "disposed")
    /\ followedBeforeTail' = (followedBeforeTail \/ (tailPending[r] /\ ~aborted[r]))
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ UNCHANGED <<policy, owner, aborted, routeRan, finallyRan, replaced, skipped, parallel, released, finallyAfterRelease, handler,
                   tail, tailPending, tailRan, finallies, routeAfterReplace>>

EndOnDispose(s) ==
    CASE s \in {"inflight", "route"} -> IF Mutant = "AbortSkipsFinally" THEN "ended" ELSE "finally"
      [] s = "tail" -> IF Mutant = "AbortSkipsFinally" THEN "ended" ELSE AbortedAt(s)
      [] s = "after" -> "ended"
      [] OTHER -> s

Dispose ==
    /\ owner = "live"
    /\ owner' = "disposed"
    /\ IF Mutant = "DisposeLeavesRunsGoing"
         THEN UNCHANGED <<pc, aborted>>
         ELSE /\ pc' = [r \in Runs |-> EndOnDispose(pc[r])]
              /\ aborted' = [r \in Runs |-> aborted[r] \/ pc[r] \in {"inflight", "route", "tail"}]
    /\ UNCHANGED <<policy, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed, parallel, released, finallyAfterRelease, handler, tails>>

Settle(b) ==
    /\ branch[b] \in {"inflight", "tail"}
    /\ \E outcome \in (IF branch[b] = "inflight" THEN {"settled", "stopped", "tail"} ELSE {"settled", "stopped"}) :
         /\ branch' = IF outcome = "stopped" /\ Mutant = "BranchStopStopsOthers"
                        THEN [c \in Branches |-> IF c = b \/ branch[c] \in {"inflight", "tail"} THEN "stopped" ELSE branch[c]]
                        ELSE [branch EXCEPT ![b] = outcome]
         /\ stoppedItself' = [stoppedItself EXCEPT ![b] = (outcome = "stopped")]
         /\ tailed' = [tailed EXCEPT ![b] = @ \/ outcome = "tail"]
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, handler, allSettled, parallelDone, tails>>

Release ==
    /\ owner = "disposed"
    /\ ~released
    /\ \A r \in Runs : pc[r] \notin {"finally", "innerFinally"} \/ Mutant = "ReleaseBeforeFinally"
    /\ released' = TRUE
    /\ UNCHANGED <<policy, owner, pc, aborted, routeRan, finallyRan, afterRan, replaced, skipped, ranWhileDisposed,
                   parallel, finallyAfterRelease, handler, tails>>

AllSettled ==
    /\ ~parallelDone
    /\ CASE Mutant = "AllSettledEarly" -> \E b \in Branches : branch[b] # "inflight"
         [] Mutant = "JoinAtTheBranchAwait" -> \A b \in Branches : branch[b] # "inflight"
         [] OTHER -> \A b \in Branches : branch[b] \in {"settled", "stopped"}
    /\ allSettled' = allSettled + 1
    /\ parallelDone' = TRUE
    /\ UNCHANGED <<trigger, released, finallyAfterRelease, handler, branch, stoppedItself, tails, tailed>>

Next ==
    \/ FireFirst \/ FireSecond \/ Dispose \/ Release \/ AllSettled
    \/ \E r \in Runs : Respond(r) \/ Route(r) \/ SettleTheTail(r) \/ InnerFinally(r) \/ Finally(r) \/ After(r)
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

ReplacedRunRunsNoRoute == ~routeAfterReplace

SkippedTriggerStartsNothing == skipped => pc["second"] = "skipped"

AllSettledAfterEveryRequest == allSettled > 0 => (allSettled = 1 /\ \A b \in Branches : branch[b] # "inflight")

BranchStopEndsOnlyItself == \A b \in Branches : branch[b] = "stopped" => stoppedItself[b]

ReachReplaced == ~(replaced /\ pc["first"] = "ended" /\ finallyRan["first"] = 1 /\ routeRan["second"])

ReachDisposedInFlight == ~(owner = "disposed" /\ aborted["first"] /\ finallyRan["first"] = 1)

ReachAllSettledAfterAStop == ~(parallelDone /\ \E b \in Branches : branch[b] = "stopped")

TheRouteTailBeforeWhatFollows == ~followedBeforeTail

InnerFinallyFirst == \A r \in Runs : finallies[r] \in {<<>>, <<"inner">>, <<"outer">>, <<"inner", "outer">>}

JoinAfterEveryBranchTail == parallelDone => \A b \in Branches : branch[b] # "tail"

ReachTheRouteTail == ~(tailRan["first"] /\ afterRan["first"])

ReachAbortInTheTail == ~(aborted["first"] /\ finallies["first"] = <<"inner", "outer">>)

ReachJoinAfterABranchTail == ~(parallelDone /\ \E b \in Branches : tailed[b])

====
