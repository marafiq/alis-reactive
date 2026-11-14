---- MODULE Ownership ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "ReleaseOnlyLoadedSlot", "LoadKeepsOldListeners", "BootContentUnderPage", "RegistrationPerLoadOnPage",
            "ReloadSkipsRegistration", "ReleaseLeavesRules", "FirstReleaseStopsHub", "ReleaseKeepsOffer", "UnsubscribeDestroyed",
            "ListenTwice", "ListenSurvivesRelease", "RemoveOnCollected", "RemoveOnDestroyed", "InlineByName",
            "SummaryOnlyWithoutInline", "ReleaseEndsAtFirstThrow", "ThrownListenerStillRuns", "ThrowLeavesTheContent",
            "LoadRestartsThePage"}

ASSUME Mutant \in Mutants

Slots == {"a", "b"}
Objects == {"x", "y"}
Owners == {"page"} \cup Slots
MaxLoads == 2

VARIABLES live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
          destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, gone, listenOffGone,
          req, sentGen, inline, inlineGen, summarized,
          throws, attached, heard, met, covered, lastStep, loadedGen, selfRun, joined, ran

vars == <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
          destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, gone, listenOffGone,
          req, sentGen, inline, inlineGen, summarized,
          throws, attached, heard, met, covered, lastStep, loadedGen, selfRun, joined, ran>>

listens == <<listenOn, listenGen, alive, listenThrew, gone, listenOffGone>>

checks == <<req, sentGen, inline, inlineGen, summarized>>

releases == <<throws, attached, heard, met, covered, lastStep, loadedGen, selfRun, joined, ran>>

stepOutcome == <<selfRun>>

HasField(content) == content \in {"boot", "card"}

Page == <<"page", 0>>

Without(s, left) == SelectSeq(s, LAMBDA e : e \notin left)

Joined(s, left, added) ==
    LET next == Without(s, left) \o (IF added = {} THEN <<>> ELSE <<CHOOSE e \in added : TRUE>>)
    IN IF Mutant = "LoadRestartsThePage" /\ added # {} THEN Without(next, {Page}) \o <<Page>> ELSE next

Holders(l, content) == {o \in {"a"} : l[o] /\ content = "card"} \cup {o \in {"b"} : l[o]}

