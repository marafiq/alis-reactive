import type { ComponentObject, ComponentValidation, PlanDocument } from "../types";

type ValidationContainerComponent = Extract<ComponentObject["container"], { kind: "validation-container" }>;

export function mergeBootComponent(target: PlanDocument, componentKey: string, incoming: ComponentObject): void {
  const existing = target.components[componentKey];

  if (isLayoutObject(incoming)) {
    mergeLayoutObject(target, componentKey, incoming);
    return;
  }

  if (joinsExistingRuntimeObject(existing, incoming)) return;

  replaceComponent(target, componentKey, incoming);
}

export function mergeSlotComponent(
  target: PlanDocument,
  componentKey: string,
  incoming: ComponentObject,
  rootOwnsComponent: boolean,
): void {
  const existing = target.components[componentKey];

  if (rootOwnsComponent
    && isValidationContainer(existing)
    && isValidationContainer(incoming)
    && incoming.binding.kind === "none"
    && sameRuntimeIdentity(existing, incoming)) {
    appendPartialValidationRulesToRootContainer(existing.container, incoming.container);
    return;
  }

  if (isLayoutObject(incoming)) {
    mergeLayoutObject(target, componentKey, incoming);
    return;
  }

  if (joinsExistingRuntimeObject(existing, incoming)) return;

  replaceComponent(target, componentKey, incoming);
}

function mergeLayoutObject(target: PlanDocument, componentKey: string, incoming: ComponentObject): void {
  const existing = target.components[componentKey];
  if (sameRuntimeIdentity(existing, incoming)) return;

  replaceComponent(target, componentKey, incoming);
}

function joinsExistingRuntimeObject(
  existing: ComponentObject | undefined,
  incoming: ComponentObject,
): boolean {
  return existing !== undefined
    && incoming.role.kind === "object-target"
    && sameRuntimeIdentity(existing, incoming);
}

function isLayoutObject(component: ComponentObject): boolean {
  return component.role.kind === "layout-object";
}

function sameRuntimeIdentity(existing: ComponentObject | undefined, incoming: ComponentObject): boolean {
  if (existing === undefined) return false;

  return existing.id === incoming.id
    && existing.vendor === incoming.vendor
    && existing.type === incoming.type;
}

function replaceComponent(target: PlanDocument, componentKey: string, incoming: ComponentObject): void {
  const next = cloneComponent(incoming);
  const existingRules = validationRulesOf(target.components[componentKey]);
  const nextContainer = validationContainerOf(next);
  if (existingRules !== undefined && nextContainer !== undefined) {
    replaceValidationRules(
      nextContainer,
      replaceRulesForSameValidatedComponent(existingRules, nextContainer.validationRules),
    );
  }

  target.components[componentKey] = next;
}

function appendPartialValidationRulesToRootContainer(
  rootContainer: ValidationContainerComponent,
  partialContainer: ValidationContainerComponent,
): void {
  const existingComponents = new Set(rootContainer.validationRules.map(rule => rule.component));
  appendRulesForNewComponents(
    rootContainer.validationRules,
    existingComponents,
    partialContainer.validationRules,
  );
}

function validationContainerOf(component: ComponentObject | undefined): ValidationContainerComponent | undefined {
  if (!isValidationContainer(component)) return undefined;

  return component.container;
}

function isValidationContainer(
  component: ComponentObject | undefined,
): component is ComponentObject & { container: ValidationContainerComponent } {
  return component?.container.kind === "validation-container";
}

function validationRulesOf(component: ComponentObject | undefined): ComponentValidation[] | undefined {
  return validationContainerOf(component)?.validationRules;
}

function replaceValidationRules(container: ValidationContainerComponent, validationRules: ComponentValidation[]): void {
  container.validationRules = validationRules;
}

function replaceRulesForSameValidatedComponent(
  existingRules: ComponentValidation[],
  incomingRules: ComponentValidation[],
): ComponentValidation[] {
  const rulesByComponent = new Map(existingRules.map(rule => [rule.component, rule]));
  for (const rule of incomingRules) {
    rulesByComponent.set(rule.component, rule);
  }

  return [...rulesByComponent.values()];
}

function appendRulesForNewComponents(
  target: ComponentValidation[],
  seenComponents: Set<string>,
  incomingRules: ComponentValidation[],
): void {
  for (const rule of incomingRules) {
    if (seenComponents.has(rule.component)) continue;
    target.push(rule);
    seenComponents.add(rule.component);
  }
}

function cloneComponent(component: ComponentObject): ComponentObject {
  return structuredClone(component);
}
