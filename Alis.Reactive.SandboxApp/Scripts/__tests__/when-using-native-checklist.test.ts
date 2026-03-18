import { describe, it, expect, beforeEach } from "vitest";

describe("when-using-native-checklist", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  function setupChecklist(elementId: string, options: string[], checked: string[] = []): void {
    const hidden = document.createElement("input");
    hidden.type = "hidden";
    hidden.id = elementId;
    hidden.value = checked.join(",");
    document.body.appendChild(hidden);

    const container = document.createElement("div");
    container.setAttribute("data-alis-checklist", elementId);
    options.forEach((opt, i) => {
      const cb = document.createElement("input");
      cb.type = "checkbox";
      cb.id = `${elementId}_c${i}`;
      cb.name = "test";
      cb.value = opt;
      if (checked.includes(opt)) cb.checked = true;
      container.appendChild(cb);
    });
    document.body.appendChild(container);
  }

  function initChecklist(): void {
    const containers = document.querySelectorAll<HTMLElement>("[data-alis-checklist]");
    for (const container of containers) {
      const elementId = container.getAttribute("data-alis-checklist")!;
      const hidden = document.getElementById(elementId) as HTMLInputElement | null;
      if (!hidden) continue;

      container.addEventListener("change", () => {
        const values: string[] = [];
        let i = 0;
        while (true) {
          const cb = document.getElementById(`${elementId}_c${i}`) as HTMLInputElement | null;
          if (!cb) break;
          if (cb.checked) values.push(cb.value);
          i++;
        }
        hidden.value = values.join(",");
      });
    }
  }

  it("syncs checked values to hidden input on change", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const cb2 = document.getElementById("allergies_c2") as HTMLInputElement;
    const hidden = document.getElementById("allergies") as HTMLInputElement;

    cb0.checked = true;
    cb2.checked = true;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Dairy");
  });

  it("produces empty string when nothing checked", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb0 = document.getElementById("allergies_c0") as HTMLInputElement;
    const hidden = document.getElementById("allergies") as HTMLInputElement;

    cb0.checked = false;
    cb0.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("");
  });

  it("unchecking removes value from comma-separated list", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"], ["Peanuts", "Shellfish", "Dairy"]);
    initChecklist();

    const cb1 = document.getElementById("allergies_c1") as HTMLInputElement;
    const hidden = document.getElementById("allergies") as HTMLInputElement;

    cb1.checked = false;
    cb1.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Dairy");
  });

  it("handles multiple checklists independently", () => {
    setupChecklist("allergies", ["Peanuts", "Dairy"]);
    setupChecklist("amenities", ["WiFi", "Parking", "Pool"]);
    initChecklist();

    const allergyHidden = document.getElementById("allergies") as HTMLInputElement;
    const amenityHidden = document.getElementById("amenities") as HTMLInputElement;

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

    const hidden = document.getElementById("allergies") as HTMLInputElement;

    // Check in reverse order — output should follow DOM order (index order)
    (document.getElementById("allergies_c3") as HTMLInputElement).checked = true;
    (document.getElementById("allergies_c1") as HTMLInputElement).checked = true;
    (document.getElementById("allergies_c0") as HTMLInputElement).checked = true;
    document.getElementById("allergies_c0")!.dispatchEvent(new Event("change", { bubbles: true }));

    expect(hidden.value).toBe("Peanuts,Shellfish,Gluten");
  });

  it("pre-checked values are reflected in initial hidden input", () => {
    setupChecklist("allergies", ["Peanuts", "Shellfish", "Dairy"], ["Peanuts", "Dairy"]);
    const hidden = document.getElementById("allergies") as HTMLInputElement;
    expect(hidden.value).toBe("Peanuts,Dairy");
  });
});
