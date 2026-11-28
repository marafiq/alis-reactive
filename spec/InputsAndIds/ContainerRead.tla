---- MODULE ContainerRead ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "DuplicatesSent", "TwoInputsReadTheLast", "BlankReadsNothing", "UnsentReadsNothing",
            "CachedInputs", "PageWideRead", "UnlistedStopsTheSend", "UnlistedReadsItsText", "UnnamedStartSentAsHeld",
            "JsonSentAsHeld", "ItemNothingStopsTheSendAlone", "BlankAloneStopsTheSendAlone", "HandWrittenControlSent",
            "HiddenLeftUnsent", "HiddenSentOutsideAFormBody", "PrefixKept", "OutsideThePrefixSent"}

ASSUME Mutant \in Mutants

Inputs == {"own", "slot", "other"}

Texts == {"n1", "n2", "blank", "unlisted", "itemNothing", "blankAlone"}

Hosts == {"form", "json"}

Wires == {"form", "json", "query"}

Servers == {"net10", "net48"}

DomOrder == <<"own", "slot">>

ContainerOf(i) == IF i = "other" THEN "elsewhere" ELSE "container"

VARIABLES containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, hidden, prefixed, slotOutside, read, readFrom, hiddenSent, cached, cache

page == <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, hidden, prefixed, slotOutside>>
vars == <<page, read, readFrom, hiddenSent, cached, cache>>

Range(s) == {s[i] : i \in DOMAIN s}

OnPage(i) == CASE i = "own" -> ownRendered
               [] i = "slot" -> slotLoaded
               [] i = "other" -> TRUE

Value(i) == IF text[i] \in {"blank", "unlisted"} THEN "start" ELSE text[i]

Body == SelectSeq(DomOrder, LAMBDA i : OnPage(i))

Unlisted(i) == text[i] = "unlisted" \/ (text[i] = "blank" /\ ~startNamed)

Outside(i) == prefixed /\ slotOutside /\ i = "slot"

KeepsThePrefix == prefixed /\ Mutant = "PrefixKept"

