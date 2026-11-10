---- MODULE CallbackProperty ----
EXTENDS Naturals, Sequences, FiniteSets

CONSTANT Mutant

Mutants == {"none", "Replace", "PutBackAtFirst", "PutBackWhatever", "IgnoreRefusal", "ReleaseOnReplaced", "SkipOnThrow"}

ASSUME Mutant \in Mutants

Handlers == {"p1", "p2", "a1", "b1"}
Objects == {1, 2}
Fw == <<"f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8">>
FwNames == {Fw[i] : i \in 1..Len(Fw)}
Scripts == <<"s1", "s2">>
MaxOps == 3

VARIABLES own, gen, prop, writable, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts,
          freezes, work, holdsA, holdsB, ops, atRestart, lastHeard, raised, heldThrew, middle, oldReleased, refused

vars == <<own, gen, prop, writable, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts,
          freezes, work, holdsA, holdsB, ops, atRestart, lastHeard, raised, heldThrew, middle, oldReleased, refused>>

Start(h) == [do |-> "start", h |-> h]
Release(h) == [do |-> "release", h |-> h]

Own(g) == IF own THEN (IF g = 1 THEN "own1" ELSE "own2") ELSE "none"

Range(s) == {s[i] : i \in DOMAIN s}
IsHandler(x) == x \in Handlers
Called(v) == IF v = "none" THEN <<>> ELSE <<v>>
FwIndex(v) == CHOOSE i \in 1..Len(Fw) : Fw[i] = v
IndexOf(s, h) == IF h \in Range(s) THEN CHOOSE i \in DOMAIN s : s[i] = h ELSE 0

HeldOracle(g) == IF scriptAt[g] > 0 THEN lastScript[g] ELSE Own(g)
LiveOn(g) == {h \in Handlers : on[h] = g /\ since[h] > scriptAt[g]}
Ordered(S) == CHOOSE s \in [1..Cardinality(S) -> S] : \A i, j \in 1..Cardinality(S) : i < j => since[s[i]] < since[s[j]]
Expected == Called(HeldOracle(gen)) \o Ordered(LiveOn(gen))

Init ==
    /\ own \in BOOLEAN
    /\ gen = 1
    /\ prop = [g \in Objects |-> Own(g)]
    /\ writable = [g \in Objects |-> TRUE]
    /\ installs = 0
    /\ chain = [i \in 1..Len(Fw) |-> [held |-> "none", list |-> <<>>, obj |-> 0]]
    /\ since = [h \in Handlers |-> 0]
    /\ on = [h \in Handlers |-> 0]
    /\ fwOf = [h \in Handlers |-> 0]
    /\ clock = 0
    /\ scriptAt = [g \in Objects |-> 0]
    /\ lastScript = [g \in Objects |-> "none"]
    /\ scripts = 0
    /\ freezes = 0
    /\ work = <<Start("p1"), Start("p2")>>
    /\ holdsA = FALSE
    /\ holdsB = FALSE
    /\ ops = 0
    /\ atRestart = "none"
    /\ lastHeard = <<>>
    /\ raised = FALSE
    /\ heldThrew = FALSE
    /\ middle = FALSE
    /\ oldReleased = FALSE
    /\ refused = FALSE

Quiet == /\ raised' = FALSE
         /\ lastHeard' = <<>>
         /\ heldThrew' = FALSE

Records(h, g, k) == /\ since' = [since EXCEPT ![h] = clock + 1]
                    /\ on' = [on EXCEPT ![h] = g]
                    /\ fwOf' = [fwOf EXCEPT ![h] = k]
                    /\ clock' = clock + 1

Cleared(h) == /\ since' = [since EXCEPT ![h] = 0]
              /\ on' = [on EXCEPT ![h] = 0]
              /\ fwOf' = [fwOf EXCEPT ![h] = 0]

