import { describe, it, expect, beforeEach } from "vitest";

describe("when-using-native-checklist", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  /**
   * Sets up a checklist matching the new DOM structure:
   *   <div id="{elementId}" data-alis-checklist>
   *     <input type="hidden" name="{name}" value="csv" />
   *     <input type="checkbox" id="{elementId}_c0" ... />
   *     ...
   *   </div>
   */
  function setupChecklist(elementId: string, options: string[], checked: string[] = []): void {
    const container = document.createElement("div");
    container.id = elementId;
    container.setAttribute("data-alis-checklist", "");

    const hidden = document.createElement("input");
    hidden.type = "hidden";
    hidden.name = elementId;
    hidden.value = checked.join(",");
    container.appendChild(hidden);

    options.forEach((opt, i) => {
      const cb = document.createElement("input");
      cb.type = "checkbox";
      cb.id = `${elementId}_c${i}`;
      cb.name = elementId;
      cb.value = opt;
      if (checked.includes(opt)) cb.checked = true;
      container.appendChild(cb);
    });
    document.body.appendChild(container);
  }

  /** Mirrors checklist.ts init logic */
  function initChecklist(): void {
    const containers = document.querySelectorAll<HTMLElement>("[data-alis-checklist]");
    for (const container of containers) {
      const hidden = container.querySelector('input[type="hidden"]') as HTMLInputElement | null;
      if (!hidden) continue;

      // Initialize container.value as string[]
      const initial = hidden.value.split(",").filter(Boolean);
      (container as any).value = initial;

      container.addEventListener("change", () => {
        const values: string[] = [];
        const checkboxes = container.querySelectorAll<HTMLInputElement>('input[type="checkbox"]');
        for (const cb of checkboxes) {
          if (cb.checked) values.push(cb.value);
        }
        (container as any).value = values;
        hidden.value = values.join(",");
      });
    }
  }

  // ── Hidden input CSV sync (for MVC form submission) ──

  it("syncs checked values to hidden input on change", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const cb2 = document.getElementById("allergies_c2") as HTMLInputElement;
    const hidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;

    cb0.checked = true;
    cb2.checked = true;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Dairy");
  });

  it("produces empty string when nothing checked", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const hidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;

    cb0.checked = false;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("");
  });

  it("unchecking removes value from comma-separated list", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"], ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb1 = document.getElementById("allergies_c1") as HTMLInputElement;
    const hidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;

    cb1.checked = false;
    cb1.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Dairy");
  });

  it("handles multiple checklists independently", () => {
    setupChecklist("allergies", ["Peanuts", "Dairy"]);
    setupChecklist("amenities", ["WiFi", "Parking", "Pool"]);
    initChecklist();

    const allergyHidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;
    const amenityHidden = document.getElementById("amenities")!.querySelector('input[type="hidden"]') as HTMLInputElement;

    (document.getElementById("allergies_c0") as HTMLInputElement).checked = true;
    document.getElementById("allergies_c0")!.dispatchEvent(new Event("change", { bubbles: true }));

    (document.getElementById("amenities_c1") as HTMLInputElement).checked = true;
    (document.getElementById("amenities_c2") as HTMLInputElement).checked = true;
    document.getElementById("amenities_c1")!.dispatchEvent(new Event("change", { bubbles: true }));

    expect(allergyHidden.value).toBe("Peanuts");
    expect(amenityHidden.value).toBe("Parking,Pool");
  });

  it("preserves order based on option index", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy", "Gluten"]);
    initChecklist();

    const hidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;

    // Check in reverse order — output should follow DOM order (index order)
    (document.getElementById("allergies_c3") as HTMLInputElement).checked = true;
    (document.getElementById("allergies_c1") as HTMLInputElement).checked = true;
    (document.getElementById("allergies_c0") as HTMLInputElement).checked = true;
    document.getElementById("allergies_c0")!.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Shellfish,Gluten");
  });

  it("pre-checked values are reflected in initial hidden input", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"], ["Peanuts", "Dairy"]);
    const hidden = document.getElementById("allergies")!.querySelector('input[type="hidden"]') as HTMLInputElement;
    expect(hidden.value).toBe("Peanuts,Dairy");
  });

  // ── Container.value as string[] (for evalRead + gather) ──

  it("container.value is initialized as string array from pre-checked values", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"], ["Peanuts", "Dairy"]);
    initChecklist();

    const container = document.getElementById("allergies") as any;
    expect(Array.isArray(container.value)).toBe(true);
    expect(container.value).toEqual(["Peanuts", "Dairy"]);
  });

  it("container.value is empty array when nothing pre-checked", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const container = document.getElementById("allergies") as any;
    expect(Array.isArray(container.value)).toBe(true);
    expect(container.value).toEqual([]);
  });

  it("container.value updates to string array on checkbox change", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const container = document.getElementById("allergies") as any;
    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const cb2 = document.getElementById("allergies_c2") as HTMLInputElement;

    cb0.checked = true;
    cb2.checked = true;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(Array.isArray(container.value)).toBe(true);
    expect(container.value).toEqual(["Peanuts", "Dairy"]);
  });

  it("container.value becomes empty array when all unchecked", () => {
    setupChecklist("allergies", ["Peanuts", "Dairy"], ["Peanuts", "Dairy"]);
    initChecklist();

    const container = document.getElementById("allergies") as any;
    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const cb1 = document.getElementById("allergies_c1") as HTMLInputElement;

    cb0.checked = false;
    cb1.checked = false;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(Array.isArray(container.value)).toBe(true);
    expect(container.value).toEqual([]);
  });
});