Init ==
    /\ live = [o \in Slots |-> o = "a" /\ Mutant # "BootContentUnderPage"]
    /\ aContent = "boot"
    /\ aGen = 0
    /\ bGen = 0
    /\ aLoads = 0
    /\ bLoads = 0
    /\ listenersOf = [o \in Owners |-> CASE o = "page" -> IF Mutant = "BootContentUnderPage" THEN 2 ELSE 1
                                         [] o = "a" -> IF Mutant = "BootContentUnderPage" THEN 0 ELSE 1
                                         [] OTHER -> 0]
    /\ registeredOn = [o \in Owners |-> IF o = "b" THEN 0 ELSE 1]
    /\ fieldRegistered = TRUE
    /\ rules = IF Mutant = "BootContentUnderPage" THEN {<<"page", 0>>} ELSE {<<"page", 0>>, <<"a", 0>>}
    /\ managed = FALSE
    /\ offer = FALSE
    /\ destroyed = FALSE
    /\ threw = FALSE
    /\ releasedDestroyed = FALSE
    /\ listenOn = [o \in Objects |-> 0]
    /\ listenGen = [o \in Objects |-> 0]
    /\ alive = Objects
    /\ listenThrew = FALSE
    /\ gone = {}
    /\ listenOffGone = FALSE
    /\ req = "idle"
    /\ sentGen = 0
    /\ inline = FALSE
    /\ inlineGen = 0
    /\ summarized = FALSE
    /\ throws = {}
    /\ attached = IF Mutant = "BootContentUnderPage" THEN {} ELSE {<<"a", 0>>}
    /\ joined = IF Mutant = "BootContentUnderPage" THEN <<Page>> ELSE <<<<"a", 0>>, Page>>
    /\ ran = <<>>
    /\ heard = [o \in Slots |-> 0]
    /\ met = {}
    /\ covered = {}
    /\ lastStep = "none"
    /\ loadedGen = 0
    /\ selfRun = FALSE

KeepsB == Mutant = "ReleaseOnlyLoadedSlot"

HubAfter(l, content) ==
    IF Mutant = "FirstReleaseStopsHub" THEN FALSE ELSE Holders(l, content) # {}

OfferAfter(stillManaged) ==
    IF stillManaged THEN offer ELSE IF Mutant = "ReleaseKeepsOffer" THEN offer ELSE FALSE

RulesWithoutA == IF Mutant = "ReleaseLeavesRules" THEN rules ELSE {r \in rules : r[1] # "a"}

ReleaseA ==
    /\ threw' = (threw \/ (Mutant = "UnsubscribeDestroyed" /\ destroyed /\ live["a"]))
    /\ releasedDestroyed' = (releasedDestroyed \/ (destroyed /\ live["a"]))
    /\ listenThrew' = (listenThrew \/ (Mutant = "RemoveOnCollected" /\ \E o \in Objects : listenOn[o] > 0 /\ o \notin alive))
    /\ listenOffGone' = (listenOffGone \/ (Mutant = "RemoveOnDestroyed" /\ \E o \in Objects : listenOn[o] > 0 /\ o \in alive /\ o \in gone))
    /\ listenOn' = IF Mutant = "ListenSurvivesRelease" THEN listenOn ELSE [o \in Objects |-> 0]
    /\ listenGen' = IF Mutant = "ListenSurvivesRelease" THEN listenGen ELSE [o \in Objects |-> 0]
    /\ UNCHANGED <<alive, gone>>

Range(s) == {s[i] : i \in 1..Len(s)}

OwnerGen(o) == IF o = "a" THEN aGen ELSE bLoads

Current(owners) == {<<o, OwnerGen(o)>> : o \in owners}

NoneHeard == [o \in Slots |-> 0]

ReleaseOrder == IF live["b"] THEN <<"b", "a">> ELSE <<"a">>

FirstThrow(order) ==
    IF \E i \in 1..Len(order) : order[i] \in throws
    THEN CHOOSE i \in 1..Len(order) : order[i] \in throws /\ \A j \in 1..(i - 1) : order[j] \notin throws
    ELSE 0

EndsAt(order) == IF Mutant = "ReleaseEndsAtFirstThrow" THEN FirstThrow(order) ELSE 0

LeavesTheContent(order) == Mutant = "ThrowLeavesTheContent" /\ Range(order) \cap throws # {}

Issuers(x) == {"page"} \cup (IF live[x] THEN {x} ELSE {})

StepOutcome(by, x, ended) == selfRun' = (by = x /\ x \in ended)

ReleasedWhole(order, added, by, x) ==
    /\ met' = Range(order) \cap throws
    /\ throws' = throws \ Range(order)
    /\ attached' = (attached \ Current(Range(order) \ throws)) \cup added
    /\ joined' = Joined(joined, Current(Range(order) \ throws), added)
    /\ heard' = NoneHeard
    /\ ran' = <<>>
    /\ covered' = Current(Range(order))
    /\ StepOutcome(by, x, Range(order))

EndedAtTheThrow(order, step, by, x) ==
    LET first == EndsAt(order)
        before == {order[i] : i \in 1..(first - 1)} IN
    /\ IF "b" \in before
         THEN LET l == [live EXCEPT !["b"] = FALSE] IN
              /\ live' = l
              /\ managed' = HubAfter(l, aContent)
              /\ offer' = OfferAfter(HubAfter(l, aContent))
              /\ listenersOf' = [listenersOf EXCEPT !["b"] = 0]
         ELSE UNCHANGED <<live, managed, offer, listenersOf>>
    /\ UNCHANGED <<aContent, aGen, bGen, aLoads, bLoads, registeredOn, fieldRegistered, rules, destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ met' = {order[first]}
    /\ throws' = throws \ before
    /\ attached' = attached \ Current(before)
    /\ joined' = Joined(joined, Current(before), {})
    /\ heard' = NoneHeard
    /\ ran' = <<>>
    /\ covered' = Current(Range(order))
    /\ lastStep' = step
    /\ loadedGen' = CASE step = "load" -> aGen + 1 [] step = "loadB" -> bLoads + 1 [] OTHER -> loadedGen
    /\ StepOutcome(by, x, before \cup {order[first]})

ReleasedLeavingTheContent(order, step, by) ==
    LET l == [o \in Slots |-> FALSE] IN
    /\ live' = l
    /\ managed' = HubAfter(l, aContent)
    /\ offer' = OfferAfter(HubAfter(l, aContent))
    /\ listenersOf' = [listenersOf EXCEPT !["a"] = 0, !["b"] = 0]
    /\ registeredOn' = [registeredOn EXCEPT !["a"] = 0]
    /\ fieldRegistered' = FALSE
    /\ rules' = RulesWithoutA
    /\ destroyed' = FALSE
    /\ ReleaseA
    /\ UNCHANGED <<aContent, aGen, bGen, aLoads, bLoads>>
    /\ UNCHANGED checks
    /\ ReleasedWhole(order, {}, by, "a")
    /\ lastStep' = step
    /\ loadedGen' = IF step = "load" THEN aGen + 1 ELSE loadedGen

LoadReplaces(by) ==
    /\ LET l == [o \in Slots |-> IF o = "a" THEN TRUE ELSE live["b"] /\ KeepsB]
           released == [o \in Slots |-> IF o = "a" THEN FALSE ELSE live["b"] /\ KeepsB] IN
       /\ live' = l
       /\ managed' = TRUE
       /\ offer' = IF Holders(released, "empty") # {} /\ Mutant # "FirstReleaseStopsHub" THEN offer
                   ELSE IF Mutant = "ReleaseKeepsOffer" THEN offer ELSE FALSE
    /\ aContent' = "card"
    /\ aGen' = aGen + 1
    /\ aLoads' = aLoads + 1
    /\ listenersOf' = [listenersOf EXCEPT !["a"] = IF Mutant = "LoadKeepsOldListeners" THEN @ + 1 ELSE 1,
                                          !["b"] = IF KeepsB THEN @ ELSE 0]
    /\ registeredOn' = IF Mutant = "RegistrationPerLoadOnPage" THEN [registeredOn EXCEPT !["page"] = @ + 1, !["a"] = 0]
                  ELSE [registeredOn EXCEPT !["a"] = 1]
    /\ fieldRegistered' = (Mutant # "ReloadSkipsRegistration")
    /\ rules' = RulesWithoutA \cup {<<"a", aGen + 1>>}
    /\ destroyed' = FALSE
    /\ ReleaseA
    /\ UNCHANGED <<bGen, bLoads>>
    /\ UNCHANGED checks
    /\ ReleasedWhole(ReleaseOrder, {<<"a", aGen + 1>>}, by, "a")
    /\ lastStep' = "load"
    /\ loadedGen' = aGen + 1

LoadA ==
    /\ aLoads < MaxLoads
    /\ \E by \in Issuers("a") :
         CASE EndsAt(ReleaseOrder) > 0 -> EndedAtTheThrow(ReleaseOrder, "load", by, "a")
           [] LeavesTheContent(ReleaseOrder) -> ReleasedLeavingTheContent(ReleaseOrder, "load", by)
           [] OTHER -> LoadReplaces(by)

UnloadEmpties(by) ==
    /\ LET l == [o \in Slots |-> IF o = "a" THEN FALSE ELSE live["b"] /\ KeepsB] IN
       /\ live' = l
       /\ managed' = HubAfter(l, "empty")
       /\ offer' = OfferAfter(HubAfter(l, "empty"))
    /\ aContent' = "empty"
    /\ listenersOf' = [listenersOf EXCEPT !["a"] = IF Mutant = "LoadKeepsOldListeners" THEN @ ELSE 0,
                                          !["b"] = IF KeepsB THEN @ ELSE 0]
    /\ registeredOn' = IF Mutant = "RegistrationPerLoadOnPage" THEN registeredOn ELSE [registeredOn EXCEPT !["a"] = 0]
    /\ fieldRegistered' = FALSE
    /\ rules' = RulesWithoutA
    /\ destroyed' = FALSE
    /\ ReleaseA
    /\ UNCHANGED <<aGen, bGen, aLoads, bLoads>>
    /\ UNCHANGED checks
    /\ ReleasedWhole(ReleaseOrder, {}, by, "a")
    /\ lastStep' = "unload"
    /\ UNCHANGED loadedGen

