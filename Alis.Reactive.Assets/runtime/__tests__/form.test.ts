// alis.form (grammar.md 6.4): validation messages, scope, unmounted fields, server errors, live checks.

import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { FormField, FormPluginData, ModelPluginData } from "../types/index";
import type { FormObject } from "../plugins/form/form";
import { e, element, fakeWidget, plan, plugin, s } from "./support/plan";

// Each test runs a fresh runtime: the plugin registry, the forms and the model start empty, with no
// reset hook shipped in the runtime for it.
let Owner: (typeof import("../objects/owner"))["Owner"];
let startOwner: (typeof import("../plans/start"))["startOwner"];
let formPlugin: (typeof import("../plugins/form/form"))["formPlugin"];
let modelPlugin: (typeof import("../plugins/model"))["modelPlugin"];

beforeEach(async () => {
  vi.resetModules();
  ({ Owner } = await import("../objects/owner"));
  ({ startOwner } = await import("../plans/start"));
  ({ formPlugin } = await import("../plugins/form/form"));
  ({ modelPlugin } = await import("../plugins/model"));
  const { registerPlugin } = await import("../objects/plugins");
  registerPlugin("alis.form", formPlugin);
  registerPlugin("alis.model", modelPlugin);
});

afterEach(() => {
  document.body.innerHTML = "";
});

// A field and the input that renders it: a required text field, whose check passes when the value is not blank.
interface Rendered {
  readonly id: string;
  readonly field: FormField;
}

function requiredField(id: string, name: string, message: string, when = e.lit(true)): Rendered {
  return { id, field: { name, tests: [required(message, when)] } };
}

function required(message: string, when = e.lit(true)): FormField["tests"][number] {
  return { rule: "required", check: e.fn(["v", "i"], e.op("not", e.op("empty", e.ref("v")))) as never, when: e.fn(["i"], when) as never, message };
}

function textInput(id: string, name: string): ModelPluginData["inputs"][string] {
  return { name, body: [name], value: e.as(e.get(e.obj(id), ["value"]), s.string), shape: s.string, committed: "change", list: false };
}

function render(html: string): void {
  document.body.insertAdjacentHTML("beforeend", html);
}

function field(id: string, value = ""): string {
  return `<input id="${id}" value="${value}"><span><span id="${id}_error" hidden></span></span>`;
}

// The view renders each field's input, so the page's table holds it under its binding name (6.3).
function startForm(
  rendered: Rendered[],
  containerId = "admission",
  summary = "admission_validation_summary",
  others: string[] = [],
): FormObject {
  const data: FormPluginData = { kind: "form", summary, fields: rendered.map(each => each.field) };
  const inputs: ModelPluginData["inputs"] = {};
  for (const each of rendered) inputs[each.id] = textInput(each.id, each.field.name);
  const objects: Record<string, ReturnType<typeof element>> = {
    model: plugin("alis.model", "", { kind: "model", inputs } as ModelPluginData),
    form: plugin("alis.form", containerId, data),
  };
  for (const id of [...rendered.map(each => each.id), ...others]) objects[id] = element(id, "native");
  startOwner(Owner.page(), [plan(objects, [])]);
  return formPlugin.object(containerId, data) as FormObject;
}

const errorText = (id: string) => document.getElementById(`${id}_error`)?.textContent;

