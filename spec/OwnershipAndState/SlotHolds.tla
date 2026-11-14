---- MODULE SlotHolds ----
CONSTANT Mutant

Mutants == {"none", "OwnSlotUnchecked", "ContentPlanHoldsNothing", "SlotOwnerHoldsNoKey", "OwnElementOnly",
            "HoldSurvivesRelease"}

ASSUME Mutant \in Mutants

Slots == {"card", "notes", "inner"}

Plans == {"page", "card"}

Elements == Slots \cup {"cards", "status"}

SlotsNamedBy(p) == IF p = "page" THEN {"card"} ELSE {"notes", "inner"}

Contains(e) == CASE e = "cards" -> {"card", "inner"}
                 [] e = "card" -> {"card", "inner"}
                 [] e = "inner" -> {"inner"}
                 [] e = "notes" -> {"notes"}
                 [] e = "status" -> {}

ReleasedWith(s) == IF s = "card" THEN {"inner", "card"} ELSE {s}

VARIABLES content, holds, left, write

vars == <<content, holds, left, write>>

OnPage(e) == e # "inner" \/ content["card"] # "none"

Live(p) == p = "page" \/ content["card"] # "none"

SlotsIn(e) == {s \in Contains(e) : OnPage(s)}

RefusedWhereTheViewRenders(p, e) == e \in SlotsNamedBy(p)

PlanHolds(p) == IF Mutant = "ContentPlanHoldsNothing" /\ p # "page" THEN {} ELSE {<<p, s>> : s \in SlotsNamedBy(p)}

ContentHold(s) == IF Mutant = "SlotOwnerHoldsNoKey" THEN {} ELSE {<<s, s>>}

Started(s) == ContentHold(s) \cup (IF s = "card" THEN PlanHolds("card") ELSE {})

Kept(released) == IF Mutant = "HoldSurvivesRelease" THEN holds ELSE {h \in holds : h[1] \notin released}

HeldKeys == {h[2] : h \in holds}

Checked(p, e) ==
    CASE Mutant = "OwnSlotUnchecked" /\ e = p -> {}
      [] Mutant = "OwnElementOnly" -> SlotsIn(e) \cap {e}
      [] OTHER -> SlotsIn(e)

Unwritten == [by |-> "none", on |-> "none", outcome |-> "none"]

Boots == {[s \in Slots |-> "none"],
          [s \in Slots |-> IF s = "card" THEN "served" ELSE "none"],
          [s \in Slots |-> IF s = "inner" THEN "none" ELSE "served"]}

Init ==
    /\ content \in Boots
    /\ holds = PlanHolds("page") \cup UNION {Started(s) : s \in {x \in Slots : content[x] = "served"}}
    /\ left = {}
    /\ write = Unwritten

Load(p, s) ==
    /\ Live(p)
    /\ s \in SlotsNamedBy(p)
    /\ OnPage(s)
    /\ content' = [x \in Slots |-> IF x = s THEN "loaded" ELSE IF x \in ReleasedWith(s) THEN "none" ELSE content[x]]
    /\ holds' = Kept(ReleasedWith(s)) \cup Started(s)
    /\ left' = left \cup {x \in ReleasedWith(s) : content[x] # "none"}
    /\ write' = Unwritten

Unload(p, s) ==
    /\ Live(p)
    /\ s \in SlotsNamedBy(p)
    /\ content[s] # "none"
    /\ content' = [x \in Slots |-> IF x \in ReleasedWith(s) THEN "none" ELSE content[x]]
    /\ holds' = Kept(ReleasedWith(s))
    /\ left' = left \cup {x \in ReleasedWith(s) : content[x] # "none"}
    /\ write' = Unwritten

Write(p, e) ==
    /\ Live(p)
    /\ OnPage(e)
    /\ ~RefusedWhereTheViewRenders(p, e)
    /\ write' = [by |-> p, on |-> e, outcome |-> IF Checked(p, e) \cap HeldKeys # {} THEN "stopped" ELSE "wrote"]
    /\ UNCHANGED <<content, holds, left>>

Next ==
    \/ \E p \in Plans, s \in Slots : Load(p, s) \/ Unload(p, s)
    \/ \E p \in Plans, e \in Elements : Write(p, e)

Spec == Init /\ [][Next]_vars

NamedByALivePlan(s) == \E p \in Plans : Live(p) /\ s \in SlotsNamedBy(p)

Held(s) == NamedByALivePlan(s) \/ content[s] # "none"

IsOrHoldsAHeldSlot(e) == \E s \in SlotsIn(e) : Held(s)

Done == write.outcome # "none"

Stopped == write.outcome = "stopped"

At(p, e, result) == write = [by |-> p, on |-> e, outcome |-> result]

AStepOverANamedSlotThatHoldsContentStops ==
    (Done /\ write.on \in Slots /\ content[write.on] # "none" /\ NamedByALivePlan(write.on)) => Stopped

AStepOverANamedSlotThatHoldsNoneStops ==
    (Done /\ write.on \in Slots /\ content[write.on] = "none" /\ NamedByALivePlan(write.on)) => Stopped

AStepOverContentInASlotNoPlanNamesStops ==
    (Done /\ write.on \in Slots /\ content[write.on] # "none" /\ ~NamedByALivePlan(write.on)) => Stopped

AStepOverAnElementHoldingAHeldSlotStops == (Done /\ write.on \notin Slots /\ IsOrHoldsAHeldSlot(write.on)) => Stopped

OnlyAHeldSlotStopsAStep == (Done /\ ~IsOrHoldsAHeldSlot(write.on)) => ~Stopped

ReachACardsStepOverItsOwnSlotStops == ~At("card", "card", "stopped")

ReachAPagesStepOverALoadedSlotAnOpenCardNamesStops ==
    ~(At("page", "notes", "stopped") /\ content["card"] # "none" /\ content["notes"] = "loaded")

ReachAStepOverASlotAnOpenCardNamesStops ==
    ~(At("page", "notes", "stopped") /\ content["card"] # "none" /\ content["notes"] = "none" /\ "notes" \notin left)

ReachAnEmptiedSlotStillStops ==
    ~(At("page", "notes", "stopped") /\ content["card"] # "none" /\ content["notes"] = "none" /\ "notes" \in left)

ReachACardsOwnSlotStops == ~(At("page", "inner", "stopped") /\ content["inner"] = "none")

ReachNotesAClosedCardLoadedStop ==
    ~(At("page", "notes", "stopped") /\ content["card"] = "none" /\ content["notes"] = "loaded")

ReachNotesTheServerPutThereStop ==
    ~(At("page", "notes", "stopped") /\ content["card"] = "none" /\ content["notes"] = "served")

ReachAPagesStepOverTheElementHoldingTheCardStops == ~(At("page", "cards", "stopped") /\ content["card"] = "loaded")

ReachTheElementAroundAnEmptiedSlotStillStops ==
    ~(At("page", "cards", "stopped") /\ content["card"] = "none" /\ "card" \in left)

ReachASlotItsOnlyPlanLeftTakesTheWrite ==
    ~(At("page", "notes", "wrote") /\ "card" \in left /\ "notes" \notin left)

ReachNotesClearedBeforeTheirCardClosedTakeTheWrite ==
    ~(At("page", "notes", "wrote") /\ {"card", "notes"} \subseteq left)

ReachAnElementHoldingNoSlotTakesTheWrite == ~At("card", "status", "wrote")

====