SendStops == ~containerOnPage \/ Len(Body) >= 2 \/ (\E i \in Range(Body) : text[i] \in {"itemNothing", "blankAlone"})
             \/ (Mutant = "UnlistedStopsTheSend" /\ \E i \in Range(Body) : Unlisted(i))
             \/ (Mutant # "OutsideThePrefixSent" /\ \E i \in Range(Body) : Outside(i))

BodyLeaves == IF Mutant = "DuplicatesSent" THEN containerOnPage ELSE ~SendStops

Sent(i) == CASE text[i] \in {"n1", "n2"} -> "name"
             [] startNamed -> "name"
             [] Mutant = "UnnamedStartSentAsHeld" -> "held"
             [] OTHER -> "none"

Bound(i, h, s) == CASE h = "json" -> IF Mutant = "JsonSentAsHeld" /\ heldRefused /\ s = "net10" THEN "refused" ELSE Value(i)
                    [] Sent(i) = "name" -> Value(i)
                    [] Sent(i) = "none" -> "start"
                    [] OTHER -> IF s = "net10" THEN "refused" ELSE "start"

HandWrittenBinds == Mutant = "HandWrittenControlSent" /\ containerOnPage /\ handWritten = "typed"

Bind(h, s) == CASE Body = <<>> /\ HandWrittenBinds -> "handWritten"
                [] Body = <<>> -> "start"
                [] KeepsThePrefix -> "start"
                [] h = "form" -> Bound(Body[1], h, s)
                [] h = "json" -> Bound(Body[Len(Body)], h, s)

HiddenReaches(w) == CASE Mutant = "HiddenLeftUnsent" -> FALSE
                    [] Mutant = "HiddenSentOutsideAFormBody" -> hidden
                    [] OTHER -> hidden /\ w = "form"

Table == {i \in Inputs : OnPage(i)}

Hits == IF Mutant = "PageWideRead" THEN Table ELSE {i \in Table : ContainerOf(i) = "container"}

Found == IF Mutant = "CachedInputs" /\ cached THEN cache ELSE Hits

ValueRead(i) == CASE KeepsThePrefix -> "start"
                  [] text[i] = "blank" /\ Mutant = "BlankReadsNothing" -> "nothing"
                  [] text[i] = "unlisted" /\ Mutant = "UnlistedReadsItsText" -> "unlisted"
                  [] OTHER -> Value(i)

TwoFound == Cardinality(Found) >= 2

ItemNothingFound == Mutant # "ItemNothingStopsTheSendAlone" /\ \E i \in Found : text[i] = "itemNothing"

BlankAloneFound == Mutant # "BlankAloneStopsTheSendAlone" /\ \E i \in Found : text[i] = "blankAlone"

TakesTheLast == TwoFound /\ Mutant = "TwoInputsReadTheLast"

OutsideFound == Mutant # "OutsideThePrefixSent" /\ \E i \in Found : Outside(i)

ReadValue ==
    CASE ~containerOnPage -> "stop"
      [] TakesTheLast -> ValueRead("slot")
      [] ItemNothingFound -> "stop"
      [] BlankAloneFound -> "stop"
      [] OutsideFound -> "stop"
      [] Found = {} -> IF Mutant = "UnsentReadsNothing" THEN "nothing" ELSE "start"
      [] TwoFound -> "stop"
      [] OTHER -> ValueRead(CHOOSE i \in Found : TRUE)

ReadNames ==
    CASE ~containerOnPage -> {}
      [] TakesTheLast -> {"slot"}
      [] OTHER -> Found

Unread ==
    /\ read' = "unread"
    /\ readFrom' = {}
    /\ hiddenSent' = [w \in Wires |-> FALSE]

Init ==
    /\ containerOnPage \in BOOLEAN
    /\ ownRendered \in IF containerOnPage THEN BOOLEAN ELSE {FALSE}
    /\ slotLoaded = FALSE
    /\ slotLeft = FALSE
    /\ text \in {t \in [Inputs -> Texts] : t["slot"] = "blank"}
    /\ startNamed \in BOOLEAN
    /\ heldRefused \in BOOLEAN
    /\ handWritten \in {"none", "typed"}
    /\ hidden \in BOOLEAN
    /\ prefixed \in BOOLEAN
    /\ slotOutside \in IF prefixed THEN BOOLEAN ELSE {FALSE}
    /\ read = "unread"
    /\ readFrom = {}
    /\ hiddenSent = [w \in Wires |-> FALSE]
    /\ cached = FALSE
    /\ cache = {}

Read ==
    /\ read = "unread"
    /\ read' = ReadValue
    /\ readFrom' = ReadNames
    /\ hiddenSent' = [w \in Wires |-> containerOnPage /\ ~SendStops /\ HiddenReaches(w)]
    /\ cached' = (cached \/ Mutant = "CachedInputs")
    /\ cache' = IF Mutant = "CachedInputs" /\ ~cached THEN Hits ELSE cache
    /\ UNCHANGED page

Type(i, t) ==
    /\ OnPage(i)
    /\ text[i] # t
    /\ text' = [text EXCEPT ![i] = t]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, startNamed, heldRefused, handWritten, hidden, prefixed, slotOutside, cached, cache>>

LoadSlot ==
    /\ containerOnPage
    /\ ~slotLoaded
    /\ slotLoaded' = TRUE
    /\ \E t \in Texts : text' = [text EXCEPT !["slot"] = t]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLeft, startNamed, heldRefused, handWritten, hidden, prefixed, slotOutside, cached, cache>>

UnloadSlot ==
    /\ slotLoaded
    /\ slotLoaded' = FALSE
    /\ slotLeft' = TRUE
    /\ text' = [text EXCEPT !["slot"] = "blank"]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, startNamed, heldRefused, handWritten, hidden, prefixed, slotOutside, cached, cache>>

HoldHidden ==
    /\ containerOnPage
    /\ ~hidden
    /\ hidden' = TRUE
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, prefixed, slotOutside, cached, cache>>

DropHidden ==
    /\ hidden
    /\ hidden' = FALSE
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, prefixed, slotOutside, cached, cache>>

Next == Read \/ LoadSlot \/ UnloadSlot \/ HoldHidden \/ DropHidden \/ \E i \in Inputs, t \in Texts : Type(i, t)

Spec == Init /\ [][Next]_vars

Done == read # "unread"

OneValueOnEveryHost == BodyLeaves => \A h \in Hosts, s \in Servers : Bind(h, s) = Bind("form", "net10")

ReadStopsWhenItsSendStops == Done => ((read = "stop") <=> SendStops)

ReadIsWhatTheEndpointBinds == (Done /\ ~SendStops /\ Len(Body) = 1) => \A h \in Hosts, s \in Servers : read = Bind(h, s)

AnUnsentMemberReadsWhatTheEndpointBinds == (Done /\ containerOnPage /\ Body = <<>>) => \A h \in Hosts, s \in Servers : read = Bind(h, s)

ReadSeesTheSlotThatJoinedOrLeftBeforeIt == (Done /\ containerOnPage) => (("slot" \in readFrom) <=> slotLoaded)

ReadFindsWhatItsContainerSends == (Done /\ containerOnPage) => readFrom = Range(Body)

AHiddenInputReachesAFormBodyAlone == Done => \A w \in Wires : hiddenSent[w] = (w = "form" /\ hidden /\ containerOnPage /\ ~SendStops)

ATypedTextReachesTheEndpoint ==
    (BodyLeaves /\ Len(Body) = 1 /\ text[Body[1]] \in {"n1", "n2"}) => \A h \in Hosts, s \in Servers : Bind(h, s) = text[Body[1]]

AnInputOutsideThePrefixStopsTheSend == (containerOnPage /\ \E i \in Range(Body) : Outside(i)) => ~BodyLeaves

ReachStopOnceASlotsInputJoins == ~(read = "stop" /\ containerOnPage /\ ownRendered /\ slotLoaded)

ReachStartOnceTheSlotLeft == ~(read = "start" /\ slotLeft /\ ~slotLoaded /\ ~ownRendered /\ containerOnPage)

ReachBlankReadsItsStart == ~(read = "start" /\ Len(Body) = 1 /\ text[Body[1]] = "blank")

ReachMissingContainerStops == ~(read = "stop" /\ ~containerOnPage)

ReachUnlistedReadsItsStart == ~(read = "start" /\ Len(Body) = 1 /\ text[Body[1]] = "unlisted")

ReachUnnamedStartLeftOut == ~(read = "start" /\ BodyLeaves /\ Len(Body) = 1 /\ text[Body[1]] = "blank" /\ ~startNamed)

ReachHeldRefusedSentByRows == ~(heldRefused /\ BodyLeaves /\ Len(Body) = 1 /\ text[Body[1]] \in {"n1", "n2"})

ReachItemNothingStopsTheRead == ~(read = "stop" /\ containerOnPage /\ Len(Body) = 1 /\ text[Body[1]] = "itemNothing")

ReachBlankAloneStopsTheRead == ~(read = "stop" /\ containerOnPage /\ Len(Body) = 1 /\ text[Body[1]] = "blankAlone")

ReachAHandWrittenControlLeftUnsent == ~(read = "start" /\ containerOnPage /\ Body = <<>> /\ handWritten = "typed")

ReachAHiddenInputInAFormBody == ~(Done /\ hiddenSent["form"] /\ ~hiddenSent["json"] /\ ~hiddenSent["query"])

ReachATextBoundUnderAPrefix == ~(prefixed /\ read = "n1" /\ BodyLeaves /\ Len(Body) = 1 /\ text[Body[1]] = "n1")

ReachStopOnAnInputOutsideThePrefix == ~(prefixed /\ read = "stop" /\ containerOnPage /\ ~ownRendered /\ slotLoaded /\ slotOutside /\ text["slot"] = "n1")

====
