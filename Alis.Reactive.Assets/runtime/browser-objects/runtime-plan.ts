import type {
  BrowserObjectContract,
  ComponentObject,
  PlanDocument,
  RuntimeObjectSource,
} from "../types/index";
import { browserPlugins, type BrowserPluginCatalog } from "../plugins/catalog";
import { RuntimeObject } from "./runtime-object";
import { type ComponentDriver, requireComponentDriver } from "./component-driver";

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
      `[alis] component not active in Reactive Plan: ${componentKey}`,
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
  readonly objectContracts: RuntimeObjectContracts;
  readonly plugins: RuntimePlugins;

  private constructor(readonly document: PlanDocument) {
    this.objectContracts = new RuntimeObjectContracts(document);
    this.components = new RuntimeComponents(document, this.objectContracts);
    this.plugins = new RuntimePlugins(this.objectContracts, browserPlugins);
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

export class RuntimeObjectContracts {
  constructor(private readonly plan: PlanDocument) {}

  contract(typeKey: string): BrowserObjectContract {
    return this.plan.types[typeKey]!;
  }
}

export class RuntimeComponents {
  constructor(
    private readonly plan: PlanDocument,
    private readonly objectContracts: RuntimeObjectContracts,
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
    private readonly objectContracts: RuntimeObjectContracts,
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
    return this.objectContracts.contract(this.definition.type);
  }

  object(): RuntimeObject {
    return new RuntimeObject(
      `component "${this.key}"`,
      this.root(),
      this.objectContract(),
    );
  }

  runtime(): ComponentDriver {
    return requireComponentDriver(this.id, this.definition.vendor);
  }
}

export class RuntimePlugins {
  constructor(
    private readonly objectContracts: RuntimeObjectContracts,
    private readonly instances: BrowserPluginCatalog,
  ) {}

  objectContract(typeKey: string): BrowserObjectContract {
    return this.objectContracts.contract(typeKey);
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