UnloadA ==
    /\ live["a"]
    /\ \E by \in Issuers("a") :
         CASE EndsAt(ReleaseOrder) > 0 -> EndedAtTheThrow(ReleaseOrder, "unload", by, "a")
           [] LeavesTheContent(ReleaseOrder) -> ReleasedLeavingTheContent(ReleaseOrder, "unload", by)
           [] OTHER -> UnloadEmpties(by)

LoadsB(by) ==
    /\ live' = [live EXCEPT !["b"] = TRUE]
    /\ bGen' = aGen
    /\ bLoads' = bLoads + 1
    /\ listenersOf' = [listenersOf EXCEPT !["b"] = IF Mutant = "LoadKeepsOldListeners" /\ live["b"] THEN @ + 1 ELSE 1]
    /\ managed' = TRUE
    /\ UNCHANGED <<aContent, aGen, aLoads, registeredOn, fieldRegistered, rules, offer, destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ IF live["b"]
         THEN ReleasedWhole(<<"b">>, {<<"b", bLoads + 1>>}, by, "b")
         ELSE /\ met' = {}
              /\ covered' = {}
              /\ attached' = attached \cup {<<"b", bLoads + 1>>}
              /\ joined' = Joined(joined, {}, {<<"b", bLoads + 1>>})
              /\ heard' = NoneHeard
              /\ ran' = <<>>
              /\ UNCHANGED throws
              /\ StepOutcome(by, "b", {})
    /\ lastStep' = "loadB"
    /\ loadedGen' = bLoads + 1

