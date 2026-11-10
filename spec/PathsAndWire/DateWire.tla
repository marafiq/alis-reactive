---- MODULE DateWire ----
EXTENDS Integers

CONSTANT Mutant

Mutants == {"none", "DateSentWithOffset", "MidnightWritesTime", "PageDayOffset", "EmptyInstantSentAsEarliest",
            "DayWritesTime", "TimeWritesDay", "FormSendsDayAlone"}

ASSUME Mutant \in Mutants

Days == 1..2
MinutesOfDay == {0, 30, 1410}
Offsets == {-60, 0, 60}
PageDay == 1

VARIABLES offset, kind, clrKind, present, day, minute, carrier

vars == <<offset, kind, clrKind, present, day, minute, carrier>>

Init ==
    /\ offset \in [Days -> Offsets]
    /\ kind \in {"date", "instant", "day", "time"}
    /\ clrKind \in {"Unspecified", "Local", "Utc"}
    /\ present \in BOOLEAN
    /\ day \in Days
    /\ minute \in MinutesOfDay
    /\ carrier \in {"text", "form"}

Next == UNCHANGED vars

Spec == Init /\ [][Next]_vars

Utc == day * 1440 + minute - offset[day]

Text(form, d, m, time, hasZone, zone) ==
    [form |-> form, day |-> d, minute |-> m, time |-> time, hasZone |-> hasZone, zone |-> zone]

Earliest == Text("text", 0, 0, TRUE, TRUE, 0)

InForm == carrier = "form" /\ Mutant # "FormSendsDayAlone"

DateText ==
    Text("text", day, minute, minute # 0 \/ InForm \/ Mutant = "MidnightWritesTime",
         Mutant = "DateSentWithOffset", offset[day])

DayText == Text("text", day, 0, InForm \/ Mutant = "DayWritesTime", FALSE, 0)

TimeText == Text("text", IF Mutant = "TimeWritesDay" THEN day ELSE 0, minute, TRUE, FALSE, 0)

InstantText ==
    Text("text", day, minute, TRUE, TRUE,
         IF Mutant = "PageDayOffset" THEN offset[PageDay] ELSE offset[day])

Sent ==
    IF ~present
      THEN IF kind = "instant" /\ Mutant = "EmptyInstantSentAsEarliest" THEN Earliest ELSE Text("nothing", 0, 0, FALSE, FALSE, 0)
      ELSE IF kind = "date" THEN DateText
      ELSE IF kind = "day" THEN DayText
      ELSE IF kind = "time" THEN TimeText
      ELSE InstantText

ParsedUtc(text) == text.day * 1440 + text.minute - text.zone

DateTimeCarriesNoOffset ==
    (present /\ kind = "date") => (~Sent.hasZone /\ Sent.day = day /\ Sent.minute = minute)

DateShowsItsDayAtMidnight ==
    (present /\ kind = "date" /\ carrier = "text") => (Sent.time <=> minute # 0)

DateInFormCarriesItsTime ==
    (present /\ kind \in {"date", "day"} /\ carrier = "form") => (Sent.time /\ Sent.day = day /\ ~Sent.hasZone)

InstantCarriesItsClockOffset ==
    (present /\ kind = "instant") =>
        /\ Sent.hasZone
        /\ Sent.zone = offset[day]
        /\ Sent.day = day
        /\ Sent.minute = minute
        /\ ParsedUtc(Sent) = Utc

DayIsItsDayAlone == (present /\ kind = "day" /\ carrier = "text") => (Sent.day = day /\ ~Sent.time /\ ~Sent.hasZone)

TimeIsItsTimeAlone == (present /\ kind = "time") => (Sent.day = 0 /\ Sent.minute = minute /\ ~Sent.hasZone)

NothingWritesNoText == ~present => Sent.form = "nothing"

ReachUtcKindWritten == ~(present /\ kind = "date" /\ clrKind = "Utc" /\ ~Sent.hasZone /\ Sent.minute = minute)

ReachOffsetChangesBetweenDays == ~(present /\ kind = "instant" /\ day # PageDay /\ offset[day] # offset[PageDay])

====
