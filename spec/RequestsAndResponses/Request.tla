---- MODULE Request ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "PlaceholderUnchecked", "DateInPath", "FileInJson", "NullForValueType",
            "FailedCheckSends", "FinallyWithoutLoading", "CatchAllBeatsStatus", "FailureRunsSuccess",
            "ChainedAfterError", "StepsAfterStop", "RetryAnyPost", "RetryPastAttempts", "LoadingEachAttempt",
            "FailedConditionSendsNothing", "ChainedIgnoresCondition", "CatchAllRunsOnSuccess",
            "EmptyFieldForObject", "EmptyFieldForObjectItem", "OneFlagForBothLayers", "HoldBeforeLoading",
            "HoldOnAFailedGuard"}

ASSUME Mutant \in Mutants

MaxAttempts == 2
Outcomes == {"ok", "s404", "s503", "network", "offOrigin", "mismatch"}
Statuses == {"s404", "s503"}
Failures == {"network", "offOrigin", "mismatch"}
Retryable == {"network", "s503"}

Configs == [placeholder : {"filled", "unfilled"}, filler : {"scalar", "date", "list"},
            file : {"none", "set"}, method : {"GET", "HEAD", "OPTIONS", "POST"},
            format : {"json", "form", "urlencoded", "text", "file"}]
Bodiless == {"GET", "HEAD", "OPTIONS"}
Idempotent == {"GET", "HEAD", "OPTIONS", "PUT", "DELETE"}
FileBodies == {"form", "file"}
Kinds == {"valueType", "list", "nullable"}
Bodies == {"json", "form"}
Hosts == {"net10", "net48"}
Items == {"nullableItem", "valueItem", "objectItem"}

VARIABLES part, config,
          checkPasses, loadingStops, guardOk, method, policy, outcomes, successRoute, reqRoutes, defRoutes,
          routeStops, finallyStops, chained, chainedWhen, takesDefaults, defaultLoadingStops,
          pc, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, afterRan,
          defaultLoadings, defaultFinallies, outerEntered, held, holds

scenario == <<checkPasses, loadingStops, guardOk, method, policy, outcomes, successRoute, reqRoutes, defRoutes,
              routeStops, finallyStops, chained, chainedWhen, takesDefaults, defaultLoadingStops>>
run == <<pc, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, afterRan,
         defaultLoadings, defaultFinallies, outerEntered, held, holds>>
layers == <<defaultLoadings, defaultFinallies, outerEntered, held, holds>>
vars == <<part, config, scenario, run>>

Unset == <<"unset", "unset">>
Success == <<"success", "ok">>
NoRoute == <<"none", "none">>

SuccessRoute == IF successRoute THEN Success ELSE NoRoute

IdleRun ==
    /\ attempt = 1
    /\ sends = 0
    /\ loadings = 0
    /\ finallies = 0
    /\ routes = 0
    /\ entered = FALSE
    /\ stopped = FALSE
    /\ final = "none"
    /\ routeRan = Unset
    /\ chainedRan = FALSE
    /\ afterRan = FALSE
    /\ defaultLoadings = 0
    /\ defaultFinallies = 0
    /\ outerEntered = FALSE
    /\ held = FALSE
    /\ holds = 0

PlacementInit ==
    /\ part = "placement"
    /\ config \in Configs
    /\ checkPasses = TRUE /\ loadingStops = FALSE /\ guardOk = TRUE /\ method = "GET"
    /\ policy = FALSE /\ outcomes = [i \in 1..MaxAttempts |-> "ok"] /\ successRoute = TRUE
    /\ reqRoutes = {} /\ defRoutes = {}
    /\ routeStops = FALSE /\ finallyStops = FALSE /\ chained = FALSE /\ chainedWhen = TRUE
    /\ takesDefaults = FALSE /\ defaultLoadingStops = FALSE
    /\ pc = "idle"
    /\ IdleRun

