> Lowering of `conditions.md` into the grammar, read at `195d3a1`. The verdicts were recorded before [`../grammar.md`](../grammar.md) settled the gaps and ambiguities; that file's sections 8 and 9 give the decisions and the final counts. Each table was produced by one reader against the code and spot-checked.

# Lowering: conditions, arrays, dispatch, elements, plugins (165 rows)

Read at `feature/component-redesign`, HEAD `195d3a1`, clean tree. Inventory: `docs/design/use-cases/conditions.md`.
Grammar and vocabulary: `docs/design/architecture.md` §0, §4.0–4.9, §6. Today's semantics: `Alis.Reactive.Assets/runtime/`
(`conditions/compare-engine.ts`, `conditions/conditions.ts`, `values/array-op-engine.ts`, `values/evaluate.ts`,
`shared/shape-convert.ts`, `browser-objects/runtime-*.ts`, `plugins/catalog.ts`) and `Alis.Reactive/PlanAuthoring/**`.

## Reading assumption A1

Every verdict assumes **A1: a `get`, `call` or `lit` node converts its value by the node's shape exactly as today's
read does** (`applyShapeWhenPresent`, runtime-value.ts:27-30, called at evaluate.ts:134,161,212,219,230; literals
through `applyShape`, evaluate.ts:83). Missing (`null`/`undefined`) passes through; a failed conversion keeps the
original value. architecture.md does not say this: §0.3 law 3 says nodes carry shape, §6 says "conversions are explicit
nodes". If A1 is rejected, the compiler must emit the conversion ops below wherever a JS value can differ in type from
the C# type, and every EXPRESSED row that reads such a value becomes OBLIGATION. Sentences show `toNumber`/`toTime`
explicitly where the source is always text (URL, JSON dates).

## Obligation codes (exact rules in the operator table at the end)

