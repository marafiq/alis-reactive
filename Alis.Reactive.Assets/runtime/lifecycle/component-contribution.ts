import type { Component, ComponentValidation, Plan } from "../types";
import {
  rootOwnerId,
  type ContributionId,
  type PlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";

type ValidationContainerComponent = Extract<Component["container"], { kind: "validation-container" }>;

export interface ValidationRuleContribution {
  readonly containerKey: string;
  readonly rules: ComponentValidation[];
}

export interface ComponentContributionDeclaration {
  readonly planId: PlanId;
  readonly key: string;
  readonly component: Component;
  readonly source: PlanContributionSource;
}

export interface ComponentMergeState {
  readonly ownership: ComponentOwnership;
  readonly layoutObjects: LayoutObjectReferences;
}

export class ComponentOwnership {
  private readonly owners = new Map<string, ContributionId>();

  ownerOf(planId: PlanId, key: string): ContributionId | undefined {
    return this.owners.get(this.ownershipKey(planId, key));
  }

  record(planId: PlanId, key: string, source: PlanContributionSource): void {
    if (source.kind === "root") {
      this.recordRoot(planId, key);
      return;
    }

    this.owners.set(this.ownershipKey(planId, key), source.contributionId);
  }

  recordRoot(planId: PlanId, key: string): void {
    this.owners.set(this.ownershipKey(planId, key), rootOwnerId);
  }

  isOwnedBy(planId: PlanId, key: string, contributionId: ContributionId): boolean {
    return this.ownerOf(planId, key) === contributionId;
  }

  release(planId: PlanId, key: string): void {
    this.owners.delete(this.ownershipKey(planId, key));
  }

  reset(): void {
    this.owners.clear();
  }

  private ownershipKey(planId: PlanId, key: string): string {
    return `${planId}:component:${key}`;
  }
}

export class LayoutObjectReferences {
  private readonly references = new Map<string, LayoutObjectContributionReferences>();

  track(planId: PlanId, key: string, source: PlanContributionSource, materializedByPartial: boolean): void {
    if (source.kind === "root") return;

    const referenceKey = this.referenceKey(planId, key);
    const references = this.references.get(referenceKey) ?? new LayoutObjectContributionReferences();
    references.add(source.contributionId, materializedByPartial);
    this.references.set(referenceKey, references);
  }

  releaseMaterializedBy(planId: PlanId, key: string, contributionId: ContributionId): boolean {
    const referenceKey = this.referenceKey(planId, key);
    const references = this.references.get(referenceKey);
    if (references === undefined) return false;

    references.release(contributionId);
    if (references.hasPartialReferences) return false;

    this.references.delete(referenceKey);
    return references.wasMaterializedByPartial;
  }

  markRootOwned(planId: PlanId, key: string): void {
    this.references.get(this.referenceKey(planId, key))?.markRootOwned();
  }

  reset(): void {
    this.references.clear();
  }

  private referenceKey(planId: PlanId, key: string): string {
    return `${planId}:${key}`;
  }
}

class LayoutObjectContributionReferences {
  private readonly contributionIds = new Set<ContributionId>();
  private materialized = false;

  add(contributionId: ContributionId, materializedByPartial: boolean): void {
    this.contributionIds.add(contributionId);
    this.materialized = this.materialized || materializedByPartial;
  }

  release(contributionId: ContributionId): void {
    this.contributionIds.delete(contributionId);
  }

  markRootOwned(): void {
    this.materialized = false;
  }

  get hasPartialReferences(): boolean {
    return this.contributionIds.size > 0;
  }

  get wasMaterializedByPartial(): boolean {
    return this.materialized;
  }
}

export function mergeComponentIntoPlan(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  const existing = target.components[declaration.key];

  if (extendsRootValidationContainer(existing, declaration, state.ownership)) {
    appendRulesForNewValidatedComponents(existing, declaration.component);
    return;
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration, state);
    return;
  }

  if (joinsExistingRuntimeObject(existing, declaration.component)) {
    return;
  }

  replaceComponent(target, declaration.key, declaration.component);
  state.ownership.record(declaration.planId, declaration.key, declaration.source);
}

export function composeInitialComponentIntoPlan(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  const existing = target.components[declaration.key];

  if (isInitialValidationContainerMerge(existing, declaration.component)) {
    replaceComponent(target, declaration.key, declaration.component);
    state.ownership.record(declaration.planId, declaration.key, declaration.source);
    return;
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration, state);
    return;
  }

  if (joinsExistingRuntimeObject(existing, declaration.component)) {
    return;
  }

  replaceComponent(target, declaration.key, declaration.component);
  state.ownership.record(declaration.planId, declaration.key, declaration.source);
}