FlowInit ==
    /\ part = "flow"
    /\ config = CHOOSE c \in Configs : TRUE
    /\ checkPasses \in BOOLEAN
    /\ loadingStops \in BOOLEAN
    /\ guardOk \in BOOLEAN
    /\ method \in {"GET", "HEAD", "POST", "PUT"}
    /\ policy \in BOOLEAN
    /\ outcomes \in [1..MaxAttempts -> Outcomes]
    /\ successRoute \in BOOLEAN
    /\ reqRoutes \in SUBSET {"s404", "s503", "catchAll"}
    /\ takesDefaults \in BOOLEAN
    /\ defRoutes \in IF takesDefaults THEN SUBSET {"s503", "catchAll"} ELSE {{}}
    /\ defaultLoadingStops \in IF takesDefaults THEN BOOLEAN ELSE {FALSE}
    /\ routeStops \in BOOLEAN
    /\ finallyStops \in BOOLEAN
    /\ chained \in BOOLEAN
    /\ chainedWhen \in BOOLEAN
    /\ pc = "check"
    /\ IdleRun

Init == PlacementInit \/ FlowInit

Refused(c) ==
    \/ c.placeholder = "unfilled" /\ Mutant # "PlaceholderUnchecked"
    \/ c.placeholder = "filled" /\ c.filler # "scalar" /\ Mutant # "DateInPath"
    \/ c.file = "set" /\ (c.method \in Bodiless \/ c.format \notin FileBodies) /\ Mutant # "FileInJson"

Built(c) == ~Refused(c)

Sent(kind, body) ==
    IF kind = "valueType" /\ body = "json" /\ Mutant = "NullForValueType" THEN "null"
    ELSE IF kind \in {"valueType", "list"} THEN "leftOut"
    ELSE IF body = "json" THEN "null" ELSE "emptyField"

Bound(kind, body, sent) ==
    CASE sent = "leftOut" -> "start"
      [] kind = "valueType" /\ sent = "null" -> "noInstance"
      [] kind = "valueType" /\ sent = "emptyField" -> "modelError"
      [] OTHER -> "null"

SentWhenItsConditionFails(kind, body) ==
    IF Mutant = "FailedConditionSendsNothing" THEN Sent(kind, body) ELSE "leftOut"

Expected(kind) == IF kind = "nullable" THEN "null" ELSE "start"

ObjectSent(body) ==
    IF body = "json" THEN "null"
    ELSE IF Mutant = "EmptyFieldForObject" THEN "emptyField"
    ELSE "leftOut"

ObjectBound(sent, host) ==
    CASE sent = "leftOut" -> "start"
      [] sent = "emptyField" /\ host = "net10" -> "start"
      [] OTHER -> "null"

ObjectExpected(body) == IF body = "json" THEN "null" ELSE "start"

ItemSent(item) ==
    CASE item = "nullableItem" -> "emptyField"
      [] item = "objectItem" /\ Mutant = "EmptyFieldForObjectItem" -> "emptyField"
      [] OTHER -> "stop"

ItemBound(item, sent, host) ==
    CASE sent = "stop" -> "noRequest"
      [] item = "nullableItem" -> "nullItem"
      [] item = "objectItem" /\ host = "net10" -> "listEnds"
      [] item = "objectItem" -> "nullItem"
      [] OTHER -> "modelError"

ItemExpected(item) == IF item = "nullableItem" THEN "nullItem" ELSE "noRequest"

CatchAll ==
    IF "catchAll" \in reqRoutes THEN <<"request", "catchAll">>
    ELSE IF "catchAll" \in defRoutes THEN <<"default", "catchAll">>
    ELSE NoRoute

ForStatus(o) ==
    IF o \in reqRoutes THEN <<"request", o>>
    ELSE IF o \in defRoutes THEN <<"default", o>>
    ELSE CatchAll

CatchAllFirst(o) ==
    IF o \in reqRoutes THEN <<"request", o>>
    ELSE IF "catchAll" \in reqRoutes THEN <<"request", "catchAll">>
    ELSE ForStatus(o)

RouteFor(o) ==
    CASE o = "ok" -> IF ~successRoute /\ Mutant = "CatchAllRunsOnSuccess" THEN CatchAll ELSE SuccessRoute
      [] o \in Statuses -> IF Mutant = "CatchAllBeatsStatus" THEN CatchAllFirst(o) ELSE ForStatus(o)
      [] OTHER -> IF Mutant = "FailureRunsSuccess" /\ o = "mismatch" THEN Success ELSE CatchAll

Index(i) == ((i - 1) % MaxAttempts) + 1

