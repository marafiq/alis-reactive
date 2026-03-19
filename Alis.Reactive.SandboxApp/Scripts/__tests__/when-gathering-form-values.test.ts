import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { GatherItem, ComponentEntry } from "../types";

let resolveGather: typeof import("../execution/gather").resolveGather;

// ── DOM helpers ──

function nativeEl(id: string, tag: string, props: Record<string, unknown> = {}): HTMLElement {
  const el = document.createElement(tag);
  el.id = id;
  for (const [k, v] of Object.entries(props)) (el as any)[k] = v;
  document.body.appendChild(el);
  return el;
}

function fusionEl(id: string, instance: Record<string, unknown>): HTMLElement {
  const el = document.createElement("div");
  el.id = id;
  (el as any).ej2_instances = [instance];
  document.body.appendChild(el);
  return el;
}

function gather(id: string, vendor: "native" | "fusion", name: string, readExpr = "value"): GatherItem {
  return { kind: "component", componentId: id, vendor, name, readExpr };
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body></body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).FormData = dom.window.FormData;
  const mod = await import("../execution/gather");
  resolveGather = mod.resolveGather;
});

// ═══════════════════════════════════════════════════════════════
// Native scalar components — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("NativeTextBox gather", () => {
  it("gathers string value in JSON POST", () => {
    nativeEl("ResidentName", "input", { value: "Margaret Thompson" });
    const result = resolveGather([gather("ResidentName", "native", "ResidentName")], "POST", {});
    expect(result.body).toEqual({ ResidentName: "Margaret Thompson" });
  });

  it("gathers string value as GET param", () => {
    nativeEl("ResidentName", "input", { value: "Margaret Thompson" });
    const result = resolveGather([gather("ResidentName", "native", "ResidentName")], "GET", {});
    expect(result.urlParams).toEqual(["ResidentName=Margaret%20Thompson"]);
  });

  it("empty string sends null in JSON POST", () => {
    nativeEl("ResidentName", "input", { value: "" });
    const result = resolveGather([gather("ResidentName", "native", "ResidentName")], "POST", {});
    expect(result.body).toEqual({ ResidentName: null });
  });
});

describe("NativeDropDown gather", () => {
  it("gathers selected option value in JSON POST", () => {
    const select = document.createElement("select");
    select.id = "CareLevel";
    const opt = document.createElement("option");
    opt.value = "assisted";
    opt.selected = true;
    select.appendChild(opt);
    document.body.appendChild(select);

    const result = resolveGather([gather("CareLevel", "native", "CareLevel")], "POST", {});
    expect(result.body).toEqual({ CareLevel: "assisted" });
  });

  it("gathers selected option value as GET param", () => {
    const select = document.createElement("select");
    select.id = "CareLevel";
    const opt = document.createElement("option");
    opt.value = "independent";
    opt.selected = true;
    select.appendChild(opt);
    document.body.appendChild(select);

    const result = resolveGather([gather("CareLevel", "native", "CareLevel")], "GET", {});
    expect(result.urlParams).toEqual(["CareLevel=independent"]);
  });
});

describe("NativeCheckBox gather", () => {
  it("gathers checked=true in JSON POST", () => {
    nativeEl("HasAllergies", "input", { type: "checkbox", checked: true });
    const result = resolveGather([gather("HasAllergies", "native", "HasAllergies", "checked")], "POST", {});
    expect(result.body).toEqual({ HasAllergies: true });
  });

  it("gathers checked=false in JSON POST", () => {
    nativeEl("HasAllergies", "input", { type: "checkbox", checked: false });
    const result = resolveGather([gather("HasAllergies", "native", "HasAllergies", "checked")], "POST", {});
    // false is not empty string — it stays as false, not null
    expect(result.body).toEqual({ HasAllergies: false });
  });

  it("gathers checked as GET param", () => {
    nativeEl("HasAllergies", "input", { type: "checkbox", checked: true });
    const result = resolveGather([gather("HasAllergies", "native", "HasAllergies", "checked")], "GET", {});
    expect(result.urlParams).toEqual(["HasAllergies=true"]);
  });
});