function mergeLayoutObject(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  const existing = target.components[declaration.key];
  const materializedByThisContribution = existing === undefined || !sameRuntimeIdentity(existing, declaration.component);
  if (materializedByThisContribution) {
    replaceComponent(target, declaration.key, declaration.component);
    state.ownership.recordRoot(declaration.planId, declaration.key);
  }

  if (declaration.source.kind === "root") {
    state.layoutObjects.markRootOwned(declaration.planId, declaration.key);
  }
  state.layoutObjects.track(
    declaration.planId,
    declaration.key,
    declaration.source,
    materializedByThisContribution,
  );
}

function extendsRootValidationContainer(
  existing: Component | undefined,
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnership,
): boolean {
  if (!targetsRootOwnedComponentFromPartial(declaration, ownership)) return false;
  return isValidationContainer(existing)
    && isValidationContainer(declaration.component)
    && declaration.component.binding.kind === "none"
    && sameRuntimeIdentity(existing, declaration.component);
}

function targetsRootOwnedComponentFromPartial(
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnership,
): boolean {
  return declaration.source.kind === "partial"
    && ownership.ownerOf(declaration.planId, declaration.key) === rootOwnerId;
}

function joinsExistingRuntimeObject(
  existing: Component | undefined,
  incoming: Component,
): boolean {
  return existing !== undefined
    && incoming.contribution.kind === "object-target"
    && sameRuntimeIdentity(existing, incoming);
}

function isLayoutObject(component: Component): boolean {
  return component.contribution.kind === "layout-object";
}

function sameRuntimeIdentity(existing: Component | undefined, incoming: Component): boolean {
  if (existing === undefined) return false;

  return existing.id === incoming.id
    && existing.vendor === incoming.vendor
    && existing.type === incoming.type;
}

function isInitialValidationContainerMerge(existing: Component | undefined, incoming: Component): boolean {
  return isValidationContainer(existing)
    && isValidationContainer(incoming)
    && sameRuntimeIdentity(existing, incoming);
}

function replaceComponent(target: Plan, key: string, incoming: Component): void {
  const existingRules = validationRulesOf(target.components[key]);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules !== undefined && incomingRules !== undefined) {
    replaceValidationRules(
      incoming,
      replaceRulesForSameValidatedComponent(existingRules, incomingRules),
    );
  }

  target.components[key] = incoming;
}

function appendRulesForNewValidatedComponents(existing: Component | undefined, incoming: Component): void {
  const existingRules = validationRulesOf(existing);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules === undefined || incomingRules === undefined) return;

  appendOnlyNewValidatedComponents(existingRules, incomingRules);
}

function validationContainerOf(component: Component | undefined): ValidationContainerComponent | undefined {
  if (!isValidationContainer(component)) return undefined;

  return component.container;
}

function isValidationContainer(
  component: Component | undefined,
): component is Component & { container: ValidationContainerComponent } {
  return component?.container.kind === "validation-container";
}

function validationRulesOf(component: Component | undefined): ComponentValidation[] | undefined {
  return validationContainerOf(component)?.validationRules;
}

function replaceValidationRules(component: Component, validationRules: ComponentValidation[]): void {
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

function appendOnlyNewValidatedComponents(
  existingRules: ComponentValidation[],
  incomingRules: ComponentValidation[],
): void {
  const existingComponents = new Set(existingRules.map(rule => rule.component));
  for (const rule of incomingRules) {
    if (existingComponents.has(rule.component)) continue;
    existingRules.push(rule);
    existingComponents.add(rule.component);
  }
}

export function captureValidationRuleContributions(plan: Plan): ValidationRuleContribution[] {
  const contributions: ValidationRuleContribution[] = [];
  for (const [containerKey, component] of Object.entries(plan.components)) {
    const validationRules = validationRulesOf(component);
    if (validationRules === undefined) continue;
    contributions.push({ containerKey, rules: [...validationRules] });
  }

  return contributions;
}

export function removeValidationRuleContribution(plan: Plan, contribution: ValidationRuleContribution): void {
  const component = plan.components[contribution.containerKey];
  if (component === undefined) return;

  const validationRules = validationRulesOf(component);
  if (validationRules === undefined) return;

  const removedRules = new Set(contribution.rules);
  replaceValidationRules(
    component,
    validationRules.filter(rule => !removedRules.has(rule)),
  );
}

export function layoutObjectKeysFrom(plan: Plan): string[] {
  return Object.entries(plan.components)
    .filter(([, component]) => component.contribution.kind === "layout-object")
    .map(([key]) => key);
}
