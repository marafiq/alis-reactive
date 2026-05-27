import type { ComponentObject, ComponentValidation, Plan } from "../types";

type ValidationContainerComponent = Extract<ComponentObject["container"], { kind: "validation-container" }>;

interface ComponentDeclaration {
  readonly componentKey: string;
  readonly component: ComponentObject;
}

export type SlotComponentLoad =
  | MountedComponentLoad
  | LayoutObjectLoad
  | ValidationRulesLoad;

export interface MountedComponentLoad {
  readonly kind: "component";
  readonly componentKey: string;
  readonly componentId: string;
}

export interface LayoutObjectLoad {
  readonly kind: "layout-object";
  readonly componentKey: string;
  readonly componentId: string;
}

export interface ValidationRulesLoad {
  readonly kind: "validation-rules";
  readonly containerKey: string;
  readonly rules: ComponentValidation[];
}

export function mergeBootComponent(target: Plan, declaration: ComponentDeclaration): void {
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

export function mergeSlotComponent(
  target: Plan,
  declaration: ComponentDeclaration,
  rootOwnsComponent: boolean,
): SlotComponentLoad[] {
  const existing = target.components[declaration.componentKey];

  if (extendsRootValidationContainer(existing, declaration, rootOwnsComponent)) {
    const declaredRules = validationRulesOf(declaration.component) ?? [];
    appendRulesForNewValidatedComponents(existing, declaration.component);
    return declaredRules.length === 0
      ? []
      : [{ kind: "validation-rules", containerKey: declaration.componentKey, rules: declaredRules }];
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration);
    return [{
      kind: "layout-object",
      componentKey: declaration.componentKey,
      componentId: declaration.component.id,
    }];
  }

  if (joinsExistingRuntimeObject(existing, declaration.component)) return [];

  replaceComponent(target, declaration.componentKey, declaration.component);
  return [{
    kind: "component",
    componentKey: declaration.componentKey,
    componentId: declaration.component.id,
  }];
}

function mergeLayoutObject(target: Plan, declaration: ComponentDeclaration): void {
  const existing = target.components[declaration.componentKey];
  if (sameRuntimeIdentity(existing, declaration.component)) return;

  replaceComponent(target, declaration.componentKey, declaration.component);
}

function extendsRootValidationContainer(
  existing: ComponentObject | undefined,
  declaration: ComponentDeclaration,
  rootOwnsComponent: boolean,
): boolean {
  return rootOwnsComponent
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

function replaceComponent(target: Plan, componentKey: string, incoming: ComponentObject): void {
  const existingRules = validationRulesOf(target.components[componentKey]);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules !== undefined && incomingRules !== undefined) {
    replaceValidationRules(
      incoming,
      replaceRulesForSameValidatedComponent(existingRules, incomingRules),
    );
  }

  target.components[componentKey] = incoming;
}

function appendRulesForNewValidatedComponents(
  existing: ComponentObject | undefined,
  incoming: ComponentObject,
): ComponentValidation[] {
  const existingRules = validationRulesOf(existing);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules === undefined || incomingRules === undefined) return [];

  return appendOnlyNewValidatedComponents(existingRules, incomingRules);
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

export function replaceValidationRules(component: ComponentObject, validationRules: ComponentValidation[]): void {
  const container = validationContainerOf(component);
  if (container === undefined) return;

  container.validationRules = validationRules;
}

export function mergeValidationRules(ruleSets: Iterable<ComponentValidation[]>): ComponentValidation[] {
  const merged: ComponentValidation[] = [];
  const seenComponents = new Set<string>();
  for (const rules of ruleSets) {
    appendRulesForNewComponents(merged, seenComponents, rules);
  }

  return merged;
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

function appendOnlyNewValidatedComponents(
  existingRules: ComponentValidation[],
  incomingRules: ComponentValidation[],
): ComponentValidation[] {
  const existingComponents = new Set(existingRules.map(rule => rule.component));
  const appendedRules: ComponentValidation[] = [];
  appendRulesForNewComponents(existingRules, existingComponents, incomingRules, appendedRules);

  return appendedRules;
}

function appendRulesForNewComponents(
  target: ComponentValidation[],
  seenComponents: Set<string>,
  incomingRules: ComponentValidation[],
  appendedRules?: ComponentValidation[],
): void {
  for (const rule of incomingRules) {
    if (seenComponents.has(rule.component)) continue;
    target.push(rule);
    appendedRules?.push(rule);
    seenComponents.add(rule.component);
  }
}