describe("NativeRadioGroup gather", () => {
  it("gathers selected radio value in JSON POST", () => {
    // Radio group uses a hidden input to store the selected value
    nativeEl("MobilityLevel", "input", { type: "hidden", value: "wheelchair" });
    const result = resolveGather([gather("MobilityLevel", "native", "MobilityLevel")], "POST", {});
    expect(result.body).toEqual({ MobilityLevel: "wheelchair" });
  });

  it("gathers selected radio value as GET param", () => {
    nativeEl("MobilityLevel", "input", { type: "hidden", value: "ambulatory" });
    const result = resolveGather([gather("MobilityLevel", "native", "MobilityLevel")], "GET", {});
    expect(result.urlParams).toEqual(["MobilityLevel=ambulatory"]);
  });
});

// ═══════════════════════════════════════════════════════════════
// Native array component — JSON POST + GET + FormData
// ═══════════════════════════════════════════════════════════════

describe("NativeCheckList gather", () => {
  it("gathers string array in JSON POST", () => {
    const container = document.createElement("div");
    container.id = "Allergies";
    (container as any).value = ["Peanuts", "Dairy", "Gluten"];
    document.body.appendChild(container);

    const result = resolveGather([gather("Allergies", "native", "Allergies")], "POST", {});
    expect(result.body).toEqual({ Allergies: ["Peanuts", "Dairy", "Gluten"] });
  });

  it("gathers repeated params in GET", () => {
    const container = document.createElement("div");
    container.id = "Allergies";
    (container as any).value = ["Peanuts", "Dairy"];
    document.body.appendChild(container);

    const result = resolveGather([gather("Allergies", "native", "Allergies")], "GET", {});
    expect(result.urlParams).toEqual(["Allergies=Peanuts", "Allergies=Dairy"]);
  });

  it("gathers multiple FormData entries", () => {
    const container = document.createElement("div");
    container.id = "Allergies";
    (container as any).value = ["Peanuts", "Dairy", "Gluten"];
    document.body.appendChild(container);

    const result = resolveGather([gather("Allergies", "native", "Allergies")], "POST", {}, "form-data");
    const fd = result.body as FormData;
    expect(fd.getAll("Allergies")).toEqual(["Peanuts", "Dairy", "Gluten"]);
  });

  it("empty array gathers as empty array in JSON POST", () => {
    const container = document.createElement("div");
    container.id = "Allergies";
    (container as any).value = [];
    document.body.appendChild(container);

    const result = resolveGather([gather("Allergies", "native", "Allergies")], "POST", {});
    expect(result.body).toEqual({ Allergies: [] });
  });

  it("empty array produces no GET params", () => {
    const container = document.createElement("div");
    container.id = "Allergies";
    (container as any).value = [];
    document.body.appendChild(container);

    const result = resolveGather([gather("Allergies", "native", "Allergies")], "GET", {});
    expect(result.urlParams).toEqual([]);
  });
});

// ═══════════════════════════════════════════════════════════════
// Fusion scalar components — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("FusionNumericTextBox gather", () => {
  it("gathers number in JSON POST", () => {
    fusionEl("MonthlyRate", { value: 4250.75 });
    const result = resolveGather([gather("MonthlyRate", "fusion", "MonthlyRate")], "POST", {});
    expect(result.body).toEqual({ MonthlyRate: 4250.75 });
  });

  it("gathers number as GET param", () => {
    fusionEl("MonthlyRate", { value: 4250.75 });
    const result = resolveGather([gather("MonthlyRate", "fusion", "MonthlyRate")], "GET", {});
    expect(result.urlParams).toEqual(["MonthlyRate=4250.75"]);
  });

  it("gathers null number in JSON POST", () => {
    fusionEl("MonthlyRate", { value: null });
    const result = resolveGather([gather("MonthlyRate", "fusion", "MonthlyRate")], "POST", {});
    expect(result.body).toEqual({ MonthlyRate: null });
  });
});

