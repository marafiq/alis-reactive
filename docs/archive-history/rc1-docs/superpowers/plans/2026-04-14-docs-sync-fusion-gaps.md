# Docs Site Sync: Fusion Component Gaps — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring `docs-site/src/content/docs/components/fusion-components.md` in sync with the public Fusion component surface on `release/1.0.0-preview1`, covering the 7 Fusion components currently shipped in code but missing from docs.

**Architecture:** Each missing component already has a C# vertical slice (`FusionX.cs`, `FusionXBuilder.cs`, `FusionXHtmlExtensions.cs`, `FusionXReactiveExtensions.cs`, `FusionXExtensions.cs`, `FusionXEvents.cs`, `Events/*.cs`). Five of them (`ColorPicker`, `Grid`, `Accordion`, `Tab`, `Schedule`) also have sandbox views under `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/<Name>/Index.cshtml`. Two (`Dialog`, `Tooltip`) do not. Phase A documents the 5 components with existing sandboxes — every code example is copied verbatim from a verified sandbox page. Phase B scaffolds minimal sandbox pages for `Dialog` and `Tooltip` first, then documents them. Phase C does a final build + visual review of the complete page.

**Tech Stack:** Astro + Starlight (`docs-site/`), C# .NET 10 (`Alis.Reactive.Fusion`), Razor + Syncfusion EJ2 (`Alis.Reactive.SandboxApp`).

**Worktree:** All work happens in `/Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/docs-sync-preview1` on branch `docs/sync-release-preview1` (based at `release/1.0.0-preview1`, commit `8e70f4e7`). Every agent dispatched as part of this plan MUST be told to operate only against this absolute path and never read `.codex-worktrees/*` or the main worktree.

**Docs convention (mandatory for every task in this plan):**

- Open the sandbox page in a running browser before writing any text. Do not write from the source files alone.
- Every fenced `csharp` block MUST be copied (not paraphrased) from the sandbox view. The only edits allowed are: trim unrelated `native-card` markup, rename verbose locals to short names that fit docs width, drop comments the dev page uses for layout.
- Use dev-facing voice: "How do I …?", "When should I …?", "Why does this …?". No "descriptor", "runtime", "plan model" in section prose — those are internals.
- No em-dashes in code comments (Rider flags). ASCII hyphen `-` or `--` is fine.
- Section order in `fusion-components.md` MUST stay alphabetical within each natural group: pickers, selectors, surfaces, app-level. The task list below inserts new sections at the correct anchor.

**Shared section skeleton (used by every documentation task):**

Each new component gets a section that follows this exact shape. Individual tasks only vary the values marked `<<ANGLE>>`. Do not simplify, expand, or reorder headings without revisiting the plan.

```markdown
## Fusion<<Name>>

<<One-sentence description that says what a dev would use this for in a senior-living app.>>

| Property | Value |
|----------|-------|
| ReadExpr | `"<<value-member-or-primary-read-expression>>"` |
| Events | <<comma-list of events from FusionXEvents.cs>> |
| Typed Source | `TypedComponentSource<<<clr-type>>>` |

### How do I render a <<name>>?

```csharp
<<render snippet copied from the sandbox — Html.InputField(...).FusionX(b => b...) OR Html.FusionX(...) for app-level>>
```

### <<One or two more "How do I …?" questions answered with code from the sandbox. Cover: setting a value or state, reacting to the primary event, any cascade/chain pattern the sandbox demonstrates.>>

```csharp
<<sandbox snippet>>
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `<<Ext1(args)>>` | <<one-line description from XML doc>> |
| `<<Ext2(args)>>` | <<one-line description from XML doc>> |
| ... | ... |

