import { Owner } from "./owners";
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
  const inside = [...owners.keys()].filter((held) => held !== key && holds(container, held));
  for (const held of inside.sort((one, other) => afterInTheDocument(one, other))) release(held);
  release(key);
}

function holds(container: HTMLElement, key: string): boolean {
  const element = document.getElementById(key);
  return element !== null && container.contains(element);
}

function afterInTheDocument(one: string, other: string): number {
  const first = containerOf(one);
  const second = containerOf(other);
  return first.compareDocumentPosition(second) & Node.DOCUMENT_POSITION_FOLLOWING ? 1 : -1;
}

function release(key: string): void {
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