describe("FusionDropDownList gather", () => {
  it("gathers selected string value in JSON POST", () => {
    fusionEl("FacilityId", { value: "facility-42" });
    const result = resolveGather([gather("FacilityId", "fusion", "FacilityId")], "POST", {});
    expect(result.body).toEqual({ FacilityId: "facility-42" });
  });

  it("gathers selected value as GET param", () => {
    fusionEl("FacilityId", { value: "facility-42" });
    const result = resolveGather([gather("FacilityId", "fusion", "FacilityId")], "GET", {});
    expect(result.urlParams).toEqual(["FacilityId=facility-42"]);
  });
});

describe("FusionAutoComplete gather", () => {
  it("gathers typed text value in JSON POST", () => {
    fusionEl("PhysicianName", { value: "Dr. Smith" });
    const result = resolveGather([gather("PhysicianName", "fusion", "PhysicianName")], "POST", {});
    expect(result.body).toEqual({ PhysicianName: "Dr. Smith" });
  });

  it("gathers typed text as GET param", () => {
    fusionEl("PhysicianName", { value: "Dr. Smith" });
    const result = resolveGather([gather("PhysicianName", "fusion", "PhysicianName")], "GET", {});
    expect(result.urlParams).toEqual(["PhysicianName=Dr.%20Smith"]);
  });
});

describe("FusionDatePicker gather", () => {
  it("gathers date value in JSON POST", () => {
    fusionEl("AdmissionDate", { value: new Date("2024-03-15T00:00:00") });
    const result = resolveGather([gather("AdmissionDate", "fusion", "AdmissionDate")], "POST", {});
    expect(result.body).toHaveProperty("AdmissionDate");
    // Fusion DatePicker stores a Date object — gather passes it as-is
    expect((result.body as any).AdmissionDate).toBeInstanceOf(Date);
  });

  it("gathers date as GET param string", () => {
    const d = new Date("2024-03-15T00:00:00");
    fusionEl("AdmissionDate", { value: d });
    const result = resolveGather([gather("AdmissionDate", "fusion", "AdmissionDate")], "GET", {});
    // Date.toString() is used — just verify it's a non-empty param
    expect(result.urlParams.length).toBe(1);
    expect(result.urlParams[0]).toMatch(/^AdmissionDate=.+/);
  });
});

describe("FusionTimePicker gather", () => {
  it("gathers time value in JSON POST", () => {
    fusionEl("MedicationTime", { value: new Date("1970-01-01T08:30:00") });
    const result = resolveGather([gather("MedicationTime", "fusion", "MedicationTime")], "POST", {});
    expect((result.body as any).MedicationTime).toBeInstanceOf(Date);
  });

  it("gathers time as GET param string", () => {
    fusionEl("MedicationTime", { value: new Date("1970-01-01T08:30:00") });
    const result = resolveGather([gather("MedicationTime", "fusion", "MedicationTime")], "GET", {});
    expect(result.urlParams.length).toBe(1);
    expect(result.urlParams[0]).toMatch(/^MedicationTime=.+/);
  });
});

describe("FusionMultiColumnComboBox gather", () => {
  it("gathers selected value in JSON POST", () => {
    fusionEl("InsuranceProvider", { value: "blue-cross" });
    const result = resolveGather([gather("InsuranceProvider", "fusion", "InsuranceProvider")], "POST", {});
    expect(result.body).toEqual({ InsuranceProvider: "blue-cross" });
  });

  it("gathers selected value as GET param", () => {
    fusionEl("InsuranceProvider", { value: "blue-cross" });
    const result = resolveGather([gather("InsuranceProvider", "fusion", "InsuranceProvider")], "GET", {});
    expect(result.urlParams).toEqual(["InsuranceProvider=blue-cross"]);
  });
});

// ═══════════════════════════════════════════════════════════════
// Fusion array component — JSON POST + GET + FormData
// ═══════════════════════════════════════════════════════════════

