import type {
  Component,
  JsType,
  PayloadSource,
  Plan,
  Source,
} from "../types";
import { browserPlugins, type BrowserPluginCatalog } from "../core/plugin-registry";
import { RuntimeObject } from "./runtime-object";
import { ExecutionContext } from "./execution-context";
import { ComponentRuntime } from "./component-runtime";

export { RuntimeComponentReadinessError } from "./component-runtime";

const cache = new WeakMap<Plan, RuntimePlan>();

type RuntimeValidationContainer = Extract<Component["container"], { kind: "validation-container" }>;

type RuntimeResolutionTarget =
  | { readonly kind: "component"; readonly key: string }
  | { readonly kind: "element"; readonly id: string };

export class RuntimeResolutionError extends Error {
  private constructor(readonly target: RuntimeResolutionTarget, message: string) {
    super(message);
    this.name = "RuntimeResolutionError";
  }

  static componentNotFound(componentKey: string): RuntimeResolutionError {
    return new RuntimeResolutionError(
      { kind: "component", key: componentKey },
      `[alis] component not found: ${componentKey}`,
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
  readonly types: RuntimeTypeCatalog;
  readonly plugins: RuntimePluginCatalog;

  private constructor(readonly document: Plan) {
    this.types = new RuntimeTypeCatalog(document);
    this.components = new RuntimeComponentCatalog(document, this.types);
    this.plugins = new RuntimePluginCatalog(this.types, browserPlugins);
  }

  static from(plan: Plan): RuntimePlan {
    const existing = cache.get(plan);
    if (existing) return existing;

    const runtimePlan = new RuntimePlan(plan);
    cache.set(plan, runtimePlan);
    return runtimePlan;
  }

  get planId(): string {
    return this.document.planId;
  }

  objectForSource(source: Source): RuntimeObject {
    switch (source.kind) {
      case "component":
        return this.components.object(source.component);
      case "plugin":
        return this.plugins.object(source.name);
      default:
        throw new Error(`[alis] objectForSource does not support source kind "${source.kind}"`);
    }
  }

  urlParameters(): URLSearchParams {
    return new URLSearchParams(window.location.search);
  }

  resolvePayload(source: PayloadSource, ctx: ExecutionContext): unknown {
    return ctx.resolvePayload(source);
  }
}

export class RuntimeTypeCatalog {
  constructor(private readonly plan: Plan) {}

  require(typeKey: string): JsType {
    const jsType = this.plan.types[typeKey];
    if (!jsType) throw new Error(`[alis] type not found: ${typeKey}`);
    return jsType;
  }
}

export class RuntimeComponentCatalog {
  constructor(
    private readonly plan: Plan,
    private readonly types: RuntimeTypeCatalog,
  ) {}

  find(componentKey: string): RuntimeComponent | undefined {
    const component = this.plan.components[componentKey];
    if (!component) return undefined;
    return new RuntimeComponent(componentKey, component, this.types);
  }

  entries(): RuntimeComponent[] {
    return Object.entries(this.plan.components)
      .map(([key, component]) => new RuntimeComponent(key, component, this.types));
  }

  requireComponent(componentKey: string): RuntimeComponent {
    const component = this.find(componentKey);
    if (!component) throw RuntimeResolutionError.componentNotFound(componentKey);
    return component;
  }

  element(componentKey: string): HTMLElement {
    return this.requireComponent(componentKey).element();
  }

  jsType(componentKey: string): JsType {
    return this.requireComponent(componentKey).jsType();
  }

  object(componentKey: string): RuntimeObject {
    return this.requireComponent(componentKey).object();
  }
}

export class RuntimeComponent {
  constructor(
    readonly key: string,
    readonly definition: Component,
    private readonly types: RuntimeTypeCatalog,
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

  jsType(): JsType {
    return this.types.require(this.definition.type);
  }

  object(): RuntimeObject {
    return new RuntimeObject(
      `component "${this.key}"`,
      this.root(),
      this.jsType(),
    );
  }

  runtime(): ComponentRuntime {
    return ComponentRuntime.for(this.id, this.definition.vendor);
  }
}

export class RuntimePluginCatalog {
  constructor(
    private readonly types: RuntimeTypeCatalog,
    private readonly instances: BrowserPluginCatalog,
  ) {}

  jsType(pluginName: string): JsType {
    return this.types.require("plugin." + pluginName);
  }

  object(pluginName: string): RuntimeObject {
    const jsType = this.jsType(pluginName);
    return new RuntimeObject(
      `plugin "${pluginName}"`,
      this.instances.resolve(pluginName),
      jsType,
    );
  }
}
