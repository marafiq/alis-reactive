---- MODULE DateWire ----
EXTENDS Integers

CONSTANT Mutant

Mutants == {"none", "DateSentWithOffset", "MidnightWritesTime", "PageDayOffset", "EmptyInstantSentAsNull"}

ASSUME Mutant \in Mutants

Days == 1..2
MinutesOfDay == {0, 30, 1410}
Offsets == {-60, 0, 60}
PageDay == 1

VARIABLES offset, kind, present, day, minute

vars == <<offset, kind, present, day, minute>>

Init ==
    /\ offset \in [Days -> Offsets]
    /\ kind \in {"date", "instant"}
    /\ present \in BOOLEAN
    /\ day \in Days
    /\ minute \in MinutesOfDay

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

Utc == day * 1440 + minute - offset[day]

Text(form, d, m, time, hasZone, zone) ==
    [form |-> form, day |-> d, minute |-> m, time |-> time, hasZone |-> hasZone, zone |-> zone]

Earliest == Text("text", 0, 0, TRUE, TRUE, 0)

DateText ==
    Text("text", day, minute, minute # 0 \/ Mutant = "MidnightWritesTime",
         Mutant = "DateSentWithOffset", offset[day])

InstantText ==
    Text("text", day, minute, TRUE, TRUE,
         IF Mutant = "PageDayOffset" THEN offset[PageDay] ELSE offset[day])

Sent ==
    IF ~present
      THEN IF kind = "instant"
             THEN IF Mutant = "EmptyInstantSentAsNull" THEN Text("null", 0, 0, FALSE, FALSE, 0) ELSE Earliest
             ELSE Text("nothing", 0, 0, FALSE, FALSE, 0)
      ELSE IF kind = "date" THEN DateText ELSE InstantText

ParsedUtc(text) == text.day * 1440 + text.minute - text.zone

DateTimeCarriesNoOffset ==
    (present /\ kind = "date") => (~Sent.hasZone /\ Sent.day = day /\ Sent.minute = minute)

DateShowsItsDayAtMidnight ==
    (present /\ kind = "date") => (Sent.time <=> minute # 0)

InstantCarriesItsClockOffset ==
    (present /\ kind = "instant") =>
        /\ Sent.hasZone
        /\ Sent.zone = offset[day]
        /\ Sent.day = day
        /\ Sent.minute = minute
        /\ ParsedUtc(Sent) = Utc

InstantNothingSentAsEarliest ==
    (~present /\ kind = "instant") => Sent = Earliest

ReachOffsetChangesBetweenDays == ~(present /\ kind = "instant" /\ day # PageDay /\ offset[day] # offset[PageDay])

====
