---- MODULE Absence ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "RequiredAbsentContinues", "PromiseHeldContinues", "DataAbsentStops",
            "MissingTextShowsUndefined", "MissingClassAddsUndefined", "UnawaitedPromiseContinues",
            "EagerTerms", "ConversionPassesRaw", "AnyOverNothingIsFalse", "StopSkipsFinally",
            "StopEndsEveryHandler", "MissingValueWritesUndefined"}

ASSUME Mutant \in Mutants

Program == <<"ReadLive", "ReadData", "ReadPast", "SetText", "SetClass", "SetProperty", "Call", "Cond", "Convert", "Any", "Request">>
Pos(s) == CHOOSE i \in 1..Len(Program) : Program[i] = s
Last == Len(Program) + 1
Reads == {"live", "data", "past", "as", "any"}

VARIABLES liveMember, readKind, dataMember, holder, textValue, className, methodReturns, op, term1,
          convertible, list, route, propertyValue,
          pcA, stoppedA, stopAt, result, shown, classes, written, called, sent, finallyRan, pcB, stoppedB

page == <<liveMember, readKind, dataMember, holder, textValue, className, methodReturns, op, term1,
          convertible, list, route, propertyValue>>
outcome == <<result, shown, classes, written, called, sent, finallyRan>>
vars == <<page, outcome, pcA, stoppedA, stopAt, pcB, stoppedB>>

Init ==
    /\ liveMember \in {"present", "absent"}
    /\ readKind \in {"value", "promise"}
    /\ dataMember \in {"present", "absent"}
    /\ holder \in {"holdsField", "lacksField", "nothing"}
    /\ textValue \in {"text", "nothing"}
    /\ className \in {"cls", "nothing"}
    /\ methodReturns \in {"nothing", "value", "promise"}
    /\ op \in {"and", "or"}
    /\ term1 \in BOOLEAN
    /\ convertible \in BOOLEAN
    /\ list \in {"nothing", "empty", "items"}
    /\ route \in {"value", "nothing"}
    /\ propertyValue \in {"value", "nothing"}
    /\ pcA = 1
    /\ stoppedA = FALSE
    /\ stopAt = "none"
    /\ result = [r \in Reads |-> "unread"]
    /\ shown = "unset"
    /\ classes = {}
    /\ written = "unset"
    /\ called = {}
    /\ sent = FALSE
    /\ finallyRan = FALSE
    /\ pcB = 1
    /\ stoppedB = FALSE

At(s) == pcA = Pos(s) /\ ~stoppedA

Stop(here) ==
    /\ stoppedA' = TRUE
    /\ stopAt' = here
    /\ pcA' = pcA
    /\ stoppedB' = IF Mutant = "StopEndsEveryHandler" THEN TRUE ELSE stoppedB

Advance ==
    /\ pcA' = pcA + 1
    /\ UNCHANGED <<stoppedA, stopAt, stoppedB>>

ReadLive ==
    /\ At("ReadLive")
    /\ IF liveMember = "absent" /\ Mutant # "RequiredAbsentContinues"
         THEN Stop("ReadLive") /\ UNCHANGED result
       ELSE IF liveMember = "present" /\ readKind = "promise" /\ Mutant # "PromiseHeldContinues"
         THEN Stop("ReadLive") /\ UNCHANGED result
       ELSE Advance /\ result' = [result EXCEPT !["live"] = IF liveMember = "absent" THEN "nothing" ELSE "value"]
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

ReadData ==
    /\ At("ReadData")
    /\ IF dataMember = "absent" /\ Mutant = "DataAbsentStops"
         THEN Stop("ReadData") /\ UNCHANGED result
       ELSE Advance /\ result' = [result EXCEPT !["data"] = IF dataMember = "absent" THEN "nothing" ELSE "value"]
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

ReadPast ==
    /\ At("ReadPast")
    /\ Advance
    /\ result' = [result EXCEPT !["past"] = IF holder = "holdsField" THEN "value" ELSE "nothing"]
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

SetText ==
    /\ At("SetText")
    /\ Advance
    /\ shown' = IF textValue = "text" THEN "text"
                ELSE IF Mutant = "MissingTextShowsUndefined" THEN "undefined" ELSE ""
    /\ UNCHANGED <<page, result, classes, written, called, sent, finallyRan, pcB>>

SetClass ==
    /\ At("SetClass")
    /\ Advance
    /\ classes' = IF className = "cls" THEN classes \cup {"cls"}
                  ELSE IF Mutant = "MissingClassAddsUndefined" THEN classes \cup {"undefined"} ELSE classes
    /\ UNCHANGED <<page, result, shown, written, called, sent, finallyRan, pcB>>

