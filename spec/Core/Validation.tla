---- MODULE Validation ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "ConditionReadsNothing", "ObjectStartFromModel", "NullHolderReadsNothing", "ChildRulesOverNull",
            "PopulateIgnored", "EmptyTextStaysText", "ReadsUnsentInputs", "RunsEveryRegistration", "TypedTextLeftOut",
            "FailureUnderAnotherName"}

ASSUME Mutant \in Mutants

Inputs == {"absent", "x", "empty"}

VARIABLES host, format, handling, sendsForm, nInput, nBeside, nStart,
          leadStart, leadStartName, leadTypeName, leadPhoneInput, leadNameInput,
          itemsStart, itemTypeName, row, rowName, countText, otherRegistered

vars == <<host, format, handling, sendsForm, nInput, nBeside, nStart, leadStart, leadStartName, leadTypeName,
          leadPhoneInput, leadNameInput, itemsStart, itemTypeName, row, rowName, countText, otherRegistered>>

Init ==
    /\ host \in {"net10", "net48"}
    /\ format \in {"form", "json"}
    /\ handling \in {"replace", "populate"}
    /\ sendsForm \in BOOLEAN
    /\ nInput \in Inputs
    /\ nBeside \in BOOLEAN
    /\ nStart \in {"x", "none"}
    /\ leadStart \in {"object", "null"}
    /\ leadStartName \in {"x", "none"}
    /\ leadTypeName \in {"x", "none"}
    /\ leadPhoneInput \in BOOLEAN
    /\ leadNameInput \in Inputs
    /\ itemsStart \in {"empty", "seeded"}
    /\ itemTypeName \in {"x", "none"}
    /\ row \in BOOLEAN
    /\ rowName \in Inputs
    /\ countText \in {"5", "12a"}
    /\ otherRegistered \in BOOLEAN

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

Bound(text) == IF text = "empty" /\ format = "form" THEN "none" ELSE text

NSent == (sendsForm /\ nInput # "absent") \/ nBeside
NSentText == IF nBeside THEN "x" ELSE nInput
LeadNameSent == sendsForm /\ leadNameInput # "absent"
LeadSent == sendsForm /\ (leadPhoneInput \/ leadNameInput # "absent")
ItemsSent == sendsForm /\ row
ItemNameSent == ItemsSent /\ rowName # "absent"
CountSent == sendsForm

FillsStart == format = "form" \/ host = "net48" \/ handling = "populate"
AppendsList == format = "json" /\ host = "net10" /\ handling = "populate"

StartItems == IF itemsStart = "seeded" THEN <<"x">> ELSE <<>>

ServerN == IF NSent THEN Bound(NSentText) ELSE nStart

ServerLeadPresent == LeadSent \/ leadStart = "object"

ServerLeadName ==
    IF ~LeadSent THEN (IF leadStart = "object" THEN leadStartName ELSE "none")
    ELSE IF LeadNameSent THEN Bound(leadNameInput)
    ELSE IF leadStart = "object" /\ FillsStart THEN leadStartName
    ELSE leadTypeName

ServerItems ==
    IF ~ItemsSent THEN StartItems
    ELSE (IF AppendsList THEN StartItems ELSE <<>>) \o <<IF ItemNameSent THEN Bound(rowName) ELSE itemTypeName>>

ServerLeadRulesRun == ServerLeadPresent

ServerCountRefused == CountSent /\ countText = "12a"

HasNone(items) == \E i \in 1..Len(items) : items[i] = "none"

ServerFails ==
    (IF ~ServerLeadPresent /\ ServerN = "none" THEN {"n"} ELSE {})
    \cup (IF ServerLeadRulesRun /\ ServerLeadName = "none" THEN {"lead.name"} ELSE {})
    \cup (IF HasNone(ServerItems) THEN {"items.name"} ELSE {})
    \cup (IF ServerCountRefused THEN {"count"} ELSE {})

BrowserN ==
    IF NSent THEN (IF Mutant = "EmptyTextStaysText" THEN NSentText ELSE Bound(NSentText))
    ELSE IF Mutant = "ReadsUnsentInputs" /\ nInput # "absent" THEN Bound(nInput)
    ELSE nStart

BrowserLeadPresent ==
    IF Mutant = "ConditionReadsNothing" THEN LeadSent ELSE LeadSent \/ leadStart = "object"

StartOfSentLead ==
    IF leadStart = "object" /\ (FillsStart \/ Mutant = "ObjectStartFromModel") THEN leadStartName
    ELSE IF leadStart = "null" /\ Mutant = "NullHolderReadsNothing" THEN "none"
    ELSE leadTypeName

BrowserLeadName ==
    IF LeadNameSent THEN Bound(leadNameInput)
    ELSE IF LeadSent THEN StartOfSentLead
    ELSE IF leadStart = "object" THEN leadStartName
    ELSE "none"

BrowserItems ==
    IF ~ItemsSent THEN StartItems
    ELSE (IF AppendsList /\ Mutant # "PopulateIgnored" THEN StartItems ELSE <<>>)
         \o <<IF ItemNameSent THEN Bound(rowName) ELSE itemTypeName>>

BrowserLeadRulesRun == IF Mutant = "ChildRulesOverNull" THEN TRUE ELSE BrowserLeadPresent

BrowserCountRefused == CountSent /\ countText = "12a" /\ Mutant # "TypedTextLeftOut"

BrowserRunsOther == otherRegistered /\ Mutant = "RunsEveryRegistration"

LeadFailure == IF Mutant = "FailureUnderAnotherName" THEN "n" ELSE "lead.name"

BrowserFails ==
    (IF ~BrowserLeadPresent /\ BrowserN = "none" THEN {"n"} ELSE {})
    \cup (IF BrowserLeadRulesRun /\ BrowserLeadName = "none" THEN {LeadFailure} ELSE {})
    \cup (IF HasNone(BrowserItems) THEN {"items.name"} ELSE {})
    \cup (IF BrowserCountRefused THEN {"count"} ELSE {})
    \cup (IF BrowserRunsOther THEN {"other"} ELSE {})

ConditionAgrees == BrowserLeadPresent = ServerLeadPresent

ObjectStartAgrees == (LeadSent /\ leadStart = "object") => BrowserLeadName = ServerLeadName

NewHolderAgrees == (LeadSent /\ leadStart = "null") => BrowserLeadName = ServerLeadName

ChildSkipAgrees == BrowserLeadRulesRun = ServerLeadRulesRun

ListStartAgrees == BrowserItems = ServerItems

EmptyTextAgrees == NSent => BrowserN = ServerN

UnsentInputsUnread == ~NSent => BrowserN = ServerN

OnlyTheNamedValidator == ~BrowserRunsOther

NoTypedValueLost == BrowserCountRefused = ServerCountRefused

Agreement == BrowserFails = ServerFails

ReachPopulatedList == ~(AppendsList /\ itemsStart = "seeded" /\ ItemsSent /\ Len(BrowserItems) = 2 /\ Agreement)

ReachConditionOnStart == ~(~LeadSent /\ leadStart = "object" /\ ~NSent /\ nStart = "none" /\ "n" \notin BrowserFails /\ Agreement)

====
