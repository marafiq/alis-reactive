// A vendor is data in the plan: where the object lives on the rendered element, how its objects take a
// listener (a pair of methods, or assignment to the property the event names), and what a listener
// receives (grammar.md 6.9). This driver follows that data, so the runtime holds no code for any vendor.

import type { MethodListening, Vendor } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { call, read, write } from "../grammar/paths";
import { isMissing } from "../grammar/shape";

export type Listener = (payload: unknown) => void;

type VendorListener = (first: unknown) => void;

/** The vendor's object on the element, or a boundary error when the widget is not constructed. */
export function vendorObject(element: HTMLElement, vendorName: string, vendor: Vendor): unknown {
  const root = read(element, vendor.objectPath, "optional", `#${element.id}`);
  if (!isMissing(root)) return root;
  throw new Error(`[alis] #${element.id} has no ${vendorName} object yet (${vendor.objectPath.join(".")})`);
}

/** Subscribes the way the vendor's data says and returns the unsubscribe. */
export function subscribeVendor(
  root: unknown,
  vendorName: string,
  vendor: Vendor,
  event: string,
  listener: Listener,
): () => void {
  const vendorListener: VendorListener = first => listener(payloadOf(vendor, first));
  const listen = vendor.listen;
  switch (listen.kind) {
    case "method":     return subscribeByMethod(root, vendorName, listen, event, vendorListener);
    case "assignment": return assignListener(root, vendorName, event, vendorListener);
    default:           return assertNever(listen, "vendor listening");
  }
}

function subscribeByMethod(
  root: unknown,
  vendorName: string,
  listen: MethodListening,
  event: string,
  vendorListener: VendorListener,
): () => void {
  call(root, [listen.subscribe], [event, vendorListener], "checked", `${vendorName} ${listen.subscribe}`);
  return () => {
    call(root, [listen.unsubscribe], [event, vendorListener], "checked", `${vendorName} ${listen.unsubscribe}`);
  };
}

/** A listener assigned to a callback property; it remembers what the property held and whether it is gone. */
interface AssignedListener {
  (this: unknown, ...args: unknown[]): void;
  previous: unknown;
  disposed: boolean;
}

// Assigning adds a listener as `on` would: the function the property already holds runs first and keeps running, so a widget's own callback and an earlier handler both stay; disposing puts back what the property held.
function assignListener(root: unknown, vendorName: string, event: string, vendorListener: VendorListener): () => void {
  const label = `${vendorName} ${event}`;
  const previous = read(root, [event], "checked", label);
  const assigned = function (this: unknown, ...args: unknown[]): void {
    if (typeof previous === "function") previous.apply(this, args);
    if (!assigned.disposed) vendorListener(args[0]);
  } as AssignedListener;
  assigned.previous = previous;
  assigned.disposed = false;
  write(root, [event], assigned, "checked", label);
  return () => {
    assigned.disposed = true;
    // A listener assigned later holds this one as its previous and stays in place; it puts the property
    // back when it goes, skipping every listener that went before it, so a disposed handler never returns.
    if (read(root, [event], "checked", label) === assigned) write(root, [event], live(assigned.previous), "checked", label);
  };
}

function live(value: unknown): unknown {
  let current = value;
  while (isAssignedListener(current) && current.disposed) current = current.previous;
  return current;
}

function isAssignedListener(value: unknown): value is AssignedListener {
  return typeof value === "function" && "disposed" in value && "previous" in value;
}

// A member read off a payload is an optional path (grammar.md 1.3), so a listener argument the vendor
// did not pass reads as missing members; nothing stands in for it.
function payloadOf(vendor: Vendor, first: unknown): unknown {
  switch (vendor.payload) {
    case "argument": return first;
    case "element":  return elementPayload(first);
    default:         return assertNever(vendor.payload, "vendor payload");
  }
}

// A native input's handler reads the element, which stays valid after an await; a custom event
// carries its detail, or is the payload itself when it has none, as the document's events are.
function elementPayload(first: unknown): unknown {
  if (first instanceof CustomEvent) return isMissing(first.detail) ? first : first.detail;
  // A vendor that hands the listener an event it did not dispatch has no current target.
  if (first instanceof Event) return first.currentTarget ?? first;
  return first;
}