SetProperty ==
    /\ At("SetProperty")
    /\ Advance
    /\ written' = IF propertyValue = "value" THEN "value"
                  ELSE IF Mutant = "MissingValueWritesUndefined" THEN "undefined" ELSE "null"
    /\ UNCHANGED <<page, result, shown, classes, called, sent, finallyRan, pcB>>

Call ==
    /\ At("Call")
    /\ called' = called \cup {"m1"}
    /\ IF methodReturns = "promise" /\ Mutant # "UnawaitedPromiseContinues"
         THEN Stop("Call")
       ELSE Advance
    /\ UNCHANGED <<page, result, shown, classes, written, sent, finallyRan, pcB>>

SecondTermReached ==
    IF Mutant = "EagerTerms" THEN TRUE
    ELSE IF op = "and" THEN term1 ELSE ~term1

Cond ==
    /\ At("Cond")
    /\ Advance
    /\ called' = IF SecondTermReached THEN called \cup {"m2"} ELSE called
    /\ UNCHANGED <<page, result, shown, classes, written, sent, finallyRan, pcB>>

Convert ==
    /\ At("Convert")
    /\ Advance
    /\ result' = [result EXCEPT !["as"] = IF convertible THEN "value"
                                          ELSE IF Mutant = "ConversionPassesRaw" THEN "raw" ELSE "nothing"]
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

AnyOf ==
    CASE list = "items" -> "true"
      [] list = "empty" -> "false"
      [] list = "nothing" -> IF Mutant = "AnyOverNothingIsFalse" THEN "false" ELSE "nothing"

Any ==
    /\ At("Any")
    /\ Advance
    /\ result' = [result EXCEPT !["any"] = AnyOf]
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

Request ==
    /\ At("Request")
    /\ IF route = "nothing"
         THEN /\ Stop("Request")
              /\ sent' = FALSE
              /\ finallyRan' = (Mutant # "StopSkipsFinally")
       ELSE /\ Advance
            /\ sent' = TRUE
            /\ finallyRan' = TRUE
    /\ UNCHANGED <<page, result, shown, classes, written, called, pcB>>

OtherHandler ==
    /\ pcB = 1
    /\ ~stoppedB
    /\ pcB' = 2
    /\ UNCHANGED <<page, outcome, pcA, stoppedA, stopAt, stoppedB>>

Next == ReadLive \/ ReadData \/ ReadPast \/ SetText \/ SetClass \/ SetProperty \/ Call \/ Cond \/ Convert \/ Any \/ Request \/ OtherHandler

Spec == Init /\ [][Next]_vars

Ran(s) == pcA > Pos(s)
StoppedAt(s) == stoppedA /\ stopAt = s

RequiredAbsentStops == liveMember = "absent" => pcA <= Pos("ReadLive")

PromiseWhereValueStops == (liveMember = "present" /\ readKind = "promise") => pcA <= Pos("ReadLive")

DataAbsentReadsNothing ==
    /\ stopAt \notin {"ReadData", "ReadPast"}
    /\ Ran("ReadData") => result["data"] = IF dataMember = "absent" THEN "nothing" ELSE "value"
    /\ Ran("ReadPast") => result["past"] = IF holder = "holdsField" THEN "value" ELSE "nothing"

MissingTextShowsNothing == (Ran("SetText") /\ textValue = "nothing") => shown = ""

MissingClassChangesNothing == className = "nothing" => classes = {}

UnawaitedPromiseStops == methodReturns = "promise" => pcA <= Pos("Call")

ConditionTermsLazy == Ran("Cond") => (("m2" \in called) <=> ((op = "and" /\ term1) \/ (op = "or" /\ ~term1)))

FailedConversionReadsNothing == Ran("Convert") => result["as"] = IF convertible THEN "value" ELSE "nothing"

MissingListReadsNothing == (Ran("Any") /\ list = "nothing") => result["any"] = "nothing"

MissingRouteStopsAndFinallyRuns ==
    /\ (route = "nothing" /\ StoppedAt("Request")) => (~sent /\ finallyRan)
    /\ route = "nothing" => ~Ran("Request")
    /\ Ran("Request") => (sent /\ finallyRan)

StopEndsOnlyItsHandler == ~stoppedB

MissingValueWritesNull == Ran("SetProperty") => written = IF propertyValue = "value" THEN "value" ELSE "null"

ReachCompleted == ~(pcA = Last /\ pcB = 2)

ReachRouteStopRunsFinally == ~(StoppedAt("Request") /\ finallyRan /\ pcB = 2)

====
