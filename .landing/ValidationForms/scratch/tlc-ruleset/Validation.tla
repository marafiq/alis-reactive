---- MODULE Validation ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "ConditionReadsNothing", "ObjectStartFromModel", "NullHolderReadsNothing", "ChildRulesOverNull",
            "PopulateIgnored", "EmptyTextStaysText", "ReadsUnsentInputs", "RunsEveryRegistration", "TypedTextLeftOut",
            "FailureUnderAnotherName", "PeerReadFromTheItem", "UnboundBodyReadsBeside",
            "CheckRunsTheDefaultSet", "ANamedSetNoRuleDeclaresRenders"}

ASSUME Mutant \in Mutants

Inputs == {"absent", "x", "empty"}

Checks == {"item"}

ItemValues == {"none", "low", "high"}

VARIABLES host, format, handling, sendsForm, nInput, nBeside, nStart,
          leadStart, leadStartName, leadTypeName, leadPhoneInput, leadNameInput,
          itemsStart, itemTypeName, row, rowName, countText, otherRegistered,
          plane, check, typed, start, peer, asked, inDefault, inNamed

vars == <<host, format, handling, sendsForm, nInput, nBeside, nStart, leadStart, leadStartName, leadTypeName,
          leadPhoneInput, leadNameInput, itemsStart, itemTypeName, row, rowName, countText, otherRegistered,
          plane, check, typed, start, peer, asked, inDefault, inNamed>>

Init ==
    /\ format \in {"form", "json", "none"}
    /\ sendsForm \in BOOLEAN
    /\ \/ /\ plane = "binding"
          /\ host \in {"net10", "net48"}
          /\ handling \in {"replace", "populate"}
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
          /\ check = "item"
          /\ typed = "none"
          /\ start = "none"
          /\ peer = "none"
          /\ asked = "default"
          /\ inDefault = TRUE
          /\ inNamed = FALSE
       \/ /\ plane = "check"
          /\ host = "net10"
          /\ handling = "replace"
          /\ nInput = "x"
          /\ nBeside = FALSE
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
          /\ asked \in {"default", "named"}
          /\ inDefault \in BOOLEAN
          /\ inNamed \in BOOLEAN
          /\ (asked = "named" /\ ~inNamed) => Mutant = "ANamedSetNoRuleDeclaresRenders"

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

Bound(text) == IF text = "empty" /\ format = "form" THEN "none" ELSE text

BindsMembers == format # "none"

NSent == BindsMembers /\ ((sendsForm /\ nInput # "absent") \/ nBeside)
NSentText == IF nBeside THEN "x" ELSE nInput
LeadNameSent == BindsMembers /\ sendsForm /\ leadNameInput # "absent"
LeadSent == BindsMembers /\ sendsForm /\ (leadPhoneInput \/ leadNameInput # "absent")
ItemsSent == BindsMembers /\ sendsForm /\ row
ItemNameSent == ItemsSent /\ rowName # "absent"
CountSent == BindsMembers /\ sendsForm

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
    ELSE IF Mutant = "UnboundBodyReadsBeside" /\ nBeside THEN NSentText
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

RuleRuns(set) == IF set = "named" THEN inNamed ELSE inDefault

BrowserAsked == IF Mutant = "CheckRunsTheDefaultSet" THEN "default" ELSE asked

ServerPasses == CASE check = "item" -> ~RuleRuns(asked) \/ AtLeast(Held, peer)

BrowserPeer == IF Mutant = "PeerReadFromTheItem" THEN "none" ELSE peer

BrowserPasses == CASE check = "item" -> ~RuleRuns(BrowserAsked) \/ AtLeast(Held, BrowserPeer)

ConditionAgrees == BrowserLeadPresent = ServerLeadPresent

ObjectStartAgrees == (LeadSent /\ leadStart = "object") => BrowserLeadName = ServerLeadName

NewHolderAgrees == (LeadSent /\ leadStart = "null") => BrowserLeadName = ServerLeadName

ChildSkipAgrees == BrowserLeadRulesRun = ServerLeadRulesRun

ListStartAgrees == BrowserItems = ServerItems

EmptyTextAgrees == NSent => BrowserN = ServerN

UnsentInputsUnread == ~NSent => BrowserN = ServerN

UnboundBodyChecksTheStarts == ~BindsMembers => BrowserN = nStart /\ ServerN = nStart

OnlyTheNamedValidator == ~BrowserRunsOther

NoTypedValueLost == BrowserCountRefused = ServerCountRefused

ItemPeerAgrees == (plane = "check" /\ check = "item") => BrowserPasses = ServerPasses

TheNamedSetIsTheOneRun == plane = "check" => RuleRuns(BrowserAsked) = RuleRuns(asked)

ANamedSetSelectsARule == (plane = "check" /\ asked = "named") => inNamed

Agreement == BrowserFails = ServerFails /\ (plane = "check" => BrowserPasses = ServerPasses)

ReachANamedSetRunsARuleTheDefaultLeavesOut == ~(plane = "check" /\ asked = "named" /\ inNamed /\ ~inDefault /\ ~ServerPasses /\ Agreement)

ReachTheDefaultSetLeavesANamedRuleOut == ~(plane = "check" /\ asked = "default" /\ inNamed /\ ~inDefault /\ Held = "low" /\ peer = "mid" /\ ServerPasses /\ Agreement)

ReachPopulatedList == ~(AppendsList /\ itemsStart = "seeded" /\ ItemsSent /\ Len(BrowserItems) = 2 /\ Agreement)

ReachAnItemBelowItsPeer == ~(plane = "check" /\ check = "item" /\ Held = "low" /\ peer = "mid" /\ ~ServerPasses /\ Agreement)

ReachConditionOnStart == ~(~LeadSent /\ leadStart = "object" /\ ~NSent /\ nStart = "none" /\ "n" \notin BrowserFails /\ Agreement)

ReachUnboundBodyOnTheStarts == ~(~BindsMembers /\ nBeside /\ leadStart = "null" /\ nStart = "none" /\ "n" \in BrowserFails /\ Agreement)

====