import type { Vendor } from "../types/index";
import { wire as wireFusion } from "../events/event-fusion";
import { wire as wireNative } from "../events/event-native";

interface FusionElement extends HTMLElement {
  readonly ej2_instances?: unknown[];
}

/** The per-vendor driver — THE sole place vendor knowledge lives. Was ComponentRuntime. */
export interface ComponentDriver {
  resolveRoot(element: HTMLElement): unknown;

  wireEvent(
    root: unknown,
    channel: string,
    handler: (data: unknown) => void,
    opts: AddEventListenerOptions | undefined,
  ): void;
}

export class RuntimeComponentReadinessError extends Error {
  private constructor(
    readonly componentId: string,
    readonly vendor: Vendor,
    message: string,
  ) {
    super(message);
    this.name = "RuntimeComponentReadinessError";
  }

  static vendorRootMissing(componentId: string, vendor: Vendor): RuntimeComponentReadinessError {
    return new RuntimeComponentReadinessError(
      componentId,
      vendor,
      `[alis] component root not ready: ${componentId} (vendor: ${vendor})`,
    );
  }

  static is(error: unknown): error is RuntimeComponentReadinessError {
    return error instanceof RuntimeComponentReadinessError;
  }
}

const componentDrivers = new Map<Vendor, ComponentDriver>();

function componentDriverNotRegistered(
  componentId: string,
  vendor: Vendor,
  registeredVendors: readonly Vendor[],
): Error {
  const registered = registeredVendors.length === 0
    ? "none"
    : registeredVendors.join(", ");

  return new Error(
    `[alis] component runtime not registered for component "${componentId}" ` +
    `(vendor: ${vendor}; registered: ${registered})`,
  );
}

export function requireComponentDriver(componentId: string, vendor: Vendor): ComponentDriver {
  const driver = componentDrivers.get(vendor);
  if (driver) return driver;

  throw componentDriverNotRegistered(componentId, vendor, [...componentDrivers.keys()]);
}

export function registerComponentDriver(vendor: Vendor, driver: ComponentDriver): void {
  if (componentDrivers.has(vendor)) throw new Error(`[alis] component runtime already registered for vendor "${vendor}"`);

  componentDrivers.set(vendor, driver);
}

const nativeComponentDriver: ComponentDriver = {
  resolveRoot: element => element,
  wireEvent: wireNative,
};

const fusionComponentDriver: ComponentDriver = {
  resolveRoot: element => {
    const root = (element as FusionElement).ej2_instances?.[0];
    if (root !== undefined && root !== null) return root;

    throw RuntimeComponentReadinessError.vendorRootMissing(element.id, "fusion");
  },
  wireEvent: wireFusion,
};

registerComponentDriver("native", nativeComponentDriver);
registerComponentDriver("fusion", fusionComponentDriver);