describe("validate", () => {
  it("shows the first failing test's message inline and returns false", () => {
    render(`<form id="admission">${field("Name")}</form><div id="admission_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    expect(form.validate()).toBe(false);
    expect(errorText("Name")).toBe("Enter the resident's name.");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("skips a test whose condition is false", () => {
    render(`<form id="admission">${field("VaId")}<input id="IsVeteran" type="checkbox"></form>`);
    const veteran = e.as(e.get(e.obj("IsVeteran"), ["checked"]), s.boolean);
    const form = startForm([requiredField("VaId", "VaId", "'VA ID' is required.", veteran)], "admission",
      "admission_validation_summary", ["IsVeteran"]);
    (document.getElementById("IsVeteran") as HTMLInputElement).checked = false;
    expect(form.validate()).toBe(true);
  });

  it("ignores a field outside the container", () => {
    render(`<form id="admission"></form>${field("Name")}`);
    expect(startForm([requiredField("Name", "Name", "msg")]).validate()).toBe(true);
  });

  it("puts a message in the summary when the inline slot cannot render", () => {
    render(`<form id="admission"><input id="Name"><div hidden><span id="Name_error"></span></div></form>` +
      `<div id="admission_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.validate();
    const summary = document.getElementById("admission_validation_summary")!;
    expect(summary.hidden).toBe(false);
    expect(summary.textContent).toBe("Enter the resident's name.");
  });

  it("fails an unmounted field whose test is active, as the server fails a missing required member", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Ghost", "Ghost is required.")]).validate()).toBe(false);
  });

  it("passes an unmounted field whose every test is inactive", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Ghost", "Ghost is required.", e.lit(false))]).validate()).toBe(true);
  });

  // Lines[*].Qty is required while the line's own Ordered holds: the second line is ordered, the first is not.
  it("checks each item a name with [*] names, reading the members of its own item", () => {
    render(`<form id="admission">${field("Qty0")}${field("Qty1")}${field("Ordered0", "no")}${field("Ordered1", "yes")}</form>`);
    const ordered = e.op("eq", e.call(e.obj("inputs"), ["valueOf"], [e.lit("Lines[*].Ordered"), e.ref("i")]), e.lit("yes"));
    const data: FormPluginData = { kind: "form", summary: "admission_validation_summary", fields: [{ name: "Lines[*].Qty", tests: [required("Give the quantity.", ordered)] }] };
    const inputs: ModelPluginData["inputs"] = {
      Qty0: textInput("Qty0", "Lines[0].Qty"), Qty1: textInput("Qty1", "Lines[1].Qty"),
      Ordered0: textInput("Ordered0", "Lines[0].Ordered"), Ordered1: textInput("Ordered1", "Lines[1].Ordered"),
    };
    const objects: Record<string, ReturnType<typeof element>> = {
      model: plugin("alis.model", "", { kind: "model", inputs } as ModelPluginData),
      inputs: plugin("alis.model", "admission"),
      form: plugin("alis.form", "admission", data),
    };
    for (const id of Object.keys(inputs)) objects[id] = element(id, "native");
    startOwner(Owner.page(), [plan(objects, [])]);

    expect((formPlugin.object("admission", data) as FormObject).validate()).toBe(false);
    expect(errorText("Qty1")).toBe("Give the quantity.");
    expect(errorText("Qty0")).toBe("");
  });

  it("checks nothing for an item name no item on the page binds", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Lines[*].Qty", "Give the quantity.")]).validate()).toBe(true);
  });

  it("fails when the container is missing and the form has tests", () => {
    render(field("Name", "x"));
    expect(startForm([requiredField("Name", "Name", "msg")], "nowhere").validate()).toBe(false);
  });
});

