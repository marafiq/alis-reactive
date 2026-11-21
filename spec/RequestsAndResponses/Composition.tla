---- MODULE Composition ----
EXTENDS Naturals, FiniteSets

CONSTANT Mutant

Mutants == {"none", "IndicatorNotCounted", "BeginEachAttempt", "NoticeEachAttempt", "OneCountForAllTargets"}

ASSUME Mutant \in Mutants

Requests == {"a", "b", "c", "d"}
Page == {"a", "b"}
Slot == {"c", "d"}
MaxAttempts == 2
Holding == {"attempt", "route", "defaultFinally"}
Targets == {"x", "y"}

VARIABLES shape, takesDefaults, target, slot, pc, attempt, outcome, completion, open, shown, notices

vars == <<shape, takesDefaults, target, slot, pc, attempt, outcome, completion, open, shown, notices>>

Init ==
    /\ shape \in {"single", "parallel", "chained"}
    /\ takesDefaults \in [Requests -> BOOLEAN]
    /\ target \in {f \in [Requests -> SUBSET Targets] : f["a"] # {} /\ f["b"] # {} /\ f["c"] = {"y"} /\ f["d"] = {"y"}}
    /\ slot = "live"
    /\ pc = [r \in Requests |-> "idle"]
    /\ attempt = [r \in Requests |-> 0]
    /\ outcome = [r \in Requests |-> "none"]
    /\ completion = [r \in Requests |-> "none"]
    /\ open = [x \in Targets |-> 0]
    /\ shown = [x \in Targets |-> FALSE]
    /\ notices = [r \in Requests |-> 0]

Starts(r) ==
    CASE r = "a" -> TRUE
      [] r = "b" -> \/ shape = "parallel" /\ pc["a"] # "idle"
                    \/ shape = "chained" /\ pc["a"] = "done" /\ outcome["a"] = "ok"
      [] r = "c" -> slot = "live"
      [] r = "d" -> slot = "live" /\ pc["c"] # "idle"

Holds(r, x) == takesDefaults[r] /\ x \in target[r]

Begin(r) ==
    /\ pc[r] = "idle"
    /\ Starts(r)
    /\ LET replacing == r = "d" /\ pc["c"] \in {"attempt", "route"}
       IN /\ pc' = [q \in Requests |-> IF q = r THEN "attempt" ELSE IF replacing /\ q = "c" THEN "defaultFinally" ELSE pc[q]]
          /\ completion' = IF replacing THEN [completion EXCEPT !["c"] = "aborted"] ELSE completion
    /\ attempt' = [attempt EXCEPT ![r] = 1]
    /\ open' = [x \in Targets |-> IF Holds(r, x) THEN open[x] + 1 ELSE open[x]]
    /\ shown' = [x \in Targets |-> shown[x] \/ Holds(r, x)]
    /\ UNCHANGED <<shape, takesDefaults, target, slot, outcome, notices>>

Stop(r) ==
    /\ pc[r] = "idle"
    /\ r # "d"
    /\ Starts(r)
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ completion' = [completion EXCEPT ![r] = "stopped"]
    /\ UNCHANGED <<shape, takesDefaults, target, slot, attempt, outcome, open, shown, notices>>

Answer(r) ==
    /\ pc[r] = "attempt"
    /\ \E answer \in {"ok", "fail", "retry"} :
         IF answer = "retry" /\ attempt[r] < MaxAttempts
           THEN /\ attempt' = [attempt EXCEPT ![r] = @ + 1]
                /\ open' = [x \in Targets |-> IF Mutant = "BeginEachAttempt" /\ Holds(r, x) THEN open[x] + 1 ELSE open[x]]
                /\ notices' = IF Mutant = "NoticeEachAttempt" /\ takesDefaults[r] THEN [notices EXCEPT ![r] = @ + 1] ELSE notices
                /\ UNCHANGED <<pc, outcome>>
           ELSE /\ pc' = [pc EXCEPT ![r] = "route"]
                /\ outcome' = [outcome EXCEPT ![r] = IF answer = "ok" THEN "ok" ELSE "fail"]
                /\ UNCHANGED <<attempt, open, notices>>
    /\ UNCHANGED <<shape, takesDefaults, target, slot, completion, shown>>

Route(r) ==
    /\ pc[r] = "route"
    /\ pc' = [pc EXCEPT ![r] = "defaultFinally"]
    /\ notices' = IF outcome[r] = "fail" /\ takesDefaults[r] THEN [notices EXCEPT ![r] = @ + 1] ELSE notices
    /\ completion' = [completion EXCEPT ![r] = "completed"]
    /\ UNCHANGED <<shape, takesDefaults, target, slot, attempt, outcome, open, shown>>

DefaultFinally(r) ==
    /\ pc[r] = "defaultFinally"
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ open' = [x \in Targets |-> IF Holds(r, x) THEN open[x] - 1 ELSE open[x]]
    /\ LET anotherHolds == \E q \in Requests \ {r} : takesDefaults[q] /\ pc[q] \in Holding
       IN shown' = [x \in Targets |->
                      IF ~Holds(r, x) THEN shown[x]
                      ELSE IF Mutant = "IndicatorNotCounted" THEN FALSE
                      ELSE IF Mutant = "OneCountForAllTargets" THEN anotherHolds
                      ELSE open[x] - 1 > 0]
    /\ UNCHANGED <<shape, takesDefaults, target, slot, attempt, outcome, completion, notices>>

