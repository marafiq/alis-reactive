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
  const target = container(id);

  const host = document.createElement("div");
  host.innerHTML = html;
  const plans = discoverPlans(host, true).map(found => found.plan);

  announceUnmount(target);
  target.innerHTML = "";
  target.append(...Array.from(host.childNodes));
  runInsertedScripts(target);
  announceMount(target);
  if (plans.length === 0) return;

  startOwner(slotOwner(id), plans);
  log.info("partial-slot.load", { slotId: id, plans: plans.length });
}

export function slotOwner(id: string): Owner {
  listenForUnmounts();
  const owner = Owner.slot(id);
  owners.set(id, owner);
  return owner;
}

function unload(id: string): void {
  const target = container(id);
  announceUnmount(target);
  target.innerHTML = "";
}

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