describe("server errors", () => {
  it("places each error on the field with that binding name, joining messages, else in the summary", () => {
    render(`<form id="admission">${field("Ns__Name")}</form><div id="admission_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Ns__Name", "Name", "msg")]);
    form.showErrors({ errors: { Name: ["Too short.", "Not unique."], Room: "Taken" } });
    expect(errorText("Ns__Name")).toBe("Too short. Not unique.");
    expect(document.getElementById("admission_validation_summary")!.textContent).toBe("Taken");
  });

  it("shows nothing when the response carried no problem record: the body is not errors", () => {
    render(`<form id="admission">${field("Name")}</form>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.showErrors(undefined);
    expect(errorText("Name")).toBe("");
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

  // One validator serves a section saved on its own and the whole form: both containers list every
  // field, and the section, contributed first, does not hold the resident's name.
  it("revalidates a field against the container that holds it, whichever container was contributed first", () => {
    render(`<form id="intake">${field("ResidentName")}<div id="contacts">${field("ContactPhone")}</div></form>`);
    const fields = [
      requiredField("ResidentName", "ResidentName", "Enter the resident's name.").field,
      requiredField("ContactPhone", "ContactPhone", "Enter the contact's phone.").field,
    ];
    const inputs = { ResidentName: textInput("ResidentName", "ResidentName"), ContactPhone: textInput("ContactPhone", "ContactPhone") };
    const objects = {
      model: plugin("alis.model", "", { kind: "model", inputs } as ModelPluginData),
      sectionForm: plugin("alis.form", "contacts", { kind: "form", summary: "admission_validation_summary", fields } as FormPluginData),
      wholeForm: plugin("alis.form", "intake", { kind: "form", summary: "admission_validation_summary", fields } as FormPluginData),
      ResidentName: element("ResidentName", "native"),
      ContactPhone: element("ContactPhone", "native"),
    };
    startOwner(Owner.page(), [plan(objects, [])]);
    const name = document.getElementById("ResidentName") as HTMLInputElement;

    name.dispatchEvent(new Event("blur"));
    expect(errorText("ResidentName")).toBe("Enter the resident's name.");

    name.value = "Margaret Hale";
    name.dispatchEvent(new Event("blur"));
    expect(errorText("ResidentName")).toBe("");
  });

  it("revalidates when the input commits, by whatever name its component raises the commit", () => {
    render(`<form id="admission"></form>`);
    const rating = fakeWidget("Rating", { value: 4 });
    document.getElementById("admission")!.append(rating, Object.assign(document.createElement("span"), { id: "Rating_error" }));
    const scored: FormField = {
      name: "Rating",
      tests: [{ rule: "min", check: e.fn(["v", "i"], e.op("gte", e.ref("v"), e.lit(1))) as never, when: e.fn(["i"], e.lit(true)) as never, message: "Rate the visit." }],
    };
    const model: ModelPluginData = {
      kind: "model",
      inputs: { Rating: { name: "Rating", body: ["Rating"], value: e.as(e.get(e.obj("Rating"), ["value"]), s.number), shape: s.number, committed: "valueChanged", list: false } },
    };
    const form: FormPluginData = { kind: "form", summary: "admission_validation_summary", fields: [scored] };
    startOwner(Owner.page(), [plan({
      Rating: element("Rating", "fake"),
      model: plugin("alis.model", "", model),
      form: plugin("alis.form", "admission", form),
    }, [])]);

    rating.widget.value = 0;
    rating.widget.trigger("change", {});
    expect(errorText("Rating")).toBe("");

    rating.widget.trigger("valueChanged", { value: 0 });
    expect(errorText("Rating")).toBe("Rate the visit.");
  });

  // EJ2's MaskedTextBox commits from its own input handler when a value is pasted; the clear that typing
  // brings must not undo the check that commit just made.
  it("keeps the message a commit raised by the same input event shows", () => {
    render(`<form id="admission"></form>`);
    const phone = fakeWidget("Phone", { value: "" });
    phone.addEventListener("input", () => phone.widget.trigger("valueChanged", {}));
    document.getElementById("admission")!.append(phone, Object.assign(document.createElement("span"), { id: "Phone_error" }));
    const phoneField: FormField = { name: "Phone", tests: [required("Enter the family phone.")] };
    const model: ModelPluginData = {
      kind: "model",
      inputs: { Phone: { ...textInput("Phone", "Phone"), committed: "valueChanged" } },
    };
    const form: FormPluginData = { kind: "form", summary: "admission_validation_summary", fields: [phoneField] };
    startOwner(Owner.page(), [plan({
      Phone: element("Phone", "fake"),
      model: plugin("alis.model", "", model),
      form: plugin("alis.form", "admission", form),
    }, [])]);

    phone.dispatchEvent(new Event("input", { bubbles: true }));

    expect(errorText("Phone")).toBe("Enter the family phone.");
  });
});