LoadB ==
    /\ live["a"]
    /\ aContent = "card"
    /\ bLoads < MaxLoads
    /\ \E by \in Issuers("b") :
         IF live["b"] /\ EndsAt(<<"b">>) > 0 THEN EndedAtTheThrow(<<"b">>, "loadB", by, "b") ELSE LoadsB(by)

UnloadsB(by) ==
    /\ LET l == [live EXCEPT !["b"] = FALSE] IN
       /\ live' = l
       /\ managed' = HubAfter(l, aContent)
       /\ offer' = OfferAfter(HubAfter(l, aContent))
    /\ listenersOf' = [listenersOf EXCEPT !["b"] = IF Mutant = "LoadKeepsOldListeners" THEN @ ELSE 0]
    /\ UNCHANGED <<aContent, aGen, bGen, aLoads, bLoads, registeredOn, fieldRegistered, rules, destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ ReleasedWhole(<<"b">>, {}, by, "b")
    /\ lastStep' = "unloadB"
    /\ UNCHANGED loadedGen

UnloadB ==
    /\ live["b"]
    /\ \E by \in Issuers("b") :
         IF EndsAt(<<"b">>) > 0 THEN EndedAtTheThrow(<<"b">>, "unloadB", by, "b") ELSE UnloadsB(by)

