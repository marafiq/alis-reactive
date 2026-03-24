/**
 * FAILING TESTS — gather pipeline with Date values
 *
 * The gather pipeline calls String(value) directly — never through coerce().
 * For Date objects:
 *   - FormData: String(date) → locale garbage (not ISO)
 *   - GET params: String(date) → locale garbage → URL-encoded locale garbage
 *   - JSON POST: passes Date as-is → JSON.stringify(date) → ISO ✓ (accidentally correct)
 *
 * These tests prove gather needs coercion for non-JSON transports.
 */
import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { GatherItem, ComponentEntry } from "../types";

let resolveGather: typeof import("../execution/gather").resolveGather;

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
// Gap: Date in FormData → String(date) → locale garbage
//
// FormData transport calls: formData.append(name, String(value ?? ""))
// String(new Date("2024-03-15T00:00:00Z"))
//   → "Sat Mar 15 2024 19:00:00 GMT-0500 (Central Daylight Time)"
//
// Server expects ISO: "2024-03-15T00:00:00.000Z"
// ═══════════════════════════════════════════════════════════════

describe("FusionDatePicker gather in FormData", () => {
  it("serializes Date as ISO string in FormData", () => {
    fusionEl("AdmissionDate", { value: new Date("2024-03-15T00:00:00Z") });
    const result = resolveGather(
      [gather("AdmissionDate", "fusion", "AdmissionDate")],
      "POST", {}, "form-data",
    );
    const fd = result.body as FormData;
    const val = fd.get("AdmissionDate") as string;
    // ACTUAL: "Sat Mar 15 2024 19:00:00 GMT-0500 (Central Daylight Time)"
    // EXPECTED: ISO 8601 string
    expect(val).toBe("2024-03-15T00:00:00.000Z");
  });
});

// ═══════════════════════════════════════════════════════════════
// Gap: Date in GET params → String(date) → locale garbage URL-encoded
//
// GET transport calls: encodeURIComponent(String(value))
// String(date) → locale garbage → URL-encoded locale garbage
//
// Server gets: AdmissionDate=Sat%20Mar%2015%202024%2019%3A00%3A00...
// Server expects: AdmissionDate=2024-03-15T00%3A00%3A00.000Z
// ═══════════════════════════════════════════════════════════════

describe("FusionDatePicker gather in GET params", () => {
  it("serializes Date as ISO string in GET param", () => {
    fusionEl("AdmissionDate", { value: new Date("2024-03-15T00:00:00Z") });
    const result = resolveGather(
      [gather("AdmissionDate", "fusion", "AdmissionDate")],
      "GET", {},
    );
    // ACTUAL: "AdmissionDate=Sat%20Mar%2015%202024%2019%3A00%3A00..."
    // EXPECTED: ISO-encoded param
    expect(result.urlParams[0]).toBe(
      `AdmissionDate=${encodeURIComponent("2024-03-15T00:00:00.000Z")}`,
    );
  });
});

// ═══════════════════════════════════════════════════════════════
// Gap: DateRangePicker startDate in FormData
// ═══════════════════════════════════════════════════════════════

describe("FusionDateRangePicker gather in FormData", () => {
  it("serializes startDate as ISO string in FormData", () => {
    fusionEl("StayRange", { startDate: new Date("2024-01-15T00:00:00Z") });
    const result = resolveGather(
      [gather("StayRange", "fusion", "StayStart", "startDate")],
      "POST", {}, "form-data",
    );
    const fd = result.body as FormData;
    expect(fd.get("StayStart")).toBe("2024-01-15T00:00:00.000Z");
  });
});

// ═══════════════════════════════════════════════════════════════
// Gap: IncludeAll with Date components in FormData
// ═══════════════════════════════════════════════════════════════

describe("IncludeAll with Date components in FormData", () => {
  it("serializes all Date components as ISO strings", () => {
    fusionEl("AdmissionDate", { value: new Date("2024-03-15T00:00:00Z") });
    fusionEl("DischargeDate", { value: new Date("2024-06-15T00:00:00Z") });

    const components: Record<string, ComponentEntry> = {
      "AdmissionDate": { id: "AdmissionDate", vendor: "fusion", readExpr: "value" },
      "DischargeDate": { id: "DischargeDate", vendor: "fusion", readExpr: "value" },
    };

    const result = resolveGather(
      [{ kind: "all" }],
      "POST", components, "form-data",
    );
    const fd = result.body as FormData;
    expect(fd.get("AdmissionDate")).toBe("2024-03-15T00:00:00.000Z");
    expect(fd.get("DischargeDate")).toBe("2024-06-15T00:00:00.000Z");
  });
});
