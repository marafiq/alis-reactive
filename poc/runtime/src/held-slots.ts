import type { Owner } from "./owners";
import { Stop } from "./read";

const held: string[] = [];

export function holdSlots(keys: readonly string[], owner: Owner): void {
  for (const key of keys) {
    held.push(key);
    owner.holds(() => {
      held.splice(held.indexOf(key), 1);
    });
  }
}

export function requireNoSlotWithin(element: Node): void {
  for (const key of new Set(held)) {
    const container = document.getElementById(key);
    if (container !== null && element.contains(container)) {
      throw new Stop("The step sets the content of an element that holds the slot '" + key + "', which a live plan loads or unloads, so what the slot loaded would keep running under content it no longer shows.");
    }
  }
}
