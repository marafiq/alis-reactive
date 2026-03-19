import { describe, it, expect, beforeEach } from "vitest";
import { boot, mergePlan, getBootedPlan, resetBootStateForTests } from "../lifecycle/boot";
import type { Entry, ValidationField, ComponentEntry } from "../types";

/**
 * Tests the enrichment behavior when validation descriptors exist in the root plan
 * and components arrive later via partial merge. This is the core AJAX partial pattern:
 *   1. Root plan has validation rules (extracted from validator at C# render time)
 *   2. Partial arrives with components that match those rules
 *   3. After merge, validation fields get enriched with fieldId/vendor/readExpr
 */

function comp(id: string, vendor: "native" | "fusion" = "native", readExpr = "value"): ComponentEntry {
  return { id, vendor, readExpr };
}

function validationHttpEntry(formId: string, fields: ValidationField[]): Entry {
  return {
    trigger: { kind: "custom-event", event: "submit" },
    reaction: {
      kind: "http",
      request: {
        verb: "POST",
        url: "/save",
        validation: { formId, fields },
      },
    },
  };
}

function getValidationFields(planId: string): ValidationField[] | undefined {
  const plan = getBootedPlan(planId);
  if (!plan) return undefined;
  for (const entry of plan.entries) {
    if (entry.reaction.kind === "http") {
      const req = (entry.reaction as any).request;
      if (req?.validation) return req.validation.fields;
    }
  }
  return undefined;
}

beforeEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
});

// ── Root plan with validation + no components ──────────

describe("When root plan has validation rules but no components", () => {
  it("validation fields start unenriched", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [validationHttpEntry("form", [
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "Street required" }] },
        { fieldName: "Address.City", rules: [{ rule: "required", message: "City required" }] },
        { fieldName: "Address.ZipCode", rules: [
          { rule: "required", message: "Zip required" },
          { rule: "regex", message: "5 digits", constraint: "^\\d{5}$" },
        ] },
      ])],
    });

    const fields = getValidationFields("Resident.Model")!;
    expect(fields[0].fieldId).toBeUndefined();
    expect(fields[1].fieldId).toBeUndefined();
    expect(fields[2].fieldId).toBeUndefined();
  });
});

// ── Partial merge enriches validation ──────────────────

describe("When partial with address components merges into plan with validation", () => {
  it("all matching validation fields become enriched", () => {
    boot({
      planId: "Resident.Model",
      components: { "Name": comp("name-input") },
      entries: [validationHttpEntry("form", [
        { fieldName: "Name", rules: [{ rule: "required", message: "Name required" }] },
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "Street required" }] },
        { fieldName: "Address.City", rules: [{ rule: "required", message: "City required" }] },
        { fieldName: "Address.ZipCode", rules: [
          { rule: "required", message: "Zip required" },
          { rule: "regex", message: "5 digits", constraint: "^\\d{5}$" },
        ] },
      ])],
    });

    // Name is enriched from root, address fields are not
    const fieldsBefore = getValidationFields("Resident.Model")!;
    expect(fieldsBefore[0].fieldId).toBe("name-input"); // Name — root component
    expect(fieldsBefore[1].fieldId).toBeUndefined(); // Address.Street — no component
    expect(fieldsBefore[2].fieldId).toBeUndefined(); // Address.City
    expect(fieldsBefore[3].fieldId).toBeUndefined(); // Address.ZipCode

    // Merge partial with address components
    mergePlan({
      planId: "Resident.Model",
      sourceId: "address-container",
      components: {
        "Address.Street": comp("street-input"),
        "Address.City": comp("city-input"),
        "Address.ZipCode": comp("zip-input"),
      },
      entries: [],
    });

    const fieldsAfter = getValidationFields("Resident.Model")!;
    expect(fieldsAfter[0].fieldId).toBe("name-input"); // Name — still enriched
    expect(fieldsAfter[1].fieldId).toBe("street-input"); // Address.Street — NOW enriched
    expect(fieldsAfter[1].vendor).toBe("native");
    expect(fieldsAfter[1].readExpr).toBe("value");
    expect(fieldsAfter[2].fieldId).toBe("city-input");
    expect(fieldsAfter[3].fieldId).toBe("zip-input");
  });

  it("validation rules (including regex) are preserved on enriched fields", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [validationHttpEntry("form", [
        { fieldName: "Address.ZipCode", rules: [
          { rule: "required", message: "Zip required" },
          { rule: "regex", message: "5 digits", constraint: "^\\d{5}$" },
        ] },
      ])],
    });

    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: { "Address.ZipCode": comp("zip") },
      entries: [],
    });

    const fields = getValidationFields("Resident.Model")!;
    expect(fields[0].fieldId).toBe("zip");
    expect(fields[0].rules).toHaveLength(2);
    expect(fields[0].rules[0].rule).toBe("required");
    expect(fields[0].rules[1].rule).toBe("regex");
    expect(fields[0].rules[1].constraint).toBe("^\\d{5}$");
  });
});

// ── Partial removal unenriches ──────────────────────────

describe("When partial with address components is removed", () => {
  it("address validation fields revert to unenriched", () => {
    boot({
      planId: "Resident.Model",
      components: { "Name": comp("name-input") },
      entries: [validationHttpEntry("form", [
        { fieldName: "Name", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "req" }] },
      ])],
    });

    // Add then remove
    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: { "Address.Street": comp("street") },
      entries: [],
    });

    expect(getValidationFields("Resident.Model")![1].fieldId).toBe("street");

    // Remove
    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: {},
      entries: [],
    });

    const fields = getValidationFields("Resident.Model")!;
    expect(fields[0].fieldId).toBe("name-input"); // Root component survives
    expect(fields[1].fieldId).toBeUndefined(); // Address reverts
  });
});

