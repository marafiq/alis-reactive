---- MODULE ContainerRead ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "DuplicatesSent", "TwoInputsReadTheLast", "BlankReadsNothing", "UnsentReadsNothing",
            "CachedInputs", "PageWideRead", "UnlistedStopsTheSend", "UnlistedReadsItsText", "UnnamedStartSentAsHeld",
            "JsonSentAsHeld", "ItemNothingStopsTheSendAlone", "BlankAloneStopsTheSendAlone", "HandWrittenControlSent",
            "HiddenLeftUnsent", "HiddenSentOutsideAFormBody"}

ASSUME Mutant \in Mutants

Inputs == {"own", "slot", "other"}

Texts == {"n1", "n2", "blank", "unlisted", "itemNothing", "blankAlone"}

Hosts == {"form", "json"}

Wires == {"form", "json", "query"}

Servers == {"net10", "net48"}

DomOrder == <<"own", "slot">>

ContainerOf(i) == IF i = "other" THEN "elsewhere" ELSE "container"

VARIABLES containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, hidden, read, readFrom, hiddenSent, cached, cache

page == <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, hidden>>
vars == <<page, read, readFrom, hiddenSent, cached, cache>>

Range(s) == {s[i] : i \in DOMAIN s}

OnPage(i) == CASE i = "own" -> ownRendered
               [] i = "slot" -> slotLoaded
               [] i = "other" -> TRUE

Value(i) == IF text[i] \in {"blank", "unlisted"} THEN "start" ELSE text[i]

Body == SelectSeq(DomOrder, LAMBDA i : OnPage(i))

Unlisted(i) == text[i] = "unlisted" \/ (text[i] = "blank" /\ ~startNamed)

SendStops == ~containerOnPage \/ Len(Body) >= 2 \/ (\E i \in Range(Body) : text[i] \in {"itemNothing", "blankAlone"})
             \/ (Mutant = "UnlistedStopsTheSend" /\ \E i \in Range(Body) : Unlisted(i))

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
                [] h = "form" -> Bound(Body[1], h, s)
                [] h = "json" -> Bound(Body[Len(Body)], h, s)

HiddenReaches(w) == CASE Mutant = "HiddenLeftUnsent" -> FALSE
                    [] Mutant = "HiddenSentOutsideAFormBody" -> hidden
                    [] OTHER -> hidden /\ w = "form"

Table == {i \in Inputs : OnPage(i)}

Hits == IF Mutant = "PageWideRead" THEN Table ELSE {i \in Table : ContainerOf(i) = "container"}

Found == IF Mutant = "CachedInputs" /\ cached THEN cache ELSE Hits

ValueRead(i) == CASE text[i] = "blank" /\ Mutant = "BlankReadsNothing" -> "nothing"
                  [] text[i] = "unlisted" /\ Mutant = "UnlistedReadsItsText" -> "unlisted"
                  [] OTHER -> Value(i)

TwoFound == Cardinality(Found) >= 2

ItemNothingFound == Mutant # "ItemNothingStopsTheSendAlone" /\ \E i \in Found : text[i] = "itemNothing"

BlankAloneFound == Mutant # "BlankAloneStopsTheSendAlone" /\ \E i \in Found : text[i] = "blankAlone"

TakesTheLast == TwoFound /\ Mutant = "TwoInputsReadTheLast"

ReadValue ==
    CASE ~containerOnPage -> "stop"
      [] ItemNothingFound -> "stop"
      [] BlankAloneFound -> "stop"
      [] Found = {} -> IF Mutant = "UnsentReadsNothing" THEN "nothing" ELSE "start"
      [] TakesTheLast -> ValueRead("slot")
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
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, startNamed, heldRefused, handWritten, hidden, cached, cache>>

LoadSlot ==
    /\ containerOnPage
    /\ ~slotLoaded
    /\ slotLoaded' = TRUE
    /\ \E t \in Texts : text' = [text EXCEPT !["slot"] = t]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLeft, startNamed, heldRefused, handWritten, hidden, cached, cache>>

UnloadSlot ==
    /\ slotLoaded
    /\ slotLoaded' = FALSE
    /\ slotLeft' = TRUE
    /\ text' = [text EXCEPT !["slot"] = "blank"]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, startNamed, heldRefused, handWritten, hidden, cached, cache>>

HoldHidden ==
    /\ containerOnPage
    /\ ~hidden
    /\ hidden' = TRUE
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, cached, cache>>

DropHidden ==
    /\ hidden
    /\ hidden' = FALSE
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, text, startNamed, heldRefused, handWritten, cached, cache>>

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

====
