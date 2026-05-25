import type { Vendor } from "../types";
import { wire as wireFusion } from "../resolution/event-fusion";
import { wire as wireNative } from "../resolution/event-native";

interface FusionElement extends HTMLElement {
  readonly ej2_instances?: unknown[];
}

export interface ComponentRuntimeDriver {
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

export class ComponentRuntime {
  constructor(private readonly driver: ComponentRuntimeDriver) {}

  static for(componentId: string, vendor: Vendor): ComponentRuntime {
    return componentRuntimes.require(componentId, vendor);
  }

  resolveRoot(element: HTMLElement): unknown {
    return this.driver.resolveRoot(element);
  }

  wireEvent(
    root: unknown,
    channel: string,
    handler: (data: unknown) => void,
    opts: AddEventListenerOptions | undefined,
  ): void {
    this.driver.wireEvent(root, channel, handler, opts);
  }
}

class ComponentRuntimeRegistry {
  private readonly drivers = new Map<Vendor, ComponentRuntimeDriver>();

  register(vendor: Vendor, driver: ComponentRuntimeDriver): void {
    if (this.drivers.has(vendor)) throw new Error(`[alis] component runtime already registered for vendor "${vendor}"`);

    this.drivers.set(vendor, driver);
  }

  require(componentId: string, vendor: Vendor): ComponentRuntime {
    const driver = this.drivers.get(vendor);
    if (driver) return new ComponentRuntime(driver);

    throw componentRuntimeNotRegistered(componentId, vendor, [...this.drivers.keys()]);
  }
}

function componentRuntimeNotRegistered(
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

const componentRuntimes = new ComponentRuntimeRegistry();

export function registerComponentRuntime(vendor: Vendor, driver: ComponentRuntimeDriver): void {
  componentRuntimes.register(vendor, driver);
}

const nativeComponentRuntime: ComponentRuntimeDriver = {
  resolveRoot: element => element,
  wireEvent: wireNative,
};

const fusionComponentRuntime: ComponentRuntimeDriver = {
  resolveRoot: element => {
    const root = (element as FusionElement).ej2_instances?.[0];
    if (root !== undefined && root !== null) return root;

    throw RuntimeComponentReadinessError.vendorRootMissing(element.id, "fusion");
  },
  wireEvent: wireFusion,
};

registerComponentRuntime("native", nativeComponentRuntime);
registerComponentRuntime("fusion", fusionComponentRuntime);
