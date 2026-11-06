---- MODULE ValidationToday ----
EXTENDS Naturals

VARIABLES signoffInput, signoffStart, wingSent, wingInput, wingStart, leadInput, leadStart, bagsSent, bagsInput, itemBagsStart

vars == <<signoffInput, signoffStart, wingSent, wingInput, wingStart, leadInput, leadStart, bagsSent, bagsInput, itemBagsStart>>

Init ==
    /\ signoffInput \in BOOLEAN
    /\ signoffStart \in {"yes", "none"}
    /\ wingSent \in BOOLEAN
    /\ wingInput \in {"absent", "east", "empty"}
    /\ wingStart \in {"east", "none"}
    /\ leadInput \in BOOLEAN
    /\ leadStart \in {"object", "null"}
    /\ bagsSent \in BOOLEAN
    /\ bagsInput \in {"absent", "one", "empty"}
    /\ itemBagsStart \in {"one", "none"}

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

RefusedAtRender == ~signoffInput /\ signoffStart # "none"

BrowserWing == IF wingInput = "absent" THEN "none" ELSE wingInput

ServerWing == IF wingSent THEN BrowserWing ELSE wingStart

BrowserLeadNull == ~leadInput

ServerLeadNull == ~leadInput /\ leadStart = "null"

BrowserBags == IF bagsInput = "absent" THEN "none" ELSE bagsInput

ServerBags == IF bagsSent THEN BrowserBags ELSE itemBagsStart

AnUnrenderedMemberRendersItsView == ~RefusedAtRender

AListNotSentReadsItsStart == BrowserWing = ServerWing

AConditionReadsTheObjectsStart == BrowserLeadNull = ServerLeadNull

AnItemsListNotSentReadsTheItemsStart == BrowserBags = ServerBags

====