DoStart(h) ==
    LET v == prop[gen] IN
    IF Mutant # "Replace" /\ v \in FwNames THEN
        /\ chain' = [chain EXCEPT ![FwIndex(v)].list = Append(@, h)]
        /\ Records(h, gen, FwIndex(v))
        /\ UNCHANGED <<prop, installs, refused>>
    ELSE IF writable[gen] THEN
        /\ chain' = [chain EXCEPT ![installs + 1] = [held |-> v, list |-> <<h>>, obj |-> gen]]
        /\ prop' = [prop EXCEPT ![gen] = Fw[installs + 1]]
        /\ installs' = installs + 1
        /\ Records(h, gen, installs + 1)
        /\ UNCHANGED refused
    ELSE IF Mutant = "IgnoreRefusal" THEN
        /\ Records(h, gen, 0)
        /\ refused' = TRUE
        /\ UNCHANGED <<chain, prop, installs>>
    ELSE
        /\ refused' = TRUE
        /\ UNCHANGED <<chain, prop, installs, since, on, fwOf, clock>>

PutsBack(rest, holds) ==
    CASE Mutant = "PutBackAtFirst" -> holds
      [] Mutant = "PutBackWhatever" -> rest = <<>>
      [] OTHER -> rest = <<>> /\ holds

