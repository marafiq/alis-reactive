---- MODULE Partials ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANTS Mutant, Candidate

Mutants == {"none", "NoLatePrefixRefusal", "IdWithoutPrefix", "NameWithoutPrefix", "NoCompositionRefusal",
            "NoDuplicateStop"}

Candidates == {"none", "OwnPlanStops"}

ASSUME Mutant \in Mutants /\ Candidate \in Candidates

Members == {"NextOfKin", "Guardian"}

PrefixDoors == {"EditorFor", "PartialFor", "PartialTagFor", "ViewDataCopy", "SlotWithPrefix"}

BareDoors == {"PartialWithModel", "SlotBare"}

SlotDoors == {"SlotWithPrefix", "SlotBare"}

NestedDoors == {"EditorFor", "PartialFor", "PartialWithModel"}

DoorsOn(h) == IF h = "net10" THEN PrefixDoors \cup BareDoors ELSE (PrefixDoors \cup BareDoors) \ {"PartialTagFor"}

Page == <<"page", "Family">>

KinPlan(k) == <<"kin", k>>

AddressPlan(k) == <<"address", k>>

Plans == {Page} \cup {KinPlan(k) : k \in Members} \cup {AddressPlan(k) : k \in Members}

VARIABLES host, door, nested, ownAt, phase, loaded, live

vars == <<host, door, nested, ownAt, phase, loaded, live>>

Range(s) == {s[i] : i \in DOMAIN s}

Max(S) == CHOOSE x \in S : \A y \in S : y <= x

Moves(k) == host = "net48" /\ door[k] = "ViewDataCopy"

PrefixBefore(k) == IF k = "Guardian" /\ Moves("NextOfKin") THEN <<"NextOfKin">> ELSE <<>>

PrefixAtEnd == IF Moves("Guardian") THEN <<"Guardian">> ELSE PrefixBefore("Guardian")

KinPrefix(k) ==
    CASE door[k] \in {"EditorFor", "PartialFor", "PartialTagFor"} -> PrefixBefore(k) \o <<k>>
      [] door[k] \in {"ViewDataCopy", "SlotWithPrefix"} -> <<k>>
      [] door[k] = "PartialWithModel" -> IF host = "net10" THEN PrefixBefore(k) ELSE <<>>
      [] door[k] = "SlotBare" -> <<>>

AddressPrefix(k) ==
    IF nested = "PartialWithModel"
      THEN IF host = "net10" THEN KinPrefix(k) ELSE <<>>
      ELSE KinPrefix(k) \o <<"Address">>

Model(p) == CASE p[1] = "page" -> "Family" [] p[1] = "kin" -> "Kin" [] p[1] = "address" -> "Address"

Start(p) == CASE p[1] = "page" -> <<>> [] p[1] = "kin" -> KinPrefix(p[2]) [] p[1] = "address" -> AddressPrefix(p[2])

Base(p) == CASE p[1] = "page" -> <<>> [] p[1] = "kin" -> <<p[2]>> [] p[1] = "address" -> <<p[2], "Address">>

Field(p) == IF p[1] = "address" THEN <<"Street">> ELSE <<"Name">>

Member(p) == Base(p) \o Field(p)

RenderedUnder(p) == IF p = Page /\ ownAt = "last" THEN PrefixAtEnd ELSE Start(p)

Attached(p) ==
    CASE p[1] = "page" -> TRUE
      [] p[1] = "kin" -> door[p[2]] \in PrefixDoors
      [] p[1] = "address" -> door[p[2]] \in PrefixDoors /\ nested # "PartialWithModel"

HoldsInput(p) == p # Page \/ ownAt # "none"

IdOf(model, prefix, path) == IF Mutant = "IdWithoutPrefix" THEN <<model, path>> ELSE <<model, prefix \o path>>

InputId(p) == IdOf(Model(p), RenderedUnder(p), Field(p))

InputName(p) == IF Mutant = "NameWithoutPrefix" THEN Field(p) ELSE RenderedUnder(p) \o Field(p)

ReadOf(p, model, path) == [member |-> Base(p) \o path, id |-> IdOf(model, Start(p), path)]

Reads(p) ==
    CASE p[1] = "page" ->
            {ReadOf(p, "Family", <<"Name">>)}
            \cup {ReadOf(p, "Kin", <<k, "Name">>) : k \in Members}
            \cup {ReadOf(p, "Address", <<k, "Address", "Street">>) : k \in Members}
      [] p[1] = "kin" -> {ReadOf(p, "Kin", <<"Name">>), ReadOf(p, "Address", <<"Address", "Street">>)}
      [] p[1] = "address" -> {ReadOf(p, "Address", <<"Street">>)}

Collides(p, started) == HoldsInput(p) /\ \E q \in Range(started) : HoldsInput(q) /\ InputId(q) = InputId(p)

RECURSIVE StartIn(_, _)
StartIn(order, started) ==
    IF order = <<>>
      THEN started
      ELSE StartIn(Tail(order),
                   IF Mutant # "NoCompositionRefusal" /\ Collides(Head(order), started)
                     THEN started
                     ELSE Append(started, Head(order)))