describe("FusionMultiSelect gather", () => {
  it("gathers string array in JSON POST", () => {
    fusionEl("DietaryRestrictions", { value: ["vegetarian", "halal", "low-sodium"] });
    const result = resolveGather([gather("DietaryRestrictions", "fusion", "DietaryRestrictions")], "POST", {});
    expect(result.body).toEqual({ DietaryRestrictions: ["vegetarian", "halal", "low-sodium"] });
  });

  it("gathers repeated params in GET", () => {
    fusionEl("DietaryRestrictions", { value: ["vegetarian", "halal"] });
    const result = resolveGather([gather("DietaryRestrictions", "fusion", "DietaryRestrictions")], "GET", {});
    expect(result.urlParams).toEqual([
      "DietaryRestrictions=vegetarian",
      "DietaryRestrictions=halal",
    ]);
  });

  it("gathers multiple FormData entries", () => {
    fusionEl("DietaryRestrictions", { value: ["vegetarian", "halal", "low-sodium"] });
    const result = resolveGather([gather("DietaryRestrictions", "fusion", "DietaryRestrictions")], "POST", {}, "form-data");
    const fd = result.body as FormData;
    expect(fd.getAll("DietaryRestrictions")).toEqual(["vegetarian", "halal", "low-sodium"]);
  });

  it("null value gathers as null in JSON POST", () => {
    fusionEl("DietaryRestrictions", { value: null });
    const result = resolveGather([gather("DietaryRestrictions", "fusion", "DietaryRestrictions")], "POST", {});
    expect(result.body).toEqual({ DietaryRestrictions: null });
  });

  it("empty array gathers as empty array in JSON POST", () => {
    fusionEl("DietaryRestrictions", { value: [] });
    const result = resolveGather([gather("DietaryRestrictions", "fusion", "DietaryRestrictions")], "POST", {});
    expect(result.body).toEqual({ DietaryRestrictions: [] });
  });
});

// ═══════════════════════════════════════════════════════════════
// IncludeAll — mixed form with all component types
// ═══════════════════════════════════════════════════════════════

describe("IncludeAll with mixed component types", () => {
  it("gathers all registered components in JSON POST", () => {
    // Set up one of each component type
    // Native scalars
    nativeEl("ResidentName", "input", { value: "Margaret Thompson" });
    nativeEl("HasAllergies", "input", { type: "checkbox", checked: true });
    nativeEl("MobilityLevel", "input", { type: "hidden", value: "wheelchair" });

    // Native array
    const checklistContainer = document.createElement("div");
    checklistContainer.id = "Allergies";
    (checklistContainer as any).value = ["Peanuts", "Dairy"];
    document.body.appendChild(checklistContainer);

    // Fusion scalars
    fusionEl("MonthlyRate", { value: 4250 });
    fusionEl("FacilityId", { value: "fac-42" });
    fusionEl("AdmissionDate", { value: new Date("2024-03-15T00:00:00") });

    // Fusion array
    fusionEl("DietaryRestrictions", { value: ["vegetarian", "halal"] });

    const components: Record<string, ComponentEntry> = {
      "ResidentName":       { id: "ResidentName", vendor: "native", readExpr: "value" },
      "HasAllergies":       { id: "HasAllergies", vendor: "native", readExpr: "checked" },
      "MobilityLevel":      { id: "MobilityLevel", vendor: "native", readExpr: "value" },
      "Allergies":          { id: "Allergies", vendor: "native", readExpr: "value" },
      "MonthlyRate":        { id: "MonthlyRate", vendor: "fusion", readExpr: "value" },
      "FacilityId":         { id: "FacilityId", vendor: "fusion", readExpr: "value" },
      "AdmissionDate":      { id: "AdmissionDate", vendor: "fusion", readExpr: "value" },
      "DietaryRestrictions": { id: "DietaryRestrictions", vendor: "fusion", readExpr: "value" },
    };

    const items: GatherItem[] = [{ kind: "all" }];
    const result = resolveGather(items, "POST", components);
    const body = result.body as Record<string, unknown>;

    // Native scalars
    expect(body.ResidentName).toBe("Margaret Thompson");
    expect(body.HasAllergies).toBe(true);
    expect(body.MobilityLevel).toBe("wheelchair");

    // Native array
    expect(body.Allergies).toEqual(["Peanuts", "Dairy"]);

    // Fusion scalars
    expect(body.MonthlyRate).toBe(4250);
    expect(body.FacilityId).toBe("fac-42");
    expect(body.AdmissionDate).toBeInstanceOf(Date);

    // Fusion array
    expect(body.DietaryRestrictions).toEqual(["vegetarian", "halal"]);
  });

  it("gathers all registered components as GET params", () => {
    nativeEl("ResidentName", "input", { value: "Margaret" });
    nativeEl("HasAllergies", "input", { type: "checkbox", checked: true });

    const checklistContainer = document.createElement("div");
    checklistContainer.id = "Allergies";
    (checklistContainer as any).value = ["Peanuts", "Dairy"];
    document.body.appendChild(checklistContainer);

    fusionEl("MonthlyRate", { value: 4250 });

    const components: Record<string, ComponentEntry> = {
      "ResidentName": { id: "ResidentName", vendor: "native", readExpr: "value" },
      "HasAllergies": { id: "HasAllergies", vendor: "native", readExpr: "checked" },
      "Allergies":    { id: "Allergies", vendor: "native", readExpr: "value" },
      "MonthlyRate":  { id: "MonthlyRate", vendor: "fusion", readExpr: "value" },
    };

    const items: GatherItem[] = [{ kind: "all" }];
    const result = resolveGather(items, "GET", components);

    expect(result.urlParams).toContain("ResidentName=Margaret");
    expect(result.urlParams).toContain("HasAllergies=true");
    // Array produces repeated params
    expect(result.urlParams).toContain("Allergies=Peanuts");
    expect(result.urlParams).toContain("Allergies=Dairy");
    expect(result.urlParams).toContain("MonthlyRate=4250");
  });
});

