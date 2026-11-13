---- MODULE ContainerRead ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "DuplicatesSent", "TwoInputsReadTheLast", "BlankReadsNothing", "UnsentReadsNothing",
            "CachedInputs", "PageWideRead"}

ASSUME Mutant \in Mutants

Inputs == {"own", "slot", "other"}

Texts == {"n1", "n2", "blank"}

Hosts == {"form", "json"}

DomOrder == <<"own", "slot">>

ContainerOf(i) == IF i = "other" THEN "elsewhere" ELSE "container"

VARIABLES containerOnPage, ownRendered, slotLoaded, slotLeft, text, read, readFrom, cached, cache

page == <<containerOnPage, ownRendered, slotLoaded, slotLeft, text>>
vars == <<page, read, readFrom, cached, cache>>

Range(s) == {s[i] : i \in DOMAIN s}

OnPage(i) == CASE i = "own" -> ownRendered
               [] i = "slot" -> slotLoaded
               [] i = "other" -> TRUE

Value(i) == IF text[i] = "blank" THEN "start" ELSE text[i]

Body == SelectSeq(DomOrder, LAMBDA i : OnPage(i))

SendStops == ~containerOnPage \/ Len(Body) >= 2

BodyLeaves == IF Mutant = "DuplicatesSent" THEN containerOnPage ELSE ~SendStops

Bind(h) == CASE Body = <<>> -> "start"
             [] h = "form" -> Value(Body[1])
             [] h = "json" -> Value(Body[Len(Body)])

Table == {i \in Inputs : OnPage(i)}

Hits == IF Mutant = "PageWideRead" THEN Table ELSE {i \in Table : ContainerOf(i) = "container"}

Found == IF Mutant = "CachedInputs" /\ cached THEN cache ELSE Hits

ValueRead(i) == IF text[i] = "blank" /\ Mutant = "BlankReadsNothing" THEN "nothing" ELSE Value(i)

TwoFound == Cardinality(Found) >= 2

TakesTheLast == TwoFound /\ Mutant = "TwoInputsReadTheLast"

ReadValue ==
    CASE ~containerOnPage -> "stop"
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

Init ==
    /\ containerOnPage \in BOOLEAN
    /\ ownRendered \in IF containerOnPage THEN BOOLEAN ELSE {FALSE}
    /\ slotLoaded = FALSE
    /\ slotLeft = FALSE
    /\ text \in {t \in [Inputs -> Texts] : t["slot"] = "blank"}
    /\ read = "unread"
    /\ readFrom = {}
    /\ cached = FALSE
    /\ cache = {}

Read ==
    /\ read = "unread"
    /\ read' = ReadValue
    /\ readFrom' = ReadNames
    /\ cached' = (cached \/ Mutant = "CachedInputs")
    /\ cache' = IF Mutant = "CachedInputs" /\ ~cached THEN Hits ELSE cache
    /\ UNCHANGED page

Type(i, t) ==
    /\ OnPage(i)
    /\ text[i] # t
    /\ text' = [text EXCEPT ![i] = t]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLoaded, slotLeft, cached, cache>>

LoadSlot ==
    /\ containerOnPage
    /\ ~slotLoaded
    /\ slotLoaded' = TRUE
    /\ \E t \in Texts : text' = [text EXCEPT !["slot"] = t]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, slotLeft, cached, cache>>

UnloadSlot ==
    /\ slotLoaded
    /\ slotLoaded' = FALSE
    /\ slotLeft' = TRUE
    /\ text' = [text EXCEPT !["slot"] = "blank"]
    /\ Unread
    /\ UNCHANGED <<containerOnPage, ownRendered, cached, cache>>

Next == Read \/ LoadSlot \/ UnloadSlot \/ \E i \in Inputs, t \in Texts : Type(i, t)

Spec == Init /\ [][Next]_vars

Done == read # "unread"

OneValueOnEveryHost == BodyLeaves => \A h \in Hosts : Bind(h) = Bind("form")

ReadStopsWhenItsSendStops == Done => ((read = "stop") <=> SendStops)

ReadIsWhatTheEndpointBinds == (Done /\ ~SendStops /\ Len(Body) = 1) => \A h \in Hosts : read = Bind(h)

AnUnsentMemberReadsWhatTheEndpointBinds == (Done /\ containerOnPage /\ Body = <<>>) => \A h \in Hosts : read = Bind(h)

ReadSeesTheSlotThatJoinedOrLeftBeforeIt == (Done /\ containerOnPage) => (("slot" \in readFrom) <=> slotLoaded)

ReadFindsWhatItsContainerSends == (Done /\ containerOnPage) => readFrom = Range(Body)

ReachStopOnceASlotsInputJoins == ~(read = "stop" /\ containerOnPage /\ ownRendered /\ slotLoaded)

ReachStartOnceTheSlotLeft == ~(read = "start" /\ slotLeft /\ ~slotLoaded /\ ~ownRendered /\ containerOnPage)

ReachBlankReadsItsStart == ~(read = "start" /\ Len(Body) = 1 /\ text[Body[1]] = "blank")

ReachMissingContainerStops == ~(read = "stop" /\ ~containerOnPage)

====