ViewPlans(k) == <<AddressPlan(k), KinPlan(k)>>

RenderedMembers == {k \in Members : door[k] \notin SlotDoors}

RenderOrder ==
    (IF "NextOfKin" \in RenderedMembers THEN ViewPlans("NextOfKin") ELSE <<>>)
    \o (IF "Guardian" \in RenderedMembers THEN ViewPlans("Guardian") ELSE <<>>)
    \o <<Page>>

PageRefused == Mutant # "NoLatePrefixRefusal" /\ PrefixAtEnd # <<>>

Init ==
    /\ host \in {"net10", "net48"}
    /\ door \in [Members -> DoorsOn(host)]
    /\ nested \in NestedDoors
    /\ ownAt \in {"first", "last", "none"}
    /\ phase = "rendering"
    /\ loaded = {}
    /\ live = <<>>

Render ==
    /\ phase = "rendering"
    /\ phase' = IF PageRefused THEN "refused" ELSE "rendered"
    /\ live' = IF PageRefused THEN <<>> ELSE StartIn(RenderOrder, <<>>)
    /\ UNCHANGED <<host, door, nested, ownAt, loaded>>

LoadSlot(k) ==
    /\ phase = "rendered"
    /\ door[k] \in SlotDoors
    /\ k \notin loaded
    /\ loaded' = loaded \cup {k}
    /\ live' = StartIn(ViewPlans(k), live)
    /\ UNCHANGED <<host, door, nested, ownAt, phase>>

Next == Render \/ \E k \in Members : LoadSlot(k)

Spec == Init /\ [][Next]_vars

LiveSet == Range(live)

Carried == {p \in LiveSet : HoldsInput(p)}

SharesName(p) == \E q \in Carried : q # p /\ InputName(q) = InputName(p)

OwnPlanOfAnotherModel(p) == Start(p) = <<>> /\ Model(p) # "Family"

SendStops ==
    \/ Mutant # "NoDuplicateStop" /\ \E p \in Carried : SharesName(p)
    \/ Candidate = "OwnPlanStops" /\ \E p \in Carried : OwnPlanOfAnotherModel(p)

Sent == phase = "rendered" /\ ~SendStops

FamilyPaths == {<<"Name">>} \cup {<<k, "Name">> : k \in Members} \cup {<<k, "Address", "Street">> : k \in Members}

LandsIn(p) == IF InputName(p) \in FamilyPaths THEN {InputName(p)} ELSE {}

DomOrder ==
    (IF ownAt = "first" THEN <<Page>> ELSE <<>>)
    \o <<KinPlan("NextOfKin"), AddressPlan("NextOfKin"), KinPlan("Guardian"), AddressPlan("Guardian")>>
    \o (IF ownAt = "last" THEN <<Page>> ELSE <<>>)

DomIndex(p) == CHOOSE i \in DOMAIN DomOrder : DomOrder[i] = p

BindsItsValue(p) == \A q \in Carried : (q # p /\ InputName(q) = InputName(p)) => DomIndex(p) < DomIndex(q)

Found(r) ==
    LET hits == {i \in DOMAIN live : HoldsInput(live[i]) /\ InputId(live[i]) = r.id}
    IN IF hits = {} THEN {} ELSE {live[Max(hits)]}

LiveReads == UNION {Reads(p) : p \in LiveSet}

OneIdPerMember ==
    \A p, q \in Plans \ {Page} : (p # q /\ Attached(p) /\ Attached(q)) => InputId(p) # InputId(q)

PageReadFindsEachAttachedInput ==
    Page \in LiveSet =>
        \A r \in Reads(Page) : \A p \in Carried : (Attached(p) /\ Member(p) = r.member) => Found(r) = {p}

AttachedInputBindsItsMember == Sent => \A p \in Carried : Attached(p) => LandsIn(p) = {Member(p)}

ReadNeverFindsAnotherMember == \A r \in LiveReads : \A p \in Found(r) : Member(p) = r.member

EverySentValueIsTheOneBound == Sent => \A p \in Carried : BindsItsValue(p)

ReachNet48CopyRefused == ~(phase = "refused" /\ host = "net48")

ReachSlotUnderItsPrefixBound ==
    ~(Sent /\ \E p \in Carried : p[1] = "kin" /\ door[p[2]] = "SlotWithPrefix" /\ LandsIn(p) = {Member(p)})

ReachNestedChainBoundOnNet48 ==
    ~(Sent /\ host = "net48" /\ \E p \in Carried : p[1] = "address" /\ Attached(p) /\ LandsIn(p) = {Member(p)})

ReachOwnPlansValueLandsInAnotherMember ==
    ~(Sent /\ \E p \in Carried : OwnPlanOfAnotherModel(p) /\ LandsIn(p) # {} /\ LandsIn(p) # {Member(p)})

ReachInheritedPrefixMisnames ==
    ~(Sent /\ \E p \in Carried : ~Attached(p) /\ ~OwnPlanOfAnotherModel(p) /\ LandsIn(p) # {Member(p)})

====