// ── Partial reload re-enriches ──────────────────────────

describe("When partial is reloaded (remove + add) with different components", () => {
  it("validation fields match the new components", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [validationHttpEntry("form", [
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] },
      ])],
    });

    // First load
    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: {
        "Address.Street": comp("street-v1"),
        "Address.City": comp("city-v1"),
      },
      entries: [],
    });

    // Reload with different IDs
    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: {
        "Address.Street": comp("street-v2"),
        "Address.City": comp("city-v2"),
      },
      entries: [],
    });

    const fields = getValidationFields("Resident.Model")!;
    expect(fields[0].fieldId).toBe("street-v2");
    expect(fields[1].fieldId).toBe("city-v2");
  });
});

// ── Conditional validation with cross-plan components ───

describe("When validation has conditional rules and condition source comes from partial", () => {
  it("condition field is enriched after partial merge", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [validationHttpEntry("form", [
        { fieldName: "CareLevel", rules: [] },
        { fieldName: "PhysicianName", rules: [
          { rule: "required", message: "Physician required",
            when: { field: "CareLevel", op: "neq", value: "Independent" } },
        ] },
      ])],
    });

    // Partial provides the CareLevel component
    mergePlan({
      planId: "Resident.Model",
      sourceId: "care-slot",
      components: {
        "CareLevel": comp("care-dropdown"),
        "PhysicianName": comp("physician-input"),
      },
      entries: [],
    });

    const fields = getValidationFields("Resident.Model")!;
    expect(fields[0].fieldId).toBe("care-dropdown"); // condition source enriched
    expect(fields[1].fieldId).toBe("physician-input"); // conditional field enriched
    expect(fields[1].rules[0].when!.field).toBe("CareLevel");
    expect(fields[1].rules[0].when!.op).toBe("neq");
  });
});

// ── Full validation + enrichment integration ────────────

// ── Enrichment through parallel-http and conditional reactions ──

describe("When validation lives inside a parallel-http reaction", () => {
  it("enriches validation fields after merge", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "submit" },
        reaction: {
          kind: "parallel-http",
          requests: [{
            verb: "POST",
            url: "/save",
            validation: {
              formId: "form",
              fields: [{ fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] }],
            },
          }],
        },
      }],
    });

    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: { "Address.City": comp("city") },
      entries: [],
    });

    const plan = getBootedPlan("Resident.Model")!;
    const reaction = plan.entries[0].reaction as any;
    expect(reaction.requests[0].validation.fields[0].fieldId).toBe("city");
  });
});

describe("When validation lives inside a conditional reaction branch", () => {
  it("enriches validation fields after merge", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "submit" },
        reaction: {
          kind: "conditional",
          branches: [{
            guard: null,
            reaction: {
              kind: "http",
              request: {
                verb: "POST",
                url: "/save",
                validation: {
                  formId: "form",
                  fields: [{ fieldName: "Name", rules: [{ rule: "required", message: "req" }] }],
                },
              },
            },
          }],
        },
      }],
    });

    mergePlan({
      planId: "Resident.Model",
      sourceId: "slot",
      components: { "Name": comp("name-input") },
      entries: [],
    });

    const plan = getBootedPlan("Resident.Model")!;
    const branch = (plan.entries[0].reaction as any).branches[0];
    expect(branch.reaction.request.validation.fields[0].fieldId).toBe("name-input");
  });
});

describe("When validation lives in a chained request", () => {
  it("enriches validation fields in the chained request after merge", () => {
    boot({
      planId: "Resident.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "submit" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/step1",
            chained: {
              verb: "POST",
              url: "/step2",
              validation: {
                formId: "form",
                fields: [{ fieldName: "Email", rules: [{ rule: "email", message: "bad" }] }],
              },
            },
          },
        },
      }],
    });

    mergePlan({
      planId: "Resident.Model",
      sourceId: "slot",
      components: { "Email": comp("email-input") },
      entries: [],
    });

    const plan = getBootedPlan("Resident.Model")!;
    const chained = (plan.entries[0].reaction as any).request.chained;
    expect(chained.validation.fields[0].fieldId).toBe("email-input");
  });
});

// ── Integration: validate after merge ───────────────────

describe("When validate() is called after partial merge", () => {
  it("enriched fields validate against DOM values", async () => {
    document.body.innerHTML = `
      <form id="form">
        <input id="zip" name="Address.ZipCode" value="bad" />
        <span data-valmsg-for="Address.ZipCode"></span>
      </form>
    `;

    boot({
      planId: "Resident.Model",
      components: {},
      entries: [validationHttpEntry("form", [
        { fieldName: "Address.ZipCode", rules: [
          { rule: "required", message: "Zip required" },
          { rule: "regex", message: "5 digits", constraint: "^\\d{5}$" },
        ] },
      ])],
    });

    const { validate } = await import("../validation");

    // Before merge: field unenriched → blocks (fail-closed, summary error)
    const descBefore = (getBootedPlan("Resident.Model")!.entries[0].reaction as any).request.validation;
    expect(validate(descBefore)).toBe(false); // unenriched = block

    // Merge partial with component
    mergePlan({
      planId: "Resident.Model",
      sourceId: "addr",
      components: { "Address.ZipCode": comp("zip") },
      entries: [],
    });

    // After merge: field enriched → validates → regex fails on "bad"
    const descAfter = (getBootedPlan("Resident.Model")!.entries[0].reaction as any).request.validation;
    expect(descAfter.fields[0].fieldId).toBe("zip");
    expect(validate(descAfter)).toBe(false);

    const span = document.querySelector('span[data-valmsg-for="Address.ZipCode"]');
    expect(span!.textContent).toBe("5 digits");
  });
});
