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

function load(key: string, html: string): void {
  const container = containerOf(key);
  release(key);
  container.innerHTML = html;
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: container } }));
  const owner = new Owner();
  owners.set(key, owner);
  startWithin(container, owner);
}

function unload(key: string): void {
  const container = containerOf(key);
  release(key);
  container.replaceChildren();
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
