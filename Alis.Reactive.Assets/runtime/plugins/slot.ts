// alis.slot (grammar.md 6.5), keyed by the container element's id, which is looked up at every call so
// a slot can sit inside content that appears after boot.
// S1 load(html), in order: take the plan scripts out of the HTML; raise alis:unmount on the old
//    content, which releases the owners of the slots inside it, innermost first, and the slot's own;
//    replace it; re-create the inserted <script> nodes in document order so vendor scripts run; raise
//    alis:mount; start a new owner with the plans found. HTML with no plan only unloads.
// S2 unload() raises alis:unmount on the content, releasing the same owners, and empties the slot.
// S3 Wired HTML that anything else removes (a drawer closing on content a plan loaded into it) is
//    announced the same way, alis:unmount on its root, and the owners inside it are released alike.

import type { Plugin } from "../objects/plugins";
import { Owner } from "../objects/owner";
import { discoverPlans } from "../plans/discover";
import { announceMount, announceUnmount, UNMOUNT_EVENT, type MountEventDetail } from "../plans/mount-events";
import { startOwner } from "../plans/start";
import { logger } from "../diagnostics/trace";

const log = logger("boot");

const owners = new Map<string, Owner>();
let unmountsListened = false;

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
  listenForUnmounts();
  const target = container(id);

  const host = document.createElement("div");
  host.innerHTML = html;
  const plans = discoverPlans(host, true);

  announceUnmount(target);
  target.innerHTML = "";
  target.append(...Array.from(host.childNodes));
  runInsertedScripts(target);
  announceMount(target);
  if (plans.length === 0) return;

  const owner = Owner.slot(id);
  owners.set(id, owner);
  startOwner(owner, plans);
  log.info("partial-slot.load", { slotId: id, plans: plans.length });
}

function unload(id: string): void {
  listenForUnmounts();
  const target = container(id);
  announceUnmount(target);
  target.innerHTML = "";
}

// Wired HTML about to go: the owners of the slots inside it go first, innermost first, then the owner
// of the slot it is, when it is one.
function releaseWithin(root: HTMLElement): void {
  const inside = [...owners.keys()].filter(id => {
    const element = document.getElementById(id);
    return element !== null && element !== root && root.contains(element);
  });
  for (const id of inside.reverse()) release(id);
  release(root.id);
}

function release(id: string): void {
  const owner = owners.get(id);
  if (!owner) return;
  owners.delete(id);
  owner.dispose();
  log.info("partial-slot.unload", { slotId: id });
}

function listenForUnmounts(): void {
  if (unmountsListened) return;
  unmountsListened = true;
  document.addEventListener(UNMOUNT_EVENT, event => releaseWithin((event as CustomEvent<MountEventDetail>).detail.root));
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

export const slotPlugin: Plugin = {
  object: (id): SlotObject => ({
    load: html => load(id, html),
    unload: () => unload(id),
  }),
};

export function resetSlotsForTests(): void {
  for (const owner of owners.values()) owner.dispose();
  owners.clear();
}
