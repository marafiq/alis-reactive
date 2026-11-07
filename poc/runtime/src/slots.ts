import { Owner } from "./owners";
import { holdSlots } from "./held-slots";
import { startWithin } from "./plans";
import { Stop } from "./read";

const owners = new Map<string, Owner>();

export function slot(key: string): object {
  return {
    load: (html: string) => {
      load(key, html);
    },
    unload: () => {
      unload(key);
    },
  };
}

export function slotOwner(key: string): Owner {
  const held = owners.get(key);
  if (held !== undefined) return held;
  const owner = new Owner();
  owners.set(key, owner);
  holdSlots([key], owner);
  return owner;
}

function load(key: string, html: string): void {
  const container = containerOf(key);
  releaseWithin(container, key);
  container.innerHTML = html;
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: container } }));
  startWithin(container, slotOwner(key));
}

function unload(key: string): void {
  const container = containerOf(key);
  releaseWithin(container, key);
  container.replaceChildren();
}

function releaseWithin(container: HTMLElement, key: string): void {
  const inside = [...owners.keys()].flatMap((held) => {
    const element = document.getElementById(held);
    return held !== key && element !== null && container.contains(element) ? [{ held, element }] : [];
  });
  inside.sort((one, other) => (one.element.compareDocumentPosition(other.element) & Node.DOCUMENT_POSITION_FOLLOWING ? 1 : -1));
  for (const { held, element } of inside) release(held, element);
  release(key, container);
}

function release(key: string, container: HTMLElement): void {
  document.dispatchEvent(new CustomEvent("alis:unmount", { detail: { root: container } }));
  const owner = owners.get(key);
  if (owner === undefined) return;
  owners.delete(key);
  owner.release();
}

function containerOf(key: string): HTMLElement {
  const container = document.getElementById(key);
  if (container === null) throw new Stop("No element has the id '" + key + "', the slot's container.");
  return container;
}
