import { mountsEach } from "../mounted";

const marker = "data-native-check-list";

export interface NativeCheckListElement extends HTMLDivElement {
  get value(): string[];
  set value(written: readonly string[] | null);
}

function inputNamed(list: HTMLDivElement, id: unknown): HTMLInputElement {
  if (typeof id !== "string") throw new Error("The check list '" + list.id + "' names an input by no id.");
  const input = document.getElementById(id);
  if (!(input instanceof HTMLInputElement)) throw new Error("The check list '" + list.id + "' names the input '" + id + "', which the page lacks.");
  return input;
}

function startOf(list: HTMLDivElement): object {
  const written = list.getAttribute(marker);
  if (written === null) throw new Error("The check list '" + list.id + "' carries no " + marker + ".");
  const start: unknown = JSON.parse(written);
  if (typeof start !== "object" || start === null) throw new Error("The check list '" + list.id + "' starts from no object.");
  return start;
}

function boxesOf(list: HTMLDivElement, start: object): HTMLInputElement[] {
  if (!("boxes" in start) || !Array.isArray(start.boxes)) throw new Error("The check list '" + list.id + "' names its boxes in no list.");
  return start.boxes.map((id: unknown) => inputNamed(list, id));
}

function joinedOf(list: HTMLDivElement, start: object): HTMLInputElement[] {
  return "joined" in start ? [inputNamed(list, start.joined)] : [];
}

function valuesOf(boxes: readonly HTMLInputElement[]): string[] {
  return boxes.filter((box) => box.checked).map((box) => box.value);
}

function heldIn(list: HTMLDivElement, written: unknown): readonly unknown[] {
  if (written === null || written === undefined) return [];
  if (!Array.isArray(written)) throw new Error("The check list '" + list.id + "' takes a list or nothing.");
  return written;
}

function mountsList(list: HTMLDivElement): void {
  const start = startOf(list);
  const boxes = boxesOf(list, start);
  const joined = joinedOf(list, start);
  const writesJoinedText = (): void => {
    for (const input of joined) input.value = valuesOf(boxes).join(",");
  };
  Object.defineProperty(list, "value", {
    configurable: true,
    get: () => valuesOf(boxes),
    set: (written: unknown) => {
      const held = heldIn(list, written);
      for (const box of boxes) box.checked = held.includes(box.value);
      writesJoinedText();
    },
  });
  Object.defineProperty(list, "focus", {
    configurable: true,
    value: () => {
      boxes[0]?.focus();
    },
  });
  for (const box of boxes) box.addEventListener("change", writesJoinedText);
}

export function mountsCheckLists(): void {
  mountsEach(marker, HTMLDivElement, mountsList);
}
