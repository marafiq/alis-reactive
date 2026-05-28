import type { ComponentObject, ComponentValidation, PlanDocument } from "../types";

type ValidationContainerComponent = Extract<ComponentObject["container"], { kind: "validation-container" }>;

interface ComponentDeclaration {
  readonly componentKey: string;
  readonly component: ComponentObject;
}

export function mergeBootComponent(target: PlanDocument, declaration: ComponentDeclaration): void {
  const existing = target.components[declaration.componentKey];

  if (isInitialValidationContainerMerge(existing, declaration.component)) {
    replaceComponent(target, declaration.componentKey, declaration.component);
    return;
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration);
    return;
  }

  if (joinsExistingRuntimeObject(existing, declaration.component)) return;

  replaceComponent(target, declaration.componentKey, declaration.component);
}

export function mergeLoadedComponent(
  target: PlanDocument,
  declaration: ComponentDeclaration,
  bootHasComponent: boolean,
): void {
  const existing = target.components[declaration.componentKey];

  if (extendsRootValidationContainer(existing, declaration, bootHasComponent)) {
    appendRulesForNewValidatedComponents(existing, declaration.component);
    return;
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration);
    return;
  }

  if (joinsExistingRuntimeObject(existing, declaration.component)) return;

  replaceComponent(target, declaration.componentKey, declaration.component);
}

function mergeLayoutObject(target: PlanDocument, declaration: ComponentDeclaration): void {
  const existing = target.components[declaration.componentKey];
  if (sameRuntimeIdentity(existing, declaration.component)) return;

  replaceComponent(target, declaration.componentKey, declaration.component);
}

function extendsRootValidationContainer(
  existing: ComponentObject | undefined,
  declaration: ComponentDeclaration,
  bootHasComponent: boolean,
): boolean {
  return bootHasComponent
    && isValidationContainer(existing)
    && isValidationContainer(declaration.component)
    && declaration.component.binding.kind === "none"
    && sameRuntimeIdentity(existing, declaration.component);
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

function isInitialValidationContainerMerge(existing: ComponentObject | undefined, incoming: ComponentObject): boolean {
  return isValidationContainer(existing)
    && isValidationContainer(incoming)
    && sameRuntimeIdentity(existing, incoming);
}

function replaceComponent(target: PlanDocument, componentKey: string, incoming: ComponentObject): void {
  const next = cloneComponent(incoming);
  const existingRules = validationRulesOf(target.components[componentKey]);
  const incomingRules = validationRulesOf(next);
  if (existingRules !== undefined && incomingRules !== undefined) {
    replaceValidationRules(
      next,
      replaceRulesForSameValidatedComponent(existingRules, incomingRules),
    );
  }

  target.components[componentKey] = next;
}

function appendRulesForNewValidatedComponents(
  existing: ComponentObject | undefined,
  incoming: ComponentObject,
): void {
  const existingRules = validationRulesOf(existing);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules === undefined || incomingRules === undefined) return;

  const existingComponents = new Set(existingRules.map(rule => rule.component));
  appendRulesForNewComponents(existingRules, existingComponents, incomingRules);
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

function replaceValidationRules(component: ComponentObject, validationRules: ComponentValidation[]): void {
  const container = validationContainerOf(component);
  if (container === undefined) return;

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
