---- MODULE Request ----
EXTENDS Naturals, Sequences

CONSTANT Mutant

Mutants == {"none", "PlaceholderUnchecked", "DateInPath", "FileInJson", "NullForValueType",
            "FailedCheckSends", "FinallyWithoutLoading", "CatchAllBeatsStatus", "FailureRunsSuccess",
            "ChainedAfterError", "StepsAfterStop", "RetryAnyPost", "RetryPastAttempts", "LoadingEachAttempt",
            "FailedConditionSendsNothing", "ChainedIgnoresCondition"}

ASSUME Mutant \in Mutants

MaxAttempts == 2
Outcomes == {"ok", "s404", "s503", "network", "offOrigin", "mismatch"}
Statuses == {"s404", "s503"}
Failures == {"network", "offOrigin", "mismatch"}
Retryable == {"network", "s503"}

Configs == [placeholder : {"filled", "unfilled"}, filler : {"scalar", "date", "list"},
            file : {"none", "set"}, method : {"GET", "POST"}, format : {"json", "form"}]
Kinds == {"valueType", "list", "nullable"}
Bodies == {"json", "form"}

VARIABLES part, config,
          checkPasses, loadingStops, guardOk, method, repeatable, policy, outcomes, reqRoutes, defRoutes,
          routeStops, chained, chainedWhen,
          pc, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, afterRan

scenario == <<checkPasses, loadingStops, guardOk, method, repeatable, policy, outcomes, reqRoutes, defRoutes,
              routeStops, chained, chainedWhen>>
run == <<pc, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan, afterRan>>
vars == <<part, config, scenario, run>>

Unset == <<"unset", "unset">>
Success == <<"success", "ok">>
NoRoute == <<"none", "none">>

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

PlacementInit ==
    /\ part = "placement"
    /\ config \in Configs
    /\ checkPasses = TRUE /\ loadingStops = FALSE /\ guardOk = TRUE /\ method = "GET" /\ repeatable = FALSE
    /\ policy = FALSE /\ outcomes = [i \in 1..MaxAttempts |-> "ok"] /\ reqRoutes = {} /\ defRoutes = {}
    /\ routeStops = FALSE /\ chained = FALSE /\ chainedWhen = TRUE
    /\ pc = "idle"
    /\ IdleRun

FlowInit ==
    /\ part = "flow"
    /\ config = CHOOSE c \in Configs : TRUE
    /\ checkPasses \in BOOLEAN
    /\ loadingStops \in BOOLEAN
    /\ guardOk \in BOOLEAN
    /\ method \in {"GET", "POST"}
    /\ repeatable \in BOOLEAN
    /\ policy \in BOOLEAN
    /\ outcomes \in [1..MaxAttempts -> Outcomes]
    /\ reqRoutes \in SUBSET {"s404", "s503", "catchAll"}
    /\ defRoutes \in SUBSET {"s503", "catchAll"}
    /\ routeStops \in BOOLEAN
    /\ chained \in BOOLEAN
    /\ chainedWhen \in BOOLEAN
    /\ pc = "check"
    /\ IdleRun

Init == PlacementInit \/ FlowInit

Refused(c) ==
    \/ c.placeholder = "unfilled" /\ Mutant # "PlaceholderUnchecked"
    \/ c.placeholder = "filled" /\ c.filler # "scalar" /\ Mutant # "DateInPath"
    \/ c.file = "set" /\ (c.method = "GET" \/ c.format = "json") /\ Mutant # "FileInJson"

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
    CASE o = "ok" -> Success
      [] o \in Statuses -> IF Mutant = "CatchAllBeatsStatus" THEN CatchAllFirst(o) ELSE ForStatus(o)
      [] OTHER -> IF Mutant = "FailureRunsSuccess" /\ o = "mismatch" THEN Success ELSE CatchAll

Index(i) == ((i - 1) % MaxAttempts) + 1

CanRetry == policy /\ (method = "GET" \/ repeatable \/ Mutant = "RetryAnyPost")

Limit == MaxAttempts + (IF Mutant = "RetryPastAttempts" THEN 1 ELSE 0)

Check ==
    /\ pc = "check"
    /\ IF checkPasses \/ Mutant = "FailedCheckSends"
         THEN pc' = "loading" /\ UNCHANGED stopped
         ELSE pc' = "done" /\ stopped' = TRUE
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, final, routeRan, chainedRan, afterRan>>

