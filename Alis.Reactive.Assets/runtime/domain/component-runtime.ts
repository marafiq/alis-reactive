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

class RuntimeVendorToken {
  private constructor(readonly value: Vendor) {}

  static from(vendor: Vendor): RuntimeVendorToken {
    const vendorWasDeclared = typeof vendor === "string" && vendor.trim().length > 0;
    if (vendorWasDeclared) return new RuntimeVendorToken(vendor);

    throw ComponentRuntimeRegistrationError.invalidVendor(vendor);
  }
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

export class ComponentRuntimeRegistrationError extends Error {
  private constructor(message: string) {
    super(message);
    this.name = "ComponentRuntimeRegistrationError";
  }

  static invalidVendor(vendor: unknown): ComponentRuntimeRegistrationError {
    return new ComponentRuntimeRegistrationError(
      `[alis] component runtime vendor must be a non-empty string; received ${String(vendor)}`,
    );
  }

  static duplicate(vendor: Vendor): ComponentRuntimeRegistrationError {
    return new ComponentRuntimeRegistrationError(
      `[alis] component runtime already registered for vendor "${vendor}"`,
    );
  }
}

export class ComponentRuntimeNotRegisteredError extends Error {
  private constructor(
    readonly componentId: string,
    readonly vendor: Vendor,
    message: string,
  ) {
    super(message);
    this.name = "ComponentRuntimeNotRegisteredError";
  }

  static missing(
    componentId: string,
    vendor: Vendor,
    registeredVendors: readonly Vendor[],
  ): ComponentRuntimeNotRegisteredError {
    const registered = registeredVendors.length === 0
      ? "none"
      : registeredVendors.join(", ");

    return new ComponentRuntimeNotRegisteredError(
      componentId,
      vendor,
      `[alis] component runtime not registered for component "${componentId}" (vendor: ${vendor}; registered: ${registered})`,
    );
  }
}

export class ComponentRuntime {
  private constructor(private readonly driver: ComponentRuntimeDriver) {}

  static for(componentId: string, vendor: Vendor): ComponentRuntime {
    return componentRuntimes.require(componentId, vendor);
  }

  static fromDriver(driver: ComponentRuntimeDriver): ComponentRuntime {
    return new ComponentRuntime(driver);
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
    const token = RuntimeVendorToken.from(vendor);
    if (this.drivers.has(token.value)) throw ComponentRuntimeRegistrationError.duplicate(token.value);

    this.drivers.set(token.value, driver);
  }

  require(componentId: string, vendor: Vendor): ComponentRuntime {
    const token = RuntimeVendorToken.from(vendor);
    const driver = this.drivers.get(token.value);
    if (driver) return ComponentRuntime.fromDriver(driver);

    throw ComponentRuntimeNotRegisteredError.missing(
      componentId,
      token.value,
      [...this.drivers.keys()],
    );
  }
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
