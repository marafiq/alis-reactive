import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField, ValidationRule } from "../../types";

// ══════════════════════════════════════════════════════════
// Bug #1: NaN fail-open — compareValues returns NaN,
// comparison operators (< > <= >=) all return false → rule passes
// ══════════════════════════════════════════════════════════

let ruleFails: typeof import("../../validation/rule-engine").ruleFails;

describe("Bug #1: NaN fail-open in comparison rules", () => {
  beforeEach(async () => {
    const mod = await import("../../validation/rule-engine");
    ruleFails = mod.ruleFails;
  });

  const noPeer = { readPeer: () => undefined };

  it("gt rule should FAIL when value is non-numeric string (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "gt", message: "must be > 0", constraint: 0, coerceAs: "number" };
    // "abc" coerces to NaN → compareValues returns NaN → NaN <= 0 is false → ruleFails returns false (passes)
    // BUG: should fail (return true) because we can't prove value > 0
    expect(ruleFails(rule, "abc", noPeer)).toBe(true);
  });

  it("min rule should FAIL when value is non-numeric string", () => {
    const rule: ValidationRule = { rule: "min", message: "min 5", constraint: 5, coerceAs: "number" };
    // "abc" → NaN → NaN < 0 is false → passes. BUG: should fail.
    expect(ruleFails(rule, "abc", noPeer)).toBe(true);
  });

  it("max rule should FAIL when value is non-numeric string", () => {
    const rule: ValidationRule = { rule: "max", message: "max 100", constraint: 100, coerceAs: "number" };
    // "abc" → NaN → NaN > 0 is false → passes. BUG: should fail.
    expect(ruleFails(rule, "abc", noPeer)).toBe(true);
  });

  it("lt rule should FAIL when value is non-numeric string", () => {
    const rule: ValidationRule = { rule: "lt", message: "must be < 10", constraint: 10, coerceAs: "number" };
    // "abc" → NaN → NaN >= 0 is false → passes. BUG: should fail.
    expect(ruleFails(rule, "abc", noPeer)).toBe(true);
  });
});

// ══════════════════════════════════════════════════════════
// Bug #2: Outside-form field passes — !container.contains(el) → return true
// Field exists in DOM but outside the form container → validation skips it
// ══════════════════════════════════════════════════════════

let validate: typeof import("../../validation/orchestrator").validate;

function enrichedField(id: string, rules: ValidationRule[]): ValidationField {
  return { fieldName: id, fieldId: id, vendor: "native", readExpr: "value", rules };
}

function desc(formId: string, fields: ValidationField[]): ValidationDescriptor {
  return { formId, planId: "Test.Plan", fields };
}

describe("Bug #2: Outside-form field passes validation", () => {
  beforeEach(async () => {
    const dom = new JSDOM(`<!DOCTYPE html><html><body>
      <form id="form">
        <input id="InsideField" value="" />
        <span id="InsideField_error" data-valmsg-for="InsideField"></span>
      </form>
      <!-- This field is OUTSIDE the form -->
      <input id="OutsideField" value="" />
      <span id="OutsideField_error" data-valmsg-for="OutsideField"></span>
      <div id="Test_Plan_validation_summary" data-reactive-validation-summary="Test.Plan" hidden></div>
    </body></html>`);

    (globalThis as any).document = dom.window.document;
    (globalThis as any).HTMLElement = dom.window.HTMLElement;
    (globalThis as any).Event = dom.window.Event;

    const mod = await import("../../validation/orchestrator");
    validate = mod.validate;
  });

  it("field outside form should NOT silently pass — should block or route to summary", () => {
    // OutsideField is required but lives outside <form id="form">
    // Current: !container.contains(el) → return true (passes silently)
    // BUG: required field passes validation just because it's outside the form container
    const result = validate(desc("form", [
      enrichedField("OutsideField", [{ rule: "required", message: "Outside field required" }]),
    ]));
    // If this is a real field the developer declared, it should NOT silently pass
    expect(result).toBe(false);
  });
});

// Bug #3 (silent block with no summary) — skipped per review, lower priority
