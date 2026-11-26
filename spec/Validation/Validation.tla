---- MODULE Validation ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "ConditionReadsNothing", "ObjectStartFromModel", "NullHolderReadsNothing", "ChildRulesOverNull",
            "PopulateIgnored", "EmptyTextStaysText", "ReadsUnsentInputs", "RunsEveryRegistration", "TypedTextLeftOut",
            "FailureUnderAnotherName", "PeerReadFromTheItem", "OutsideReadAtItsStart", "QueryBesideUnreadOnNet48",
            "QueryBesideReadOnNet10", "OutsideReadOnNet48Alone", "OutsideEmptyStaysText", "ConfinedEndpointLeavesWhatWasSent",
            "ModelTakesTheQueryBesideItsBody"}

ASSUME Mutant \in Mutants

Inputs == {"absent", "x", "empty"}

Checks == {"item"}

ItemValues == {"none", "low", "high"}

VARIABLES host, format, handling, sendsForm, nInput, nBeside, nPlace, nPlaceText, binding, nStart,
          leadStart, leadStartName, leadTypeName, leadPhoneInput, leadNameInput,
          itemsStart, itemTypeName, row, rowName, countText, otherRegistered,
          plane, check, typed, start, peer

vars == <<host, format, handling, sendsForm, nInput, nBeside, nPlace, nPlaceText, binding, nStart, leadStart, leadStartName, leadTypeName,
          leadPhoneInput, leadNameInput, itemsStart, itemTypeName, row, rowName, countText, otherRegistered,
          plane, check, typed, start, peer>>

BindingsOf(h, f) == IF h = "net48" THEN {"everyPlace"}
                    ELSE IF f = "json" THEN {"bodyAlone", "copied"}
                    ELSE {"everyPlace", "bodyAlone", "copied"}

InTheBody == format # "none" /\ ((sendsForm /\ nInput # "absent") \/ nBeside)

Carried == nPlace = "path" /\ format # "none"

SentOutsideAlone == nPlace # "none" /\ ~InTheBody /\ ~Carried

