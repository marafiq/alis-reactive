import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { Expr, FormField, FormPluginData, ModelPluginData } from "../types/index";
import type { FormObject } from "../plugins/form/form";
import type { ContainedInputs } from "../plugins/model";
import { e, element, fakeWidget, plan, plugin, s } from "./support/plan";

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

interface Rendered {
  readonly id: string;
  readonly field: FormField;
}

function sentValueOf(name: string, unsent: unknown = null): Expr {
  return e.call(e.obj("model"), ["sentValueOf"], [e.ref("sent"), e.lit(name), e.ref("i"), e.lit(unsent)]);
}

function formField(name: string, tests: FormField["tests"], items = e.list(e.list()), unsent: unknown = null): FormField {
  return { name, items: e.fn(["sent"], items), value: e.fn(["i", "sent"], sentValueOf(name, unsent)), tests };
}

function requiredField(id: string, name: string, message: string, when = e.lit(true), items = e.list(e.list())): Rendered {
  return { id, field: formField(name, [required(message, when)], items) };
}

function required(message: string, when = e.lit(true)): FormField["tests"][number] {
  return { check: e.fn(["v", "i", "sent"], e.op("not", e.op("empty", e.ref("v")))) as never, when: e.fn(["i", "sent"], when) as never, message };
}

function formData(fields: FormField[], inputs = "inputs"): FormPluginData {
  return { kind: "form", summary: "admission_validation_summary", live: e.call(e.obj(inputs), ["fill"], [e.record({})]), fields };
}

function sentBy(containerId: string): () => unknown {
  return () => (modelPlugin.object(containerId, { kind: "none" }) as ContainedInputs).fill({});
}

function textInput(id: string, name: string): ModelPluginData["inputs"][string] {
  const body = name.match(/[^.[\]]+/g)!.map(step => (/^\d+$/.test(step) ? Number(step) : step));
  return { name, body, value: e.as(e.get(e.obj(id), ["value"]), s.string), shape: s.string, committed: "change", invalidClass: "alis-has-error", list: false };
}

function render(html: string): void {
  document.body.insertAdjacentHTML("beforeend", html);
}

function field(id: string, value = ""): string {
  return `<input id="${id}" value="${value}"><span><span id="${id}_error" hidden></span></span>`;
}