Unload ==
    /\ slot = "live"
    /\ slot' = "gone"
    /\ pc' = [r \in Requests |-> IF r \in Slot /\ pc[r] \in {"attempt", "route"} THEN "defaultFinally" ELSE pc[r]]
    /\ completion' = [r \in Requests |-> IF r \in Slot /\ pc[r] \in {"attempt", "route"} THEN "aborted" ELSE completion[r]]
    /\ UNCHANGED <<shape, takesDefaults, target, attempt, outcome, open, shown, notices>>

Next == Unload \/ \E r \in Requests : Begin(r) \/ Stop(r) \/ Answer(r) \/ Route(r) \/ DefaultFinally(r)

Spec == Init /\ [][Next]_vars

Covered == {r \in Requests : takesDefaults[r] /\ pc[r] \in Holding}

CoveredAt(x) == {r \in Covered : x \in target[r]}

AnyShown == \E x \in Targets : shown[x]

IndicatorNeverHidesEarly == \A x \in Targets : CoveredAt(x) # {} => shown[x]

IndicatorNeverStaysShown == \A x \in Targets : shown[x] => CoveredAt(x) # {}

HoldOncePerRequest == \A x \in Targets : open[x] = Cardinality(CoveredAt(x))

NoticeOncePerFailure ==
    \A r \in Requests :
        notices[r] = IF takesDefaults[r] /\ completion[r] # "aborted" /\ outcome[r] = "fail" /\ pc[r] \in {"defaultFinally", "done"} THEN 1 ELSE 0

ReachPageAndSlotCovered == ~(\E p \in Page, s \in Slot : p \in Covered /\ s \in Covered)

ReachReplacedWhileParallel == ~(shape = "parallel" /\ completion["c"] = "aborted" /\ pc["c"] = "done" /\ Covered = {"a", "b", "d"} /\ shown["y"])

ReachUnloadedInFlightThenHidden ==
    ~(slot = "gone" /\ (\E r \in Slot : takesDefaults[r] /\ completion[r] = "aborted" /\ pc[r] = "done") /\ ~AnyShown /\ Covered = {})

ReachChainedCovered == ~(shape = "chained" /\ pc["a"] = "done" /\ "b" \in Covered /\ AnyShown)

ReachStoppedWhileAnotherHolds == ~(completion["c"] = "stopped" /\ "a" \in Covered /\ AnyShown)

ReachSameTargetLaterSettles ==
    ~(shape = "parallel" /\ target["a"] = {"x"} /\ target["b"] = {"x"} /\ pc["a"] = "done" /\ "b" \in Covered /\ shown["x"])

ReachOwnTargetHidesFirst ==
    ~(shape = "parallel" /\ target["a"] = {"x"} /\ target["b"] = {"y"} /\ pc["a"] = "done" /\ "b" \in Covered
      /\ ~shown["x"] /\ shown["y"])

ReachTwoTargetsOneRequest == ~(target["a"] = {"x", "y"} /\ "a" \in Covered /\ shown["x"] /\ shown["y"])

ReachRetriedFailureNoticedOnce == ~(attempt["a"] = 2 /\ pc["a"] = "done" /\ outcome["a"] = "fail" /\ notices["a"] = 1)

====