| Code | Owner | Rule (today's source) |
|---|---|---|
| OP-EQ | op `==` `!=` | both missing → equal; one missing → unequal; else `===` (compare-engine.ts:280-291, 424-426) |
| OP-ORD | op `>` `>=` `<` `<=` | true only for two finite numbers, two strings or two booleans; any other pair (null, undefined, NaN, ±Infinity, mixed kinds) → false for all four (compare-engine.ts:342-358, 371-399) |
| OP-EMPTY | op `empty` | `""`, null, undefined, `[]` (Array.isArray) only; no trim; 0/false/" " not empty (compare-engine.ts:410-415) |
| OP-TEXT | `call` of includes/startsWith/endsWith, `get .length` | missing receiver → false (no throw); non-string receiver via toText (compare-engine.ts:405-408, 417-422) |
| OP-MATCHES | op `matches` | missing → false; `new RegExp(p).test(text)`, no flags (compare-engine.ts:326-333) |
| CV-NUM / CV-TIME / CV-BOOL / CV-TEXT | conversions | shape-convert.ts:160-169 / 181-219 / 171-179 / 151-158 |
| CV-ARRSHAPE | conversion at an array-shaped read | `""` → `[]`, scalar → `[v]`, object kept, missing kept (shape-convert.ts:221-231) |
| CV-ARR | `toArray` before an array method | missing → `[]`; number/string → `[v]`; iterable (DOMTokenList, HTMLCollection) → Array.from; else throw (array-op-engine.ts:108-116) |
| CV-SUM (NEW op) | Sum's per-item coercion | `Number(v)` if finite else 0 (array-op-engine.ts:25-27, 118-121); plain `+` would concatenate text |
| OP-ORDER (NEW op) | OrderBy comparator | compareKeys (array-op-engine.ts:123-135); not reproducible with OP-ORD |
| FIND-NULL | Find | no match → `null` (array-op-engine.ts:55); lowered `find(p) ?? null` |
| DOC-RAISE | `document.raise` | sync `document.dispatchEvent(new CustomEvent(name,{detail}))`, non-bubbling, detail `{}` when none (execute.ts:196-200, 293-297); listener gets `detail`, or the Event when detail is null (trigger.ts:95-101) |
| DOC-READY | `document` `ready` | all non-ready handlers of the plan subscribe first; ready handlers then run in authored order, synchronously if the DOM is parsed, else on DOMContentLoaded (boot.ts:46-60, trigger.ts:37-45) |
| DOC-QUERY | `document.query` | today `URLSearchParams.get`: case-sensitive key, first value, `null` when absent (evaluate.ts:215-220, runtime-plan.ts:80-82); architecture widens to case-insensitive (§0.4:134, §4.6:591) |
| HANDLER-ISO | interpreter | a failing handler is logged and stops no other handler (trigger.ts:17-26) |
| CONFIRM | `confirm.ask` | Promise<boolean>; OK → true, Cancel/close/Escape → false; asks queued FIFO; missing dialog → throws naming FusionConfirmDialog (confirm.ts:6, 51-71, 103-107; conditions.ts:119-129) |
| BUILDER-CONFIRM | C# builder | a Confirm term inside All/Any/Not/ElseIf is asked only when reached, left to right (conditions.ts:68-117): builder nests `let await` + `if`, duplicating the continuation |
| PLUGIN | `plugin(name)` | registry fed from `window.__alisPlugins` before boot; unknown name throws `plugin not found` (catalog.ts:18-23); member call `this` = owner, root call `this` undefined (runtime-path.ts:136-143, 213-230); absent property throws (runtime-path.ts:125-130, 188-192) |
| WRITE-SHAPE | `set`/`call` on an object member | value converted by the member's shape, dates delivered as JS `Date` (runtime-object.ts:251-257, 283-287; runtime-shape.ts:79-94) |
| ELEM | `element(id, dom)` | resolved by getElementById at each use, absent → throws (evaluate.ts:206-213) |
| ALL | `all(block+)` | every branch settles; rejections logged, not rethrown; completion runs (execute.ts:131-152) |
| HTTP | `http.send` | per `use-cases/http.md` lowering (architecture §0.5) |

## Table

| # | Use case (short) | Vocabulary used | Sentence in the grammar | Verdict |
|---|---|---|---|---|
| 1 | CustomEvent<T> payload as source | payload, document | `if (e.score >= 90) grade.textContent = "A"; else if (e.score >= 80) grade.textContent = "B"; else grade.textContent = "F"` | OBLIGATION: OP-ORD |
| 2 | Nested payload path | payload | `if (e.address.city === "Seattle") r.textContent = "Found Seattle"; else r.textContent = "Other City"` | EXPRESSED (missing link → undefined, §4.3:485) |
| 3 | Null object / null leaf / missing key → Else, no crash | payload | `if (e.address == null) …; if (e.address.city === "Seattle") … else …` | OBLIGATION: OP-EQ (missing key is undefined; today is-null treats it as null, compare-engine.ts:263-264) |
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
| 94 | From(args, e => e.Value) (CheckList `string[]`) | arrays, component payload | `count.textContent = toArray(args.value).length` | OBLIGATION: CV-ARRSHAPE then CV-ARR (`""` → `[]` → 0; CV-ARR alone gives `[""]` → 1) |
| 95 | From(payload, x => x.Alerts) | arrays, payload | `total.textContent = toArray(e.alerts).length` | OBLIGATION: CV-ARR |
| 96 | From(component property `Rows<T>()`) | arrays, component | `grid.dataSource = toArray(grid.dataSource).filter(x => x.status === "active").toSorted((a, b) => order(a.name, b.name)); grid.refresh()` | OBLIGATION: CV-ARR (non-iterable DataManager throws), OP-ORDER, WRITE-SHAPE |
| 97 | FromDom classList | arrays, element | `total.textContent = toArray(card.classList).length` | OBLIGATION: CV-ARR (DOMTokenList → Array.from) |
| 98 | FromDom children | arrays, element | `kids.textContent = toArray(list.children).length` | OBLIGATION: CV-ARR |
| 99 | FromDom<T> with per-element GetAttribute | arrays, element | `high.textContent = toArray(riskList.children).filter(x => x.getAttribute("data-risk") === "high").length` | GAP: the C# lambda calls a JsObject member on an element of `Children`. §6's table maps only `string` and `IEnumerable<T>` members (architecture.md:815-822) and `Children`'s element type is undeclared (:672). Today: allow-list `getAttribute`, `hasAttribute`, `getDay`…`getTime`, `toUpperCase`, `toLowerCase`, `trim` (ElementExpressionCompiler.cs:20-25, 140-154) |
| 100 | Where(x => x.Status == "active") | arrays | `toArray(rows).filter(x => x.status === "active")` | OBLIGATION: CV-ARR (predicate exact) |
| 101 | Re-read classList after AddClass in one handler | element, arrays | `card.classList.add("risk-extra"); n.textContent = toArray(card.classList).filter(x => x.startsWith("risk-")).length` | OBLIGATION: CV-ARR, ELEM (fresh read, no caching) |
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
| 120 | AsSource bound to a DataSource | arrays, component | `grid.dataSource = toArray(res.body.residents).toSorted((a, b) => order(a.name, b.name))` | OBLIGATION: OP-ORDER, WRITE-SHAPE |
| 121 | Array result as text | arrays, element | `total.textContent = toArray(res.body.residents).length` | OBLIGATION: CV-ARR |
| 122 | Array result in gather or dispatch | arrays, http, document | `document.raise("x", { count: toArray(r).length })`; `http.send({ …, headers: { "X-Count": toArray(r).length } })` | OBLIGATION: CV-ARR, DOC-RAISE, HTTP |
| 123 | DomReady | document, element | `on(document, "ready", e) { s1.classList.add("text-green-600"); s1.textContent = "dom-ready fired…"; document.raise("test") }` | OBLIGATION: DOC-READY (the "test" listener must already be subscribed), DOC-RAISE |
| 124 | Several DomReady handlers all run | document | two `on(document, "ready", e) {…}` handlers | OBLIGATION: DOC-READY, HANDLER-ISO |
| 125 | Dispatch(name) → untyped CustomEvent(name) | document | `document.raise("test")`; `on(document, "test", e) { s2.classList.add(…); … }` | OBLIGATION: DOC-RAISE (detail `{}`) |
| 126 | Dispatch<T>(name, object) → typed reads | document, payload | `document.raise("payload-test", { intValue: 42, longValue: …, address: {…} })`; `on(document, "payload-test", e) { iv.textContent = e.intValue; city.textContent = e.address.city; … }` | OBLIGATION: DOC-RAISE |
| 127 | Dispatch with an anonymous object | document | `document.raise("supported-trigger-review", { score: 95 })` → listener `if (e.score >= 90) …` | OBLIGATION: DOC-RAISE, OP-ORD (the anonymous object must become the event's declared `T`, §4.7:631) |
| 128 | Two handlers on one event name | document | `on(document, "set-score", e) { sd.textContent = e.score }` and `on(document, "set-score", e) { if (e.score >= 90) … }` | OBLIGATION: DOC-RAISE (one subscription per handler, subscription order), HANDLER-ISO, OP-ORD |
| 129 | DispatchWith component values, nested path, literal | document, component | `document.raise("transfer", { name: nameBox.value, status: "active", address: { city: cityBox.value } })` | OBLIGATION: DOC-RAISE |
| 130 | DispatchWith returning method | document, component | `document.raise("filters-applied", { selection: chips.getSelectedChips() })` | OBLIGATION: DOC-RAISE |
| 131 | DispatchWith response body in OnSuccess | document, response | `if (res.ok) document.raise("shift-report", { alerts: res.body.alerts })` | OBLIGATION: DOC-RAISE |
| 132 | DispatchWith int / bool literals | document | `document.raise("x", { count: 3, active: true })` | OBLIGATION: DOC-RAISE |
| 133 | DispatchWith plugin sources | document, plugin | `document.raise("plugin-request-input", { slug: tools.slugify(e.filter), token: tools.token })` | OBLIGATION: DOC-RAISE, PLUGIN |
| 134 | SetText literal | element | `grade.textContent = "A"` | EXPRESSED |
| 135 | SetText(payload, path) | element, payload | `iv.textContent = e.intValue` | EXPRESSED (DOM ToString equals today's toText for numbers, booleans, arrays; null/undefined → "") |
| 136 | SetText(responseBody, path) | element, response | `if (res.ok) s1.textContent = res.body.receivedName` | EXPRESSED |
| 137 | SetText(component) | element, component | `lvl.textContent = care.value` | EXPRESSED |
| 138 | SetText(URL) | element, document.query | `tab.textContent = document.query.tab` | OBLIGATION: DOC-QUERY |
| 139 | SetText(plugin) | element, plugin | `total.textContent = array.count(res.body.items)` | OBLIGATION: PLUGIN |
| 140 | SetHtml("") clears markup | element | `container.innerHTML = ""` | EXPRESSED |
| 141 | SetHtml(payload, path) | element, component payload | `mention.innerHTML = args.value` | EXPRESSED (undefined → "undefined" both, innerHTML is LegacyNullToEmptyString) |
| 142 | SetHtml(TypedSource) | element | `el.innerHTML = src` | EXPRESSED |
| 143 | Show | element | `badge.hidden = false` | EXPRESSED (today a `hidden` write, ElementBuilder.cs:116-119; §4.8 lists `Show()` as a method) |
| 144 | Hide after Show | element | `badge.hidden = true` | EXPRESSED |
| 145 | AddClass | element | `card.classList.add("risk-extra")` | EXPRESSED (`this` = classList) |
| 146 | RemoveClass | element | `chain.classList.remove("text-text-muted")` | EXPRESSED |
| 147 | ToggleClass | element | `el.classList.toggle("x")` | EXPRESSED |
| 148 | Typed plugin; function with response-path arg | plugin | `[Plugin("array")]`; `total.textContent = array.count(res.body.items)` | OBLIGATION: PLUGIN |
| 149 | Plugin result as another plugin's argument | plugin | `active.textContent = array.count(array.filter(res.body.items, "status", "active"))` | OBLIGATION: PLUGIN |
| 150 | Plugin int and string literal args | plugin | `first.textContent = array.pluck(res.body.items, 0, "name")` | OBLIGATION: PLUGIN |
| 151 | Plugin arg from event payload | plugin, payload | `slugify(e.category)` (root function: `call` with empty path) | OBLIGATION: PLUGIN (`this` undefined) |
| 152 | Plugin bool/long/decimal/double/DateTime literal args | plugin | `tools.f(true, 9007199254740993, 1.5, 2.5, toTime("2026-06-01T00:00:00.0000000"))` | OBLIGATION: PLUGIN; a DateTime literal reaches an open-argument member as a timestamp number and an exact-argument member as a JS `Date` (evaluate.ts:83; runtime-object.ts:272-287). §4.8 does not say which. |
| 153 | Root-callable plugin function | plugin | `slug.textContent = slugify("John Doe")` | OBLIGATION: PLUGIN |
| 154 | Plugin bool result into text | plugin, element | `some.textContent = array.some(array.filter(res.body.items, "status", "active"), "status", "active")` | OBLIGATION: PLUGIN |
| 155 | Plugin command Fire | plugin | `analytics.track("array-sent");` | OBLIGATION: PLUGIN |
| 156 | Plugin command with a source argument | plugin | `tools.track(tools.slugify(e.filter));` | OBLIGATION: PLUGIN |
| 157 | Root plugin command | plugin | `tools(…);` (`do` of `call` with empty path) | OBLIGATION: PLUGIN |
| 158 | Plugin property read | plugin | `tools.expectedStatus` | OBLIGATION: PLUGIN (absent member throws today; a data `get` gives undefined) |
| 159 | PluginProperty<T>("name", "member") by name | plugin | as row 158 | RENDER: string-named form (PipelineBuilder.cs:209) replaced by a `[Plugin]` declaration (architecture.md:675-687); same runtime sentence |
| 160 | RegisterPlugin("name", b => ..) + Plugin<T>("name", "member") | plugin | `metrics.count(e.filter)` | RENDER: PluginTypeBuilder/PluginMemberBuilder deleted (architecture.md:685-686); sentence as row 148 |
| 161 | Plugin<T>("name") root fn; Plugin("name", "member") / Plugin("name") commands | plugin | `p(…)`; `p.m(…);` | RENDER: as row 160 |
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

"Missing" = `null` or `undefined` (compare-engine.ts:424-426). Comparisons see both operands after the source's shape
(compare-engine.ts:155-164, 179-200); a literal operand is shaped with `applyShapeWhenPresent`, so a null literal stays
null (:190-200).

| Op (lowering) | Exact rule | Today | Where plain JS / C# differs |
|---|---|---|---|
| `a == b` (Eq; IsNull = `a == null`) | both missing → true; one missing → false; else `a === b` | compare-engine.ts:99-104, 263-266, 280-291 | JS `===`: null ≠ undefined. JS `==`: coerces (`"1" == 1`, `0 == ""`). C# has no undefined |
| `a != b` (NotEq; NotNull = `a != null`) | `!(a == b)` | :286-291 | as above |
| `a > b`, `a >= b`, `a < b`, `a <= b` | numeric when both are finite numbers; code-unit order when both are strings (equal → 0); false < true when both are booleans; **every other pair → false** (null, undefined, NaN, ±Infinity, mixed kinds, objects, arrays) | :106-113, 342-358, 371-403 | JS: `null < 15`, `null >= 0`, `null <= 120` true; `"10" < "9"`; `Infinity > 1`. C# lifted operators give false on null (matches) but C# has no text `<` |
| `a >= lo && a <= hi` (Between) | as above; bounds are same-kind typed literals, so today's "bounds orderable" check always holds | :122-126, 360-369 | none beyond the row above |
| `&&` `\|\|` `!` over conditions | left-to-right short-circuit on booleans | :65-78; conditions.ts:47-117 | none inside `if` |
| `if (a)` / `if (!a)` (Truthy / Falsy) | JS ToBoolean of the value **after** the source's shape; a bool-typed source passes through toBoolean first | :271-274 | C# has no truthiness. Raw JS: `"false"`, `"0"`, `[]` are truthy |
| `empty(a)` (IsEmpty, NotEmpty = `!empty(a)`) | `a === "" \|\| a == null \|\| (Array.isArray(a) && a.length === 0)`, on the read value; no trim; `" "`, 0, false, `{}`, an empty DOMTokenList are not empty | :267-270, 410-415 | FluentValidation NotEmpty (architecture.md:739-740) trims and treats default values as empty |
| In / NotIn | `[items].some(v => v == a)` (op `==`); each item shaped by the source's shape | :115-120, 202-216, 293-298 | `includes` (SameValueZero) differs only when an item is null and `a` is undefined |
| ArrayContains | `a` through CV-ARRSHAPE; not an array → false; else `.some(v => v == item)`, items and `item` shaped by the element shape | :128-133, 300-304 | `includes` without element shaping (`["1"]` vs 1) |
| Contains / StartsWith / EndsWith | missing receiver → false; receiver via toText (number/bool → `${v}`, Date → ISO, array → `String(a)`, plain object → false); then JS ordinal `includes` / `startsWith` / `endsWith` | :135-142, 306-324, 405-408, 417-422 | JS `null.includes` throws. C# `StartsWith(string)`/`EndsWith(string)` are culture-sensitive |
| `matches(a, p)` | missing → false; toText; `new RegExp(p).test(text)`: no flags, unanchored, fresh RegExp each time | :144-145, 326-333 | `re.test(undefined)` tests `"undefined"` |
| MinLength n | toText; missing/object → false; `text.length >= n` (UTF-16 units) | :147-148, 335-340 | `.length` of an array or number source |
| `toNumber` (number shape) | finite number → same; non-finite → kept; bool → 1/0; text → `Number(t)` if finite (`""`/whitespace → 0, `"1e3"` → 1000, `"0x10"` → 16) else kept; Date → getTime; missing kept | shape-convert.ts:160-169, 244-257; runtime-value.ts:27-30 | `Number(null)` = 0; C# parsing rejects `"0x10"` |
| nullable number/bool/date | missing or .NET-whitespace-only text → null; else inner conversion | shape-convert.ts:45-66 | JS trim differs on U+FEFF, U+0085 |
| `toTime` (date shape) | Date → getTime; finite number → same; `YYYY-MM-DD` → **local** midnight (invalid calendar date → kept); other text → `new Date(t).getTime()` (offset-less = local) if finite, else kept; missing kept | shape-convert.ts:181-219 | `new Date("2026-06-01")` is UTC midnight |
| `toBoolean` (bool shape) | bool → same; text false iff `""`, `"false"` (any case), `"0"`; number false iff 0/NaN; Date → true; array → length > 0; object kept | shape-convert.ts:171-179, 259-269 | JS ToBoolean |
| `toText` (string shape; also element text writes) | text → same; number/bool → `${v}`; Date → toISOString; array → `String(a)`; object kept | shape-convert.ts:151-158 | DOM ToString(Date) = `Date.toString()` |
| `toArray` at an array-shaped read (CV-ARRSHAPE) | array → items through the item shape; `""` → `[]`; other scalar → `[v]`; object kept; missing kept | shape-convert.ts:221-231, 87-89 | — |
| `toArray` before an array method (CV-ARR) | array → same; missing → `[]`; number/text → `[v]`; iterable object → `Array.from`; boolean, plain object, DataManager → throw | array-op-engine.ts:108-116 | `null.filter` throws; `Array.from({})` is `[]` |
| `toFinite` (Sum; NEW) | `Number(v)` if finite else 0 | array-op-engine.ts:25-27, 118-121 | `0 + "5"` = `"05"`; toNumber keeps `"abc"` |
| `order(a, b)` (OrderBy comparator; NEW) | both `typeof` number: finite by `a - b`, non-finite after finite and equal to each other; otherwise `String(a)` vs `String(b)` in code-unit order (null → `"null"`); descending = negated; stable; source not mutated (`toSorted`) | array-op-engine.ts:36-39, 59-69, 123-135 | C# Comparer puts null first; lifted `<`/`>` make null equal to every key |
| Find | first match, none → null, projection only when found: `find(p) ?? null` | array-op-engine.ts:34-35, 45-57 | JS `find` → undefined |
| Count / Any() / All | `length`; `length > 0`; `every` (empty → true) | array-op-engine.ts:18-20, 28-33 | — |
| `??`, `?:` | used by Find and by any comparator the compiler builds | — | — |

The 21 compare-engine cases map as: is-null, not-null → `==`/`!=` null; is-empty, not-empty → `empty`; truthy, falsy →
`if`; eq, neq → `==`, `!=`; gt, gte, lt, lte → ordered; in, not-in → `some(==)`; between → ordered pair; array-contains →
`toArray` + `some(==)`; contains, starts-with, ends-with → text calls; matches → `matches`; min-length → `length >=`.