UnsubscribeThrows(o) ==
    /\ live[o]
    /\ o \notin throws
    /\ throws' = throws \cup {o}
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ UNCHANGED <<attached, heard, met, covered, lastStep, loadedGen, joined, ran>>
    /\ UNCHANGED stepOutcome

Running == IF Mutant = "ThrownListenerStillRuns" THEN attached ELSE {e \in attached : live[e[1]] /\ OwnerGen(e[1]) = e[2]}

Raise ==
    /\ heard' = [o \in Slots |-> Cardinality({e \in Running : e[1] = o})]
    /\ ran' = SelectSeq(joined, LAMBDA e : e = Page \/ e \in Running)
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ UNCHANGED <<throws, attached, met, covered, lastStep, loadedGen, joined>>
    /\ UNCHANGED stepOutcome

HostDestroys ==
    /\ live["a"]
    /\ aContent = "card"
    /\ ~destroyed
    /\ destroyed' = TRUE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ UNCHANGED releases

ServerHangsUp ==
    /\ managed
    /\ ~offer
    /\ offer' = TRUE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ UNCHANGED releases

Retry ==
    /\ offer
    /\ managed
    /\ offer' = FALSE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks
    /\ UNCHANGED releases

ListenA(o) ==
    /\ live["a"]
    /\ aContent # "empty"
    /\ o \in alive
    /\ o \notin gone
    /\ IF Mutant = "ListenTwice" THEN listenOn[o] < 2 ELSE listenOn[o] = 0
    /\ listenOn' = [listenOn EXCEPT ![o] = @ + 1]
    /\ listenGen' = [listenGen EXCEPT ![o] = aGen]
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed, alive, listenThrew, gone, listenOffGone>>
    /\ UNCHANGED checks
    /\ UNCHANGED releases

Collect(o) ==
    /\ o \in alive
    /\ alive' = alive \ {o}
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed, listenOn, listenGen, listenThrew, gone, listenOffGone>>
    /\ UNCHANGED checks
    /\ UNCHANGED releases

HostDestroysListened(o) ==
    /\ o \in alive
    /\ o \notin gone
    /\ gone' = gone \cup {o}
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, listenOffGone>>
    /\ UNCHANGED checks
    /\ UNCHANGED releases

pageState == <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
               destroyed, threw, releasedDestroyed>>

SendsSlotInput ==
    /\ req # "attempt"
    /\ live["a"]
    /\ HasField(aContent)
    /\ req' = "attempt"
    /\ sentGen' = aGen
    /\ inline' = FALSE
    /\ inlineGen' = 0
    /\ summarized' = FALSE
    /\ UNCHANGED pageState
    /\ UNCHANGED listens
    /\ UNCHANGED releases

ShowsFailures ==
    /\ req = "attempt"
    /\ req' = "done"
    /\ LET here == live["a"] /\ HasField(aContent)
           at == IF Mutant = "InlineByName" THEN here ELSE here /\ aGen = sentGen IN
       /\ inline' = at
       /\ inlineGen' = IF at THEN aGen ELSE 0
       /\ summarized' = IF Mutant = "SummaryOnlyWithoutInline" THEN ~at ELSE TRUE
    /\ UNCHANGED sentGen
    /\ UNCHANGED pageState
    /\ UNCHANGED listens
    /\ UNCHANGED releases

Next == LoadA \/ UnloadA \/ LoadB \/ UnloadB \/ HostDestroys \/ ServerHangsUp \/ Retry \/ SendsSlotInput \/ ShowsFailures
        \/ Raise \/ \E s \in Slots : UnsubscribeThrows(s)
        \/ \E o \in Objects : ListenA(o) \/ Collect(o) \/ HostDestroysListened(o)

Spec == Init /\ [][Next]_vars

NoSlotOutlivesItsContainer == live["b"] => (live["a"] /\ aContent = "card" /\ bGen = aGen)

ListenersDoNotAccumulate == \A o \in Slots : listenersOf[o] = IF live[o] THEN 1 ELSE 0

