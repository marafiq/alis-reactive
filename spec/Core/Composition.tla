---- MODULE Composition ----
EXTENDS Naturals, FiniteSets

CONSTANT Mutant

Mutants == {"none", "IndicatorNotCounted", "BeginEachAttempt", "NoticeEachAttempt"}

ASSUME Mutant \in Mutants

Requests == {"a", "b", "c", "d"}
Page == {"a", "b"}
Slot == {"c", "d"}
MaxAttempts == 2
InFlight == {"sending", "route", "settle"}

VARIABLES shape, takesDefaults, slot, pc, attempt, outcome, aborted, open, shown, notices

vars == <<shape, takesDefaults, slot, pc, attempt, outcome, aborted, open, shown, notices>>

Init ==
    /\ shape \in {"single", "parallel", "chained"}
    /\ takesDefaults \in [Requests -> BOOLEAN]
    /\ slot = "live"
    /\ pc = [r \in Requests |-> "idle"]
    /\ attempt = [r \in Requests |-> 0]
    /\ outcome = [r \in Requests |-> "none"]
    /\ aborted = [r \in Requests |-> FALSE]
    /\ open = 0
    /\ shown = FALSE
    /\ notices = [r \in Requests |-> 0]

Starts(r) ==
    CASE r = "a" -> TRUE
      [] r = "b" -> \/ shape = "parallel" /\ pc["a"] # "idle"
                    \/ shape = "chained" /\ pc["a"] = "done" /\ outcome["a"] = "ok"
      [] r = "c" -> slot = "live"
      [] r = "d" -> slot = "live" /\ pc["c"] # "idle"

Begin(r) ==
    /\ pc[r] = "idle"
    /\ Starts(r)
    /\ LET replacing == r = "d" /\ pc["c"] \in {"sending", "route"}
       IN /\ pc' = [q \in Requests |-> IF q = r THEN "sending" ELSE IF replacing /\ q = "c" THEN "settle" ELSE pc[q]]
          /\ aborted' = [aborted EXCEPT !["c"] = @ \/ replacing]
    /\ attempt' = [attempt EXCEPT ![r] = 1]
    /\ open' = IF takesDefaults[r] THEN open + 1 ELSE open
    /\ shown' = (shown \/ takesDefaults[r])
    /\ UNCHANGED <<shape, takesDefaults, slot, outcome, notices>>

Answer(r) ==
    /\ pc[r] = "sending"
    /\ \E answer \in {"ok", "fail", "retry"} :
         IF answer = "retry" /\ attempt[r] < MaxAttempts
           THEN /\ attempt' = [attempt EXCEPT ![r] = @ + 1]
                /\ open' = IF Mutant = "BeginEachAttempt" /\ takesDefaults[r] THEN open + 1 ELSE open
                /\ notices' = IF Mutant = "NoticeEachAttempt" /\ takesDefaults[r] THEN [notices EXCEPT ![r] = @ + 1] ELSE notices
                /\ UNCHANGED <<pc, outcome>>
           ELSE /\ pc' = [pc EXCEPT ![r] = "route"]
                /\ outcome' = [outcome EXCEPT ![r] = IF answer = "ok" THEN "ok" ELSE "fail"]
                /\ UNCHANGED <<attempt, open, notices>>
    /\ UNCHANGED <<shape, takesDefaults, slot, aborted, shown>>

Route(r) ==
    /\ pc[r] = "route"
    /\ pc' = [pc EXCEPT ![r] = "settle"]
    /\ notices' = IF outcome[r] = "fail" /\ takesDefaults[r] THEN [notices EXCEPT ![r] = @ + 1] ELSE notices
    /\ UNCHANGED <<shape, takesDefaults, slot, attempt, outcome, aborted, open, shown>>

Settle(r) ==
    /\ pc[r] = "settle"
    /\ pc' = [pc EXCEPT ![r] = IF aborted[r] THEN "ended" ELSE "done"]
    /\ open' = IF takesDefaults[r] THEN open - 1 ELSE open
    /\ shown' = IF ~takesDefaults[r] THEN shown
                ELSE IF Mutant = "IndicatorNotCounted" THEN FALSE
                ELSE open - 1 > 0
    /\ UNCHANGED <<shape, takesDefaults, slot, attempt, outcome, aborted, notices>>

Unload ==
    /\ slot = "live"
    /\ slot' = "gone"
    /\ pc' = [r \in Requests |-> IF r \in Slot /\ pc[r] \in {"sending", "route"} THEN "settle"
                                 ELSE IF r \in Slot /\ pc[r] = "idle" THEN "never"
                                 ELSE pc[r]]
    /\ aborted' = [r \in Requests |-> aborted[r] \/ (r \in Slot /\ pc[r] \in {"sending", "route"})]
    /\ UNCHANGED <<shape, takesDefaults, attempt, outcome, open, shown, notices>>

Next == Unload \/ \E r \in Requests : Begin(r) \/ Answer(r) \/ Route(r) \/ Settle(r)

Spec == Init /\ [][Next]_vars

Covered == {r \in Requests : takesDefaults[r] /\ pc[r] \in InFlight}

IndicatorNeverHidesEarly == Covered # {} => shown

IndicatorNeverStaysShown == shown => Covered # {}

NoticeOncePerFailure ==
    \A r \in Requests :
        notices[r] = IF takesDefaults[r] /\ ~aborted[r] /\ outcome[r] = "fail" /\ pc[r] \in {"settle", "done"} THEN 1 ELSE 0

ReachPageAndSlotCovered == ~(\E p \in Page, s \in Slot : p \in Covered /\ s \in Covered)

ReachReplacedWhileParallel == ~(shape = "parallel" /\ aborted["c"] /\ pc["c"] = "ended" /\ Covered = {"a", "b", "d"} /\ shown)

ReachUnloadedInFlightThenHidden ==
    ~(slot = "gone" /\ (\E r \in Slot : takesDefaults[r] /\ aborted[r] /\ pc[r] = "ended") /\ ~shown /\ Covered = {})

ReachChainedCovered == ~(shape = "chained" /\ pc["a"] = "done" /\ "b" \in Covered /\ shown)

ReachRetriedFailureNoticedOnce == ~(attempt["a"] = 2 /\ pc["a"] = "done" /\ outcome["a"] = "fail" /\ notices["a"] = 1)

====