---
```

After the last `---`, the next component section begins.

---

## File Structure

**Modified:**
- `docs-site/src/content/docs/components/fusion-components.md` — add 7 sections (5 in Phase A, 2 in Phase B). One file, multiple small edits — use `Edit` with a unique anchor per insertion.

**Created (Phase B only, minimum viable sandbox pages):**
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Dialog/DialogModel.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/DialogController.cs` (or reuse existing Fusion sandbox controller — inspect first)
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Dialog/Index.cshtml`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tooltip/TooltipModel.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/TooltipController.cs` (or reuse)
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tooltip/Index.cshtml`

**Not touched:**
- `Alis.Reactive.Fusion/**` — no C# source changes. All 7 components are already public and shipped.
- Schema, TS runtime, Playwright tests — out of scope. This is a Layer 4→5 change (docs) that relies on existing Layer 1–4.

---

## Task 0: Baseline Verification

**Purpose:** Confirm the worktree builds, the sandbox runs, all 5 existing Fusion sandbox pages load cleanly, and the docs site baseline passes. No writing yet — pure gate.

**Files:** none (read-only + run)

- [ ] **Step 0.1: Confirm current branch + worktree**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/docs-sync-preview1
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
```

Expected: `docs/sync-release-preview1` and a commit that matches `release/1.0.0-preview1` (currently `8e70f4e7`).

- [ ] **Step 0.2: Build JS bundle + C# projects**

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
```

Expected: both complete with no errors. Warnings are OK.

- [ ] **Step 0.3: Build the docs site**

```bash
cd docs-site
npm install   # skip if already done in Task 0.2 chain
npm run build
cd ..
```

Expected: `40 page(s) built`. Record the exact number — subsequent phases must match 40 + new pages.

- [ ] **Step 0.4: Kill any stale sandbox processes, then start SandboxApp**

```bash
lsof -ti:5220 | xargs kill -9 2>/dev/null
dotnet run --project Alis.Reactive.SandboxApp
```

Leave this running in one terminal. Open a second terminal for subsequent tasks.

- [ ] **Step 0.5: Smoke-test each of the 5 existing Fusion sandbox pages**

Open each URL in a browser. For each page, confirm the component renders, the primary event fires (click / pick / select), and no console errors appear.

| # | Component | URL |
|---|-----------|-----|
| 1 | FusionColorPicker | `http://localhost:5220/Sandbox/Components/ColorPicker` |
| 2 | FusionGrid | `http://localhost:5220/Sandbox/Components/Grid` |
| 3 | FusionAccordion | `http://localhost:5220/Sandbox/Components/Accordion` |
| 4 | FusionTab | `http://localhost:5220/Sandbox/Components/Tab` |
| 5 | FusionSchedule | `http://localhost:5220/Sandbox/Components/Schedule` |

Route convention (verified during baseline): Fusion sandbox controllers use `[Route("Sandbox/Components/{Name}")]` — they do NOT include `Fusion/` in the route segment, even though the controller/view files live under `Controllers/Components/Fusion/`. Anyone scaffolding a new sandbox (Tasks 7 and 9) must follow this same convention.

- [ ] **Step 0.6: No commit**

Baseline is a gate, not a change. Move on when all 5 pages work.

---

## Phase A — Document the 5 Components With Existing Sandboxes

Each Phase A task follows the same 7-step loop. Fill in the `<<ANGLE>>` values from the component's source files and sandbox view. Use the shared section skeleton defined in the header.

### Task 1: Document FusionColorPicker

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPicker.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPickerEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPickerExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPickerHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionColorPicker/FusionColorPickerReactiveExtensions.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/ColorPicker/Index.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/ColorPicker/ColorPickerModel.cs`
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 1.1: Extract the facts**

Create a scratch note (in the task, not on disk) with exactly these values pulled from the files above:

- Name: `FusionColorPicker`
- One-liner: _Paraphrase the XML `<summary>` on `FusionColorPicker.cs` into one dev-facing sentence using a senior-living use case (e.g. "theme and accent colors for facility branding")._
- ReadExpr: `"value"` (confirm by reading `FusionColorPicker.ValueMember` in `FusionColorPicker.cs`)
- Events: list every `public static readonly` event on `FusionColorPickerEvents.cs`
- TypedSource generic arg: CLR type the `Value()` extension returns in `FusionColorPickerExtensions.cs`
- Mutation extensions: list every `public static` extension method on `FusionColorPickerExtensions.cs` with its XML `<summary>` reduced to one line

- [ ] **Step 1.2: Verify the sandbox in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/ColorPicker`. Exercise each card: set value, change color, toggle popup, read value. Confirm every behavior you plan to document actually works. If something is broken, STOP — file a note and return to planning. Do not document behavior you have not seen work.

- [ ] **Step 1.3: Choose the code snippets**

From `Index.cshtml`, pick 2 or 3 snippets depending on what the sandbox naturally exercises. The canonical set is:
1. Render + event snippet — the `Html.InputField(...).FusionColorPicker(b => b.Reactive(plan, evt => evt.Changed, ...))` block showing a `When` + `Then`/`Else`. This is the SINGLE authoritative render pattern; merging "render" and "react to change" into one Q/A avoids a thin bare-builder Q1 that has no precedent in the sandbox.
2. Read-as-source snippet — the block that does `var comp = p.Component<FusionColorPicker>(...); p.When(comp.Value())`.
3. (Optional) any additional snippet only if the sandbox genuinely demonstrates something the first two do not cover.

Copy them verbatim. Strip only unrelated `native-card` / HTML wrapper markup — keep all C#. A separate pure-render snippet with an empty `b => { }` callback is NOT required and should not be invented for docs pedagogy if the sandbox only exercises the empty builder in a throwaway section — prefer the rich Reactive form as the first docs block.

- [ ] **Step 1.4: Find the insertion anchor in `fusion-components.md`**

`FusionColorPicker` sorts alphabetically between `FusionAutoComplete` (currently first at line 14) and `FusionNumericTextBox` (currently second). The insertion anchor is the `---` separator that closes the `FusionAutoComplete` section. Use `Grep` to locate the exact line, then `Read` the surrounding 5 lines before editing to avoid a mid-section insert.

- [ ] **Step 1.5: Insert the new section**

Use `Edit` to replace the `---` separator that closes `FusionAutoComplete`'s "Mutation extensions" table with:

```markdown
---

## FusionColorPicker

<<one-line description from Step 1.1>>

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | <<events list from Step 1.1>> |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a color picker?

```csharp
<<render snippet from Step 1.3>>
```

### How do I react to a color change?

```csharp
<<event snippet from Step 1.3>>
```

### How do I read the current color in a condition?

```csharp
<<read-as-source snippet from Step 1.3>>
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
<<one row per extension from Step 1.1>>

---
```

- [ ] **Step 1.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `41 page(s) built` (one more than Task 0 baseline, or same count if the page is added to an existing file — this task is adding a section, not a page, so **expect `40 page(s) built`**). Zero errors. Zero broken-reference warnings.

- [ ] **Step 1.7: Visually verify the new section**

```bash
cd docs-site && npm run preview -- --port 4321 &
```

Open `http://localhost:4321/components/fusion-components/#fusioncolorpicker`. Confirm:
- The anchor works (URL hash scrolls to the new heading)
- Code blocks are syntax-highlighted as `csharp`
- The mutation extensions table renders
- Sidebar still lists "Syncfusion Components" in the Components group

Kill the preview server.

- [ ] **Step 1.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionColorPicker"
```

---

### Task 2: Document FusionAccordion

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordion.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionReactiveExtensions.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/_OverviewPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/_CareLevelsPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/_ContactPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Accordion/AccordionModel.cs`
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 2.1: Extract the facts**

Scratch-note the same fields as Task 1.1 but for `FusionAccordion`. Note that Accordion is not an input — it is a surface component. Its "ReadExpr" row in the section may say `"(not input-bound)"` if `FusionAccordion.ValueMember` is null or empty. Verify by reading `FusionAccordion.cs`.

- [ ] **Step 2.2: Verify the sandbox in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Accordion`. Expand each panel in turn. Confirm the `Expanded` event fires and any `.Reactive()` wiring in `Index.cshtml` produces visible effect. Confirm the partial-view content loads.

- [ ] **Step 2.3: Choose the code snippets**

Pick exactly 2 snippets:
1. Render snippet — the `Html.FusionAccordion(plan, ...)` call (or input-field form, whichever the sandbox uses). Include the partial references if that is how the sandbox composes panels — this is the primary teaching moment for the component.
2. Event snippet — the `Reactive(plan, evt => evt.Expanded, ...)` block.

- [ ] **Step 2.4: Find the insertion anchor**

`FusionAccordion` sorts alphabetically just after `FusionAutoComplete` or wherever your alphabetization lands it. Because Task 1 added `FusionColorPicker` already, the candidate anchors are:
- Before `FusionAutoComplete` (if the file sorts by section headers alphabetically, `FusionAccordion` < `FusionAutoComplete`)

Verify by reading the current top-of-file ordering. The existing file does NOT sort strictly alphabetically — it sorts by "pickers first, then selectors, then app-level". `FusionAccordion` is a surface component, and surface components go AFTER all inputs but BEFORE the app-level group (`FusionToast`, `FusionConfirm`). Place it just before the first app-level section.

Read the 20 lines surrounding `## FusionToast` (or the first app-level section you see), use the preceding `---` separator as the insertion anchor.

- [ ] **Step 2.5: Insert the new section**

Fill in the shared section skeleton (from the plan header) with the values from Step 2.1 and the snippets from Step 2.3. "How do I …?" questions for a surface component should be:
1. "How do I render an accordion with multiple panels?"
2. "How do I react to a panel expanding?"

- [ ] **Step 2.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`. Zero errors.

- [ ] **Step 2.7: Visually verify the new section**

Preview the page, scroll to `#fusionaccordion`, confirm the section renders with working code blocks and the table.

- [ ] **Step 2.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionAccordion"
```

---

### Task 3: Document FusionTab

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionTab/FusionTab.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTab/FusionTabReactiveExtensions.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/Index.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/_ResidentsPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/_FacilitiesPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tab/_StaffPartial.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tab/TabModel.cs`
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 3.1: Extract the facts**

Same as Task 1.1, but for `FusionTab`. Primary event is `Selected` (confirm by reading `FusionTabEvents.cs`).

- [ ] **Step 3.2: Verify the sandbox in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Tab`. Click each tab, confirm the `Selected` event fires and partial content loads.

- [ ] **Step 3.3: Choose the code snippets**

Pick 2 snippets:
1. Render snippet — `Html.FusionTab(plan, ...)` with at least two partial references.
2. Event snippet — `Reactive(plan, evt => evt.Selected, ...)`.

- [ ] **Step 3.4: Find the insertion anchor**

`FusionTab` goes immediately after `FusionAccordion` in the "surface components" group established by Task 2. Read the file, find the `---` separator closing the `FusionAccordion` section you added, use it as the anchor.

- [ ] **Step 3.5: Insert the new section**

Fill in the shared section skeleton with:
- Section questions:
  1. "How do I render a tab strip with multiple panels?"
  2. "How do I react to a tab selection?"

- [ ] **Step 3.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`.

- [ ] **Step 3.7: Visually verify**

Preview, scroll to `#fusiontab`.

- [ ] **Step 3.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionTab"
```

---

### Task 4: Document FusionGrid

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGrid.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridBuilder.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/FusionGridReactiveExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Grid/GridModel.cs`
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 4.1: Extract the facts**

Same drill, for `FusionGrid`. `DataStateChange` is the primary server-pagination event — note it. Grid is NOT input-bound; its `ValueMember` is likely empty/null. `FusionGridBuilder` will expose column definitions, paging, sorting — scan its public members and record them.

- [ ] **Step 4.2: Verify the sandbox in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Grid`. Confirm the grid renders with data, paging works, the `DataStateChange` event fires an HTTP round-trip if the sandbox wires one. Watch the network tab.

- [ ] **Step 4.3: Choose the code snippets**

Pick 3 snippets (Grid is the biggest and most deserving of extra detail):
1. Render snippet — full `Html.FusionGrid(plan, ...)` with column definitions and any template references.
2. Server-paging snippet — `Reactive(plan, evt => evt.DataStateChange, ...)` with the HTTP `Get` + `OnSuccess` + `SetDataSource` / `DataBind` pattern.
3. Mutation snippet — whichever extension the sandbox calls (probably `SetDataSource(...).DataBind()` or `Refresh()`).

- [ ] **Step 4.4: Find the insertion anchor**

Alphabetically, `FusionGrid` > `FusionFileUpload` and < `FusionInputMask` among inputs, but Grid is not input-bound — it belongs in the surface group with Accordion and Tab. Place it **between FusionAccordion and FusionTab** if you ordered Accordion < Grid < Tab (alphabetical), or **after FusionTab** if you grouped "navigation surfaces" (Accordion, Tab) separately from "data surfaces" (Grid, Schedule).

Choice: alphabetical within the surface group. Order becomes: `FusionAccordion` → `FusionGrid` → `FusionSchedule` → `FusionTab` → `FusionTooltip`.

Read the file, find the `---` separator closing `FusionAccordion`, use it as the anchor. (This bumps the Tab section added in Task 3 one slot down — verify the edit replaces only the intended anchor.)

- [ ] **Step 4.5: Insert the new section**

Fill in the shared section skeleton. Section questions:
1. "How do I render a grid with columns?"
2. "How do I wire server-side pagination?"
3. "How do I reload the grid from an HTTP response?"

Add one extra paragraph after the render snippet explaining that column definitions come from the `FusionGridBuilder` fluent API and template strings are typed via `FusionTemplateExpression` (name only — do not dump its full API here; the Grid section is the one place it appears in context).

- [ ] **Step 4.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`. Zero errors.

- [ ] **Step 4.7: Visually verify**

Preview, scroll to `#fusiongrid`, confirm all 3 code blocks render and the table is intact.

- [ ] **Step 4.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionGrid"
```

---

### Task 5: Document FusionSchedule

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionSchedule.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleBuilder.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleReactiveExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnCellClick.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnEventClick.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnPopupOpen.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnActionBegin.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionSchedule/Events/FusionScheduleOnActionComplete.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Schedule/Index.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Schedule/_NewAssignmentForm.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Schedule/_EditAssignmentForm.cshtml`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Schedule/ScheduleModel.cs`
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 5.1: Extract the facts**

Schedule has many events (`CellClick`, `EventClick`, `PopupOpen`, `ActionBegin`, `ActionComplete`, `DataBound`, `Navigating`, `PopupClose`, `EventRendered`). List them all in the Events table cell. Document only the 2 or 3 most important ones in the section body.

- [ ] **Step 5.2: Verify the sandbox in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Schedule`. Click a cell, click an existing event, open the new-assignment form, submit. Watch the network tab for any HTTP round-trip. Confirm the `_NewAssignmentForm` and `_EditAssignmentForm` partials render inside the popup.

- [ ] **Step 5.3: Choose the code snippets**

Pick 3 snippets:
1. Render snippet — `Html.FusionSchedule(plan, ...)` with view settings and event source.
2. Cell-click snippet — `Reactive(plan, evt => evt.CellClick, ...)` showing how a click opens an edit form.
3. Action-complete snippet — `Reactive(plan, evt => evt.ActionComplete, ...)` showing how a create/edit/delete triggers an HTTP call and grid refresh. (Or, if the sandbox does this differently, use whatever pattern it actually uses.)

- [ ] **Step 5.4: Find the insertion anchor**

In the alphabetical surface-group order, `FusionSchedule` goes between `FusionGrid` (added in Task 4) and `FusionTab` (added in Task 3 but bumped by Tasks 4 and this task). Anchor: the `---` separator closing `FusionGrid`'s section.

- [ ] **Step 5.5: Insert the new section**

Fill in the skeleton. Section questions:
1. "How do I render a schedule?"
2. "How do I react when a user clicks an empty cell?"
3. "How do I persist changes back to the server?"

- [ ] **Step 5.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`.

- [ ] **Step 5.7: Visually verify**

Preview, scroll to `#fusionschedule`.

- [ ] **Step 5.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionSchedule"
```

---

### Task 6: Phase A Regression — Full Doc Page Review

**Purpose:** After 5 inserts into the same file, the section ordering, table of contents, and sidebar must be coherent. This task is a human (or agent) walking the whole page.

**Files:**
- Read: `docs-site/src/content/docs/components/fusion-components.md` (full file)

- [ ] **Step 6.1: Read the file top-to-bottom**

Confirm:
- Section order within the surface group reads `FusionAccordion` → `FusionGrid` → `FusionSchedule` → `FusionTab`.
- `FusionColorPicker` appears after `FusionAutoComplete` (or wherever alphabetical order places it among inputs).
- Every new section has: description paragraph, property table, at least 2 `csharp` code blocks, and a mutation extensions table.
- No duplicate `---` separators (caused by bad anchor picks).
- No dangling heading levels — each `##` is followed by `###` subheadings, nothing jumps to `#`.

- [ ] **Step 6.2: Rebuild docs with pagefind enabled**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built` and pagefind indexes the new anchors.

- [ ] **Step 6.3: Preview and click through**

```bash
cd docs-site && npm run preview -- --port 4321 &
```

Open `http://localhost:4321/components/fusion-components/`. Use the on-page table of contents (Starlight generates one from H2/H3 headings) to jump to each new section. Verify every anchor works. Kill the preview.

- [ ] **Step 6.4: Run the search index**

In the preview, use the site search (`/`) and search for each of: `ColorPicker`, `Accordion`, `Tab`, `Grid`, `Schedule`. Every term should return the fusion-components page as a top hit.

- [ ] **Step 6.5: No commit**

Step 6 is a review gate. If anything in 6.1–6.4 fails, return to the relevant Task 1–5 and amend. Only when the full page is clean do you move to Phase B.

---

## Phase B — Scaffold + Document Components Without Sandboxes

Dialog and Tooltip have shipped C# code but no sandbox view. Per worktree `CLAUDE.md`: "Every code example comes from a working, verified sandbox page." So Phase B creates a minimum viable sandbox, verifies it in the browser, then documents it — in that order. If the user decides Phase B is out of scope for this branch, stop after Phase A.

### Task 7: Scaffold FusionDialog Sandbox

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialog.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialogBuilder.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialogEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialogExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialogHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionDialog/FusionDialogReactiveExtensions.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml` (for structure)
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Accordion/AccordionModel.cs` (for structure)
- Inspect: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/` — find the controller that routes `/Sandbox/Components/Fusion/Accordion`. Decide: does that controller have a generic `Fusion` action that switches on a name, or is there one controller per component? This determines where to wire the Dialog route.
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Dialog/DialogModel.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Dialog/Index.cshtml`
- Create or modify: the controller action that serves `/Sandbox/Components/Fusion/Dialog`

- [ ] **Step 7.1: Inspect controller patterns**

```bash
grep -rn "Fusion/Dialog\|FusionDialog\|Fusion/Accordion" Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/ | head
```

Decide which controller file owns Fusion component routes. Read it top-to-bottom.

- [ ] **Step 7.2: Write the model**

`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Dialog/DialogModel.cs`:

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Dialog;

public sealed class DialogModel
{
    public string ResidentName { get; set; } = "";
    public string DischargeReason { get; set; } = "";
    public bool ConfirmDischarge { get; set; }
}
```

- [ ] **Step 7.3: Wire the route**

Add an action (or fallthrough case) on the appropriate controller that returns `View(new DialogModel())`. Use the exact naming convention you saw in Step 7.1.

- [ ] **Step 7.4: Write the sandbox view**

`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Dialog/Index.cshtml`:

```cshtml
@model DialogModel
@using Alis.Reactive.Fusion.Components
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Dialog
@{
    ViewData["Title"] = "FusionDialog";
    var plan = Html.ReactivePlan<DialogModel>();

    Html.On(plan, t => t.DomReady(p =>
    {
        // Intentionally empty — Dialog opens on button click, not DomReady.
    }));
}

<native-vstack gap="Lg">
    <div>
        <native-heading level="H1">FusionDialog — Discharge Confirmation</native-heading>
        <native-text color="Secondary">
            Exercises FusionDialog render, Open / Close methods, and BeforeClose / Closed events.
            Senior living domain: confirming a resident discharge.
        </native-text>
    </div>

    <native-card><native-card-body>
        <native-heading level="H2">1. Open Dialog</native-heading>
        @(Html.NativeButton("open-dialog-btn", "Discharge Resident")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
            .Reactive(plan, evt => evt.Click, (_, p) =>
            {
                p.Component<FusionDialog>().Open();
            }))
    </native-card-body></native-card>

    <!-- The dialog itself. Use whatever render API FusionDialogHtmlExtensions exposes.
         Read FusionDialogHtmlExtensions.cs in Step 7.1 and copy its signature exactly. -->
    @{
        // Example shape — confirm the actual signature before writing:
        // Html.FusionDialog(plan, "discharge-dialog", b => b.Header("Confirm Discharge").Content(...));
    }

    <native-card><native-card-body>
        <native-heading level="H2">2. Event Log</native-heading>
        <div class="font-mono text-sm space-y-1">
            <p>BeforeClose: <span id="dialog-before-close" class="text-text-muted">&mdash;</span></p>
            <p>Closed: <span id="dialog-closed" class="text-text-muted">&mdash;</span></p>
        </div>
    </native-card-body></native-card>
</native-vstack>

@Html.RenderPlan(plan)
```

The placeholder `@{ ... }` block is there because the exact `Html.FusionDialog(...)` signature depends on what `FusionDialogHtmlExtensions.cs` exposes. Read that file first, then replace the placeholder with the correct call. Do NOT write untested code — if the file exposes only a builder-style API that needs more than `<native-card>` scaffolding, stop and ask the user.

- [ ] **Step 7.5: Build + run**

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
lsof -ti:5220 | xargs kill -9 2>/dev/null
dotnet run --project Alis.Reactive.SandboxApp
```

Expected: clean build, sandbox boots, no Razor exceptions.

- [ ] **Step 7.6: Verify in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Dialog`. Click the button. Confirm the dialog opens, the `Closed` event fires, the event-log element updates. If anything does not work, stop and fix the sandbox before touching docs.

- [ ] **Step 7.7: Commit**

```bash
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Dialog \
        Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Dialog \
        Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers
git commit -m "feat(sandbox): add FusionDialog sandbox page"
```

---

### Task 8: Document FusionDialog

**Files:**
- Read: all files from Task 7 (source + newly-created sandbox)
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 8.1: Extract the facts**

Same drill as Task 1.1. Dialog events: `BeforeOpen`, `Opened`, `BeforeClose`, `Closed`, `OverlayClick`.

- [ ] **Step 8.2: Verify the sandbox in the browser** (again — the sandbox must still work)

- [ ] **Step 8.3: Choose the code snippets**

2 snippets:
1. Render snippet — the `Html.FusionDialog(...)` call from the sandbox.
2. Open/event snippet — the button that calls `.Open()` + the `.Reactive(plan, evt => evt.Closed, ...)` block.

- [ ] **Step 8.4: Find the insertion anchor**

Alphabetical surface-group order now includes Dialog. Order becomes: `FusionAccordion` → `FusionDialog` → `FusionGrid` → `FusionSchedule` → `FusionTab`. Anchor: the `---` separator closing `FusionAccordion`.

- [ ] **Step 8.5: Insert the new section**

Fill in the shared skeleton. Section questions:
1. "How do I render a dialog?"
2. "How do I open it and react to close?"

- [ ] **Step 8.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`.

- [ ] **Step 8.7: Visually verify**

Preview, scroll to `#fusiondialog`.

- [ ] **Step 8.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionDialog"
```

---

### Task 9: Scaffold FusionTooltip Sandbox

**Files:**
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltip.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltipBuilder.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltipEvents.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltipExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltipHtmlExtensions.cs`
- Read: `Alis.Reactive.Fusion/Components/FusionTooltip/FusionTooltipReactiveExtensions.cs`
- Read: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml` (for structure)
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tooltip/TooltipModel.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tooltip/Index.cshtml`
- Create or modify: controller action for `/Sandbox/Components/Fusion/Tooltip`

- [ ] **Step 9.1: Inspect controller patterns** (same as Task 7.1)

- [ ] **Step 9.2: Write the model**

`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tooltip/TooltipModel.cs`:

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Tooltip;

public sealed class TooltipModel
{
    public string CareLevel { get; set; } = "Memory Care";
}
```

- [ ] **Step 9.3: Wire the route** (same pattern as Task 7.3)

- [ ] **Step 9.4: Write the sandbox view**

`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tooltip/Index.cshtml`:

```cshtml
@model TooltipModel
@using Alis.Reactive.Fusion.Components
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Tooltip
@{
    ViewData["Title"] = "FusionTooltip";
    var plan = Html.ReactivePlan<TooltipModel>();
}

<native-vstack gap="Lg">
    <div>
        <native-heading level="H1">FusionTooltip — Care Level Hints</native-heading>
        <native-text color="Secondary">
            Exercises FusionTooltip render, target binding, and BeforeRender / Opened / Closed events.
            Senior living domain: describing the differences between care levels on hover.
        </native-text>
    </div>

    <native-card><native-card-body>
        <native-heading level="H2">1. Hover for Care Level Detail</native-heading>
        <p id="care-level-target" class="inline-block rounded bg-accent/10 px-3 py-2">
            Memory Care
        </p>
    </native-card-body></native-card>

    <!-- The tooltip registration. Read FusionTooltipHtmlExtensions.cs first to learn
         the exact signature. Placeholder shape: -->
    @{
        // Html.FusionTooltip(plan, "care-level-target", b => b.Content("24/7 staff, ...").Position("TopCenter"));
    }

    <native-card><native-card-body>
        <native-heading level="H2">2. Event Log</native-heading>
        <div class="font-mono text-sm space-y-1">
            <p>Opened: <span id="tooltip-opened" class="text-text-muted">&mdash;</span></p>
            <p>Closed: <span id="tooltip-closed" class="text-text-muted">&mdash;</span></p>
        </div>
    </native-card-body></native-card>
</native-vstack>

@Html.RenderPlan(plan)
```

Same rule as Task 7.4: replace the placeholder `@{ ... }` block with the real call after reading `FusionTooltipHtmlExtensions.cs`. If the signature requires significant extra scaffolding, stop and ask.

- [ ] **Step 9.5: Build + run**

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
lsof -ti:5220 | xargs kill -9 2>/dev/null
dotnet run --project Alis.Reactive.SandboxApp
```

- [ ] **Step 9.6: Verify in the browser**

Open `http://localhost:5220/Sandbox/Components/Fusion/Tooltip`. Hover the target element. Confirm the tooltip appears and `Opened`/`Closed` events fire.

- [ ] **Step 9.7: Commit**

```bash
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/Tooltip \
        Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Tooltip \
        Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers
git commit -m "feat(sandbox): add FusionTooltip sandbox page"
```

---

### Task 10: Document FusionTooltip

**Files:**
- Read: all files from Task 9
- Modify: `docs-site/src/content/docs/components/fusion-components.md`

- [ ] **Step 10.1: Extract the facts**

Events: `BeforeRender`, `BeforeOpen`, `Opened`, `BeforeClose`, `Closed`.

- [ ] **Step 10.2: Verify the sandbox in the browser**

- [ ] **Step 10.3: Choose the code snippets**

2 snippets:
1. Render snippet — `Html.FusionTooltip(...)` binding to a target element.
2. Event snippet — `.Reactive(plan, evt => evt.Opened, ...)` (or whichever event the sandbox wires).

- [ ] **Step 10.4: Find the insertion anchor**

Alphabetical surface order now: `FusionAccordion` → `FusionDialog` → `FusionGrid` → `FusionSchedule` → `FusionTab` → `FusionTooltip`. Anchor: the `---` separator closing `FusionTab`.

- [ ] **Step 10.5: Insert the new section**

Fill in the shared skeleton. Section questions:
1. "How do I attach a tooltip to an element?"
2. "How do I react when the tooltip opens?"

- [ ] **Step 10.6: Rebuild the docs site**

```bash
cd docs-site && npm run build && cd ..
```

Expected: `40 page(s) built`.

- [ ] **Step 10.7: Visually verify**

Preview, scroll to `#fusiontooltip`.

- [ ] **Step 10.8: Commit**

```bash
git add docs-site/src/content/docs/components/fusion-components.md
git commit -m "docs(fusion): document FusionTooltip"
```

---

## Task 11: Final Regression — Full Build + Test Suites

**Purpose:** Prove the docs site and the sandbox still work end-to-end after 7 new sections and 2 new sandbox pages.

- [ ] **Step 11.1: Rebuild everything**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/.worktrees/docs-sync-preview1
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
cd docs-site && npm run build && cd ..
```

Expected: all three complete with zero errors.

- [ ] **Step 11.2: Run the unit test suites that could be affected by Phase B controller additions**

```bash
dotnet test tests/Alis.Reactive.UnitTests/Alis.Reactive.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests/Alis.Reactive.Fusion.UnitTests.csproj -nologo
```

Expected: all tests pass. Record the pass counts.

- [ ] **Step 11.3: Smoke-test all 7 Fusion sandbox pages**

Run the sandbox, visit each URL, confirm each page still renders:

| Component | URL |
|-----------|-----|
| FusionColorPicker | `http://localhost:5220/Sandbox/Components/ColorPicker` |
| FusionAccordion | `http://localhost:5220/Sandbox/Components/Accordion` |
| FusionTab | `http://localhost:5220/Sandbox/Components/Tab` |
| FusionGrid | `http://localhost:5220/Sandbox/Components/Grid` |
| FusionSchedule | `http://localhost:5220/Sandbox/Components/Schedule` |
| FusionDialog | `http://localhost:5220/Sandbox/Components/Dialog` |
| FusionTooltip | `http://localhost:5220/Sandbox/Components/Tooltip` |

- [ ] **Step 11.4: Final docs preview**

```bash
cd docs-site && npm run preview -- --port 4321 &
```

Open `http://localhost:4321/components/fusion-components/`. Confirm:
- Table of contents lists 7 new `##` headings plus the 15 pre-existing ones (22 total Fusion component sections).
- All new anchors in order work.
- Search returns the new components.

Kill the preview.

- [ ] **Step 11.5: Push the branch**

Only after explicit user approval. The worktree is on `docs/sync-release-preview1`, branched from `release/1.0.0-preview1`. Do not push without the user confirming the target remote and whether a PR should be opened.

```bash
# After user approval:
git push -u origin docs/sync-release-preview1
```

- [ ] **Step 11.6: No automatic PR**

Creating the PR is the user's call. Report the branch name and a summary of the commits added. Wait for instructions.

---

## Out of Scope (explicitly deferred)

The following came up during gap analysis but will NOT be addressed by this plan. Filing them here so nothing is forgotten.

1. **`FusionTemplate` / `FusionTemplateExpression` dedicated page.** These public static helpers are used by `FusionGrid` and `FusionDropDownList` column/field templates. Task 4 name-drops `FusionTemplateExpression` in the Grid section, which is enough context for now. A dedicated reference page for typed templates across components is a separate doc improvement.
2. **`api-reference.md` regeneration.** The auto-generated API reference page will drift as new components land in code. `npm run build:api-docs` (per worktree CLAUDE.md) regenerates it. A full regen is out of scope here; Phase B introduces two new sandbox views but zero new public API surface, so the generated reference should not change.
3. **Fusion input components currently documented but with thin coverage.** `FusionInputMask`, `FusionRichTextEditor`, `FusionFileUpload` each have one short section. Deepening them is doc improvement, not gap coverage.
4. **Native components.** Reverse-verified complete — all 11 documented.

---

## Self-Review (done by the plan author before hand-off)

- **Spec coverage:** Every missing component from the gap analysis has a task. Fusion component gaps: ColorPicker (Task 1), Accordion (Task 2), Tab (Task 3), Grid (Task 4), Schedule (Task 5), Dialog (Tasks 7–8), Tooltip (Tasks 9–10). Regression gates: Tasks 0, 6, 11.
- **Placeholder scan:** Two intentional placeholders remain — the `@{ ... }` shells in Task 7.4 and Task 9.4 where the Dialog/Tooltip render-call signature depends on reading the source first. Both are annotated as "read the file first, replace the placeholder" with an explicit stop-and-ask escape hatch. These are not skipped steps; they are known unknowns that the plan cannot resolve without touching the source files at execution time.
- **Type consistency:** All tasks reference the same file (`docs-site/src/content/docs/components/fusion-components.md`) and the same skeleton (defined in the header). Section ordering is explicit: alphabetical within the surface group, final target order listed in Task 10.4.
- **Expected page count drift:** The plan assumes every insertion keeps the total page count at 40 because sections are added to an existing file. If Astro/Starlight ever starts creating a page per `##` heading (it currently does not), re-baseline in Task 0.
- **Worktree safety:** Every task starts from the worktree absolute path. No task reads `.codex-worktrees/*` or the main worktree.

---

**Plan complete.** Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints after Tasks 0, 6, 11.

Which approach?
