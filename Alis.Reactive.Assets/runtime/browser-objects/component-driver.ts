// A component's vendor is data in the plan: where the object lives on the rendered element, which
// of its methods add and remove listeners, and what a listener receives. This driver follows that
// data, so the runtime holds no code for any vendor.

import type { PathSegment, VendorContract, VendorEventPayload } from "../types/index";
import { RuntimePath } from "./runtime-path";
import { assertNever } from "../shared/assert-never";

export class RuntimeComponentReadinessError extends Error {
  private constructor(
    readonly componentId: string,
    readonly vendor: string,
    message: string,
  ) {
    super(message);
    this.name = "RuntimeComponentReadinessError";
  }

  static vendorRootMissing(componentId: string, vendor: string): RuntimeComponentReadinessError {
    return new RuntimeComponentReadinessError(
      componentId,
      vendor,
      `[alis] component object not ready: ${componentId} (vendor: ${vendor})`,
    );
  }

  static is(error: unknown): error is RuntimeComponentReadinessError {
    return error instanceof RuntimeComponentReadinessError;
  }
}

type Listener = (...args: unknown[]) => void;

export class ComponentDriver {
  constructor(
    private readonly vendorName: string,
    private readonly vendor: VendorContract,
  ) {}

  resolveRoot(element: HTMLElement): unknown {
    const root = RuntimePath.from(this.vendor.objectPath).read(element);
    if (root !== undefined && root !== null) return root;

    throw RuntimeComponentReadinessError.vendorRootMissing(element.id, this.vendorName);
  }

  wireEvent(
    root: unknown,
    channel: string,
    handler: (data: unknown) => void,
    opts: AddEventListenerOptions | undefined,
  ): void {
    if (opts?.signal?.aborted === true) return;

    const listener: Listener = (...args) => {
      handler(listenerPayload(this.vendor.payload, args[0]));
    };

    this.invoke(root, this.vendor.subscribe, channel, listener);
    opts?.signal?.addEventListener(
      "abort",
      () => this.invoke(root, this.vendor.unsubscribe, channel, listener),
      { once: true },
    );
  }

  private invoke(root: unknown, method: string, channel: string, listener: Listener): void {
    RuntimePath.from([memberSegment(method)]).call(
      root,
      [channel, listener],
      `vendor "${this.vendorName}" ${method}`,
    );
  }
}

function memberSegment(name: string): PathSegment {
  return { kind: "property", name };
}

function listenerPayload(payload: VendorEventPayload, firstArgument: unknown): unknown {
  switch (payload) {
    case "argument":
      return argumentPayload(firstArgument);
    case "dom-event":
      return domEventPayload(firstArgument as Event);
    default:
      return assertNever(payload, "vendor event payload");
  }
}

function argumentPayload(firstArgument: unknown): unknown {
  const argumentWasPassed = firstArgument !== null && firstArgument !== undefined;
  if (argumentWasPassed) return firstArgument;

  return {};
}

function domEventPayload(event: Event): unknown {
  if (event instanceof CustomEvent) return argumentPayload(event.detail);
  if (event.currentTarget !== null) return event.currentTarget;
  if (event.target !== null) return event.target;

  return event;
}
