---- MODULE Realtime ----
EXTENDS Naturals, FiniteSets

CONSTANT Mutant

Mutants == {"none", "StreamHoldsBusy", "MessageRequestUncounted", "OfferNotCounted", "ReleaseKeepsOffer"}

ASSUME Mutant \in Mutants

Streams == {"s", "t"}
Requests == {"a", "m"}
Holding == {"attempt", "defaultFinally"}

VARIABLES slot, state, takesDefaults, pc, open, shown, closedHeld, offered, releasedClosed, retriedBoth

vars == <<slot, state, takesDefaults, pc, open, shown, closedHeld, offered, releasedClosed, retriedBoth>>

Held(sl) == IF sl = "live" THEN Streams ELSE {"s"}

Unanswered == {"closed", "retrying"}

ClosedOf(st, sl) == {x \in Held(sl) : st[x] \in Unanswered}

Init ==
    /\ slot = "live"
    /\ state = [x \in Streams |-> "connecting"]
    /\ takesDefaults \in [Requests -> BOOLEAN]
    /\ pc = [r \in Requests |-> "idle"]
    /\ open = 0
    /\ shown = FALSE
    /\ closedHeld = 0
    /\ offered = FALSE
    /\ releasedClosed = FALSE
    /\ retriedBoth = FALSE

Counts(r) == takesDefaults[r] /\ ~(Mutant = "MessageRequestUncounted" /\ r = "m")

Hold(r) ==
    /\ open' = IF Counts(r) THEN open + 1 ELSE open
    /\ shown' = (shown \/ Counts(r))

Release(r) ==
    /\ open' = IF Counts(r) THEN open - 1 ELSE open
    /\ shown' = IF Counts(r) THEN open - 1 > 0 ELSE shown

BeginPage ==
    /\ pc["a"] = "idle"
    /\ pc' = [pc EXCEPT !["a"] = "attempt"]
    /\ Hold("a")
    /\ UNCHANGED <<slot, state, takesDefaults, closedHeld, offered, releasedClosed, retriedBoth>>

Message ==
    /\ state["s"] = "open"
    /\ pc["m"] = "idle"
    /\ pc' = [pc EXCEPT !["m"] = "attempt"]
    /\ Hold("m")
    /\ UNCHANGED <<slot, state, takesDefaults, closedHeld, offered, releasedClosed, retriedBoth>>

Answer(r) ==
    /\ pc[r] = "attempt"
    /\ pc' = [pc EXCEPT ![r] = "defaultFinally"]
    /\ UNCHANGED <<slot, state, takesDefaults, open, shown, closedHeld, offered, releasedClosed, retriedBoth>>

DefaultFinally(r) ==
    /\ pc[r] = "defaultFinally"
    /\ pc' = [pc EXCEPT ![r] = "done"]
    /\ Release(r)
    /\ UNCHANGED <<slot, state, takesDefaults, closedHeld, offered, releasedClosed, retriedBoth>>

Offer(n) == IF Mutant = "OfferNotCounted" THEN n > 0 /\ ~(closedHeld > 0 /\ n < closedHeld) ELSE n > 0

Move(x, to) ==
    /\ x \in Held(slot)
    /\ LET next == [state EXCEPT ![x] = to]
           n == Cardinality(ClosedOf(next, slot))
       IN /\ state' = next
          /\ closedHeld' = n
          /\ offered' = Offer(n)
    /\ LET busyHolds == Mutant = "StreamHoldsBusy"
           entering == to = "reconnecting"
           leaving == state[x] = "reconnecting"
       IN /\ open' = IF busyHolds /\ entering THEN open + 1 ELSE IF busyHolds /\ leaving THEN open - 1 ELSE open
          /\ shown' = IF busyHolds /\ entering THEN TRUE ELSE IF busyHolds /\ leaving THEN open - 1 > 0 ELSE shown

Connect(x) ==
    /\ state[x] \in {"connecting", "retrying"}
    /\ \E to \in {"open", "closed"} : Move(x, to)
    /\ UNCHANGED <<slot, takesDefaults, pc, releasedClosed, retriedBoth>>

Drop(x) ==
    /\ state[x] = "open"
    /\ Move(x, "reconnecting")
    /\ UNCHANGED <<slot, takesDefaults, pc, releasedClosed, retriedBoth>>

Reconnected(x) ==
    /\ state[x] = "reconnecting"
    /\ Move(x, "open")
    /\ UNCHANGED <<slot, takesDefaults, pc, releasedClosed, retriedBoth>>

GiveUp(x) ==
    /\ state[x] = "reconnecting"
    /\ Move(x, "closed")
    /\ UNCHANGED <<slot, takesDefaults, pc, releasedClosed, retriedBoth>>

Retry ==
    /\ offered
    /\ \E x \in Held(slot) : state[x] = "closed"
    /\ LET next == [x \in Streams |-> IF x \in Held(slot) /\ state[x] = "closed" THEN "retrying" ELSE state[x]]
       IN /\ state' = next
          /\ closedHeld' = Cardinality(ClosedOf(next, slot))
          /\ offered' = Offer(Cardinality(ClosedOf(next, slot)))
    /\ retriedBoth' = (Cardinality(ClosedOf(state, slot)) = 2)
    /\ UNCHANGED <<slot, takesDefaults, pc, open, shown, releasedClosed>>

Unload ==
    /\ slot = "live"
    /\ slot' = "gone"
    /\ LET wasClosed == state["t"] \in Unanswered
           next == [state EXCEPT !["t"] = "released"]
           n == Cardinality(ClosedOf(next, "gone"))
       IN /\ state' = next
          /\ releasedClosed' = wasClosed
          /\ closedHeld' = n
          /\ offered' = IF Mutant = "ReleaseKeepsOffer" THEN offered ELSE Offer(n)
    /\ LET leaving == state["t"] = "reconnecting" /\ Mutant = "StreamHoldsBusy"
       IN /\ open' = IF leaving THEN open - 1 ELSE open
          /\ shown' = IF leaving THEN open - 1 > 0 ELSE shown
    /\ UNCHANGED <<takesDefaults, pc, retriedBoth>>

Next ==
    \/ BeginPage \/ Message \/ Retry \/ Unload
    \/ \E r \in Requests : Answer(r) \/ DefaultFinally(r)
    \/ \E x \in Streams : Connect(x) \/ Drop(x) \/ Reconnected(x) \/ GiveUp(x)

Spec == Init /\ [][Next]_vars

Covered == {r \in Requests : takesDefaults[r] /\ pc[r] \in Holding}

IndicatorNeverHidesEarly == Covered # {} => shown

IndicatorNeverStaysShown == shown => Covered # {}

OfferNeverHidesEarly == ClosedOf(state, slot) # {} => offered

OfferNeverStaysShown == offered => ClosedOf(state, slot) # {}

ReachMessageRequestCoveredWhileSlotStreamClosed == ~("m" \in Covered /\ shown /\ state["t"] = "closed" /\ offered)

ReachReconnectingShowsNoBusy == ~(state["s"] = "reconnecting" /\ Covered = {} /\ ~shown)

ReachUnloadedClosedStreamHidesOffer == ~(slot = "gone" /\ releasedClosed /\ state["s"] # "closed" /\ ~offered)

ReachRetryReconnectsEveryClosed == ~(retriedBoth /\ ~offered /\ \A x \in Streams : state[x] = "open")

ReachRetryingKeepsOffer == ~(\E x \in Streams : state[x] = "retrying" /\ offered /\ closedHeld = 1)

====
