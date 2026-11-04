import type { MethodVendorListening, Vendor } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { call, read, write } from "../grammar/paths";
import { isMissing } from "../grammar/shape";

export type Listener = (payload: unknown) => void;

type VendorListener = (first: unknown) => void;

export function vendorObject(element: HTMLElement, vendorName: string, vendor: Vendor): unknown {
  const root = read(element, vendor.objectPath, "optional", `#${element.id}`);
  if (!isMissing(root)) return root;
  throw new Error(`[alis] #${element.id} has no ${vendorName} object yet (${vendor.objectPath.join(".")})`);
}

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
  listen: MethodVendorListening,
  event: string,
  vendorListener: VendorListener,
): () => void {
  call(root, [listen.subscribe], [event, vendorListener], "checked", `${vendorName} ${listen.subscribe}`);
  return () => {
    call(root, [listen.unsubscribe], [event, vendorListener], "checked", `${vendorName} ${listen.unsubscribe}`);
  };
}

interface AssignedListener {
  (this: unknown, ...args: unknown[]): void;
  previous: unknown;
  disposed: boolean;
}

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

function payloadOf(vendor: Vendor, first: unknown): unknown {
  switch (vendor.payload) {
    case "argument": return first;
    case "element":  return elementPayload(first);
    default:         return assertNever(vendor.payload, "vendor payload");
  }
}

function elementPayload(first: unknown): unknown {
  if (first instanceof CustomEvent) return isMissing(first.detail) ? first : first.detail;
  if (first instanceof Event) return first.currentTarget ?? first;
  return first;
}
