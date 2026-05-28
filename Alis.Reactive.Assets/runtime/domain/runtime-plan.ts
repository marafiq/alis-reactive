import type {
  BrowserObjectContract,
  ComponentObject,
  PlanDocument,
  RuntimeObjectSource,
} from "../types";
import { browserPlugins, type BrowserPluginCatalog } from "../core/plugin-catalog";
import { RuntimeObject } from "./runtime-object";
import { ComponentRuntime } from "./component-runtime";

export { RuntimeComponentReadinessError } from "./component-runtime";

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
      `[alis] component not active in browser plan: ${componentKey}`,
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
  readonly components: RuntimeComponentCatalog;
  readonly objectContracts: RuntimeObjectContractCatalog;
  readonly plugins: RuntimePluginCatalog;

  private constructor(readonly document: PlanDocument) {
    this.objectContracts = new RuntimeObjectContractCatalog(document);
    this.components = new RuntimeComponentCatalog(document, this.objectContracts);
    this.plugins = new RuntimePluginCatalog(this.objectContracts, browserPlugins);
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
        return this.plugins.object(source.name, source.type);
    }
  }

  urlParameters(): URLSearchParams {
    return new URLSearchParams(window.location.search);
  }
}

export class RuntimeObjectContractCatalog {
  constructor(private readonly plan: PlanDocument) {}

  require(typeKey: string): BrowserObjectContract {
    const objectContract = this.plan.types[typeKey];
    if (!objectContract) throw new Error(`[alis] object contract not found: ${typeKey}`);
    return objectContract;
  }
}

export class RuntimeComponentCatalog {
  constructor(
    private readonly plan: PlanDocument,
    private readonly objectContracts: RuntimeObjectContractCatalog,
  ) {}

  find(componentKey: string): RuntimeComponent | undefined {
    const component = this.plan.components[componentKey];
    if (!component) return undefined;
    return new RuntimeComponent(componentKey, component, this.objectContracts);
  }

  entries(): RuntimeComponent[] {
    return Object.entries(this.plan.components)
      .map(([key, component]) => new RuntimeComponent(key, component, this.objectContracts));
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
    private readonly objectContracts: RuntimeObjectContractCatalog,
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

  tryElement(): HTMLElement | undefined {
    return document.getElementById(this.id) ?? undefined;
  }

  root(): unknown {
    return this.runtime().resolveRoot(this.element());
  }

  objectContract(): BrowserObjectContract {
    return this.objectContracts.require(this.definition.type);
  }

  object(): RuntimeObject {
    return new RuntimeObject(
      `component "${this.key}"`,
      this.root(),
      this.objectContract(),
    );
  }

  runtime(): ComponentRuntime {
    return ComponentRuntime.for(this.id, this.definition.vendor);
  }
}

export class RuntimePluginCatalog {
  constructor(
    private readonly objectContracts: RuntimeObjectContractCatalog,
    private readonly instances: BrowserPluginCatalog,
  ) {}

  objectContract(typeKey: string): BrowserObjectContract {
    return this.objectContracts.require(typeKey);
  }

  object(pluginName: string, typeKey: string): RuntimeObject {
    const objectContract = this.objectContract(typeKey);
    return new RuntimeObject(
      `plugin "${pluginName}"`,
      this.instances.resolve(pluginName),
      objectContract,
    );
  }
}
