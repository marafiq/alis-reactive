// alis.slot (grammar.md 6.5), keyed by the container element's id, which is looked up at every call so
// a slot can sit inside content that appears after boot.
// S1 load(html), in order: take the plan scripts out of the HTML; release the owners of the slots
//    inside the old content, innermost first; raise alis:unmount on the old content; replace it;
//    re-create the inserted <script> nodes in document order so vendor scripts run; raise
//    alis:mount; release the previous owner; start a new owner with the plans found. HTML with no
//    plan only unloads.
// S2 unload() releases the owners of the slots inside the content, then the slot's own owner.

import type { Module } from "../objects/modules";
import { Owner } from "../objects/owner";
import { discoverPlans } from "../plans/discover";
import { announceMount, announceUnmount } from "../plans/mount-events";
import { startOwner } from "../plans/start";
import { logger } from "../diagnostics/trace";

const log = logger("boot");

const owners = new Map<string, Owner>();

export interface SlotObject {
  load(html: unknown): void;
  unload(): void;
}

function container(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (!element) throw new Error(`[alis] slot #${id} is not on the page`);
  return element;
}

function load(id: string, html: unknown): void {
  if (typeof html !== "string") throw new Error(`[alis] slot #${id} expects HTML text, got ${typeof html}`);
  const target = container(id);

  const host = document.createElement("div");
  host.innerHTML = html;
  const plans = discoverPlans(host, true);

  unloadInside(target);
  announceUnmount(target);
  target.innerHTML = "";
  target.append(...Array.from(host.childNodes));
  runInsertedScripts(target);
  announceMount(target);

  unload(id);
  if (plans.length === 0) return;

  const owner = Owner.slot(id);
  owners.set(id, owner);
  startOwner(owner, plans);
  log.info("partial-slot.load", { slotId: id, plans: plans.length });
}

function unload(id: string): void {
  const container = document.getElementById(id);
  if (container !== null) unloadInside(container);
  const owner = owners.get(id);
  if (!owner) return;
  owners.delete(id);
  owner.dispose();
  log.info("partial-slot.unload", { slotId: id });
}

// A slot loaded inside content that is going away is released with it; a slot inside that one goes
// first, so disposers run innermost first.
function unloadInside(container: HTMLElement): void {
  const nested = [...owners.keys()].filter(id => {
    const element = document.getElementById(id);
    return element !== null && element !== container && container.contains(element);
  });
  for (const id of nested.reverse()) unload(id);
}

// Scripts inserted through innerHTML do not run. Re-creating each one in document order runs it the
// way the browser runs a parsed page, for any vendor's initialization script.
function runInsertedScripts(root: HTMLElement): void {
  for (const original of Array.from(root.querySelectorAll("script"))) {
    const replacement = document.createElement("script");
    for (const attribute of Array.from(original.attributes)) {
      replacement.setAttribute(attribute.name, attribute.value);
    }
    replacement.text = original.text;
    original.replaceWith(replacement);
  }
}

export const slotModule: Module = {
  object: (id): SlotObject => ({
    load: html => load(id, html),
    unload: () => unload(id),
  }),
};

export function resetSlotsForTests(): void {
  for (const owner of owners.values()) owner.dispose();
  owners.clear();
}
