import { afterEach, describe, expect, it } from "vitest";
import { requireComponentDriver } from "../browser-objects/component-driver";

afterEach(() => { document.body.innerHTML = ""; });

function selectWithOptions(): HTMLSelectElement {
  const select = document.createElement("select");
  select.innerHTML = '<option value="">Choose a resident</option><option value="0">Resident zero</option><option value="1043">Arthur Bell</option><option value="null">Literal null</option>';
  document.body.appendChild(select);
  return select;
}

function nativeRoot(select: HTMLSelectElement): HTMLSelectElement {
  return requireComponentDriver(select.id, "native").resolveRoot(select) as HTMLSelectElement;
}

describe("native dropdown value delivery", () => {
  it("shows the empty-option label when a nullable model value is cleared", () => {
    const select = selectWithOptions();
    const root = nativeRoot(select);
    root.value = "1043";

    Reflect.set(root, "value", null);

    expect(select.selectedOptions[0]!.textContent).toBe("Choose a resident");
    expect(root.value).toBe("");
  });

  it("preserves numeric zero and the literal string null as selected option values", () => {
    const select = selectWithOptions();
    const root = nativeRoot(select);

    Reflect.set(root, "value", 0);
    expect(select.selectedOptions[0]!.textContent).toBe("Resident zero");
    root.value = "null";
    expect(select.selectedOptions[0]!.textContent).toBe("Literal null");
  });

  it("keeps native focus and change listeners on the real select", () => {
    const select = selectWithOptions();
    const root = nativeRoot(select);
    let changed = false;
    root.addEventListener("change", () => { changed = true; });

    root.focus();
    select.dispatchEvent(new Event("change"));

    expect(document.activeElement).toBe(select);
    expect(changed).toBe(true);
  });

  it("clears a select without an empty option without inventing a selection", () => {
    const select = selectWithOptions();
    select.options[0]!.remove();

    Reflect.set(nativeRoot(select), "value", null);

    expect(select.selectedOptions).toHaveLength(0);
    expect(select.value).toBe("");
  });
});
