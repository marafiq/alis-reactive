// A vendor is data in the plan: where the object lives on the rendered element, which of its methods
// add and remove listeners, and what a listener receives (grammar.md 6.9). This driver follows that
// data, so the runtime holds no code for any vendor.

import type { Vendor } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { call, read } from "../grammar/paths";
import { isMissing } from "../grammar/shape";

export type Listener = (payload: unknown) => void;

/** The vendor's object on the element, or a boundary error when the widget is not constructed. */
export function vendorObject(element: HTMLElement, vendorName: string, vendor: Vendor): unknown {
  const root = read(element, vendor.objectPath, "optional", `#${element.id}`);
  if (!isMissing(root)) return root;
  throw new Error(`[alis] #${element.id} has no ${vendorName} object yet (${vendor.objectPath.join(".")})`);
}

/** Subscribes through the vendor's own methods and returns the unsubscribe. */
export function subscribeVendor(
  root: unknown,
  vendorName: string,
  vendor: Vendor,
  event: string,
  listener: Listener,
): () => void {
  const vendorListener = (first: unknown): void => listener(payloadOf(vendor, first));
  call(root, [vendor.subscribe], [event, vendorListener], "checked", `${vendorName} ${vendor.subscribe}`);
  return () => {
    call(root, [vendor.unsubscribe], [event, vendorListener], "checked", `${vendorName} ${vendor.unsubscribe}`);
  };
}

function payloadOf(vendor: Vendor, first: unknown): unknown {
  switch (vendor.payload) {
    case "argument": return isMissing(first) ? {} : first;
    case "element":  return elementPayload(first);
    default:         return assertNever(vendor.payload, "vendor payload");
  }
}

// A native input's handler reads the element, which stays valid after an await; a custom event
// carries its detail.
function elementPayload(first: unknown): unknown {
  if (first instanceof CustomEvent) return isMissing(first.detail) ? {} : first.detail;
  if (first instanceof Event) return first.currentTarget ?? first.target ?? first;
  return isMissing(first) ? {} : first;
}