Loading ==
    /\ pc = "loading"
    /\ loadings' = loadings + 1
    /\ IF loadingStops
         THEN stopped' = TRUE /\ pc' = IF Mutant = "FinallyWithoutLoading" THEN "finally" ELSE "done"
         ELSE pc' = "guard" /\ UNCHANGED stopped
    /\ UNCHANGED <<part, config, scenario, attempt, sends, finallies, routes, entered, final, routeRan, chainedRan, afterRan>>

Guard ==
    /\ pc = "guard"
    /\ entered' = TRUE
    /\ IF guardOk
         THEN pc' = "attempt" /\ UNCHANGED stopped
         ELSE pc' = "finally" /\ stopped' = TRUE
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, final, routeRan, chainedRan, afterRan>>

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
    /\ UNCHANGED <<part, config, scenario, finallies, routes, entered, stopped, routeRan, chainedRan, afterRan>>

Route ==
    /\ pc = "route"
    /\ routes' = routes + 1
    /\ routeRan' = RouteFor(final)
    /\ stopped' = (routeStops /\ RouteFor(final) # NoRoute)
    /\ pc' = "finally"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, entered, final, chainedRan, afterRan>>

Finally ==
    /\ pc = "finally"
    /\ finallies' = finallies + 1
    /\ pc' = IF stopped /\ Mutant # "StepsAfterStop" THEN "done"
             ELSE IF chained /\ ~stopped /\ (final = "ok" \/ Mutant = "ChainedAfterError")
                     /\ (chainedWhen \/ Mutant = "ChainedIgnoresCondition") THEN "chained"
             ELSE "after"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, routes, entered, stopped, final, routeRan, chainedRan, afterRan>>

Chained ==
    /\ pc = "chained"
    /\ chainedRan' = TRUE
    /\ pc' = "after"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, afterRan>>

After ==
    /\ pc = "after"
    /\ afterRan' = TRUE
    /\ pc' = "done"
    /\ UNCHANGED <<part, config, scenario, attempt, sends, loadings, finallies, routes, entered, stopped, final, routeRan, chainedRan>>

Next == Check \/ Loading \/ Guard \/ Attempt \/ Route \/ Finally \/ Chained \/ After

Spec == Init /\ [][Next]_vars

Placement == part = "placement"
Flow == part = "flow"

EveryPlaceholderFilled == (Placement /\ Built(config)) => config.placeholder = "filled"

RouteHoldsScalarText == (Placement /\ Built(config) /\ config.placeholder = "filled") => config.filler = "scalar"

FileOnlyInFormData == (Placement /\ Built(config) /\ config.file = "set") => (config.method # "GET" /\ config.format = "form")

NothingBindsAsDeclared == Placement => \A kind \in Kinds, body \in Bodies : Bound(kind, body, Sent(kind, body)) = Expected(kind)

FailedConditionLeavesThePartOut ==
    Placement => \A kind \in Kinds, body \in Bodies : Bound(kind, body, SentWhenItsConditionFails(kind, body)) = "start"

ChainedOnlyWhenItsConditionHolds == chainedRan => chainedWhen

FailedCheckSendsNothing == (Flow /\ ~checkPasses) => (sends = 0 /\ loadings = 0 /\ finallies = 0 /\ ~afterRan)

FinallyRunsWhenLoadingRan == (Flow /\ pc = "done") => ((finallies = 1) <=> entered)

OneRouteRuns == (Flow /\ pc = "done" /\ final \in {"ok"} \cup Statuses) =>
    (routes = 1 /\ routeRan = IF final = "ok" THEN Success ELSE ForStatus(final))

FailureRunsTheCatchAll == (Flow /\ pc = "done" /\ final \in Failures) => (routes = 1 /\ routeRan = CatchAll)

ChainedOnlyAfterSuccess == chainedRan => (final = "ok" /\ routeRan = Success /\ finallies = 1)

StepsAfterRunUnlessStopped == (Flow /\ pc = "done") => (afterRan <=> ~stopped)

RetriesOnlyWhatIsRetryable ==
    sends > 1 => (policy /\ (method = "GET" \/ repeatable) /\ \A i \in 1..(sends - 1) : outcomes[Index(i)] \in Retryable)

RetryStopsAtItsAttempts == sends <= MaxAttempts

OneLifecycleAcrossAttempts == loadings <= 1 /\ finallies <= 1 /\ routes <= 1

ReachRetriedThenSucceeded == ~(pc = "done" /\ sends = 2 /\ final = "ok" /\ routeRan = Success)

ReachDefaultStatusRoute == ~(pc = "done" /\ routeRan = <<"default", "s503">>)

ReachChained == ~(pc = "done" /\ chainedRan)

====
