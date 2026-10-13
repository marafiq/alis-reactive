> Lowering of `conditions.md` into the grammar, read at `195d3a1`. The verdicts were recorded before [`../grammar.md`](../grammar.md) settled the gaps and ambiguities; that file's sections 8 and 9 give the decisions and the final counts. Each table was produced by one reader against the code and spot-checked.
>
> References re-anchored at `f36f62e`. The runtime the verdicts call "today's" was replaced by the grammar interpreter, so each rule now cites the code that keeps it: the interpreter (paths relative to `Alis.Reactive.Assets/runtime/`) or the C# lowering (`Alis.Reactive/Grammar/Lowering/`); where the rule changed, the row says how. Use-case names use the DSL at `f36f62e`. On the C# side, a component member, an event payload member and a plugin member are now read, written and called through the object's binding (`ComponentBinding`, `EventPayloadBinding`, `PluginBinding`, Alis.Reactive/Components/Contracts/ObjectBinding.cs:40-84, 88-109, 112-159), which emits the same plan nodes as before: the plan JSON did not change, so every sentence below stands.

# Lowering: conditions, arrays, dispatch, elements, plugins (165 rows)

Read at `feature/component-redesign`, HEAD `195d3a1`, clean tree. Inventory: `docs/design/use-cases/conditions.md`.
Grammar and vocabulary: `docs/design/architecture.md` §0, §4.0–4.9, §6. Semantics at `f36f62e`: `Alis.Reactive.Assets/runtime/`
(`grammar/operators.ts`, `grammar/shape.ts`, `grammar/evaluate.ts`, `grammar/execute.ts`, `grammar/paths.ts`,
`modules/document.ts`, `objects/*.ts`, `plans/start.ts`), `Alis.Reactive/Grammar/Lowering/**` and `Alis.Reactive/PlanAuthoring/**`
(at `195d3a1`: `conditions/compare-engine.ts`, `conditions/conditions.ts`, `values/array-op-engine.ts`, `values/evaluate.ts`,
`shared/shape-convert.ts`, `browser-objects/runtime-*.ts` and `plugins/catalog.ts`, all deleted since).

## Reading assumption A1

Every verdict assumes **A1: a `get`, `call` or `lit` node converts its value by the node's shape exactly as today's
read did** (at `f36f62e` the interpreter does exactly this: `shaped`, grammar/shape.ts:14-18, called for `lit`, `get` and
`call` at grammar/evaluate.ts:29,32,61,63). Missing (`null`/`undefined`) passes through, except into a nullable shape,
which reads it as `null`; a failed conversion keeps the original value. architecture.md does not say this: §0.3 law 3 says nodes carry shape, §6 says "conversions are explicit
nodes". If A1 is rejected, the compiler must emit the conversion ops below wherever a JS value can differ in type from
the C# type, and every EXPRESSED row that reads such a value becomes OBLIGATION. Sentences show `toNumber`/`toTime`
explicitly where the source is always text (URL, JSON dates).

## Obligation codes (exact rules in the operator table at the end)