CanRetry == policy /\ (method \in Idempotent \/ Mutant = "RetryAnyPost")

Limit == MaxAttempts + (IF Mutant = "RetryPastAttempts" THEN 1 ELSE 0)

Check ==
    /\ pc = "check"
    /\ IF checkPasses \/ Mutant = "FailedCheckSends"
         THEN (pc' = IF takesDefaults THEN "defaultLoading" ELSE "loading") /\ UNCHANGED stopped
         ELSE pc' = "done" /\ stopped' = TRUE
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, final, routeRan, chainedRan, afterRan, layers>>

DefaultLoading ==
    /\ pc = "defaultLoading"
    /\ defaultLoadings' = defaultLoadings + 1
    /\ held' = (Mutant = "HoldBeforeLoading")
    /\ holds' = holds + (IF Mutant = "HoldBeforeLoading" THEN 1 ELSE 0)
    /\ IF defaultLoadingStops
         THEN stopped' = TRUE /\ pc' = "done" /\ UNCHANGED outerEntered
         ELSE outerEntered' = TRUE /\ pc' = "loading" /\ UNCHANGED stopped
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, final, routeRan, chainedRan, afterRan, defaultFinallies>>

Loading ==
    /\ pc = "loading"
    /\ loadings' = loadings + 1
    /\ IF loadingStops
         THEN stopped' = TRUE /\ pc' = IF Mutant = "FinallyWithoutLoading" THEN "finally" ELSE "defaultFinally"
         ELSE pc' = "guard" /\ UNCHANGED stopped
    /\ UNCHANGED <<part, config, scenario, attempt, sends, finallies, routes, entered, final, routeRan, chainedRan, afterRan, layers>>

Guard ==
    /\ pc = "guard"
    /\ entered' = TRUE
    /\ LET holding == takesDefaults /\ (guardOk \/ Mutant = "HoldOnAFailedGuard") IN
         /\ held' = (held \/ holding)
         /\ holds' = holds + (IF holding /\ ~held THEN 1 ELSE 0)
    /\ IF guardOk
         THEN pc' = "attempt" /\ UNCHANGED stopped
         ELSE pc' = "finally" /\ stopped' = TRUE
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, final, routeRan, chainedRan, afterRan,
                   defaultLoadings, defaultFinallies, outerEntered>>

Attempt ==
    /\ pc = "attempt"
    /\ sends' = sends + 1
    /\ LET o == outcomes[Index(attempt)] IN
         IF CanRetry /\ o \in Retryable /\ attempt < Limit
           THEN /\ attempt' = attempt + 1
                /\ loadings' = IF Mutant = "LoadingEachAttempt" THEN loadings + 1 ELSE loadings
                /\ UNCHANGED <<pc, final>>
           ELSE /\ final' = o
                /\ pc' = "route"
                /\ UNCHANGED <<attempt, loadings>>
    /\ UNCHANGED <<part, config, scenario, finallies, routes, entered, stopped, routeRan, chainedRan, afterRan, layers>>

Route ==
    /\ pc = "route"
    /\ routes' = routes + 1
    /\ routeRan' = RouteFor(final)
    /\ stopped' = (routeStops /\ RouteFor(final) # NoRoute)
    /\ pc' = "finally"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, entered, final, chainedRan, afterRan, layers>>

Finally ==
    /\ pc = "finally"
    /\ finallies' = finallies + 1
    /\ stopped' = (stopped \/ finallyStops)
    /\ pc' = "defaultFinally"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, routes, entered, final, routeRan, chainedRan, afterRan, layers>>

OuterRuns == IF Mutant = "OneFlagForBothLayers" THEN entered ELSE outerEntered

DefaultFinally ==
    /\ pc = "defaultFinally"
    /\ defaultFinallies' = defaultFinallies + (IF OuterRuns THEN 1 ELSE 0)
    /\ held' = FALSE
    /\ pc' = IF stopped /\ Mutant # "StepsAfterStop" THEN "done"
             ELSE IF chained /\ ~stopped /\ (final = "ok" \/ Mutant = "ChainedAfterError")
                     /\ (chainedWhen \/ Mutant = "ChainedIgnoresCondition") THEN "chained"
             ELSE "after"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, afterRan,
                   defaultLoadings, outerEntered, holds>>

Chained ==
    /\ pc = "chained"
    /\ chainedRan' = TRUE
    /\ pc' = "after"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, afterRan, layers>>

After ==
    /\ pc = "after"
    /\ afterRan' = TRUE
    /\ pc' = "done"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, layers>>

Next == Check \/ DefaultLoading \/ Loading \/ Guard \/ Attempt \/ Route \/ Finally \/ DefaultFinally \/ Chained \/ After

Spec == Init /\ [][Next]_vars

Placement == part = "placement"
Flow == part = "flow"

EveryPlaceholderFilled == (Placement /\ Built(config)) => config.placeholder = "filled"

PathHoldsScalarText == (Placement /\ Built(config) /\ config.placeholder = "filled") => config.filler = "scalar"

FileOnlyInFormData == (Placement /\ Built(config) /\ config.file = "set") => (config.method \notin Bodiless /\ config.format \in FileBodies)

NothingBindsAsDeclared == Placement => \A kind \in Kinds, body \in Bodies : Bound(kind, body, Sent(kind, body)) = Expected(kind)

ObjectNothingBindsAsDeclared ==
    Placement => \A body \in Bodies, host \in Hosts : ObjectBound(ObjectSent(body), host) = ObjectExpected(body)

ItemNothingBindsAsDeclared ==
    Placement => \A item \in Items, host \in Hosts : ItemBound(item, ItemSent(item), host) = ItemExpected(item)

FailedConditionLeavesThePartOut ==
    Placement => \A kind \in Kinds, body \in Bodies : Bound(kind, body, SentWhenItsConditionFails(kind, body)) = "start"

ChainedOnlyWhenItsConditionHolds == chainedRan => chainedWhen

FailedCheckSendsNothing == (Flow /\ ~checkPasses) => (sends = 0 /\ loadings = 0 /\ finallies = 0 /\ defaultLoadings = 0 /\ ~afterRan)

FinallyRunsWhenLoadingRan == (Flow /\ pc = "done") => ((finallies = 1) <=> entered)

EachLayerPairs == (Flow /\ pc = "done") => ((defaultFinallies = 1) <=> outerEntered)

HoldSpansTheSend == (held => entered) /\ ((Flow /\ pc = "done") => ~held) /\ holds <= 1

FailedGuardHoldsNothing == (Flow /\ ~guardOk) => holds = 0

OneRouteRuns == (Flow /\ pc = "done" /\ final \in {"ok"} \cup Statuses) =>
    (routes = 1 /\ routeRan = IF final = "ok" THEN SuccessRoute ELSE ForStatus(final))

SuccessWithNoRouteRunsNone == (Flow /\ final = "ok" /\ ~successRoute /\ routes = 1) => routeRan = NoRoute

FailureRunsTheCatchAll == (Flow /\ pc = "done" /\ final \in Failures) => (routes = 1 /\ routeRan = CatchAll)

ChainedOnlyAfterSuccess == chainedRan => (final = "ok" /\ routeRan = SuccessRoute /\ finallies = 1)

StepsAfterRunUnlessStopped == (Flow /\ pc = "done") => (afterRan <=> ~stopped)

RetriesOnlyWhatIsRetryable ==
    sends > 1 => (policy /\ method \in Idempotent /\ \A i \in 1..(sends - 1) : outcomes[Index(i)] \in Retryable)

RetryStopsAtItsAttempts == sends <= MaxAttempts

OneLifecycleAcrossAttempts == loadings <= 1 /\ finallies <= 1 /\ routes <= 1 /\ defaultLoadings <= 1 /\ defaultFinallies <= 1

ReachRetriedThenSucceeded == ~(pc = "done" /\ sends = 2 /\ final = "ok" /\ routeRan = Success)

ReachDefaultStatusRoute == ~(pc = "done" /\ routeRan = <<"default", "s503">>)

ReachChained == ~(pc = "done" /\ chainedRan)

ReachRequestLoadingStoppedUnderDefaults == ~(pc = "done" /\ outerEntered /\ ~entered /\ defaultFinallies = 1 /\ ~held)

ReachSucceededPastACatchAll == ~(pc = "done" /\ final = "ok" /\ ~successRoute /\ "catchAll" \in reqRoutes /\ routeRan = NoRoute)

====
