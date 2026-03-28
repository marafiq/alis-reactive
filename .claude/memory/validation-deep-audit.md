---
name: validation-deep-audit
description: Deep audit findings for validation module — SF live-clear broken, DOM scanning violations, container explained, SF FormValidator patterns
type: project
---

# Validation Module Deep Audit — March 2026

## Critical Finding: live-clear.ts is BROKEN for Syncfusion components

### The Problem
`live-clear.ts` listens for native DOM `"input"` and `"change"` events on the form container via event delegation. SF components fire events via **callbacks on ej2 instances**, not native DOM events. These callbacks never reach the container listener.

**Result:** After validation shows errors, typing in an SF DatePicker/NumericTextBox/ComboBox does NOT auto-clear the error. Only native `<input>` elements auto-clear.

### How SF FormValidator Does It (from their GitHub source)
SF's own FormValidator wires **per-element** listeners directly on each `<input>`:
- Checkable inputs: `click` event
- `<select>`: `change` event
- Everything else: `focusout` + `keyup`
- Found via wide selector: `selectAll('input:not([type=reset]):not([type=button]), select, textarea')`
- Rules keyed by `element.name` attribute
- Error element found by: `[data-valmsg-for="inputId"]` matching the **input.id**, or `data-msg-containerid`, or custom placement

### Our InputField DOM Structure

**Native TextBox:**
```html
<div class="flex flex-col gap-1.5">
  <label for="Scope__Name">Name</label>
  <input id="Scope__Name" name="Name" type="text" />
  <span data-valmsg-for="Name" class="text-danger"></span>
</div>
```

**Fusion NumericTextBox:**
```html
<div class="flex flex-col gap-1.5">
  <label for="Scope__Amount">Amount</label>
  <span class="e-control-wrapper" id="Scope__Amount">  ← ID is on wrapper, not input
    <input class="e-numerictextbox e-input" name="Amount" />  ← inner input, no ID
    <span class="e-spin-up"></span>
    <span class="e-spin-down"></span>
  </span>
  <span data-valmsg-for="Amount" class="text-danger"></span>
</div>
```

### Key Facts About IDs
- `IdGenerator.For<TModel, TProp>()` produces the same ID for native and fusion
- Format: `{Namespace_TypeName}__{PropertyPath}` — e.g., `Scope__Amount`
- For native: ID is on the `<input>` element
- For fusion: ID is on the SF wrapper `<span class="e-control-wrapper">` — the element with ej2_instances
- `data-valmsg-for` uses the **bindingPath** (name attribute), NOT the ID

## Issues Ranked

| Issue | Severity | Description |
|-------|----------|-------------|
| SF live-clear broken | **HIGH** | SF callbacks don't bubble as DOM events. Live-clearing only works for native inputs. |
| DOM scanning for error spans | **MEDIUM** | `querySelector('[data-valmsg-for="..."]')` scans DOM. Violates ID-aware principle. |
| `data-valmsg-for` is name not ID | **MEDIUM** | Could use predictable ID like `{elementId}_error` instead of scanning by data attribute. |
| showInline/showServerErrorInline duplication | LOW | Near-identical functions, only difference is how field is looked up. |

## Design Constraints for Fix (User's Requirements)
1. **Vendor-agnostic** — same approach for native and SF, no `if (vendor === "fusion")`
2. **ID-aware** — use element IDs, not querySelector scanning
3. **No internal structure dependency** — don't depend on SF's `e-control-wrapper` nesting
4. **No wide selectors** — don't scan DOM for inputs
5. **Must support partials** — AJAX-loaded forms get validation wired too
6. **BDD tests first** — write failing tests that expose the SF problem before fixing
7. **Must support future array validations** — design must scale

## Fixed (This Session)
1. **SF live-clear** — per-field wiring via resolveRoot (commit d1a4e73)
2. **Error span IDs** — `{fieldId}_error` convention, getElementById (commit 99c67fd)
3. **Summary IDs** — `{planId_sanitized}_validation_summary`, getElementById (commit 7759848)

## Remaining Issue: Fusion Array Components
- MultiSelect (DietaryRestrictions) — validation may not work for array values
- Page: /Sandbox/ComponentGather (http://localhost:5220/Sandbox/ComponentGather)
- Need BDD tests for components that return arrays
- readExpr for MultiSelect returns array — validation rule-engine needs to handle array values

## Potential Fix Direction
The plan already carries `fieldId`, `vendor`, and `readExpr` for every validation field. Live-clear could use the same vendor-agnostic pattern as trigger.ts:
- `resolveRoot(el, vendor)` to get the vendor root
- Wire the appropriate event on the vendor root (SF: ej2 "change" callback, native: DOM "input"/"change")
- Use `fieldId` to find the error element by predictable ID instead of scanning

This aligns with how trigger.ts already wires component events vendor-agnostically.

## SF FormValidator Source
GitHub: https://github.com/syncfusion/ej2-javascript-ui-controls
Path: src/inputs/src/form-validator/form-validator.ts
- Rules schema: `{ [name: string]: { required?: boolean, email?: boolean, minLength?: number, ... } }`
- Schema is NOT discriminated unions — it's a flat object with rule names as keys
- Their schema evolved over time (has both `data-val-*` ASP.NET conventions AND their own rules)
- Their error element ID: `{inputName}-info` — predictable, ID-based
