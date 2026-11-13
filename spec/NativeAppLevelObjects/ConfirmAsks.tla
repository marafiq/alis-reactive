---- MODULE ConfirmAsks ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "ShowsEveryAsk", "LatestFirst", "IgnoresTheSignal", "AnswersAgain", "AbortAnswersYes",
            "UnmountKeepsAsks"}

ASSUME Mutant \in Mutants

Runs == {"a", "b"}

VARIABLES state, waiting, open, asked, shown, answer, answers, okBy, ended, endedWhile, mounted, unmountedWhile

vars == <<state, waiting, open, asked, shown, answer, answers, okBy, ended, endedWhile, mounted, unmountedWhile>>

Range(s) == {s[i] : i \in DOMAIN s}
Position(s, r) == CHOOSE i \in DOMAIN s : s[i] = r
Without(s, r) == SelectSeq(s, LAMBDA x : x # r)

Init ==
    /\ state = [r \in Runs |-> "idle"]
    /\ waiting = <<>>
    /\ open = {}
    /\ asked = <<>>
    /\ shown = <<>>
    /\ answer = [r \in Runs |-> "none"]
    /\ answers = [r \in Runs |-> 0]
    /\ okBy = [r \in Runs |-> FALSE]
    /\ ended = [r \in Runs |-> FALSE]
    /\ endedWhile = [r \in Runs |-> "none"]
    /\ mounted = TRUE
    /\ unmountedWhile = [r \in Runs |-> "none"]

Answered(r, value) ==
    /\ answer' = [answer EXCEPT ![r] = value]
    /\ answers' = [answers EXCEPT ![r] = @ + 1]
    /\ state' = [state EXCEPT ![r] = "answered"]

Ask(r) ==
    /\ mounted
    /\ state[r] = "idle"
    /\ ~ended[r]
    /\ asked' = Append(asked, r)
    /\ IF Mutant = "ShowsEveryAsk"
       THEN /\ state' = [state EXCEPT ![r] = "open"]
            /\ open' = open \cup {r}
            /\ shown' = Append(shown, r)
            /\ UNCHANGED waiting
       ELSE /\ state' = [state EXCEPT ![r] = "waiting"]
            /\ waiting' = Append(waiting, r)
            /\ UNCHANGED <<open, shown>>
    /\ UNCHANGED <<answer, answers, okBy, ended, endedWhile, mounted, unmountedWhile>>

Turn ==
    /\ mounted
    /\ open = {}
    /\ waiting # <<>>
    /\ LET r == IF Mutant = "LatestFirst" THEN waiting[Len(waiting)] ELSE Head(waiting) IN
       /\ waiting' = Without(waiting, r)
       /\ state' = [state EXCEPT ![r] = "open"]
       /\ open' = {r}
       /\ shown' = Append(shown, r)
    /\ UNCHANGED <<asked, answer, answers, okBy, ended, endedWhile, mounted, unmountedWhile>>

Ok(r) ==
    /\ r \in open
    /\ Answered(r, "yes")
    /\ okBy' = [okBy EXCEPT ![r] = TRUE]
    /\ open' = open \ {r}
    /\ UNCHANGED <<waiting, asked, shown, ended, endedWhile, mounted, unmountedWhile>>

Cancel(r) ==
    /\ r \in open
    /\ Answered(r, "no")
    /\ open' = open \ {r}
    /\ UNCHANGED <<waiting, asked, shown, okBy, ended, endedWhile, mounted, unmountedWhile>>

Abort(r) ==
    /\ ~ended[r]
    /\ ended' = [ended EXCEPT ![r] = TRUE]
    /\ endedWhile' = [endedWhile EXCEPT ![r] = state[r]]
    /\ CASE Mutant = "IgnoresTheSignal" ->
              UNCHANGED <<state, waiting, open, answer, answers>>
         [] state[r] = "waiting" ->
              /\ Answered(r, IF Mutant = "AbortAnswersYes" THEN "yes" ELSE "no")
              /\ waiting' = Without(waiting, r)
              /\ UNCHANGED open
         [] state[r] = "open" ->
              /\ Answered(r, IF Mutant = "AbortAnswersYes" THEN "yes" ELSE "no")
              /\ open' = open \ {r}
              /\ UNCHANGED waiting
         [] state[r] = "answered" /\ Mutant = "AnswersAgain" ->
              /\ Answered(r, "no")
              /\ UNCHANGED <<waiting, open>>
         [] OTHER ->
              UNCHANGED <<state, waiting, open, answer, answers>>
    /\ UNCHANGED <<asked, shown, okBy, mounted, unmountedWhile>>

Unmount ==
    /\ mounted
    /\ mounted' = FALSE
    /\ unmountedWhile' = [r \in Runs |-> state[r]]
    /\ IF Mutant = "UnmountKeepsAsks"
       THEN UNCHANGED <<state, waiting, open, answer, answers>>
       ELSE /\ state' = [r \in Runs |-> IF state[r] \in {"waiting", "open"} THEN "answered" ELSE state[r]]
            /\ answer' = [r \in Runs |-> IF state[r] \in {"waiting", "open"} THEN "no" ELSE answer[r]]
            /\ answers' = [r \in Runs |-> IF state[r] \in {"waiting", "open"} THEN answers[r] + 1 ELSE answers[r]]
            /\ waiting' = <<>>
            /\ open' = {}
    /\ UNCHANGED <<asked, shown, okBy, ended, endedWhile>>

Next ==
    \/ \E r \in Runs : Ask(r) \/ Ok(r) \/ Cancel(r) \/ Abort(r)
    \/ Turn
    \/ Unmount

Spec == Init /\ [][Next]_vars

OneQuestionAtATime == Cardinality(open) <= 1

QuestionsInAskedOrder ==
    \A i, j \in DOMAIN shown : i < j => Position(asked, shown[i]) < Position(asked, shown[j])

NothingWaitsForAnEndedRun == \A r \in Runs : ended[r] => state[r] \notin {"waiting", "open"}

EachAskAnsweredOnce == \A r \in Runs : answers[r] <= 1

OnlyThePersonsOkAnswersYes == \A r \in Runs : answer[r] = "yes" => okBy[r]

UnmountAnswersEveryAsk == ~mounted => \A r \in Runs : state[r] \notin {"waiting", "open"}

ReachSecondQuestionAfterTheFirst ==
    ~(Len(shown) = 2 /\ \A r \in Runs : state[r] = "answered" /\ okBy[r])

ReachAbortWhileOpen ==
    ~(\E r \in Runs : endedWhile[r] = "open" /\ answer[r] = "no" /\ r \in Range(shown))

ReachAbortWhileWaiting ==
    ~(\E r, s \in Runs : r # s /\ endedWhile[r] = "waiting" /\ answer[r] = "no" /\ r \notin Range(shown)
                         /\ shown = <<s>> /\ s \in Range(asked) /\ Position(asked, r) < Position(asked, s))

ReachUnmountWithAWaitingAsk ==
    ~(\E r, s \in Runs : unmountedWhile[r] = "open" /\ unmountedWhile[s] = "waiting" /\ answer[r] = "no"
                         /\ answer[s] = "no")
====
