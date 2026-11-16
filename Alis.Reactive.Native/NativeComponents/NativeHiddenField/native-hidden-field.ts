import { mountsEach } from "../mounted";

const marker = "data-native-hidden-field";

function heldBy(field: HTMLInputElement): unknown {
  const held: unknown = Reflect.get(HTMLInputElement.prototype, "value", field);
  return held;
}

function raisesChangeOnAWriteThatChangesIt(field: HTMLInputElement): void {
  Object.defineProperty(field, "value", {
    configurable: true,
    get: () => heldBy(field),
    set: (written: unknown) => {
      const before = heldBy(field);
      Reflect.set(HTMLInputElement.prototype, "value", written, field);
      if (heldBy(field) !== before) field.dispatchEvent(new Event("change", { bubbles: true }));
    },
  });
}

export function mountsHiddenFields(): void {
  mountsEach(marker, HTMLInputElement, raisesChangeOnAWriteThatChangesIt);
}