// ═══════════════════════════════════════════════════════════════
// Infrastructure — static, nested, empty, error cases
// ═══════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════
// NativeTextArea — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("NativeTextArea gather", () => {
  it("gathers textarea string value in JSON POST", () => {
    const textarea = document.createElement("textarea");
    textarea.id = "CareNotes";
    textarea.value = "Patient stable, vitals normal";
    document.body.appendChild(textarea);
    const result = resolveGather([gather("CareNotes", "native", "CareNotes")], "POST", {});
    expect(result.body).toEqual({ CareNotes: "Patient stable, vitals normal" });
  });

  it("gathers textarea value as GET param", () => {
    const textarea = document.createElement("textarea");
    textarea.id = "CareNotes";
    textarea.value = "Patient stable";
    document.body.appendChild(textarea);
    const result = resolveGather([gather("CareNotes", "native", "CareNotes")], "GET", {});
    expect(result.urlParams).toEqual(["CareNotes=Patient%20stable"]);
  });
});

// ═══════════════════════════════════════════════════════════════
// FusionDateTimePicker — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("FusionDateTimePicker gather", () => {
  it("gathers DateTime value in JSON POST", () => {
    fusionEl("MedicationTime", { value: new Date("2024-03-15T08:30:00") });
    const result = resolveGather([gather("MedicationTime", "fusion", "MedicationTime")], "POST", {});
    expect((result.body as any).MedicationTime).toBeInstanceOf(Date);
  });

  it("gathers DateTime as GET param", () => {
    fusionEl("MedicationTime", { value: new Date("2024-03-15T08:30:00") });
    const result = resolveGather([gather("MedicationTime", "fusion", "MedicationTime")], "GET", {});
    expect(result.urlParams.length).toBe(1);
    expect(result.urlParams[0]).toMatch(/^MedicationTime=.+/);
  });
});

// ═══════════════════════════════════════════════════════════════
// FusionDateRangePicker — JSON POST (startDate + endDate)
// ═══════════════════════════════════════════════════════════════

