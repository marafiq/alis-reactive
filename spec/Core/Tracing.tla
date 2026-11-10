---- MODULE Tracing ----
EXTENDS Naturals, FiniteSets

CONSTANT Mutant

Mutants == {"none", "RecordsWhenOff", "ReplacedRunLeftOpen", "DisposeEndsRunFirst", "AttemptHeaderFromRun",
            "HeaderToEveryOrigin", "SinkErrorStopsRun", "HeaderNeedsSink", "RetryUntracedWhenOff", "ResendCountMissing"}

ASSUME Mutant \in Mutants

SpanNames == {"run", "send", "attempt1", "attempt2"}

Parent(s) == IF s = "run" THEN "plan" ELSE IF s = "send" THEN "run" ELSE "send"

Children(s) == {c \in SpanNames : Parent(c) = s}

AttemptName(n) == IF n = 1 THEN "attempt1" ELSE "attempt2"

VARIABLES traceOn, origin, sinkFails, status, pc, attempts, spans, sent, outcome

vars == <<traceOn, origin, sinkFails, status, pc, attempts, spans, sent, outcome>>

Init ==
    /\ traceOn \in BOOLEAN
    /\ origin \in {"own", "other"}
    /\ sinkFails \in BOOLEAN
    /\ status = "idle"
    /\ pc = "idle"
    /\ attempts = 0
    /\ spans = [s \in SpanNames |-> "unstarted"]
    /\ sent = {}
    /\ outcome = "none"

Records == traceOn \/ Mutant = "RecordsWhenOff"

Started(s) == IF Records THEN [spans EXCEPT ![s] = "open"] ELSE spans

StartedAll(names) == IF Records THEN [s \in SpanNames |-> IF s \in names THEN "open" ELSE spans[s]] ELSE spans

Ended(s) == IF spans[s] = "open" THEN [spans EXCEPT ![s] = "ended"] ELSE spans

EndAll == [s \in SpanNames |-> IF spans[s] = "open" THEN "ended" ELSE spans[s]]

Header(n, failing) ==
    IF origin = "other" /\ Mutant # "HeaderToEveryOrigin" THEN "none"
    ELSE IF Records /\ (Mutant # "HeaderNeedsSink" \/ ~failing) THEN (IF Mutant = "AttemptHeaderFromRun" THEN "run" ELSE AttemptName(n))
    ELSE IF n >= 2 /\ Mutant # "RetryUntracedWhenOff" THEN "minted"
    ELSE "none"

Resend(n) == IF Mutant = "ResendCountMissing" THEN 0 ELSE n - 1

Trigger ==
    /\ status = "idle"
    /\ IF Records /\ sinkFails /\ Mutant = "SinkErrorStopsRun"
       THEN status' = "ended" /\ outcome' = "sinkError" /\ UNCHANGED <<pc, spans>>
       ELSE status' = "running" /\ pc' = "check" /\ spans' = Started("run") /\ UNCHANGED outcome
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Attempt ==
    /\ status = "running"
    /\ pc \in {"check", "attempt"}
    /\ attempts < 2
    /\ LET n == attempts + 1 IN
        /\ attempts' = n
        /\ spans' = StartedAll(IF n = 1 THEN {"send", AttemptName(n)} ELSE {AttemptName(n)})
        /\ sent' = sent \cup {[n |-> n, to |-> origin, parent |-> Header(n, sinkFails), resend |-> Resend(n)]}
    /\ pc' = "attempt"
    /\ status' = "waiting"
    /\ UNCHANGED <<traceOn, origin, sinkFails, outcome>>

Respond ==
    /\ status = "waiting"
    /\ pc = "attempt"
    /\ \E retry \in IF attempts < 2 THEN BOOLEAN ELSE {FALSE} :
        IF retry
        THEN /\ spans' = Ended(AttemptName(attempts))
             /\ UNCHANGED pc
        ELSE /\ spans' = [Ended(AttemptName(attempts)) EXCEPT !["send"] = IF @ = "open" THEN "ended" ELSE @]
             /\ pc' = "route"
    /\ status' = "running"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent, outcome>>

Route ==
    /\ status = "running"
    /\ pc = "route"
    /\ \E how \in {"completed", "stopped"} : outcome' = how
    /\ spans' = Ended("run")
    /\ pc' = "done"
    /\ status' = "ended"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Abort ==
    /\ status \in {"running", "waiting"}
    /\ pc # "done"
    /\ pc' = "done"
    /\ \E why \in {"replaced", "disposed"} :
        /\ outcome' = why
        /\ IF why = "replaced" /\ Mutant = "ReplacedRunLeftOpen"
           THEN spans' = spans /\ status' = "ended"
           ELSE IF why = "disposed" /\ Mutant = "DisposeEndsRunFirst"
           THEN spans' = Ended("run") /\ status' = "running"
           ELSE spans' = EndAll /\ status' = "ended"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Next == Trigger \/ Attempt \/ Respond \/ Route \/ Abort

Spec == Init /\ [][Next]_vars

NothingRecordedWhenOff == ~traceOn => ((\A s \in SpanNames : spans[s] = "unstarted") /\ (\A r \in sent : r.n = 1 => r.parent = "none"))

EveryStartedSpanEnds == status = "ended" => \A s \in SpanNames : spans[s] # "open"

ChildEndsBeforeParent == \A s \in SpanNames : spans[s] = "ended" => \A c \in Children(s) : spans[c] # "open"

AttemptsJoinTheTrace == \A r \in sent : (traceOn /\ ~sinkFails /\ r.to = "own") => r.parent = AttemptName(r.n)

TraceHeadersStayHome == \A r \in sent : r.to = "other" => r.parent = "none"

SinkFailureChangesNothing == outcome # "sinkError"

SinkFailureKeepsHeaders == \A r \in sent : r.parent = Header(r.n, FALSE)

RetriesAlwaysTraced == \A r \in sent : (r.n >= 2 /\ r.to = "own") => r.parent # "none"

ResendCountSent == \A r \in sent : r.parent # "none" => r.resend = r.n - 1

ReachRetriedAttemptTraced == ~(traceOn /\ ~sinkFails /\ spans["attempt2"] = "ended" /\ outcome = "completed")

ReachDisposedInFlight == ~(traceOn /\ ~sinkFails /\ outcome = "disposed" /\ spans["attempt1"] = "ended" /\ status = "ended")

ReachRetryTracedWhileOff == ~(~traceOn /\ \E r \in sent : r.n = 2 /\ r.parent = "minted" /\ r.resend = 1)

====
