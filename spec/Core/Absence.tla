---- MODULE Absence ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "RequiredAbsentContinues", "PromiseHeldContinues", "DataAbsentReadsNothing",
            "NullHolderReadsStart", "RequiredLackingReads",
            "MissingTextShowsUndefined", "MissingClassAddsUndefined", "UnawaitedPromiseContinues",
            "EagerTerms", "ConversionPassesRaw", "AnyOverNothingIsFalse", "StopSkipsFinally",
            "StopEndsEveryHandler", "MissingValueWritesUndefined"}

ASSUME Mutant \in Mutants

Program == <<"ReadLive", "ReadData", "ReadPast", "SetText", "SetClass", "SetProperty", "Call", "Cond", "Convert", "Any", "Request">>
Pos(s) == CHOOSE i \in 1..Len(Program) : Program[i] = s
Last == Len(Program) + 1
Reads == {"live", "data", "past", "as", "any"}

VARIABLES liveMember, readKind, dataMember, absence, required, holder, textValue, className, methodReturns, op, term1,
          convertible, list, pathValue, propertyValue,
          pcA, stoppedA, stopAt, result, shown, classes, written, called, sent, finallyRan, pcB, stoppedB

page == <<liveMember, readKind, dataMember, absence, required, holder, textValue, className, methodReturns, op, term1,
          convertible, list, pathValue, propertyValue>>
outcome == <<result, shown, classes, written, called, sent, finallyRan>>
vars == <<page, outcome, pcA, stoppedA, stopAt, pcB, stoppedB>>

Init ==
    /\ liveMember \in {"present", "absent"}
    /\ readKind \in {"value", "promise"}
    /\ dataMember \in {"present", "absent"}
    /\ absence \in {"start", "nothing", "unbound"}
    /\ required \in {"held", "lacking"}
    /\ holder \in {"holdsField", "lacksField", "nothing"}
    /\ textValue \in {"text", "nothing"}
    /\ className \in {"cls", "nothing"}
    /\ methodReturns \in {"nothing", "value", "promise"}
    /\ op \in {"and", "or"}
    /\ term1 \in BOOLEAN
    /\ convertible \in BOOLEAN
    /\ list \in {"nothing", "empty", "items"}
    /\ pathValue \in {"value", "nothing"}
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

AbsentRead == CASE absence = "start" -> "start"
                [] absence = "nothing" -> "nothing"
                [] absence = "unbound" -> "stop"

ReadOf(member) ==
    IF required = "lacking" /\ Mutant # "RequiredLackingReads" THEN "stop"
    ELSE IF member = "present" THEN "value"
    ELSE IF absence = "start" /\ Mutant = "DataAbsentReadsNothing" THEN "nothing"
    ELSE AbsentRead

PastMember == IF holder = "holdsField" THEN "present" ELSE "absent"

PastRead == IF holder = "nothing" /\ Mutant # "NullHolderReadsStart" THEN "nothing" ELSE ReadOf(PastMember)

ReadInto(step, slot, read) ==
    IF read = "stop" THEN Stop(step) /\ UNCHANGED result
    ELSE Advance /\ result' = [result EXCEPT ![slot] = read]

ReadData ==
    /\ At("ReadData")
    /\ ReadInto("ReadData", "data", ReadOf(dataMember))
    /\ UNCHANGED <<page, shown, classes, written, called, sent, finallyRan, pcB>>

ReadPast ==
    /\ At("ReadPast")
    /\ ReadInto("ReadPast", "past", PastRead)
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
    /\ IF pathValue = "nothing"
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

ExpectedRead(member) == IF member = "present" THEN "value" ELSE AbsentRead

DataAbsentReadsItsStart ==
    required = "held" =>
        /\ StoppedAt("ReadData") => ExpectedRead(dataMember) = "stop"
        /\ Ran("ReadData") => result["data"] = ExpectedRead(dataMember)
        /\ (StoppedAt("ReadPast") /\ holder # "nothing") => ExpectedRead(PastMember) = "stop"
        /\ (Ran("ReadPast") /\ holder # "nothing") => result["past"] = ExpectedRead(PastMember)

NullHolderReadsNothing ==
    /\ holder = "nothing" => ~StoppedAt("ReadPast")
    /\ (Ran("ReadPast") /\ holder = "nothing") => result["past"] = "nothing"

LackingRequiredStops == required = "lacking" => pcA <= Pos("ReadData")

MissingTextShowsNothing == (Ran("SetText") /\ textValue = "nothing") => shown = ""

MissingClassChangesNothing == className = "nothing" => classes = {}

UnawaitedPromiseStops == methodReturns = "promise" => pcA <= Pos("Call")

ConditionTermsLazy == Ran("Cond") => (("m2" \in called) <=> ((op = "and" /\ term1) \/ (op = "or" /\ ~term1)))

FailedConversionReadsNothing == Ran("Convert") => result["as"] = IF convertible THEN "value" ELSE "nothing"

MissingListReadsNothing == (Ran("Any") /\ list = "nothing") => result["any"] = "nothing"

MissingPathValueStopsAndFinallyRuns ==
    /\ (pathValue = "nothing" /\ StoppedAt("Request")) => (~sent /\ finallyRan)
    /\ pathValue = "nothing" => ~Ran("Request")
    /\ Ran("Request") => (sent /\ finallyRan)

StopEndsOnlyItsHandler == ~stoppedB

MissingValueWritesNull == Ran("SetProperty") => written = IF propertyValue = "value" THEN "value" ELSE "null"

ReachCompleted == ~(pcA = Last /\ pcB = 2)

ReachPathValueStopRunsFinally == ~(StoppedAt("Request") /\ finallyRan /\ pcB = 2)

====