describe("FusionDateRangePicker gather", () => {
  it("gathers startDate in JSON POST", () => {
    fusionEl("StayStart", { startDate: new Date("2024-01-15") });
    const result = resolveGather([gather("StayStart", "fusion", "StayStart", "startDate")], "POST", {});
    expect((result.body as any).StayStart).toBeInstanceOf(Date);
  });

  it("gathers endDate in JSON POST", () => {
    fusionEl("StayStart", { endDate: new Date("2024-06-15") });
    const result = resolveGather([gather("StayStart", "fusion", "StayEnd", "endDate")], "POST", {});
    expect((result.body as any).StayEnd).toBeInstanceOf(Date);
  });
});

// ═══════════════════════════════════════════════════════════════
// FusionInputMask — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("FusionInputMask gather", () => {
  it("gathers masked string in JSON POST", () => {
    fusionEl("PhoneNumber", { value: "(555) 123-4567" });
    const result = resolveGather([gather("PhoneNumber", "fusion", "PhoneNumber")], "POST", {});
    expect(result.body).toEqual({ PhoneNumber: "(555) 123-4567" });
  });

  it("gathers masked string as GET param", () => {
    fusionEl("PhoneNumber", { value: "(555) 123-4567" });
    const result = resolveGather([gather("PhoneNumber", "fusion", "PhoneNumber")], "GET", {});
    expect(result.urlParams).toEqual(["PhoneNumber=(555)%20123-4567"]);
  });
});

// ═══════════════════════════════════════════════════════════════
// FusionRichTextEditor — JSON POST + GET
// ═══════════════════════════════════════════════════════════════

describe("FusionRichTextEditor gather", () => {
  it("gathers HTML string in JSON POST", () => {
    fusionEl("CarePlan", { value: "<p>Daily medications at 8am</p>" });
    const result = resolveGather([gather("CarePlan", "fusion", "CarePlan")], "POST", {});
    expect(result.body).toEqual({ CarePlan: "<p>Daily medications at 8am</p>" });
  });

  it("gathers HTML string as GET param", () => {
    fusionEl("CarePlan", { value: "<p>Care plan</p>" });
    const result = resolveGather([gather("CarePlan", "fusion", "CarePlan")], "GET", {});
    expect(result.urlParams[0]).toMatch(/^CarePlan=.+/);
  });
});

// ═══════════════════════════════════════════════════════════════
// FusionSwitch — JSON POST + GET (readExpr: "checked")
// ═══════════════════════════════════════════════════════════════

describe("FusionSwitch gather", () => {
  it("gathers checked=true in JSON POST", () => {
    fusionEl("ReceiveNotifications", { checked: true });
    const result = resolveGather([gather("ReceiveNotifications", "fusion", "ReceiveNotifications", "checked")], "POST", {});
    expect(result.body).toEqual({ ReceiveNotifications: true });
  });

  it("gathers checked=false in JSON POST", () => {
    fusionEl("ReceiveNotifications", { checked: false });
    const result = resolveGather([gather("ReceiveNotifications", "fusion", "ReceiveNotifications", "checked")], "POST", {});
    expect(result.body).toEqual({ ReceiveNotifications: false });
  });

  it("gathers checked as GET param", () => {
    fusionEl("ReceiveNotifications", { checked: true });
    const result = resolveGather([gather("ReceiveNotifications", "fusion", "ReceiveNotifications", "checked")], "GET", {});
    expect(result.urlParams).toEqual(["ReceiveNotifications=true"]);
  });
});

// ═══════════════════════════════════════════════════════════════
// Infrastructure — static, nested, empty, error cases
// ═══════════════════════════════════════════════════════════════

