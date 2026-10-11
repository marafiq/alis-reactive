import type {
  ComponentObject,
  PlanDocument,
  RuntimeObjectSource,
  VendorContract,
} from "../types/index";
import { pluginCatalog, type PluginCatalog } from "../plugins/catalog";
import { RuntimeObject } from "./runtime-object";
import { ComponentDriver } from "./component-driver";

export { RuntimeComponentReadinessError } from "./component-driver";

const cache = new WeakMap<PlanDocument, RuntimePlan>();

type RuntimeValidationContainer = Extract<ComponentObject["container"], { kind: "validation-container" }>;

type RuntimeResolutionTarget =
  | { readonly kind: "active-component"; readonly key: string }
  | { readonly kind: "element"; readonly id: string };

export class RuntimeResolutionError extends Error {
  private constructor(readonly target: RuntimeResolutionTarget, message: string) {
    super(message);
    this.name = "RuntimeResolutionError";
  }

  static componentNotActive(componentKey: string): RuntimeResolutionError {
    return new RuntimeResolutionError(
      { kind: "active-component", key: componentKey },
      `[alis] component not active in Active Plan: ${componentKey}`,
    );
  }

  static elementNotFound(componentId: string): RuntimeResolutionError {
    return new RuntimeResolutionError(
      { kind: "element", id: componentId },
      `[alis] element not found: ${componentId}`,
    );
  }

  static is(error: unknown): error is RuntimeResolutionError {
    return error instanceof RuntimeResolutionError;
  }
}

export class RuntimePlan {
  readonly components: RuntimeComponents;
  readonly plugins: RuntimePlugins;

  private constructor(readonly document: PlanDocument) {
    this.components = new RuntimeComponents(document);
    this.plugins = new RuntimePlugins(pluginCatalog);
  }

  static from(plan: PlanDocument): RuntimePlan {
    const existing = cache.get(plan);
    if (existing) return existing;

    const runtimePlan = new RuntimePlan(plan);
    cache.set(plan, runtimePlan);
    return runtimePlan;
  }

  get planId(): string {
    return this.document.planId;
  }

  objectForSource(source: RuntimeObjectSource): RuntimeObject {
    switch (source.kind) {
      case "component":
        return this.components.component(source.component).object();
      case "plugin":
        return this.plugins.object(source.name);
    }
  }

  urlParameters(): URLSearchParams {
    return new URLSearchParams(window.location.search);
  }
}

export class RuntimeComponents {
  constructor(private readonly plan: PlanDocument) {}

  find(componentKey: string): RuntimeComponent | undefined {
    const component = this.plan.components[componentKey];
    if (!component) return undefined;
    return new RuntimeComponent(componentKey, component, this.plan.vendors);
  }

  entries(): RuntimeComponent[] {
    return Object.entries(this.plan.components)
      .map(([key, component]) => new RuntimeComponent(key, component, this.plan.vendors));
  }

  component(componentKey: string): RuntimeComponent {
    const component = this.find(componentKey);
    if (component === undefined) throw RuntimeResolutionError.componentNotActive(componentKey);

    return component;
  }

  element(componentKey: string): HTMLElement {
    return this.component(componentKey).element();
  }
}

export class RuntimeComponent {
  constructor(
    readonly key: string,
    readonly definition: ComponentObject,
    private readonly vendors: Record<string, VendorContract>,
  ) {}

  get id(): string {
    return this.definition.id;
  }

  get containerScope(): RuntimeValidationContainer | undefined {
    const container = this.definition.container;
    if (container.kind === "none") return undefined;

    return container;
  }

  element(): HTMLElement {
    const element = document.getElementById(this.id);
    if (!element) throw RuntimeResolutionError.elementNotFound(this.id);
    return element;
  }

  tryElement(): HTMLElement | null {
    return document.getElementById(this.id);
  }

  root(): unknown {
    return this.runtime().resolveRoot(this.element());
  }

  object(): RuntimeObject {
    return new RuntimeObject(`component "${this.key}"`, this.root());
  }

  runtime(): ComponentDriver {
    const vendorName = this.definition.vendor;
    return new ComponentDriver(vendorName, this.vendors[vendorName]!);
  }
}

export class RuntimePlugins {
  constructor(private readonly instances: PluginCatalog) {}

  object(pluginName: string): RuntimeObject {
    return new RuntimeObject(`plugin "${pluginName}"`, this.instances.resolve(pluginName));
  }
}
