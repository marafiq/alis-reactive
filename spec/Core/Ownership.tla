---- MODULE Ownership ----
EXTENDS Naturals

CONSTANT Mutant

Mutants == {"none", "ReleaseOnlyLoadedSlot", "LoadKeepsOldListeners", "BootContentUnderPage", "RegistrationPerLoadOnPage",
            "ReloadSkipsRegistration", "ReleaseLeavesRules", "FirstReleaseStopsHub", "ReleaseKeepsOffer", "UnsubscribeDestroyed",
            "ListenTwice", "ListenSurvivesRelease", "RemoveOnCollected", "RemoveOnDestroyed", "InlineByName",
            "SummaryOnlyWithoutInline"}

ASSUME Mutant \in Mutants

Slots == {"a", "b"}
Objects == {"x", "y"}
Owners == {"page"} \cup Slots
MaxLoads == 2

VARIABLES live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
          destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, gone, listenOffGone,
          req, sentGen, inline, inlineGen, summarized

vars == <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
          destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, gone, listenOffGone,
          req, sentGen, inline, inlineGen, summarized>>

listens == <<listenOn, listenGen, alive, listenThrew, gone, listenOffGone>>

checks == <<req, sentGen, inline, inlineGen, summarized>>

HasField(content) == content \in {"boot", "card"}

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

LoadA ==
    /\ aLoads < MaxLoads
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

UnloadA ==
    /\ live["a"]
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

LoadB ==
    /\ live["a"]
    /\ aContent = "card"
    /\ bLoads < MaxLoads
    /\ live' = [live EXCEPT !["b"] = TRUE]
    /\ bGen' = aGen
    /\ bLoads' = bLoads + 1
    /\ listenersOf' = [listenersOf EXCEPT !["b"] = IF Mutant = "LoadKeepsOldListeners" /\ live["b"] THEN @ + 1 ELSE 1]
    /\ managed' = TRUE
    /\ UNCHANGED <<aContent, aGen, aLoads, registeredOn, fieldRegistered, rules, offer, destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks

UnloadB ==
    /\ live["b"]
    /\ LET l == [live EXCEPT !["b"] = FALSE] IN
       /\ live' = l
       /\ managed' = HubAfter(l, aContent)
       /\ offer' = OfferAfter(HubAfter(l, aContent))
    /\ listenersOf' = [listenersOf EXCEPT !["b"] = IF Mutant = "LoadKeepsOldListeners" THEN @ ELSE 0]
    /\ UNCHANGED <<aContent, aGen, bGen, aLoads, bLoads, registeredOn, fieldRegistered, rules, destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks

HostDestroys ==
    /\ live["a"]
    /\ aContent = "card"
    /\ ~destroyed
    /\ destroyed' = TRUE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks

ServerHangsUp ==
    /\ managed
    /\ ~offer
    /\ offer' = TRUE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks

Retry ==
    /\ offer
    /\ managed
    /\ offer' = FALSE
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed,
                   destroyed, threw, releasedDestroyed>>
    /\ UNCHANGED listens
    /\ UNCHANGED checks

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

Collect(o) ==
    /\ o \in alive
    /\ alive' = alive \ {o}
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed, listenOn, listenGen, listenThrew, gone, listenOffGone>>
    /\ UNCHANGED checks

HostDestroysListened(o) ==
    /\ o \in alive
    /\ o \notin gone
    /\ gone' = gone \cup {o}
    /\ UNCHANGED <<live, aContent, aGen, bGen, aLoads, bLoads, listenersOf, registeredOn, fieldRegistered, rules, managed, offer,
                   destroyed, threw, releasedDestroyed, listenOn, listenGen, alive, listenThrew, listenOffGone>>
    /\ UNCHANGED checks

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

Next == LoadA \/ UnloadA \/ LoadB \/ UnloadB \/ HostDestroys \/ ServerHangsUp \/ Retry \/ SendsSlotInput \/ ShowsFailures
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

ReachNestedReleased == ~(bLoads > 0 /\ ~live["b"] /\ aLoads = MaxLoads)

ReachDestroyedThenReleased == ~(releasedDestroyed /\ live["a"])

ReachSharedHubOneReleased == ~(live["a"] /\ aContent = "card" /\ ~live["b"] /\ bLoads > 0 /\ managed /\ offer)

ReachEachObjectListened == ~(\A o \in Objects : listenOn[o] = 1)

ReachCollectedWhileListened == ~(\E o \in Objects : listenOn[o] = 1 /\ o \notin alive)

ReachDestroyedWhileListened == ~(\E o \in Objects : listenOn[o] = 1 /\ o \in alive /\ o \in gone)

ReachAnsweredAfterUnload == ~(req = "done" /\ ~live["a"] /\ summarized /\ ~inline)

ReachAnsweredAfterReload == ~(req = "done" /\ live["a"] /\ aGen # sentGen /\ summarized /\ ~inline)

ReachInlineAtTheSentInput == ~(req = "done" /\ inline /\ summarized)

====
