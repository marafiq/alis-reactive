import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let wireLiveValidation: typeof import("../validation").wireLiveValidation;
let resetLiveClearForTests: typeof import("../validation").resetLiveClearForTests;

// ── Helpers ──────────────────────────────────────────────

function fusionField(id: string, name: string, readExpr: string, rules: ValidationField["rules"]): ValidationField {
  return { fieldName: name, fieldId: id, vendor: "fusion", readExpr, rules };
}

function nativeField(id: string, name: string, rules: ValidationField["rules"]): ValidationField {
  return { fieldName: name, fieldId: id, vendor: "native", readExpr: "value", rules };
}

function desc(formId: string, fields: ValidationField[], planId?: string): ValidationDescriptor {
  return { formId, fields, planId };
}

function hasError(id: string): boolean {
  return document.getElementById(id)?.classList.contains("alis-has-error") ?? false;
}

function errorText(id: string): string {
  const byId = document.getElementById(id + "_error");
  if (byId) return byId.textContent ?? "";
  return document.querySelector(`span[data-valmsg-for="${id}"]`)?.textContent ?? "";
}

// ── FusionMultiSelect ────────────────────────────────────

describe("when validating array components", () => {

  describe("FusionMultiSelect — required rule", () => {
    let ej2Instance: any;

    beforeEach(async () => {
      ej2Instance = {
        value: null, // SF MultiSelect: null when nothing selected, string[] when items selected
        addEventListener: () => {},
      };

      // Real SF MultiSelect DOM: ej2_instances on the original <input style="display:none">
      // SF hides the input and creates its own wrapper UI
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <input id="DietaryRestrictions" name="DietaryRestrictions" style="display:none" />
            <span id="DietaryRestrictions_error" data-valmsg-for="DietaryRestrictions" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const input = dom.window.document.getElementById("DietaryRestrictions")!;
      (input as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("fails when value is null (nothing selected)", () => {
      ej2Instance.value = null;
      const d = desc("myForm", [
        fusionField("DietaryRestrictions", "DietaryRestrictions", "value", [
          { rule: "required", message: "Select at least one dietary restriction" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
      expect(errorText("DietaryRestrictions")).toBe("Select at least one dietary restriction");
    });

    it("fails when value is empty array", () => {
      ej2Instance.value = [];
      const d = desc("myForm", [
        fusionField("DietaryRestrictions", "DietaryRestrictions", "value", [
          { rule: "required", message: "Select at least one" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
    });

    it("passes when items are selected", () => {
      ej2Instance.value = ["gluten-free", "dairy-free"];
      const d = desc("myForm", [
        fusionField("DietaryRestrictions", "DietaryRestrictions", "value", [
          { rule: "required", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });

    it("passes with single item selected", () => {
      ej2Instance.value = ["vegan"];
      const d = desc("myForm", [
        fusionField("DietaryRestrictions", "DietaryRestrictions", "value", [
          { rule: "required", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });
  });

  describe("FusionMultiSelect — atLeastOne rule", () => {
    let ej2Instance: any;

    beforeEach(async () => {
      ej2Instance = { value: null, addEventListener: () => {} };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <span class="e-control-wrapper" id="Tags">
              <input class="e-multi-select e-input" name="Tags" />
            </span>
            <span id="Tags_error" data-valmsg-for="Tags" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById("Tags") as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("fails when array is empty", () => {
      ej2Instance.value = [];
      const d = desc("myForm", [
        fusionField("Tags", "Tags", "value", [
          { rule: "atLeastOne", message: "Select at least one tag" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
      expect(errorText("Tags")).toBe("Select at least one tag");
    });

    it("fails when value is null", () => {
      ej2Instance.value = null;
      const d = desc("myForm", [
        fusionField("Tags", "Tags", "value", [
          { rule: "atLeastOne", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
    });

    it("passes when at least one item selected", () => {
      ej2Instance.value = ["tag1"];
      const d = desc("myForm", [
        fusionField("Tags", "Tags", "value", [
          { rule: "atLeastOne", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });
  });

  describe("FusionMultiSelect — live-clear on change", () => {
    let ej2Instance: any;
    let ej2Listeners: Record<string, ((...args: any[]) => void)[]>;

    beforeEach(async () => {
      ej2Listeners = {};
      ej2Instance = {
        value: null,
        addEventListener: (event: string, fn: (...args: any[]) => void) => {
          if (!ej2Listeners[event]) ej2Listeners[event] = [];
          ej2Listeners[event].push(fn);
        },
      };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <span class="e-control-wrapper" id="Diet">
              <input class="e-multi-select e-input" name="Diet" />
            </span>
            <span id="Diet_error" data-valmsg-for="Diet" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById("Diet") as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("clears error when user selects an item via SF change callback", () => {
      const d = desc("myForm", [
        fusionField("Diet", "Diet", "value", [
          { rule: "atLeastOne", message: "Select at least one" }
        ]),
      ]);

      wireLiveValidation(d);
      ej2Instance.value = [];
      validate(d);
      expect(hasError("Diet")).toBe(true);

      // User selects an item — SF fires change callback
      ej2Instance.value = ["gluten-free"];
      for (const fn of ej2Listeners["change"] ?? []) fn({ value: ["gluten-free"] });

      expect(hasError("Diet")).toBe(false);
    });
  });

  // ── FusionFileUpload ───────────────────────────────────

  describe("FusionFileUpload — required rule", () => {
    let ej2Instance: any;

    beforeEach(async () => {
      ej2Instance = {
        filesData: [], // SF Uploader: empty array when no files, array of file objects when files selected
        addEventListener: () => {},
      };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <span class="e-control-wrapper" id="Documents">
              <input class="e-upload e-input" name="Documents" type="file" />
            </span>
            <span id="Documents_error" data-valmsg-for="Documents" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById("Documents") as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("fails when filesData is empty array", () => {
      ej2Instance.filesData = [];
      const d = desc("myForm", [
        fusionField("Documents", "Documents", "filesData", [
          { rule: "required", message: "Upload at least one document" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
      expect(errorText("Documents")).toBe("Upload at least one document");
    });

    it("passes when files are uploaded", () => {
      ej2Instance.filesData = [{ name: "report.pdf", size: 1024 }];
      const d = desc("myForm", [
        fusionField("Documents", "Documents", "filesData", [
          { rule: "required", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });

    it("fails with atLeastOne when no files", () => {
      ej2Instance.filesData = [];
      const d = desc("myForm", [
        fusionField("Documents", "Documents", "filesData", [
          { rule: "atLeastOne", message: "Upload at least one file" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
    });

    it("passes with atLeastOne when files present", () => {
      ej2Instance.filesData = [{ name: "photo.jpg", size: 2048 }];
      const d = desc("myForm", [
        fusionField("Documents", "Documents", "filesData", [
          { rule: "atLeastOne", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });
  });

  describe("FusionFileUpload — live-clear on change", () => {
    let ej2Instance: any;
    let ej2Listeners: Record<string, ((...args: any[]) => void)[]>;

    beforeEach(async () => {
      ej2Listeners = {};
      ej2Instance = {
        filesData: [],
        addEventListener: (event: string, fn: (...args: any[]) => void) => {
          if (!ej2Listeners[event]) ej2Listeners[event] = [];
          ej2Listeners[event].push(fn);
        },
      };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <span class="e-control-wrapper" id="Files">
              <input class="e-upload e-input" name="Files" type="file" />
            </span>
            <span id="Files_error" data-valmsg-for="Files" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById("Files") as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("clears error when user uploads a file via SF change callback", () => {
      const d = desc("myForm", [
        fusionField("Files", "Files", "filesData", [
          { rule: "required", message: "Upload required" }
        ]),
      ]);

      wireLiveValidation(d);
      ej2Instance.filesData = [];
      validate(d);
      expect(hasError("Files")).toBe(true);

      // User uploads a file — SF fires change callback
      ej2Instance.filesData = [{ name: "doc.pdf", size: 512 }];
      for (const fn of ej2Listeners["change"] ?? []) fn({ filesData: ej2Instance.filesData });

      expect(hasError("Files")).toBe(false);
    });
  });

  // ── NativeCheckList (array via container.value) ────────

  describe("NativeCheckList — required rule", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div data-reactive-checklist id="Allergies">
            <input type="hidden" name="Allergies" value="" />
            <label><input type="checkbox" value="peanuts" /> Peanuts</label>
            <label><input type="checkbox" value="dairy" /> Dairy</label>
            <label><input type="checkbox" value="gluten" /> Gluten</label>
            <span id="Allergies_error" data-valmsg-for="Allergies" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      // NativeCheckList stores array on container.value
      const container = dom.window.document.getElementById("Allergies")!;
      (container as any).value = [];

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("fails when no checkboxes checked (empty array)", () => {
      const container = document.getElementById("Allergies")!;
      (container as any).value = [];

      const d = desc("myForm", [
        nativeField("Allergies", "Allergies", [
          { rule: "atLeastOne", message: "Select at least one allergy" }
        ]),
      ]);
      expect(validate(d)).toBe(false);
      expect(errorText("Allergies")).toBe("Select at least one allergy");
    });

    it("passes when checkboxes are checked", () => {
      const container = document.getElementById("Allergies")!;
      (container as any).value = ["peanuts", "dairy"];

      const d = desc("myForm", [
        nativeField("Allergies", "Allergies", [
          { rule: "atLeastOne", message: "required" }
        ]),
      ]);
      expect(validate(d)).toBe(true);
    });
  });

  // ── Mixed form: scalar + array components ──────────────

  describe("mixed form with scalar and array fields", () => {
    let multiSelectEj2: any;

    beforeEach(async () => {
      multiSelectEj2 = { value: null, addEventListener: () => {} };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <input id="Name" name="Name" value="" />
            <span id="Name_error" data-valmsg-for="Name" hidden></span>
          </div>
          <div>
            <span class="e-control-wrapper" id="Tags">
              <input class="e-multi-select e-input" name="Tags" />
            </span>
            <span id="Tags_error" data-valmsg-for="Tags" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById("Tags") as any).ej2_instances = [multiSelectEj2];

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("validates both scalar and array fields in same form", () => {
      multiSelectEj2.value = [];
      const d = desc("myForm", [
        { fieldName: "Name", fieldId: "Name", vendor: "native" as const, readExpr: "value", rules: [
          { rule: "required", message: "Name required" }
        ]},
        fusionField("Tags", "Tags", "value", [
          { rule: "atLeastOne", message: "Tags required" }
        ]),
      ]);

      // Both empty → both fail
      expect(validate(d)).toBe(false);
      expect(errorText("Name")).toBe("Name required");
      expect(errorText("Tags")).toBe("Tags required");
    });

    it("passes when both scalar and array fields have values", () => {
      (document.getElementById("Name") as HTMLInputElement).value = "John";
      multiSelectEj2.value = ["urgent"];
      const d = desc("myForm", [
        { fieldName: "Name", fieldId: "Name", vendor: "native" as const, readExpr: "value", rules: [
          { rule: "required", message: "Name required" }
        ]},
        fusionField("Tags", "Tags", "value", [
          { rule: "atLeastOne", message: "Tags required" }
        ]),
      ]);

      expect(validate(d)).toBe(true);
    });
  });
});