Served ==
    \/ ~SentOutsideAlone
    \/ format = "none" /\ (binding # "bodyAlone" \/ Mutant = "ConfinedEndpointLeavesWhatWasSent")
    \/ format # "none" /\ (host = "net48" \/ binding = "bodyAlone" \/ Mutant = "ModelTakesTheQueryBesideItsBody")

Init ==
    /\ format \in {"form", "json", "none"}
    /\ sendsForm \in BOOLEAN
    /\ \/ /\ plane = "binding"
          /\ host \in {"net10", "net48"}
          /\ handling \in {"replace", "populate"}
          /\ nInput \in Inputs
          /\ nBeside \in BOOLEAN
          /\ nPlace \in IF nBeside THEN {"none"} ELSE {"none", "path", "query"}
          /\ nPlaceText \in IF nPlace = "query" THEN {"x", "empty"} ELSE {"x"}
          /\ binding \in BindingsOf(host, format)
          /\ Served
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
          /\ check = "item"
          /\ typed = "none"
          /\ start = "none"
          /\ peer = "none"
       \/ /\ plane = "check"
          /\ host = "net10"
          /\ handling = "replace"
          /\ nInput = "x"
          /\ nBeside = FALSE
          /\ nPlace = "none"
          /\ nPlaceText = "x"
          /\ binding = "bodyAlone"
          /\ nStart = "x"
          /\ leadStart = "object"
          /\ leadStartName = "x"
          /\ leadTypeName = "x"
          /\ leadPhoneInput = FALSE
          /\ leadNameInput = "x"
          /\ itemsStart = "empty"
          /\ itemTypeName = "x"
          /\ row = FALSE
          /\ rowName = "x"
          /\ countText = "5"
          /\ otherRegistered = FALSE
          /\ check \in Checks
          /\ typed \in ItemValues
          /\ start \in ItemValues
          /\ peer \in {"none", "mid"}

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

Bound(text) == IF text = "empty" /\ format = "form" THEN "none" ELSE text

BindsMembers == format # "none"

NSent == BindsMembers /\ ((sendsForm /\ nInput # "absent") \/ nBeside)
NSentText == IF nBeside THEN "x" ELSE nInput

OutsideBound == IF nPlaceText = "empty" THEN "none" ELSE nPlaceText

HoldsWhatWasSent == binding \in {"everyPlace", "copied"}
LeadNameSent == BindsMembers /\ sendsForm /\ leadNameInput # "absent"
LeadSent == BindsMembers /\ sendsForm /\ (leadPhoneInput \/ leadNameInput # "absent")
ItemsSent == BindsMembers /\ sendsForm /\ row
ItemNameSent == ItemsSent /\ rowName # "absent"
CountSent == BindsMembers /\ sendsForm

FillsStart == format = "form" \/ host = "net48" \/ handling = "populate"
AppendsList == format = "json" /\ host = "net10" /\ handling = "populate"

StartItems == IF itemsStart = "seeded" THEN <<"x">> ELSE <<>>

ServerN ==
    IF NSent THEN Bound(NSentText)
    ELSE IF Carried THEN Bound(nPlaceText)
    ELSE IF nPlace # "none" /\ HoldsWhatWasSent THEN OutsideBound
    ELSE nStart

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

ReadsBesideABodyThatBindsMembers ==
    \/ host = "net48" /\ Mutant # "QueryBesideUnreadOnNet48"
    \/ host = "net10" /\ Mutant = "QueryBesideReadOnNet10"

ReadsWhereTheBodyBindsNone ==
    /\ Mutant # "OutsideReadAtItsStart"
    /\ Mutant = "OutsideReadOnNet48Alone" => host = "net48"

ReadsOutside == nPlace # "none" /\ IF BindsMembers THEN ReadsBesideABodyThatBindsMembers ELSE ReadsWhereTheBodyBindsNone

BrowserN ==
    IF NSent THEN (IF Mutant = "EmptyTextStaysText" THEN NSentText ELSE Bound(NSentText))
    ELSE IF Mutant = "ReadsUnsentInputs" /\ nInput # "absent" THEN Bound(nInput)
    ELSE IF Carried THEN Bound(nPlaceText)
    ELSE IF ReadsOutside THEN (IF Mutant = "OutsideEmptyStaysText" THEN nPlaceText ELSE OutsideBound)
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

Held == IF BindsMembers /\ sendsForm THEN Bound(typed) ELSE start

AtLeast(value, least) == value = "none" \/ (least = "mid" /\ value = "high")

ServerPasses == CASE check = "item" -> AtLeast(Held, peer)

BrowserPeer == IF Mutant = "PeerReadFromTheItem" THEN "none" ELSE peer

BrowserPasses == CASE check = "item" -> AtLeast(Held, BrowserPeer)

ConditionAgrees == BrowserLeadPresent = ServerLeadPresent

ObjectStartAgrees == (LeadSent /\ leadStart = "object") => BrowserLeadName = ServerLeadName

NewHolderAgrees == (LeadSent /\ leadStart = "null") => BrowserLeadName = ServerLeadName

ChildSkipAgrees == BrowserLeadRulesRun = ServerLeadRulesRun

ListStartAgrees == BrowserItems = ServerItems

EmptyTextAgrees == NSent => BrowserN = ServerN

UnsentInputsUnread == ~NSent => BrowserN = ServerN

SentOutsideAgrees == nPlace # "none" => BrowserN = ServerN

OnlyTheNamedValidator == ~BrowserRunsOther

NoTypedValueLost == BrowserCountRefused = ServerCountRefused

ItemPeerAgrees == (plane = "check" /\ check = "item") => BrowserPasses = ServerPasses

Agreement == BrowserFails = ServerFails /\ (plane = "check" => BrowserPasses = ServerPasses)

ReachPopulatedList == ~(AppendsList /\ itemsStart = "seeded" /\ ItemsSent /\ Len(BrowserItems) = 2 /\ Agreement)

ReachAnItemBelowItsPeer == ~(plane = "check" /\ check = "item" /\ Held = "low" /\ peer = "mid" /\ ~ServerPasses /\ Agreement)

ReachConditionOnStart == ~(~LeadSent /\ leadStart = "object" /\ ~NSent /\ nStart = "none" /\ "n" \notin BrowserFails /\ Agreement)

OnTheRuleOverN == ~LeadSent /\ leadStart = "null" /\ nStart = "none"

ReachAPathMemberCheckedAsSent == ~(~BindsMembers /\ nPlace = "path" /\ OnTheRuleOverN /\ "n" \notin BrowserFails /\ Agreement)

ReachAQueryMemberCheckedBesideItsBodyOnNet48 == ~(host = "net48" /\ BindsMembers /\ ~NSent /\ nPlace = "query" /\ nPlaceText = "x" /\ OnTheRuleOverN /\ "n" \notin BrowserFails /\ Agreement)

ReachAQueryMemberLeftToItsBodyOnNet10 == ~(host = "net10" /\ binding = "bodyAlone" /\ BindsMembers /\ ~NSent /\ nPlace = "query" /\ OnTheRuleOverN /\ "n" \in BrowserFails /\ Agreement)

ReachAnInputOverWhatIsSentOutside == ~(NSent /\ nPlace = "query" /\ nPlaceText = "x" /\ NSentText = "empty" /\ format = "form" /\ OnTheRuleOverN /\ "n" \in BrowserFails /\ Agreement)

ReachAnUnsentMemberOnItsStart == ~(~BindsMembers /\ nPlace = "none" /\ OnTheRuleOverN /\ "n" \in BrowserFails /\ Agreement)

====
