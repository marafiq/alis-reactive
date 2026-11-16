import { mountsEach } from "../mounted";

const marker = "data-native-radio-group";

function radioNamed(group: HTMLInputElement, id: unknown): HTMLInputElement {
  if (typeof id !== "string") throw new Error("The radio group '" + group.id + "' names a radio by no id.");
  const radio = document.getElementById(id);
  if (!(radio instanceof HTMLInputElement)) throw new Error("The radio group '" + group.id + "' names the radio '" + id + "', which the page lacks.");
  return radio;
}

function radiosOf(group: HTMLInputElement): HTMLInputElement[] {
  const written = group.getAttribute(marker);
  if (written === null) throw new Error("The radio group '" + group.id + "' carries no " + marker + ".");
  const ids: unknown = JSON.parse(written);
  if (!Array.isArray(ids)) throw new Error("The radio group '" + group.id + "' names its radios in no list.");
  return ids.map((id: unknown) => radioNamed(group, id));
}

function heldBy(group: HTMLInputElement): unknown {
  const held: unknown = Reflect.get(HTMLInputElement.prototype, "value", group);
  return held;
}

function holds(group: HTMLInputElement, value: unknown): void {
  Reflect.set(HTMLInputElement.prototype, "value", value, group);
}

function chosenOf(radios: readonly HTMLInputElement[]): HTMLInputElement | undefined {
  return radios.find((radio) => radio.checked);
}

function checksTheRadioItHolds(group: HTMLInputElement, radios: readonly HTMLInputElement[]): void {
  const held = heldBy(group);
  for (const radio of radios) radio.checked = held !== "" && radio.value === held;
}

function mountsGroup(group: HTMLInputElement): void {
  const radios = radiosOf(group);
  const chosen = chosenOf(radios);
  if (chosen !== undefined) holds(group, chosen.value);
  Object.defineProperty(group, "value", {
    configurable: true,
    get: () => heldBy(group),
    set: (written: unknown) => {
      holds(group, written);
      checksTheRadioItHolds(group, radios);
    },
  });
  Object.defineProperty(group, "focus", {
    configurable: true,
    value: () => {
      (chosenOf(radios) ?? radios[0])?.focus();
    },
  });
  for (const radio of radios) {
    radio.addEventListener("change", () => {
      holds(group, radio.value);
      group.dispatchEvent(new Event("change", { bubbles: true }));
    });
  }
}

export function mountsRadioGroups(): void {
  mountsEach(marker, HTMLInputElement, mountsGroup);
}