BootContentBelongsToItsSlot == aContent # "boot" => listenersOf["page"] = 1

RegistrationsDoNotAccumulate == registeredOn["page"] = 1 /\ \A o \in Slots : registeredOn[o] <= 1

EveryLoadedFieldRegistered == (live["a"] /\ HasField(aContent)) => fieldRegistered

RulesLeaveWithTheirOwner ==
    rules = {<<"page", 0>>} \cup (IF live["a"] /\ HasField(aContent) THEN {<<"a", aGen>>} ELSE {})

HubKeptWhileHeld == managed <=> Holders(live, aContent) # {}

OfferOnlyWhileHeld == offer => Holders(live, aContent) # {}

NoUnsubscribeOnDestroyed == ~threw

ListenOncePerObject == \A o \in Objects : listenOn[o] <= 1

ListenLeavesWithItsOwner == \A o \in Objects : listenOn[o] > 0 => (live["a"] /\ aContent # "empty" /\ listenGen[o] = aGen)

NoRemoveOnCollected == ~listenThrew

NoRemoveOnDestroyedListened == ~listenOffGone

FailureReachesSummary == req = "done" => summarized

InlineOnlyAtTheSentInput == inline => inlineGen = sentGen

EveryOwnerReleasedPastAThrow == met # {} => \A c \in covered : ~(live[c[1]] /\ OwnerGen(c[1]) = c[2])

ThrownListenerRunsNothing == \A o \in Slots : heard[o] <= IF live[o] THEN 1 ELSE 0

Earlier(s, x, y) == \E i, j \in 1..Len(s) : i < j /\ s[i] = x /\ s[j] = y

HandlersHearInTheOrderTheySubscribed ==
    \A e \in Range(ran) : /\ (e[1] \in Slots /\ e[2] > 0) => Earlier(ran, Page, e)
                          /\ (e[1] = "b" /\ <<"a", aGen>> \in Range(ran)) => Earlier(ran, <<"a", aGen>>, e)

StepReplacesTheContent ==
    /\ lastStep = "load" => (live["a"] /\ aContent = "card" /\ aGen = loadedGen)
    /\ lastStep = "unload" => (~live["a"] /\ aContent = "empty")
    /\ lastStep = "loadB" => (live["b"] /\ bLoads = loadedGen)
    /\ lastStep = "unloadB" => ~live["b"]

ReachNestedReleased == ~(bLoads > 0 /\ ~live["b"] /\ aLoads = MaxLoads)

ReachDestroyedThenReleased == ~(releasedDestroyed /\ live["a"])

ReachSharedHubOneReleased == ~(live["a"] /\ aContent = "card" /\ ~live["b"] /\ bLoads > 0 /\ managed /\ offer)

ReachEachObjectListened == ~(\A o \in Objects : listenOn[o] = 1)

ReachCollectedWhileListened == ~(\E o \in Objects : listenOn[o] = 1 /\ o \notin alive)

ReachDestroyedWhileListened == ~(\E o \in Objects : listenOn[o] = 1 /\ o \in alive /\ o \in gone)

ReachAnsweredAfterUnload == ~(req = "done" /\ ~live["a"] /\ summarized /\ ~inline)

ReachAnsweredAfterReload == ~(req = "done" /\ live["a"] /\ aGen # sentGen /\ summarized /\ ~inline)

ReachInlineAtTheSentInput == ~(req = "done" /\ inline /\ summarized)

ReachNestedThrowReleased == ~(lastStep = "load" /\ met = Slots /\ live["a"] /\ ~live["b"] /\ aGen = loadedGen)

ReachOwnUnloadThrew == ~(lastStep = "unload" /\ selfRun /\ met # {})

ReachThrownListenerOutlivesItsLoad == ~(live["a"] /\ heard["a"] = 1 /\ \E e \in attached : e[1] = "a" /\ e[2] # aGen)

====