| Code | Owner | Rule (today's source) |
|---|---|---|
| OP-EQ | op `==` `!=` | both missing → equal; one missing → unequal; two dates by their time; else `===` (grammar/operators.ts:9-13; grammar/shape.ts:10-12) |
| OP-ORD | op `>` `>=` `<` `<=` | true only for two finite numbers (a date counts as its time), two strings or two booleans; any other pair (null, undefined, NaN, ±Infinity, mixed kinds) → false for all four (grammar/operators.ts:15-58) |
| OP-EMPTY | op `empty` | `""`, null, undefined, `[]` (Array.isArray) only; no trim; 0/false/" " not empty (grammar/operators.ts:88-91) |
| OP-TEXT | `call` of includes/startsWith/endsWith, `get .length` | missing receiver → false (no throw); the lowering converts the receiver with toText first (Alis.Reactive/Grammar/Lowering/ValueLowering.cs:385-391, 452-453), and a call on a missing receiver gives `undefined` (grammar/paths.ts:63-69) |
| OP-MATCHES | op `matches` | missing → false; `new RegExp(p, flags).test(text)`, and a condition passes no flags (grammar/operators.ts:93-99; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:388-389) |
| CV-NUM / CV-TIME / CV-BOOL / CV-TEXT | conversions | grammar/shape.ts:62-72 / 98-109 / 74-82 / 84-96 (the date shape now reads a JS `Date`, compared by its time) |
| CV-ARRSHAPE | conversion at an array-shaped read | `""` → `[]`, scalar → `[v]`, object kept, missing kept (grammar/shape.ts:156-162) |
| CV-ARR | `toArray` before an array method | missing → `[]`; number/string → `[v]`; iterable (DOMTokenList, HTMLCollection) → Array.from; else throw (grammar/operators.ts:121-132) |
| CV-SUM (NEW op) | Sum's per-item coercion | `Number(v)` if finite else 0 (grammar/operators.ts:134-138; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:256-266); plain `+` would concatenate text |
| OP-ORDER (NEW op) | OrderBy comparator | `order` (grammar/operators.ts:140-161; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:280-289); not reproducible with OP-ORD |
| FIND-NULL | Find | no match → `null`; lowered `find(p) ?? null` (Alis.Reactive/Grammar/Lowering/ValueLowering.cs:268-278) |
| DOC-RAISE | `document.raise` | sync `document.dispatchEvent(new CustomEvent(name,{detail}))`, non-bubbling, detail `{}` when none (modules/document.ts:26-29); listener gets `detail`, or the Event when detail is null (modules/document.ts:93-96) |
| DOC-READY | `document` `ready` | every handler of the owner subscribes first; the owner then starts and runs its ready handlers in authored order (plans/start.ts:18-24; modules/document.ts:87-90; objects/owner.ts:43-53), synchronously if the DOM is parsed, else on DOMContentLoaded (root.ts:40-44) |
| DOC-QUERY | `document.query` | at `195d3a1` `URLSearchParams.get`: case-sensitive key, first value, `null` when absent; at `f36f62e` keys match case-insensitively, a scalar member reads the first value, an array member every value (`queryAll`), and an absent key reads as missing (modules/document.ts:31-62), as architecture §0.4 and §4.6 asked |
| HANDLER-ISO | interpreter | a failing handler is logged and stops no other handler (plans/start.ts:45-58) |
| CONFIRM | `confirm.ask` | Promise<boolean>; OK → true, Cancel/close/Escape → false; asks queued FIFO; missing dialog → throws naming FusionConfirmDialog (`Alis.Reactive.Assets/components/fusion/confirm.ts:4-7, 50-71, 96-98, 101-105`; the guard lowers to `let ok = await confirm.ask(..)`, Alis.Reactive/Grammar/Lowering/ReactionLowering.cs:180-186) |
| BUILDER-CONFIRM | C# builder | a Confirm term inside All/Any/Not/ElseIf is asked only when reached, left to right: the lowering nests `let await` + `if`, duplicating the continuation (Alis.Reactive/Grammar/Lowering/ReactionLowering.cs:138-199) |
| PLUGIN | `plugin(name)` | registry fed from `window.__alisPlugins` before boot (root.ts:48-49); unknown name throws `plugin not found` (objects/modules.ts:36-40); member call `this` = owner, root call `this` undefined (grammar/paths.ts:57-60, 73); absent property throws (grammar/paths.ts:27-28). In C#, a plugin member reaches the plan through `PluginBinding` (Alis.Reactive/Components/Contracts/ObjectBinding.cs:112-159) |
| WRITE-SHAPE | `set`/`call` on an object member | value converted by the member's shape, dates delivered as JS `Date` (grammar/execute.ts:40-45; grammar/evaluate.ts:56-58, 71-73; grammar/shape.ts:98-109) |
| ELEM | `element(id, dom)` | resolved by getElementById at each use, absent → throws (objects/world.ts:14-18, 52-55) |
| ALL | `all(block+)` | every branch settles; rejections logged, not rethrown; completion runs (grammar/execute.ts:73-91) |
| HTTP | `http.send` | per `use-cases/http.md` lowering (architecture §0.5) |

## Table

| # | Use case (short) | Vocabulary used | Sentence in the grammar | Verdict |
|---|---|---|---|---|
| 1 | CustomEvent<T> payload as source | payload, document | `if (e.score >= 90) grade.textContent = "A"; else if (e.score >= 80) grade.textContent = "B"; else grade.textContent = "F"` | OBLIGATION: OP-ORD |
| 2 | Nested payload path | payload | `if (e.address.city === "Seattle") r.textContent = "Found Seattle"; else r.textContent = "Other City"` | EXPRESSED (missing link → undefined, §4.3:485) |
| 3 | Null object / null leaf / missing key → Else, no crash | payload | `if (e.address == null) …; if (e.address.city === "Seattle") … else …` | OBLIGATION: OP-EQ (missing key is undefined; `IsNull` lowers to `== null`, which holds for undefined too: Alis.Reactive/Grammar/Lowering/ValueLowering.cs:377, grammar/operators.ts:10) |
| 4 | Fusion NumericTextBox `args.value` | component payload | `if (args.value >= 90) {…}` | OBLIGATION: OP-ORD (value is null after clearing) |
| 5 | NativeTextBox `args.value` Contains | component payload, element | `if (args.value.includes("VIP")) badge.hidden = false; else badge.hidden = true` | OBLIGATION: OP-TEXT |
| 6 | Success body as source | http, response | `if (res.ok) { if (res.body.status === "approved") … }` | EXPRESSED |
| 7 | Typed error body inside OnError<T> | http, response | `if (res.ok) r.textContent = "success"; else { if (res.body.code === 422) r.textContent = "validation error"; else r.textContent = "other error" }` | EXPRESSED |
| 8 | Trigger payload inside OnSuccess | payload, http | `const res = await http.send(q); if (res.ok) { st.textContent = "saved"; if (e.active) b.hidden = false; else b.hidden = true }` | EXPRESSED (law 2) |
| 9 | Trigger payload inside OnError(400) | payload, http | `… else if (res.status === 400) { if (e.category === "required") m.textContent = "missing required fields"; else m.textContent = "validation error" }` | EXPRESSED |
| 10 | Component property as source | component | `if (hr.value > 100) {…}` | OBLIGATION: OP-ORD |
| 11 | Component bool property Eq(true) | component | `if (action.disabled === true) readiness.textContent = "This visit is locked…"` | EXPRESSED |
| 12 | Component returning method as source | component | `if (companion.getSelectedValue() === "Shared Companion Suite") …` | EXPRESSED |
| 13 | Component array method NotEmpty; CurrentView Eq | component | `if (!empty(schedule.getEvents()) …)`; `schedule.currentView === "Week"` | OBLIGATION: OP-EMPTY |
| 14 | URL text Eq | document.query | `if (document.query.tab === "medications") meds.hidden = false; else meds.hidden = true` | OBLIGATION: DOC-QUERY |
| 15 | Typed URL Gt | document.query | `if (toNumber(document.query.page) > 1) prev.hidden = false; else prev.hidden = true` | OBLIGATION: DOC-QUERY, CV-NUM (`""` → 0, `"abc"` kept → false), OP-ORD |
| 16 | URL as right operand | document.query, payload | `e.score >= toNumber(document.query.minScore)` | OBLIGATION: DOC-QUERY, CV-NUM, OP-ORD |
| 17 | Plugin function result Truthy | plugin | `if (array.some(res.body.items, "status", "critical")) has.hidden = false; else none.hidden = false` | OBLIGATION: PLUGIN |
| 18 | Plugin function vs plugin property | plugin | `tools.normalizeStatus(e.status) == tools.expectedStatus` | OBLIGATION: OP-EQ (both missing → true), PLUGIN (absent property throws) |
| 19 | Array aggregate as source | arrays, payload | `if (toArray(e.alerts).some(a => a.severity === "critical" && !a.acknowledged)) warn.hidden = false; else clear.hidden = false` | OBLIGATION: CV-ARR |
| 20 | Count/Sum/Find value as source | arrays | `toArray(x).length > n`; `toArray(x).reduce((t, r) => t + toFinite(r.n), 0) >= n`; `(toArray(x).find(p) ?? null) == v` | OBLIGATION: CV-ARR, CV-SUM, OP-ORD, FIND-NULL |
| 21 | Source vs source (component vs component) | component | `if (hr.value >= threshold.value) …` | OBLIGATION: OP-ORD |
| 22 | Source vs source Eq/NotEq/Gt/Lt/Lte | any | `a == b`, `a != b`, `a > b`, `a < b`, `a <= b` | OBLIGATION: OP-EQ (both missing equal), OP-ORD |
| 23 | Eq text payload | payload | `if (e.role === "Alice") … else …` | EXPRESSED |
| 24 | Eq text ElseIf ladder; branches set a component | component | `if (care.value === "Memory Care") protocol.value = "Enhanced Monitoring"; else if (care.value === "Skilled Nursing") …` | EXPRESSED |
| 25 | Eq int payload ladder | component payload | `if (args.selectedIndex === 0) s.textContent = "Care Schedule"; else if (args.selectedIndex === 1) …` | EXPRESSED |
| 26 | Eq bool component | component | as row 11 | EXPRESSED |
| 27 | Date Eq / Lt / Lte / Between | payload | `toTime(a) == toTime(d)`; `toTime(a) < toTime(d)`; `toTime(a) >= toTime(lo) && toTime(a) <= toTime(hi)` | OBLIGATION: CV-TIME, OP-EQ, OP-ORD |
| 28 | NotEq request type | component payload | `if (args.action.requestType !== "refresh") {…}` | EXPRESSED |
| 29 | Gt int at threshold | payload | `if (e.count > 0) badge.hidden = false; else badge.hidden = true` | OBLIGATION: OP-ORD |
| 30 | Gt long | payload | `if (e.balance > 1000000) …` | OBLIGATION: OP-ORD (beyond 2^53 imprecise, as today) |
| 31 | Gt double | payload | `if (e.temperature > 98.6) …` | OBLIGATION: OP-ORD |
| 32 | Gt decimal component, exact boundary | component | `if (bp.value > 140) {…} else {…}` | OBLIGATION: OP-ORD |
| 33 | Gt DateTime payload | payload | `if (toTime(e.deadline) > toTime("2026-06-01T00:00:00")) … else …` | OBLIGATION: CV-TIME (offset-less text = local time), OP-ORD |
| 34 | Gte int ladder | payload | as row 1 | OBLIGATION: OP-ORD |
| 35 | Gte decimal ladder at boundaries | component | `if (hr.value >= 120) z = "Critical"; else if (hr.value >= 100) z = "High"; else if (hr.value >= 60) z = "Normal"; else z = "Low"` | OBLIGATION: OP-ORD |
| 36 | Lt decimal ladder | component, NativeHiddenField | `if (cog.value < 15) {unit.value = "Memory Care"; …} else if (cog.value < 25) {…} else {…}` | OBLIGATION: OP-ORD. A cleared box reads null and must take Else; JS `null < 15` is true (pinned by WhenAdmissionAssessmentRoutesConditionsAcrossItsWorkflow.cs:126,131) |
| 37 | Lte decimal (And arm) at boundary | component | `if (bp.value >= 60 && bp.value <= 120) …` | OBLIGATION: OP-ORD (JS `null <= 120` is true) |
| 38 | Truthy bool payload | payload | `if (e.isActive) r.textContent = "Online"; else r.textContent = "Offline"` | EXPRESSED |
| 39 | Truthy bool component (Switch checked) | component | `override.checked` inside row 65 | EXPRESSED |
| 40 | Falsy | component | `hk.disabled = false; hk.dataBind(); if (!hk.disabled) status.textContent = "You can now…"` | EXPRESSED |
| 41 | IsNull (nullable int, object) | payload | `if (e.nullableScore == null) …`; `if (e.address == null) …` | OBLIGATION: OP-EQ |
| 42 | NotNull inside grouped And | payload | `if (e.id >= 1 && e.address.city != null) … else …` | OBLIGATION: OP-EQ, OP-ORD |
| 43 | IsEmpty text | payload | `if (empty(e.name)) r.textContent = "Empty"; else r.textContent = "Has value"` | OBLIGATION: OP-EMPTY (no trim) |
| 44 | NotEmpty text (response) | http, response | `if (!empty(res.body.mrn)) rec.textContent = "Chart linked…"; else rec.textContent = "No medical record yet…"` | OBLIGATION: OP-EMPTY |
| 45 | NotEmpty array (ListBox value) | component | `if (!empty(listBox.value)) state.textContent = "has values"; else state.textContent = "empty"` | OBLIGATION: OP-EMPTY (`[]` and null) |
| 46 | In text payload | payload | `if (["A", "B", "C"].includes(e.category)) …` | EXPRESSED (no null item; general form `[..].some(v => v == x)`) |
| 47 | In text component | component | `if (["Memory Care", "Skilled Nursing"].includes(care.value)) …` | EXPRESSED |
| 48 | NotIn | payload | `if (!["X", "Y"].includes(e.category)) …` | EXPRESSED |
| 49 | Between int | payload | `if (e.age >= 18 && e.age <= 65) …` | OBLIGATION: OP-ORD |
| 50 | Contains | payload, component payload | `if (e.name.includes("admin")) …` | OBLIGATION: OP-TEXT |
| 51 | StartsWith | payload | `if (e.email.startsWith("admin@")) …` | OBLIGATION: OP-TEXT |
| 52 | EndsWith | any text | `if (x.endsWith(s)) …` | OBLIGATION: OP-TEXT |
| 53 | Matches | payload | `if (matches(e.email, "^[a-z]+@[a-z]+\\.[a-z]+$")) …` | OBLIGATION: OP-MATCHES |
| 54 | MinLength | payload | `if (e.name.length >= 3) …` | OBLIGATION: OP-TEXT (toText first), OP-ORD (`undefined >= 3` false) |
| 55 | ArrayContains | any array | `toArray(a).some(v => v == item)` | OBLIGATION: CV-ARRSHAPE (non-array after shaping → false), OP-EQ, items shaped by element shape |
| 56 | Grouped And | payload | `if (e.score >= 90 && e.status === "active") … else …` | OBLIGATION: OP-ORD |
| 57 | Grouped Or | payload | `if (e.role === "admin" \|\| e.role === "superuser") … else …` | EXPRESSED |
| 58 | Direct And | payload | as row 56 | OBLIGATION: OP-ORD |
| 59 | Direct Or | payload | as row 57 | EXPRESSED |
| 60 | Direct Or inside an ElseIf arm | payload, document | `… else if (e.score >= 70 \|\| e.manualOverride) document.raise("route-expedited"); …` | OBLIGATION: OP-ORD, DOC-RAISE |
| 61 | And(TypedSource) on one component | component | as row 37 | OBLIGATION: OP-ORD |
| 62 | And(TypedSource) across components | component | `if (fall.value >= 7 && age.value >= 80) …` | OBLIGATION: OP-ORD |
| 63 | And(responseBody) | response | `if (res.body.status === "approved" && res.body.count > 0) …` | OBLIGATION: OP-ORD |
| 64 | And(args) inside OnSuccess | payload, response | `if (res.ok) { …; if (e.active && e.count > 5) q.hidden = false; else q.hidden = true }` | OBLIGATION: OP-ORD |
| 65 | Grouped And with nested Or | component, document | `if (assess.value >= 85 && (override.checked \|\| track.value === "memory")) document.raise("component-route-isolation"); …` | OBLIGATION: OP-ORD, DOC-RAISE |
| 66 | Not() on a guard | payload | `if (!(e.role === "admin")) … else …` | EXPRESSED |
| 67 | Not() inside grouped And | component | `… else if (assess.value >= 60 && !override.checked) document.raise("component-route-monitor"); else …` | OBLIGATION: OP-ORD, DOC-RAISE |
| 68 | Group terms reading URL / plugin / component method | document.query, plugin, component | `if (e.score >= toNumber(document.query.minScore) && sched.currentView === "Week" && (!empty(sched.getEvents()) \|\| tools.normalizeStatus(e.status) == tools.expectedStatus)) … else if (e.status === "manual") … else …` | OBLIGATION: DOC-QUERY, CV-NUM, OP-ORD, OP-EMPTY, OP-EQ, PLUGIN |
| 69 | Confirm inside a grouped condition | confirm | `if (a > 1) { const ok = await confirm.ask(m); if (ok) T; else E } else E` | OBLIGATION: BUILDER-CONFIRM, CONFIRM (law 1's spill into the active scope would ask before `a > 1`; see report) |
| 70 | Then / Else | payload | `if (e.role === "Alice") … else …` | EXPRESSED |
| 71 | ElseIf first match | payload | as row 1 (`if / else if` is first-match) | OBLIGATION: OP-ORD |
| 72 | ElseIf(TypedSource) ladder | component | as row 35 | OBLIGATION: OP-ORD |
| 73 | ElseIf(responseBody) | response | `if (res.body.status === "approved") l.textContent = "green"; else if (res.body.status === "pending") l.textContent = "yellow"; else l.textContent = "red"` | EXPRESSED |
| 74 | When/Then without Else; unguarded steps run | payload | `r.textContent = "Always runs"; if (e.score >= 90) bonus.textContent = "Bonus!"` | OBLIGATION: OP-ORD |
| 75 | Nested branch inside Then | payload | `if (e.score >= 90) { if (e.status === "active") … else … } else …` | OBLIGATION: OP-ORD |
| 76 | Confirm OK → Then, Cancel → Else | confirm | `const ok = await confirm.ask("Are you sure you want to proceed?"); if (ok) r.textContent = "Confirmed"; else r.textContent = "Cancelled"` | OBLIGATION: CONFIRM |
| 77 | Confirm gating an HTTP request | confirm, http, element | `const ok = await confirm.ask("…#42?"); if (ok) { spin.hidden = false; try { const res = await http.send({ method: "DELETE", url }); … } finally {…} }` | OBLIGATION: CONFIRM, HTTP |
| 78 | Confirm Cancel without Else aborts the request | confirm, http | as row 77, no else | OBLIGATION: CONFIRM (Cancel, close, Escape → false) |
| 79 | Sets before and after a branch run | element, document | `pre.textContent = "Review started"; document.raise("supported-trigger-audited"); if (e.score >= 90) {…} else if (e.score >= 70) {…} else {…}; sum.textContent = "Workflow complete"` | OBLIGATION: OP-ORD, DOC-RAISE |
| 80 | Set after branch runs after the branch's set | element | `proof.textContent = "before"; if (e.level === "high") proof.textContent = "conditional"; proof.textContent = "after"` | EXPRESSED |
| 81 | Dispatch authored before a branch | document | `document.raise("supported-trigger-audited"); if (…) …` | OBLIGATION: DOC-RAISE |
| 82 | Dispatch inside branches | document, payload | `if (e.score >= 95 && e.requiresIsolation) document.raise("route-isolation"); else if (e.score >= 70 \|\| e.manualOverride) document.raise("route-expedited"); else document.raise("route-routine")` | OBLIGATION: DOC-RAISE, OP-ORD |
| 83 | HTTP inside Then, endpoint per arm | component, http | `if (hr.value >= 180) { const res = await http.send({ method: "POST", url: critical, values: { heartRate: hr.value } }); if (res.ok) {…} } else if (hr.value >= 140) {…}` | OBLIGATION: OP-ORD, HTTP |
| 84 | Condition after an HTTP step | http, payload | `const res = await http.send({ method: "POST", url, values: { name: "Alice" } }); if (res.ok) s1.textContent = res.body.receivedName; if (e.active) b.hidden = false; else b.hidden = true` | EXPRESSED |
| 85 | HTTP between two conditions | http, payload | `if (e.active) …; const res = await http.send(q); if (res.ok) …; if (e.count > 0) … else …` | OBLIGATION: OP-ORD |
| 86 | set → cond → set → HTTP → cond → set | element, http | `h.textContent = "start"; if (e.active) pre.hidden = false; w.textContent = "please wait"; const res = await http.send(q); if (res.ok) …; if (e.count > 0) … else …; f.textContent = "done"` | OBLIGATION: OP-ORD |
| 87 | ElseIf ladder in OnSuccess plus outer condition | response, payload | `if (res.ok) { if (e.count > 100) … else if (e.count > 50) … else if (e.count > 10) … else … } if (e.active) trial.hidden = true; else trial.hidden = false` | OBLIGATION: OP-ORD |
| 88 | Ordered mix cond → HTTP → dispatch → plugin → cond (Then) | plugin, http, document | `seq.textContent = order("s12", "start"); if (e.active) seq.textContent = order("s12", "branch-before"); else …; const res = await http.send(q); if (res.ok) seq.textContent = order("s12", "http-success"); document.raise("s12-dispatched"); plug.textContent = slugify("After Http"); …; if (e.category === "premium") … else …` | OBLIGATION: DOC-RAISE (listener runs inside `raise`, before `slugify`), PLUGIN (root calls) |
| 89 | Same mix, Else branches | same | as row 88 | OBLIGATION: DOC-RAISE, PLUGIN |
| 90 | Ordered mix with Parallel + OnAllSettled (Then) | http, plugin, document | `…; await all({ const a = await http.send(A); if (a.ok) … }, { const b = await http.send(B); if (b.ok) … }); done.textContent = "settled"; …; document.raise("s13-dispatched"); …; if (e.count > 0) … else …` | OBLIGATION: ALL, DOC-RAISE, PLUGIN, OP-ORD |
| 91 | Same Parallel mix, Else branches | same | as row 90 | OBLIGATION: ALL, DOC-RAISE, PLUGIN, OP-ORD |
| 92 | Chained request + plugin route param + conditions | http, plugin, document | `const res = await http.send({ method: "POST", url: "/…/{residentId}", values: { residentId: e.count, action: e.category }, headers: { "X-Category": e.category } }); if (res.ok) {…}; if (res.ok) { const r2 = await http.send({ …, values: { residentId: e.count, categorySlug: slugify(e.category) } }); … }; document.raise("s14-dispatched"); plug.textContent = slugify(e.category); if (e.count > 300) … else …` | OBLIGATION: PLUGIN (called again at each use, as today), HTTP, DOC-RAISE, OP-ORD |
| 93 | From(response array) | arrays, response | `total.textContent = toArray(res.body.residents).length` | OBLIGATION: CV-ARR (null list → 0) |
| 94 | `p.From(args.Value)` (CheckList `string[]`) | arrays, component payload | `count.textContent = toArray(args.value).length` | OBLIGATION: CV-ARRSHAPE then CV-ARR (`""` → `[]` → 0; CV-ARR alone gives `[""]` → 1) |
| 95 | `p.From(payload.Get(x => x.Alerts))` | arrays, payload | `total.textContent = toArray(e.alerts).length` | OBLIGATION: CV-ARR |
| 96 | From(component property `Rows<T>()`) | arrays, component | `grid.dataSource = toArray(grid.dataSource).filter(x => x.status === "active").toSorted((a, b) => order(a.name, b.name)); grid.refresh()` | OBLIGATION: CV-ARR (non-iterable DataManager throws), OP-ORDER, WRITE-SHAPE |
| 97 | `p.Element(id).ClassList.Items` | arrays, element | `total.textContent = toArray(card.classList).length` | OBLIGATION: CV-ARR (DOMTokenList → Array.from) |
| 98 | `p.Element(id).Children` | arrays, element | `kids.textContent = toArray(list.children).length` | OBLIGATION: CV-ARR |
| 99 | `p.Element(id).Children` with per-element `GetAttribute` | arrays, element | `high.textContent = toArray(riskList.children).filter(x => x.getAttribute("data-risk") === "high").length` | GAP: the C# lambda calls a JsObject member on an element of `Children`. §6's table maps only `string` and `IEnumerable<T>` members (architecture.md:844-846) and `Children`'s element type is undeclared (:697). Today: allow-list `getAttribute`, `hasAttribute`, `getDay`…`getTime`, `toUpperCase`, `toLowerCase`, `trim` (ElementExpressionCompiler.cs:20-25, 140-154). At `f36f62e` the item type is declared: `ChildElement.GetAttribute` (Alis.Reactive/PlanAuthoring/Pipelines/HtmlElement.cs:133-147), and §6 maps declared objects too (architecture.md:844-846) |
| 100 | Where(x => x.Status == "active") | arrays | `toArray(rows).filter(x => x.status === "active")` | OBLIGATION: CV-ARR (predicate exact) |
| 101 | Re-read classList after `ClassList.Add` in one handler | element, arrays | `card.classList.add("risk-extra"); n.textContent = toArray(card.classList).filter(x => x.startsWith("risk-")).length` | OBLIGATION: CV-ARR, ELEM (fresh read, no caching) |
| 102 | Element predicate StartsWith | arrays | `x => x.startsWith("risk-")` | OBLIGATION: OP-TEXT |
| 103 | Element predicate Contains / EndsWith | arrays | `x => x.name.includes(s)`; `x => x.name.endsWith(s)` | OBLIGATION: OP-TEXT |
| 104 | Element predicate `&&` | arrays | `toArray(rows).filter(x => x.status === "active" && x.age >= 65).length` | OBLIGATION: CV-ARR, OP-ORD |
| 105 | Element predicate `>=` at boundary | arrays | as row 104 | OBLIGATION: OP-ORD |
| 106 | Element predicate `!a.Acknowledged` | arrays | `toArray(e.alerts).some(a => a.severity === "critical" && !a.acknowledged)` | OBLIGATION: CV-ARR (negation exact for booleans; CV-BOOL for text "false") |
| 107 | Element predicate `!=` `>` `<` `<=` `\|\|` | arrays | `x => x.a != v`; `x.a > v`; `x.a < v`; `x.a <= v`; `p \|\| q` | OBLIGATION: OP-EQ, OP-ORD |
| 108 | Select | arrays | `toArray(rows).map(x => x.name)` | OBLIGATION: CV-ARR |
| 109 | OrderBy | arrays, component | `grid.dataSource = toArray(res.body.residents).toSorted((a, b) => order(a.name, b.name))` | OBLIGATION: OP-ORDER (new op; lifted `<`/`>` would make null keys equal to every key) |
| 110 | OrderByDescending + Find(x => true, x => x.Name) | arrays | `old.textContent = toArray(rows).filter(x => x.status === "active").toSorted((a, b) => order(b.age, a.age)).find(x => true).name ?? null` | OBLIGATION: OP-ORDER (descending negates: non-finite first, ties stable), FIND-NULL |
| 111 | Count() | arrays | `toArray(card.classList).length` | OBLIGATION: CV-ARR |
| 112 | Count(predicate) | arrays | `toArray(rows).filter(x => x.status === "active").length` | OBLIGATION: CV-ARR |
| 113 | Any() without predicate | arrays | `toArray(x).length > 0` | OBLIGATION: CV-ARR |
| 114 | Any(predicate) | arrays | `toArray(x).some(p)` | OBLIGATION: CV-ARR |
| 115 | All(predicate) | arrays | `toArray(x).every(p)` | OBLIGATION: CV-ARR (empty → true, as C#) |
| 116 | Sum(int) | arrays | `toArray(rows).filter(x => x.status === "active").reduce((t, x) => t + toFinite(x.age), 0)` | OBLIGATION: CV-SUM (new conversion) |
| 117 | Sum(decimal / double) | arrays | as row 116 | OBLIGATION: CV-SUM; double addition, as today (not C# decimal) |
| 118 | Find(predicate) returning the element | arrays | `toArray(x).find(p) ?? null` | OBLIGATION: CV-ARR, FIND-NULL |
| 119 | Find(predicate, selector) | arrays | `toArray(x).find(p).name ?? null` | OBLIGATION: CV-ARR, FIND-NULL (a found element with an undefined field gives null, today undefined; both missing) |
| 120 | An array bound to a DataSource (`grid.DataSource.Set(roster.OrderBy(..))`) | arrays, component | `grid.dataSource = toArray(res.body.residents).toSorted((a, b) => order(a.name, b.name))` | OBLIGATION: OP-ORDER, WRITE-SHAPE |
| 121 | Array result as text | arrays, element | `total.textContent = toArray(res.body.residents).length` | OBLIGATION: CV-ARR |
| 122 | Array result in gather or dispatch | arrays, http, document | `document.raise("x", { count: toArray(r).length })`; `http.send({ …, headers: { "X-Count": toArray(r).length } })` | OBLIGATION: CV-ARR, DOC-RAISE, HTTP |
| 123 | DomReady | document, element | `on(document, "ready", e) { s1.classList.add("text-green-600"); s1.textContent = "dom-ready fired…"; document.raise("test") }` | OBLIGATION: DOC-READY (the "test" listener must already be subscribed), DOC-RAISE |
| 124 | Several DomReady handlers all run | document | two `on(document, "ready", e) {…}` handlers | OBLIGATION: DOC-READY, HANDLER-ISO |
| 125 | `Dispatch(appEvent)` → `CustomEvent(appEvent)`, no payload | document | `document.raise("test")`; `on(document, "test", e) { s2.classList.add(…); … }` | OBLIGATION: DOC-RAISE (detail `{}`) |
| 126 | `Dispatch(appEvent, payload)` → typed reads | document, payload | `document.raise("payload-test", { intValue: 42, longValue: …, address: {…} })`; `on(document, "payload-test", e) { iv.textContent = e.intValue; city.textContent = e.address.city; … }` | OBLIGATION: DOC-RAISE |
| 127 | Dispatch with a payload object (anonymous at `195d3a1`; the event's declared type at `f36f62e`) | document | `document.raise("supported-trigger-review", { score: 95 })` → listener `if (e.score >= 90) …` | OBLIGATION: DOC-RAISE, OP-ORD (the anonymous object must become the event's declared `T`, §4.7:631) |
| 128 | Two handlers on one event name | document | `on(document, "set-score", e) { sd.textContent = e.score }` and `on(document, "set-score", e) { if (e.score >= 90) … }` | OBLIGATION: DOC-RAISE (one subscription per handler, subscription order), HANDLER-ISO, OP-ORD |
| 129 | `Dispatch(appEvent, d => d.Set(..))`: component values, nested path, literal | document, component | `document.raise("transfer", { name: nameBox.value, status: "active", address: { city: cityBox.value } })` | OBLIGATION: DOC-RAISE |
| 130 | `Dispatch` payload from a returning method | document, component | `document.raise("filters-applied", { selection: chips.getSelectedChips() })` | OBLIGATION: DOC-RAISE |
| 131 | `Dispatch` payload from the response body in OnSuccess | document, response | `if (res.ok) document.raise("shift-report", { alerts: res.body.alerts })` | OBLIGATION: DOC-RAISE |
| 132 | `Dispatch` payload int / bool literals | document | `document.raise("x", { count: 3, active: true })` | OBLIGATION: DOC-RAISE |
| 133 | `Dispatch` payload from plugin sources | document, plugin | `document.raise("plugin-request-input", { slug: tools.slugify(e.filter), token: tools.token })` | OBLIGATION: DOC-RAISE, PLUGIN |
| 134 | `Text.Set` literal | element | `grade.textContent = "A"` | EXPRESSED |
| 135 | `Text.Set(payload.Get(..))` | element, payload | `iv.textContent = e.intValue` | EXPRESSED (DOM ToString equals today's toText for numbers, booleans, arrays; null/undefined → "") |
| 136 | `Text.Set(json.Get(..))` | element, response | `if (res.ok) s1.textContent = res.body.receivedName` | EXPRESSED |
| 137 | `Text.Set(component.Value)` | element, component | `lvl.textContent = care.value` | EXPRESSED |
| 138 | `Text.Set` from the URL | element, document.query | `tab.textContent = document.query.tab` | OBLIGATION: DOC-QUERY |
| 139 | `Text.Set` from a plugin | element, plugin | `total.textContent = array.count(res.body.items)` | OBLIGATION: PLUGIN |
| 140 | `Html.Set("")` clears markup | element | `container.innerHTML = ""` | EXPRESSED |
| 141 | `Html.Set(args.Value)` | element, component payload | `mention.innerHTML = args.value` | EXPRESSED (undefined → "undefined" both, innerHTML is LegacyNullToEmptyString) |
| 142 | `Html.Set` from any source | element | `el.innerHTML = src` | EXPRESSED |
| 143 | Show | element | `badge.hidden = false` | EXPRESSED (a `hidden` write, Alis.Reactive/PlanAuthoring/Pipelines/HtmlElement.cs:51-55; §4.8 lists `Show()` as a method) |
| 144 | Hide after Show | element | `badge.hidden = true` | EXPRESSED |
| 145 | `ClassList.Add` | element | `card.classList.add("risk-extra")` | EXPRESSED (`this` = classList) |
| 146 | `ClassList.Remove` | element | `chain.classList.remove("text-text-muted")` | EXPRESSED |
| 147 | `ClassList.Toggle` | element | `el.classList.toggle("x")` | EXPRESSED |
| 148 | Typed plugin; function with response-path arg | plugin | `[Plugin("array")]`; `total.textContent = array.count(res.body.items)` | OBLIGATION: PLUGIN |
| 149 | Plugin result as another plugin's argument | plugin | `active.textContent = array.count(array.filter(res.body.items, "status", "active"))` | OBLIGATION: PLUGIN |
| 150 | Plugin int and string literal args | plugin | `first.textContent = array.pluck(res.body.items, 0, "name")` | OBLIGATION: PLUGIN |
| 151 | Plugin arg from event payload | plugin, payload | `slugify(e.category)` (root function: `call` with empty path) | OBLIGATION: PLUGIN (`this` undefined) |
| 152 | Plugin bool/long/decimal/double/DateTime literal args | plugin | `tools.f(true, 9007199254740993, 1.5, 2.5, toTime("2026-06-01T00:00:00.0000000"))` | OBLIGATION: PLUGIN; a DateTime literal reaches an open-argument member as a timestamp number and an exact-argument member as a JS `Date` (at `195d3a1`). §4.8 does not say which. At `f36f62e` a plugin member is a typed C# member, so every argument is shaped by its declared `Arg<T>` and a `DateTime` arrives as a `Date` (grammar/evaluate.ts:58, 71-73) |
| 153 | Root-callable plugin function | plugin | `slug.textContent = slugify("John Doe")` | OBLIGATION: PLUGIN |
| 154 | Plugin bool result into text | plugin, element | `some.textContent = array.some(array.filter(res.body.items, "status", "active"), "status", "active")` | OBLIGATION: PLUGIN |
| 155 | Plugin command (a `void` member) | plugin | `analytics.track("array-sent");` | OBLIGATION: PLUGIN |
| 156 | Plugin command with a source argument | plugin | `tools.track(tools.slugify(e.filter));` | OBLIGATION: PLUGIN |
| 157 | Root plugin command | plugin | `tools(…);` (`do` of `call` with empty path) | OBLIGATION: PLUGIN |
| 158 | Plugin property read | plugin | `tools.expectedStatus` | OBLIGATION: PLUGIN (absent member throws today; a data `get` gives undefined) |
| 159 | ~~`p.PluginProperty<T>("name", "member")` by name~~ (deleted) | plugin | as row 158 | RENDER: string-named form (deleted at `13341f3`; a `ReadOnly<T>` member of a `Plugin` class, Alis.Reactive/PlanAuthoring/Plugins/Plugin.cs:25) replaced by a `[Plugin]` declaration (architecture.md:700-712); same runtime sentence |
| 160 | ~~`RegisterPlugin("name", b => ..)` + `p.Plugin<T>("name", "member")`~~ (deleted) | plugin | `metrics.count(e.filter)` | RENDER: PluginTypeBuilder/PluginMemberBuilder deleted (architecture.md:710-711); sentence as row 148 |
| 161 | ~~`p.Plugin<T>("name")` root fn; `p.Plugin("name", "member")` / `p.Plugin("name")` commands~~ (deleted) | plugin | `p(…)`; `p.m(…);` | RENDER: as row 160 |
| 162 | Plugin source in gather body and header | plugin, http | `const res = await http.send({ method: "GET", url, values: { count: array.count("items") }, headers: { "X-Array-Count": array.count("items") } })` | OBLIGATION: PLUGIN, HTTP (number header/query text form) |
| 163 | Plugin source as chained route param | plugin, http | `if (res.ok) { const r2 = await http.send({ …, values: { residentId: e.count, categorySlug: slugify(e.category) } }) }` | OBLIGATION: PLUGIN, HTTP |
| 164 | Plugin source written to a component property | plugin, component | `comp.value = tools.f()` | OBLIGATION: PLUGIN, WRITE-SHAPE |
| 165 | Plugin calls in authored order in branches, after HTTP and Parallel | plugin, http, document | as rows 88 and 90 | OBLIGATION: PLUGIN, DOC-RAISE, ALL |

## Counts

| Verdict | Rows |
|---|---|
| EXPRESSED | 37 |
| OBLIGATION | 124 |
| RENDER | 3 |
| GAP | 1 |
| **Total** | **165** |

## Operator table: what `op` must contain to keep today's semantics

"Missing" = `null` or `undefined` (grammar/shape.ts:10-12). Comparisons see both operands after the condition's shape
(Alis.Reactive/Grammar/Lowering/ValueLowering.cs:361-366, 397-403); a literal operand is a `lit` node shaped like any read, so a null literal stays
null (grammar/evaluate.ts:29; grammar/shape.ts:14-18).

| Op (lowering) | Exact rule | Today | Where plain JS / C# differs |
|---|---|---|---|
| `a == b` (Eq; IsNull = `a == null`) | both missing → true; one missing → false; two dates by their time; else `a === b` | grammar/operators.ts:9-13; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:369, 377 | JS `===`: null ≠ undefined. JS `==`: coerces (`"1" == 1`, `0 == ""`). C# has no undefined |
| `a != b` (NotEq; NotNull = `a != null`) | `!(a == b)` | grammar/evaluate.ts:107; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:370, 378 | as above |
| `a > b`, `a >= b`, `a < b`, `a <= b` | numeric when both are finite numbers (a date by its time); code-unit order when both are strings (equal → 0); false < true when both are booleans; **every other pair → false** (null, undefined, NaN, ±Infinity, mixed kinds, objects, arrays) | grammar/operators.ts:15-58 | JS: `null < 15`, `null >= 0`, `null <= 120` true; `"10" < "9"`; `Infinity > 1`. C# lifted operators give false on null (matches) but C# has no text `<` |
| `a >= lo && a <= hi` (Between) | as above; bounds are same-kind typed literals, so today's "bounds orderable" check always holds | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:431-440 | none beyond the row above |
| `&&` `\|\|` `!` over conditions | left-to-right short-circuit on booleans | grammar/evaluate.ts:101-103; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:300-305 | none inside `if` |
| `if (a)` / `if (!a)` (Truthy / Falsy) | JS ToBoolean of the value **after** the source's shape; a bool-typed source passes through toBoolean first | grammar/operators.ts:83-86; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:366, 375-376 | C# has no truthiness. Raw JS: `"false"`, `"0"`, `[]` are truthy |
| `empty(a)` (IsEmpty, NotEmpty = `!empty(a)`) | `a === "" \|\| a == null \|\| (Array.isArray(a) && a.length === 0)`, on the read value; no trim; `" "`, 0, false, `{}`, an empty DOMTokenList are not empty | grammar/operators.ts:88-91; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:379-380 | FluentValidation NotEmpty (architecture.md:767-768) trims and treats default values as empty |
| In / NotIn | `[items].some(v => v == a)` (op `==`); each item shaped by the source's shape | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:410-429 | `includes` (SameValueZero) differs only when an item is null and `a` is undefined |
| ArrayContains | `a` through CV-ARRSHAPE; not an array → false; else `.some(v => v == item)`, items and `item` shaped by the element shape | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:442-450 | `includes` without element shaping (`["1"]` vs 1) |
| Contains / StartsWith / EndsWith | missing receiver → false; receiver via toText (number/bool → `${v}`, Date → its local calendar text and an instant → ISO at `f36f62e`, array → `String(a)`, plain object → false); then JS ordinal `includes` / `startsWith` / `endsWith` | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:385-387, 452-453; grammar/shape.ts:84-96 | JS `null.includes` throws. C# `StartsWith(string)`/`EndsWith(string)` are culture-sensitive |
| `matches(a, p)` | missing → false; toText; `new RegExp(p).test(text)`: no flags, unanchored, fresh RegExp each time | grammar/operators.ts:93-99; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:388-389 | `re.test(undefined)` tests `"undefined"` |
| MinLength n | toText; missing/object → false; `text.length >= n` (UTF-16 units) | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:390-391 | `.length` of an array or number source |
| `toNumber` (number shape) | finite number → same; non-finite → kept; bool → 1/0; text → `Number(t)` if finite (`""`/whitespace → 0, `"1e3"` → 1000, `"0x10"` → 16) else kept; Date → getTime; missing kept | grammar/shape.ts:62-72; grammar/operators.ts:101-104 | `Number(null)` = 0; C# parsing rejects `"0x10"` |
| nullable number/bool/date | missing or .NET-whitespace-only text → null; else inner conversion | grammar/shape.ts:14-18, 181-198 | JS trim differs on U+FEFF, U+0085 |
| `toTime` (date shape) | Date → getTime; finite number → same; `YYYY-MM-DD` → **local** midnight (invalid calendar date → kept); other text → `new Date(t).getTime()` (offset-less = local) if finite, else kept; missing kept. At `f36f62e` the date shape keeps a `Date` (not its time) by the same rules | grammar/shape.ts:98-109 | `new Date("2026-06-01")` is UTC midnight |
| `toBoolean` (bool shape) | bool → same; text false iff `""`, `"false"` (any case), `"0"`; number false iff 0/NaN; Date → true; array → length > 0; object kept | grammar/shape.ts:74-82 | JS ToBoolean |
| `toText` (string shape; also element text writes) | text → same; number/bool → `${v}`; Date → toISOString (at `f36f62e`: local calendar text, an instant → ISO); array → `String(a)`; object kept | grammar/shape.ts:84-96 | DOM ToString(Date) = `Date.toString()` |
| `toArray` at an array-shaped read (CV-ARRSHAPE) | array → items through the item shape; `""` → `[]`; other scalar → `[v]`; object kept; missing kept | grammar/shape.ts:14-18, 156-162 | — |
| `toArray` before an array method (CV-ARR) | array → same; missing → `[]`; number/text → `[v]`; iterable object → `Array.from`; boolean, plain object, DataManager → throw | grammar/operators.ts:121-132; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:205-209 | `null.filter` throws; `Array.from({})` is `[]` |
| `toFinite` (Sum; NEW) | `Number(v)` if finite else 0 | grammar/operators.ts:134-138; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:256-266 | `0 + "5"` = `"05"`; toNumber keeps `"abc"` |
| `order(a, b)` (OrderBy comparator; NEW) | both `typeof` number (a date by its time at `f36f62e`): finite by `a - b`, non-finite after finite and equal to each other; otherwise `String(a)` vs `String(b)` in code-unit order (null → `"null"`); descending = negated; stable; source not mutated (`toSorted`) | grammar/operators.ts:140-161; Alis.Reactive/Grammar/Lowering/ValueLowering.cs:280-289 | C# Comparer puts null first; lifted `<`/`>` make null equal to every key |
| Find | first match, none → null, projection only when found: `find(p) ?? null` | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:268-278 | JS `find` → undefined |
| Count / Any() / All | `length`; `length > 0`; `every` (empty → true) | Alis.Reactive/Grammar/Lowering/ValueLowering.cs:215-228 | — |
| `??`, `?:` | used by Find and by any comparator the compiler builds | — | — |

The 21 comparison operators (`CompareOp`, lowered at Alis.Reactive/Grammar/Lowering/ValueLowering.cs:363-395) map as: is-null, not-null → `==`/`!=` null; is-empty, not-empty → `empty`; truthy, falsy →
`if`; eq, neq → `==`, `!=`; gt, gte, lt, lte → ordered; in, not-in → `some(==)`; between → ordered pair; array-contains →
`toArray` + `some(==)`; contains, starts-with, ends-with → text calls; matches → `matches`; min-length → `length >=`.