describe("gather infrastructure", () => {
  it("static values appear in POST body", () => {
    const items: GatherItem[] = [
      { kind: "static", param: "action", value: "save" },
      { kind: "static", param: "version", value: 2 },
    ];
    const result = resolveGather(items, "POST", {});
    expect(result.body).toEqual({ action: "save", version: 2 });
  });

  it("dotted component names produce nested JSON body", () => {
    nativeEl("Street", "input", { value: "123 Oak Lane" });
    const result = resolveGather([gather("Street", "native", "Address.Street")], "POST", {});
    expect(result.body).toEqual({ Address: { Street: "123 Oak Lane" } });
  });

  it("mixed component + static gathers combine correctly", () => {
    nativeEl("ResidentName", "input", { value: "Margaret" });
    const items: GatherItem[] = [
      gather("ResidentName", "native", "ResidentName"),
      { kind: "static", param: "csrfToken", value: "abc123" },
    ];
    const result = resolveGather(items, "POST", {});
    expect(result.body).toEqual({ ResidentName: "Margaret", csrfToken: "abc123" });
  });

  it("empty gather returns empty result", () => {
    const result = resolveGather([], "POST", {});
    expect(result.body).toEqual({});
    expect(result.urlParams).toEqual([]);
  });

  it("throws when IncludeAll fires with no registered components", () => {
    const items: GatherItem[] = [{ kind: "all" }];
    expect(() => resolveGather(items, "POST", {})).toThrow(/IncludeAll/);
  });
});

// ═══════════════════════════════════════════════════════════════
// File objects — FormData transport handles File natively
// ═══════════════════════════════════════════════════════════════

describe("File object gather", () => {
  it("File in FormData array uses formData.append with file", () => {
    const container = document.createElement("div");
    container.id = "Uploader";
    // Simulate evalRead returning a FileList-like array of File objects
    const file = new File(["test content"], "test.txt", { type: "text/plain" });
    (container as any).value = [file];
    document.body.appendChild(container);

    const result = resolveGather(
      [gather("Uploader", "native", "Documents")], "POST", {}, "form-data");
    const fd = result.body as FormData;
    const entries = fd.getAll("Documents");
    expect(entries.length).toBe(1);
    expect(entries[0]).toBeInstanceOf(File);
    expect((entries[0] as File).name).toBe("test.txt");
  });

  it("multiple Files in FormData appends each", () => {
    const container = document.createElement("div");
    container.id = "Uploader";
    const file1 = new File(["content 1"], "doc1.pdf", { type: "application/pdf" });
    const file2 = new File(["content 2"], "doc2.pdf", { type: "application/pdf" });
    (container as any).value = [file1, file2];
    document.body.appendChild(container);

    const result = resolveGather(
      [gather("Uploader", "native", "Documents")], "POST", {}, "form-data");
    const fd = result.body as FormData;
    const entries = fd.getAll("Documents");
    expect(entries.length).toBe(2);
    expect((entries[0] as File).name).toBe("doc1.pdf");
    expect((entries[1] as File).name).toBe("doc2.pdf");
  });

  it("File in JSON POST throws", () => {
    const container = document.createElement("div");
    container.id = "Uploader";
    const file = new File(["test"], "test.txt", { type: "text/plain" });
    (container as any).value = [file];
    document.body.appendChild(container);

    expect(() => resolveGather(
      [gather("Uploader", "native", "Documents")], "POST", {}))
      .toThrow(/form-data/);
  });

  it("File in GET throws", () => {
    const container = document.createElement("div");
    container.id = "Uploader";
    const file = new File(["test"], "test.txt", { type: "text/plain" });
    (container as any).value = [file];
    document.body.appendChild(container);

    expect(() => resolveGather(
      [gather("Uploader", "native", "Documents")], "GET", {}))
      .toThrow(/GET/);
  });

  it("Files coexist with scalar fields in FormData", () => {
    nativeEl("Name", "input", { value: "Margaret" });
    const container = document.createElement("div");
    container.id = "Uploader";
    const file = new File(["content"], "photo.jpg", { type: "image/jpeg" });
    (container as any).value = [file];
    document.body.appendChild(container);

    const result = resolveGather([
      gather("Name", "native", "ResidentName"),
      gather("Uploader", "native", "Documents"),
    ], "POST", {}, "form-data");
    const fd = result.body as FormData;
    expect(fd.get("ResidentName")).toBe("Margaret");
    expect(fd.getAll("Documents").length).toBe(1);
    expect((fd.getAll("Documents")[0] as File).name).toBe("photo.jpg");
  });
});
