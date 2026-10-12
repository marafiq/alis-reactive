// alis.form (grammar.md 6.4): validation messages, scope, unmounted fields, server errors, live wiring.

import { afterEach, beforeAll, describe, expect, it } from "vitest";
import type { FormField, FormPluginData, ModelPluginData } from "../types/index";
import { registerModule, resetModulesForTests } from "../objects/modules";
import { Owner } from "../objects/owner";
import { startOwner } from "../plans/start";
import { formModule, resetFormsForTests, type FormObject } from "../modules/form/form";
import { modelModule, resetModelForTests } from "../modules/model";
import { e, element, fakeWidget, plan, plugin, s } from "./support/plan";

beforeAll(() => {
  resetModulesForTests();
  registerModule("alis.form", formModule);
  registerModule("alis.model", modelModule);
});

afterEach(() => {
  resetFormsForTests();
  resetModelForTests();
  document.body.innerHTML = "";
});

// A required text field: the check passes when the value is not blank.
function requiredField(id: string, name: string, message: string, when = e.lit(true)): FormField {
  return {
    id,
    name,
    value: e.get(e.obj(id), ["value"], s.string),
    tests: [{ rule: "required", check: e.fn(["v"], e.op("not", e.op("empty", e.ref("v")))) as never, when, message }],
  };
}

function render(html: string): void {
  document.body.insertAdjacentHTML("beforeend", html);
}

function field(id: string, value = ""): string {
  return `<input id="${id}" value="${value}"><span><span id="${id}_error" hidden></span></span>`;
}

function startForm(
  fields: FormField[],
  containerId = "admission",
  summary = "Test_Plan_validation_summary",
  others: string[] = [],
): FormObject {
  const data: FormPluginData = { kind: "form", summary, fields };
  const objects: Record<string, ReturnType<typeof element>> = { form: plugin("alis.form", containerId, data) };
  for (const id of [...fields.map(f => f.id), ...others]) objects[id] = element(id, "native");
  startOwner(Owner.page(), [plan(objects, [])]);
  return formModule.object(containerId) as FormObject;
}

const errorText = (id: string) => document.getElementById(`${id}_error`)?.textContent;

describe("validate", () => {
  it("shows the first failing test's message inline and returns false", () => {
    render(`<form id="admission">${field("Name")}</form><div id="Test_Plan_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    expect(form.validate()).toBe(false);
    expect(errorText("Name")).toBe("Enter the resident's name.");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("skips a test whose condition is false", () => {
    render(`<form id="admission">${field("VaId")}<input id="IsVeteran" type="checkbox"></form>`);
    const veteran = e.get(e.obj("IsVeteran"), ["checked"], s.boolean);
    const form = startForm([requiredField("VaId", "VaId", "'VA ID' is required.", veteran)], "admission",
      "Test_Plan_validation_summary", ["IsVeteran"]);
    (document.getElementById("IsVeteran") as HTMLInputElement).checked = false;
    expect(form.validate()).toBe(true);
  });

  it("ignores a field outside the container", () => {
    render(`<form id="admission"></form>${field("Name")}`);
    expect(startForm([requiredField("Name", "Name", "msg")]).validate()).toBe(true);
  });

  it("puts a message in the summary when the inline slot cannot render", () => {
    render(`<form id="admission"><input id="Name"><div hidden><span id="Name_error"></span></div></form>` +
      `<div id="Test_Plan_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.validate();
    const summary = document.getElementById("Test_Plan_validation_summary")!;
    expect(summary.hidden).toBe(false);
    expect(summary.textContent).toBe("Enter the resident's name.");
  });

  it("fails for an unmounted field with an active test, and skips it when every test is inactive", () => {
    render(`<form id="admission"></form><div id="Test_Plan_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Ghost", "Ghost is required.")]).validate()).toBe(false);
    resetFormsForTests();
    expect(startForm([requiredField("Ghost2", "Ghost2", "msg", e.lit(false))]).validate()).toBe(true);
  });

  it("fails when the container is missing and the form has tests", () => {
    render(field("Name", "x"));
    expect(startForm([requiredField("Name", "Name", "msg")], "nowhere").validate()).toBe(false);
  });
});

describe("server errors", () => {
  it("places each error on the field with that binding name, joining messages, else in the summary", () => {
    render(`<form id="admission">${field("Ns__Name")}</form><div id="Test_Plan_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Ns__Name", "Name", "msg")]);
    form.showErrors({ errors: { Name: ["Too short", "Not unique"], Room: "Taken" } });
    expect(errorText("Ns__Name")).toBe("Too short, Not unique");
    expect(document.getElementById("Test_Plan_validation_summary")!.textContent).toBe("Taken");
  });

  it("validates in the browser when the response carried no problem record", () => {
    render(`<form id="admission">${field("Name")}</form>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.showErrors(undefined);
    expect(errorText("Name")).toBe("Enter the resident's name.");
  });
});

describe("live", () => {
  it("clears a field's message while typing and revalidates it on blur", () => {
    render(`<form id="admission">${field("Name")}</form>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.validate();
    const input = document.getElementById("Name") as HTMLInputElement;

    input.value = "M";
    input.dispatchEvent(new Event("input"));
    expect(errorText("Name")).toBe("");

    input.value = "";
    input.dispatchEvent(new Event("blur"));
    expect(errorText("Name")).toBe("Enter the resident's name.");
  });

  it("revalidates when the input commits, by whatever name its component raises the commit", () => {
    render(`<form id="admission"></form>`);
    const rating = fakeWidget("Rating", { value: 4 });
    document.getElementById("admission")!.append(rating, Object.assign(document.createElement("span"), { id: "Rating_error" }));
    const scored: FormField = {
      id: "Rating",
      name: "Rating",
      value: e.get(e.obj("Rating"), ["value"], s.number),
      tests: [{ rule: "min", check: e.fn(["v"], e.op("gte", e.ref("v"), e.lit(1))) as never, when: e.lit(true), message: "Rate the visit." }],
    };
    const model: ModelPluginData = {
      kind: "model",
      inputs: { Rating: { name: "Rating", body: ["Rating"], value: scored.value, shape: s.number, committed: "valueChanged" } },
    };
    const form: FormPluginData = { kind: "form", summary: "Test_Plan_validation_summary", fields: [scored] };
    startOwner(Owner.page(), [plan({
      Rating: element("Rating", "fake"),
      model: plugin("alis.model", "Test.Plan", model),
      form: plugin("alis.form", "admission", form),
    }, [])]);

    rating.widget.value = 0;
    rating.widget.trigger("change", {});
    expect(errorText("Rating")).toBe("");

    rating.widget.trigger("valueChanged", { value: 0 });
    expect(errorText("Rating")).toBe("Rate the visit.");
  });
});
