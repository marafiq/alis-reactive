import { beforeEach, describe, expect, it } from "vitest";
import { showServerErrors, validate, wireLiveValidation } from "../validation";
import { createPlan, nativeTextContract, scalar } from "./support/v2-fixtures";

function summaryId(planId: string): string {
  return planId.replace(/[.+]/g, "_") + "_validation_summary";
}

describe("when validating through v2 bindings", () => {
  const planId = "Resident.Editor";

  beforeEach(() => {
    document.body.innerHTML = `
      <form id="resident-form">
        <input id="resident-name" value="" />
        <span id="resident-name_error" hidden style="display:none"></span>

        <input id="resident-email" value="" />
        <span id="resident-email_error" hidden style="display:none"></span>

        <input id="resident-confirm-email" value="" />
        <span id="resident-confirm-email_error" hidden style="display:none"></span>

        <input id="resident-alternate-email" value="" />
        <span id="resident-alternate-email_error" hidden style="display:none"></span>

        <div id="monthly-rate-host"></div>
        <span id="monthly-rate-host_error" hidden style="display:none"></span>

        <div hidden>
          <input id="resident-end" value="2026-04-01" />
          <span id="resident-end_error" hidden style="display:none"></span>
        </div>

        <input id="resident-start" value="2026-04-10" />
        <span id="resident-start_error" hidden style="display:none"></span>
      </form>
      <div id="${summaryId(planId)}" hidden></div>
    `;
  });

  function createValidationPlan() {
    return createPlan({
      planId,
      contracts: {
        "native.text": nativeTextContract,
      },
      objects: {
        residentName: { contract: "native.text", elementId: "resident-name" },
        residentEmail: { contract: "native.text", elementId: "resident-email" },
        residentConfirmEmail: { contract: "native.text", elementId: "resident-confirm-email" },
        residentAlternateEmail: { contract: "native.text", elementId: "resident-alternate-email" },
        residentStart: { contract: "native.text", elementId: "resident-start" },
        residentEnd: { contract: "native.text", elementId: "resident-end" },
      },
      bindings: {
        "Resident.Name": {
          object: "residentName",
          valueMember: "value",
          shape: scalar("string"),
        },
        "Resident.Email": {
          object: "residentEmail",
          valueMember: "value",
          shape: scalar("string"),
        },
        "Resident.ConfirmEmail": {
          object: "residentConfirmEmail",
          valueMember: "value",
          shape: scalar("string"),
        },
        "Resident.AlternateEmail": {
          object: "residentAlternateEmail",
          valueMember: "value",
          shape: scalar("string"),
        },
        "Resident.Start": {
          object: "residentStart",
          valueMember: "value",
          shape: scalar("date"),
        },
        "Resident.End": {
          object: "residentEnd",
          valueMember: "value",
          shape: scalar("date"),
        },
      },
    });
  }

  it("shows visible binding errors inline and hidden binding errors in the summary", () => {
    const plan = createValidationPlan();
    const valid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.Name",
          rules: [{ rule: "required", message: "Name is required." }],
        },
        {
          binding: "Resident.End",
          rules: [{
            rule: "gt",
            message: "End must be after start.",
            otherBinding: "Resident.Start",
            as: scalar("date"),
          }],
        },
      ],
    });

    expect(valid).toBe(false);
    expect(document.getElementById("resident-name_error")?.textContent).toBe("Name is required.");
    expect(document.getElementById(summaryId(planId))?.textContent).toContain("End must be after start.");
  });

  it("clears inline errors on input and revalidates on blur", () => {
    const plan = createValidationPlan();
    const desc = {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.Name",
          rules: [{ rule: "required", message: "Name is required." }],
        },
      ],
    };

    validate(plan, desc);
    wireLiveValidation(plan, desc);

    const input = document.getElementById("resident-name") as HTMLInputElement;
    const span = document.getElementById("resident-name_error") as HTMLElement;

    input.dispatchEvent(new Event("input", { bubbles: true }));
    expect(span.textContent).toBe("");
    expect(span.hasAttribute("hidden")).toBe(true);

    input.dispatchEvent(new Event("blur", { bubbles: true }));
    expect(span.textContent).toBe("Name is required.");

    input.value = "Grace";
    input.dispatchEvent(new Event("input", { bubbles: true }));
    input.dispatchEvent(new Event("blur", { bubbles: true }));
    expect(span.textContent).toBe("");
    expect(span.hasAttribute("hidden")).toBe(true);
  });

  it("routes server-side validation errors by binding name", () => {
    const plan = createValidationPlan();
    const desc = {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.Name",
          rules: [{ rule: "required", message: "Name is required." }],
        },
      ],
    };

    showServerErrors(plan, desc, {
      errors: {
        "Resident.Name": ["Duplicate resident."],
        "Resident.Unknown": ["Missing target."],
      },
    });

    expect(document.getElementById("resident-name_error")?.textContent).toBe("Duplicate resident.");
    expect(document.getElementById(summaryId(planId))?.textContent).toContain("Missing target.");
  });

  it("treats an empty numeric binding as missing even when the binding shape is number", () => {
    const plan = createPlan({
      planId,
      contracts: {
        "fusion.numeric": {
          kind: "component",
          resolver: "fusion-instance",
          members: {
            value: {
              kind: "property",
              path: [{ prop: "value" }],
              shape: scalar("number"),
              access: "read",
            },
          },
        },
      },
      objects: {
        monthlyRate: { contract: "fusion.numeric", elementId: "monthly-rate-host" },
      },
      bindings: {
        "Resident.MonthlyRate": {
          object: "monthlyRate",
          valueMember: "value",
          shape: scalar("number"),
        },
      },
    });

    const host = document.getElementById("monthly-rate-host") as HTMLElement & {
      ej2_instances?: Array<{ value: number | null }>;
    };
    host.ej2_instances = [{ value: null }];

    const valid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.MonthlyRate",
          rules: [{ rule: "required", message: "Monthly rate is required." }],
        },
      ],
    });

    expect(valid).toBe(false);
    expect(document.getElementById("monthly-rate-host_error")?.textContent).toBe("Monthly rate is required.");

    host.ej2_instances[0].value = 0;
    const revalidated = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.MonthlyRate",
          rules: [{ rule: "required", message: "Monthly rate is required." }],
        },
      ],
    });

    expect(revalidated).toBe(true);
    expect(document.getElementById("monthly-rate-host_error")?.textContent).toBe("");
  });

  it("compares string equalTo bindings as text instead of numeric values", () => {
    const plan = createValidationPlan();
    (document.getElementById("resident-email") as HTMLInputElement).value = "nurse@facility.com";
    (document.getElementById("resident-confirm-email") as HTMLInputElement).value = "backup@facility.com";

    const invalid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.ConfirmEmail",
          rules: [{
            rule: "equalTo",
            message: "Confirm email must match.",
            otherBinding: "Resident.Email",
            as: scalar("string"),
          }],
        },
      ],
    });

    expect(invalid).toBe(false);
    expect(document.getElementById("resident-confirm-email_error")?.textContent).toBe("Confirm email must match.");

    (document.getElementById("resident-confirm-email") as HTMLInputElement).value = "nurse@facility.com";

    const valid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.ConfirmEmail",
          rules: [{
            rule: "equalTo",
            message: "Confirm email must match.",
            otherBinding: "Resident.Email",
            as: scalar("string"),
          }],
        },
      ],
    });

    expect(valid).toBe(true);
    expect(document.getElementById("resident-confirm-email_error")?.textContent).toBe("");
  });

  it("compares string notEqualTo bindings as text instead of numeric values", () => {
    const plan = createValidationPlan();
    (document.getElementById("resident-email") as HTMLInputElement).value = "nurse@facility.com";
    (document.getElementById("resident-alternate-email") as HTMLInputElement).value = "nurse@facility.com";

    const invalid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.AlternateEmail",
          rules: [{
            rule: "notEqualTo",
            message: "Alternate email must differ.",
            otherBinding: "Resident.Email",
            as: scalar("string"),
          }],
        },
      ],
    });

    expect(invalid).toBe(false);
    expect(document.getElementById("resident-alternate-email_error")?.textContent).toBe("Alternate email must differ.");

    (document.getElementById("resident-alternate-email") as HTMLInputElement).value = "backup@facility.com";

    const valid = validate(plan, {
      formId: "resident-form",
      fields: [
        {
          binding: "Resident.AlternateEmail",
          rules: [{
            rule: "notEqualTo",
            message: "Alternate email must differ.",
            otherBinding: "Resident.Email",
            as: scalar("string"),
          }],
        },
      ],
    });

    expect(valid).toBe(true);
    expect(document.getElementById("resident-alternate-email_error")?.textContent).toBe("");
  });
});