function startForm(
  rendered: Rendered[],
  containerId = "admission",
  summary = "admission_validation_summary",
  others: string[] = [],
): FormObject {
  const data: FormPluginData = { ...formData(rendered.map(each => each.field)), summary };
  const inputs: ModelPluginData["inputs"] = {};
  for (const each of rendered) inputs[each.id] = textInput(each.id, each.field.name);
  const objects: Record<string, ReturnType<typeof element>> = {
    model: plugin("alis.model", "", { kind: "model", inputs } as ModelPluginData),
    inputs: plugin("alis.model", containerId),
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
    expect(form.validate(sentBy("admission"))).toBe(false);
    expect(errorText("Name")).toBe("Enter the resident's name.");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("skips a test whose condition is false", () => {
    render(`<form id="admission">${field("VaId")}<input id="IsVeteran" type="checkbox"></form>`);
    const veteran = e.as(e.get(e.obj("IsVeteran"), ["checked"]), s.boolean);
    const form = startForm([requiredField("VaId", "VaId", "'VA ID' is required.", veteran)], "admission",
      "admission_validation_summary", ["IsVeteran"]);
    (document.getElementById("IsVeteran") as HTMLInputElement).checked = false;
    expect(form.validate(sentBy("admission"))).toBe(true);
  });

  it("reads a member whose input sits outside the form as the request leaves it, and shows its message in the summary", () => {
    render(`<form id="admission"></form>${field("Name", "Helen")}<div id="admission_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    expect(form.validate(sentBy("admission"))).toBe(false);
    expect(document.getElementById("admission_validation_summary")!.textContent).toContain("Enter the resident's name.");
    expect(errorText("Name")).toBe("");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("passes a member whose input sits outside the form when the request leaves it a value", () => {
    render(`<form id="admission"></form>${field("Name")}<div id="admission_validation_summary" hidden></div>`);
    const rendered = { id: "Name", field: formField("Name", [required("Enter the resident's name.")], e.list(e.list()), "Helen") };
    expect(startForm([rendered]).validate(sentBy("admission"))).toBe(true);
  });

  it("puts a message in the summary when the inline slot cannot render", () => {
    render(`<form id="admission"><input id="Name"><div hidden><span id="Name_error"></span></div></form>` +
      `<div id="admission_validation_summary" hidden></div>`);
    const form = startForm([requiredField("Name", "Name", "Enter the resident's name.")]);
    form.validate(sentBy("admission"));
    const summary = document.getElementById("admission_validation_summary")!;
    expect(summary.hidden).toBe(false);
    expect(summary.textContent).toBe("Enter the resident's name.");
  });

  it("fails an unmounted field whose test is active, as the server fails a missing required member", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Ghost", "Ghost is required.")]).validate(sentBy("admission"))).toBe(false);
  });

  it("passes an unmounted field whose every test is inactive", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Ghost", "Ghost is required.", e.lit(false))]).validate(sentBy("admission"))).toBe(true);
  });

  it("checks each item a name with [*] names, reading the members of its own item", () => {
    render(`<form id="admission">${field("Qty0")}${field("Qty1")}${field("Ordered0", "no")}${field("Ordered1", "yes")}</form>`);
    const ordered = e.op("eq", sentValueOf("Lines[*].Ordered"), e.lit("yes"));
    const lines = e.list(e.list(e.lit(0)), e.list(e.lit(1)));
    const data = formData([formField("Lines[*].Qty", [required("Give the quantity.", ordered)], lines)]);
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

    expect((formPlugin.object("admission", data) as FormObject).validate(sentBy("admission"))).toBe(false);
    expect(errorText("Qty1")).toBe("Give the quantity.");
    expect(errorText("Qty0")).toBe("");
  });

  it("checks nothing for an item name no item on the page binds", () => {
    render(`<form id="admission"></form><div id="admission_validation_summary" hidden></div>`);
    expect(startForm([requiredField("Ghost", "Lines[*].Qty", "Give the quantity.", e.lit(true), e.list())]).validate(sentBy("admission"))).toBe(true);
  });

  it("fails when the container is missing and the form has tests", () => {
    render(field("Name", "x"));
    expect(startForm([requiredField("Name", "Name", "msg")], "nowhere").validate(sentBy("nowhere"))).toBe(false);
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
    form.validate(sentBy("admission"));
    const input = document.getElementById("Name") as HTMLInputElement;

    input.value = "M";
    input.dispatchEvent(new Event("input"));
    expect(errorText("Name")).toBe("");

    input.value = "";
    input.dispatchEvent(new Event("blur"));
    expect(errorText("Name")).toBe("Enter the resident's name.");
  });

  it("revalidates a field against the container that holds it, whichever container was contributed first", () => {
    render(`<form id="intake">${field("ResidentName")}<div id="contacts">${field("ContactPhone")}</div></form>`);
    const fields = [
      requiredField("ResidentName", "ResidentName", "Enter the resident's name.").field,
      requiredField("ContactPhone", "ContactPhone", "Enter the contact's phone.").field,
    ];
    const inputs = { ResidentName: textInput("ResidentName", "ResidentName"), ContactPhone: textInput("ContactPhone", "ContactPhone") };
    const objects = {
      model: plugin("alis.model", "", { kind: "model", inputs } as ModelPluginData),
      contactsInputs: plugin("alis.model", "contacts"),
      intakeInputs: plugin("alis.model", "intake"),
      sectionForm: plugin("alis.form", "contacts", formData(fields, "contactsInputs")),
      wholeForm: plugin("alis.form", "intake", formData(fields, "intakeInputs")),
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
    const scored = formField("Rating", [{ check: e.fn(["v", "i", "sent"], e.op("gte", e.ref("v"), e.lit(1))) as never, when: e.fn(["i", "sent"], e.lit(true)) as never, message: "Rate the visit." }]);
    const model: ModelPluginData = {
      kind: "model",
      inputs: { Rating: { name: "Rating", body: ["Rating"], value: e.as(e.get(e.obj("Rating"), ["value"]), s.number), shape: s.number, committed: "valueChanged", invalidClass: "alis-has-error", list: false } },
    };
    const form = formData([scored]);
    startOwner(Owner.page(), [plan({
      Rating: element("Rating", "fake"),
      model: plugin("alis.model", "", model),
      inputs: plugin("alis.model", "admission"),
      form: plugin("alis.form", "admission", form),
    }, [])]);

    rating.widget.value = 0;
    rating.widget.trigger("change", {});
    expect(errorText("Rating")).toBe("");

    rating.widget.trigger("valueChanged", { value: 0 });
    expect(errorText("Rating")).toBe("Rate the visit.");
  });

  it("keeps the message a commit raised by the same input event shows", () => {
    render(`<form id="admission"></form>`);
    const phone = fakeWidget("Phone", { value: "" });
    phone.addEventListener("input", () => phone.widget.trigger("valueChanged", {}));
    document.getElementById("admission")!.append(phone, Object.assign(document.createElement("span"), { id: "Phone_error" }));
    const phoneField = formField("Phone", [required("Enter the family phone.")]);
    const model: ModelPluginData = {
      kind: "model",
      inputs: { Phone: { ...textInput("Phone", "Phone"), committed: "valueChanged" } },
    };
    const form = formData([phoneField]);
    startOwner(Owner.page(), [plan({
      Phone: element("Phone", "fake"),
      model: plugin("alis.model", "", model),
      inputs: plugin("alis.model", "admission"),
      form: plugin("alis.form", "admission", form),
    }, [])]);

    phone.dispatchEvent(new Event("input", { bubbles: true }));

    expect(errorText("Phone")).toBe("Enter the family phone.");
  });
});