DoRelease(h) ==
    LET g == on[h]
        k == fwOf[h]
    IN
    IF since[h] = 0 THEN UNCHANGED <<chain, prop, since, on, fwOf, middle, oldReleased>>
    ELSE IF k = 0 THEN
        /\ Cleared(h)
        /\ UNCHANGED <<chain, prop, middle, oldReleased>>
    ELSE IF g # gen /\ Mutant # "ReleaseOnReplaced" THEN
        /\ Cleared(h)
        /\ oldReleased' = TRUE
        /\ UNCHANGED <<chain, prop, middle>>
    ELSE
        LET list == chain[k].list
            rest == SelectSeq(list, LAMBDA x : x # h)
            at == IndexOf(list, h)
        IN
        /\ chain' = [chain EXCEPT ![k].list = rest]
        /\ prop' = IF PutsBack(rest, prop[g] = Fw[k]) /\ writable[g] THEN [prop EXCEPT ![g] = chain[k].held] ELSE prop
        /\ middle' = (middle \/ (1 < at /\ at < Len(list)))
        /\ oldReleased' = (oldReleased \/ g # gen)
        /\ Cleared(h)

Step ==
    /\ work # <<>>
    /\ work' = Tail(work)
    /\ IF Head(work).do = "start"
       THEN DoStart(Head(work).h) /\ UNCHANGED <<middle, oldReleased>>
       ELSE DoRelease(Head(work).h) /\ UNCHANGED <<installs, clock, refused>>
    /\ Quiet
    /\ UNCHANGED <<own, gen, writable, scriptAt, lastScript, scripts, freezes, holdsA, holdsB, ops, atRestart>>

Script ==
    /\ scripts < Len(Scripts)
    /\ writable[gen]
    /\ prop' = [prop EXCEPT ![gen] = Scripts[scripts + 1]]
    /\ lastScript' = [lastScript EXCEPT ![gen] = Scripts[scripts + 1]]
    /\ scriptAt' = [scriptAt EXCEPT ![gen] = clock + 1]
    /\ clock' = clock + 1
    /\ scripts' = scripts + 1
    /\ Quiet
    /\ UNCHANGED <<own, gen, writable, installs, chain, since, on, fwOf, freezes, work, holdsA, holdsB, ops,
                   atRestart, middle, oldReleased, refused>>

Freeze ==
    /\ freezes = 0
    /\ writable' = [writable EXCEPT ![gen] = FALSE]
    /\ freezes' = 1
    /\ Quiet
    /\ UNCHANGED <<own, gen, prop, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts, work,
                   holdsA, holdsB, ops, atRestart, middle, oldReleased, refused>>

Restart ==
    /\ gen = 1
    /\ gen' = 2
    /\ atRestart' = prop[1]
    /\ Quiet
    /\ UNCHANGED <<own, prop, writable, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts,
                   freezes, work, holdsA, holdsB, ops, middle, oldReleased, refused>>

ReleaseA == (IF holdsB THEN <<Release("b1")>> ELSE <<>>) \o (IF holdsA THEN <<Release("a1")>> ELSE <<>>)

Owning(next, a, b) ==
    /\ work = <<>>
    /\ ops < MaxOps
    /\ work' = next
    /\ holdsA' = a
    /\ holdsB' = b
    /\ ops' = ops + 1
    /\ Quiet
    /\ UNCHANGED <<own, gen, prop, writable, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts,
                   freezes, atRestart, middle, oldReleased, refused>>

LoadA ==
    \/ Owning(ReleaseA \o <<Start("a1")>>, TRUE, FALSE)
    \/ Owning(ReleaseA \o <<Start("a1"), Start("b1")>>, TRUE, TRUE)
    \/ Owning(ReleaseA \o <<Start("b1"), Start("a1")>>, TRUE, TRUE)

LoadB == holdsA /\ Owning((IF holdsB THEN <<Release("b1")>> ELSE <<>>) \o <<Start("b1")>>, TRUE, TRUE)

UnloadA == holdsA /\ Owning(ReleaseA, FALSE, FALSE)

UnloadB == holdsB /\ Owning(<<Release("b1")>>, TRUE, FALSE)

CalledFirst(v) == IF v \in FwNames THEN chain[FwIndex(v)].held ELSE v

Hearers(v, throws) ==
    IF v \in FwNames THEN
        LET k == FwIndex(v)
            held == IF Mutant = "Replace" THEN <<>> ELSE Called(chain[k].held)
        IN IF throws /\ Mutant = "SkipOnThrow" THEN held ELSE held \o chain[k].list
    ELSE Called(v)

Raise ==
    \E throws \in BOOLEAN :
        /\ throws => CalledFirst(prop[gen]) \notin FwNames \cup {"none"}
        /\ lastHeard' = Hearers(prop[gen], throws)
        /\ raised' = TRUE
        /\ heldThrew' = throws
        /\ UNCHANGED <<own, gen, prop, writable, installs, chain, since, on, fwOf, clock, scriptAt, lastScript, scripts,
                       freezes, work, holdsA, holdsB, ops, atRestart, middle, oldReleased, refused>>

Next == Step \/ Script \/ Freeze \/ Restart \/ LoadA \/ LoadB \/ UnloadA \/ UnloadB \/ Raise

Spec == Init /\ [][Next]_vars

EveryHearerHearsOnceInOrder == raised => lastHeard = Expected

AThrowSkipsNoHandler == raised /\ heldThrew => SelectSeq(lastHeard, IsHandler) = Ordered(LiveOn(gen))

ALiveHandlerKeepsTheFrameworksFunction == writable[gen] /\ LiveOn(gen) # {} => prop[gen] \in FwNames

TheLastReleasePutsBackItsValue ==
    writable[gen] => \/ prop[gen] = HeldOracle(gen)
                     \/ prop[gen] \in FwNames /\ LiveOn(gen) # {}

ARefusedAssignmentStartsNoHandler == ~writable[gen] /\ LiveOn(gen) # {} => prop[gen] \in FwNames

AReplacedObjectIsNotWritten == gen = 2 => prop[1] = atRestart

ReachThreeOwnersHearOneCallback == ~(raised /\ work = <<>> /\ {"p1", "a1", "b1"} \subseteq Range(lastHeard))

ReachScriptBetweenSubscribes ==
    ~(/\ raised
      /\ work = <<>>
      /\ scriptAt[gen] > 0
      /\ lastHeard # <<>>
      /\ Head(lastHeard) = lastScript[gen]
      /\ Range(lastHeard) \cap Handlers # {}
      /\ \E h \in Handlers : on[h] = gen /\ 0 < since[h] /\ since[h] < scriptAt[gen])

ReachReleaseFromTheMiddle == ~(raised /\ work = <<>> /\ middle /\ "b1" \notin Range(lastHeard) /\ {"p1", "a1"} \subseteq Range(lastHeard))

ReachAThrowThenEveryHandler == ~(raised /\ work = <<>> /\ heldThrew /\ Cardinality(Range(lastHeard) \cap Handlers) >= 2)

ReachPutBackRefused == ~(raised /\ work = <<>> /\ ~writable[gen] /\ prop[gen] \in FwNames /\ LiveOn(gen) = {} /\ lastHeard # <<>>)

ReachAssignmentRefused == ~(raised /\ work = <<>> /\ refused /\ ~writable[gen] /\ prop[gen] \notin FwNames /\ holdsA)

ReachReleasedAfterRestart == ~(raised /\ work = <<>> /\ gen = 2 /\ oldReleased /\ "a1" \in Range(lastHeard))
====
