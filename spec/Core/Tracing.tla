---- MODULE Tracing ----
EXTENDS Naturals, FiniteSets

CONSTANT Mutant

Mutants == {"none", "RecordsWhenOff", "ReplacedRunLeftOpen", "DisposeEndsRunFirst", "AttemptHeaderFromRun",
            "HeaderToEveryOrigin", "SinkErrorStopsRun"}

ASSUME Mutant \in Mutants

SpanNames == {"run", "send", "attempt1", "attempt2"}

Parent(s) == IF s = "run" THEN "plan" ELSE IF s = "send" THEN "run" ELSE "send"

Children(s) == {c \in SpanNames : Parent(c) = s}

AttemptName(n) == IF n = 1 THEN "attempt1" ELSE "attempt2"

VARIABLES traceOn, origin, sinkFails, phase, attempts, spans, sent, outcome

vars == <<traceOn, origin, sinkFails, phase, attempts, spans, sent, outcome>>

Init ==
    /\ traceOn \in BOOLEAN
    /\ origin \in {"own", "other"}
    /\ sinkFails \in BOOLEAN
    /\ phase = "idle"
    /\ attempts = 0
    /\ spans = [s \in SpanNames |-> "unstarted"]
    /\ sent = {}
    /\ outcome = "running"

Records == traceOn \/ Mutant = "RecordsWhenOff"

Recording == Records /\ ~sinkFails

Started(s) == IF Recording THEN [spans EXCEPT ![s] = "open"] ELSE spans

Ended(s) == IF spans[s] = "open" THEN [spans EXCEPT ![s] = "ended"] ELSE spans

EndAll == [s \in SpanNames |-> IF spans[s] = "open" THEN "ended" ELSE spans[s]]

Header(attempt) ==
    IF ~Recording THEN "none"
    ELSE IF origin = "other" /\ Mutant # "HeaderToEveryOrigin" THEN "none"
    ELSE IF Mutant = "AttemptHeaderFromRun" THEN "run"
    ELSE attempt

Trigger ==
    /\ phase = "idle"
    /\ IF Records /\ sinkFails /\ Mutant = "SinkErrorStopsRun"
       THEN phase' = "over" /\ outcome' = "sinkError" /\ UNCHANGED spans
       ELSE phase' = "running" /\ spans' = Started("run") /\ UNCHANGED outcome
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Send ==
    /\ phase = "running"
    /\ phase' = "sending"
    /\ spans' = Started("send")
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent, outcome>>

Attempt ==
    /\ phase = "sending"
    /\ attempts < 2
    /\ LET n == attempts + 1 IN
        /\ attempts' = n
        /\ spans' = Started(AttemptName(n))
        /\ sent' = sent \cup {[n |-> n, to |-> origin, parent |-> Header(AttemptName(n))]}
    /\ phase' = "awaiting"
    /\ UNCHANGED <<traceOn, origin, sinkFails, outcome>>

Respond ==
    /\ phase = "awaiting"
    /\ \E retry \in IF attempts < 2 THEN BOOLEAN ELSE {FALSE} :
        IF retry
        THEN /\ spans' = Ended(AttemptName(attempts))
             /\ phase' = "sending"
        ELSE /\ spans' = [Ended(AttemptName(attempts)) EXCEPT !["send"] = IF @ = "open" THEN "ended" ELSE @]
             /\ phase' = "route"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent, outcome>>

Route ==
    /\ phase = "route"
    /\ \E how \in {"completed", "stopped"} : outcome' = how
    /\ spans' = Ended("run")
    /\ phase' = "over"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Abort ==
    /\ phase \in {"running", "sending", "awaiting", "route"}
    /\ \E why \in {"replaced", "disposed"} :
        /\ outcome' = why
        /\ IF why = "replaced" /\ Mutant = "ReplacedRunLeftOpen"
           THEN spans' = spans /\ phase' = "over"
           ELSE IF why = "disposed" /\ Mutant = "DisposeEndsRunFirst"
           THEN spans' = Ended("run") /\ phase' = "ending"
           ELSE spans' = EndAll /\ phase' = "over"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent>>

Finish ==
    /\ phase = "ending"
    /\ spans' = EndAll
    /\ phase' = "over"
    /\ UNCHANGED <<traceOn, origin, sinkFails, attempts, sent, outcome>>

Next == Trigger \/ Send \/ Attempt \/ Respond \/ Route \/ Abort \/ Finish

Spec == Init /\ [][Next]_vars

NothingRecordedWhenOff == ~traceOn => ((\A s \in SpanNames : spans[s] = "unstarted") /\ (\A r \in sent : r.parent = "none"))

EveryStartedSpanEnds == phase = "over" => \A s \in SpanNames : spans[s] # "open"

ChildEndsBeforeParent == \A s \in SpanNames : spans[s] = "ended" => \A c \in Children(s) : spans[c] # "open"

AttemptsJoinTheTrace == \A r \in sent : (traceOn /\ ~sinkFails /\ r.to = "own") => r.parent = AttemptName(r.n)

TraceHeadersStayHome == \A r \in sent : r.to = "other" => r.parent = "none"

SinkFailureChangesNothing == outcome # "sinkError"

ReachRetriedAttemptTraced == ~(traceOn /\ ~sinkFails /\ spans["attempt2"] = "ended" /\ outcome = "completed")

ReachDisposedInFlight == ~(traceOn /\ ~sinkFails /\ outcome = "disposed" /\ spans["attempt1"] = "ended" /\ phase = "over")

====
