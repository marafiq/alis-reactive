import type { Component, ComponentValidation, Plan } from "../types";
import { stableJson } from "./object-contract-fragment";
import { rootOwnerId, type PartId, type PlanContributionSource, type PlanId } from "./plan-contribution-source";

type ValidationContainerComponent = Extract<Component["container"], { kind: "validation-container" }>;

export interface ComponentContributionDeclaration {
  readonly planId: PlanId;
  readonly key: string;
  readonly component: Component;
  readonly source: PlanContributionSource;
}

export interface ComponentMergeState {
  readonly ownership: ComponentOwnershipLedger;
  readonly layoutObjects: LayoutObjectReferenceLedger;
}

export class ComponentOwnershipLedger {
  private readonly owners = new Map<string, PartId>();

  ownerOf(planId: PlanId, key: string): PartId | undefined {
    return this.owners.get(this.ownershipKey(planId, key));
  }

  canBeDeclaredBy(planId: PlanId, key: string, source: PlanContributionSource): boolean {
    if (source.kind === "root") return true;

    const owner = this.ownerOf(planId, key);
    return owner === undefined || owner === source.partId;
  }

  claim(planId: PlanId, key: string, source: PlanContributionSource): void {
    if (source.kind === "root") {
      this.claimRoot(planId, key);
      return;
    }

    this.owners.set(this.ownershipKey(planId, key), source.partId);
  }

  claimRoot(planId: PlanId, key: string): void {
    this.owners.set(this.ownershipKey(planId, key), rootOwnerId);
  }

  isOwnedBy(planId: PlanId, key: string, partId: PartId): boolean {
    return this.ownerOf(planId, key) === partId;
  }

  release(planId: PlanId, key: string): void {
    this.owners.delete(this.ownershipKey(planId, key));
  }

  collisionError(planId: PlanId, key: string, source: PlanContributionSource): Error {
    return new Error(
      `[alis] ${source.description} cannot declare component "${key}"; ` +
      `that key is already owned by ${this.ownerDescription(planId, key)}. ` +
      "Partial plan keys are deterministic join keys and must have one owner.",
    );
  }

  reset(): void {
    this.owners.clear();
  }

  private ownerDescription(planId: PlanId, key: string): string {
    const owner = this.ownerOf(planId, key);
    if (owner === undefined) return "another source";
    if (owner === rootOwnerId) return "root";
    return `"${owner}"`;
  }

  private ownershipKey(planId: PlanId, key: string): string {
    return `${planId}:component:${key}`;
  }
}

export class LayoutObjectReferenceLedger {
  private readonly references = new Map<string, LayoutObjectReferences>();

  track(planId: PlanId, key: string, source: PlanContributionSource, materializedByPartial: boolean): void {
    if (source.kind === "root") return;

    const referenceKey = this.referenceKey(planId, key);
    const references = this.references.get(referenceKey) ?? new LayoutObjectReferences();
    references.add(source.partId, materializedByPartial);
    this.references.set(referenceKey, references);
  }

  releaseMaterializedBy(planId: PlanId, key: string, partId: PartId): boolean {
    const referenceKey = this.referenceKey(planId, key);
    const references = this.references.get(referenceKey);
    if (references === undefined) return false;

    references.release(partId);
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

class LayoutObjectReferences {
  private readonly partIds = new Set<PartId>();
  private materialized = false;

  add(partId: PartId, materializedByPartial: boolean): void {
    this.partIds.add(partId);
    this.materialized = this.materialized || materializedByPartial;
  }

  release(partId: PartId): void {
    this.partIds.delete(partId);
  }

  markRootOwned(): void {
    this.materialized = false;
  }

  get hasPartialReferences(): boolean {
    return this.partIds.size > 0;
  }

  get wasMaterializedByPartial(): boolean {
    return this.materialized;
  }
}

export function assertComponentCanMerge(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  if (canMergeComponent(target, declaration, state, false)) return;
  throw state.ownership.collisionError(declaration.planId, declaration.key, declaration.source);
}

export function assertComponentCanComposeInitialPlan(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  if (canMergeComponent(target, declaration, state, true)) return;
  throw state.ownership.collisionError(declaration.planId, declaration.key, declaration.source);
}

export function mergeComponentIntoPlan(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  if (referenceIntentCarriesOwnedState(declaration.component)) {
    throw state.ownership.collisionError(declaration.planId, declaration.key, declaration.source);
  }

  if (isLayoutObject(declaration.component)) {
    mergeLayoutObject(target, declaration, state);
    return;
  }

  if (state.ownership.canBeDeclaredBy(declaration.planId, declaration.key, declaration.source)) {
    replaceComponent(target, declaration.key, declaration.component);
    state.ownership.claim(declaration.planId, declaration.key, declaration.source);
    if (declaration.source.kind === "root") {
      state.layoutObjects.markRootOwned(declaration.planId, declaration.key);
    }
    return;
  }

  if (extendsRootValidationContainer(target, declaration, state.ownership)) {
    addNewValidationRules(target.components[declaration.key], declaration.component);
    return;
  }

  if (referencesRootComponent(target, declaration, state.ownership)) return;

  throw state.ownership.collisionError(declaration.planId, declaration.key, declaration.source);
}

export function composeInitialComponentIntoPlan(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  if (coalescesInitialOwnedDefinition(target, declaration, state.ownership)
    || extendsRootValidationContainer(target, declaration, state.ownership)) {
    replaceComponent(target, declaration.key, declaration.component);
    return;
  }

  mergeComponentIntoPlan(target, declaration, state);
}

function canMergeComponent(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
  allowInitialOwnedCoalescing: boolean,
): boolean {
  if (referenceIntentCarriesOwnedState(declaration.component)) return false;
  if (isLayoutObject(declaration.component)) {
    return canMergeLayoutObject(target, declaration, state.ownership);
  }

  return state.ownership.canBeDeclaredBy(declaration.planId, declaration.key, declaration.source)
    || extendsRootValidationContainer(target, declaration, state.ownership)
    || referencesRootComponent(target, declaration, state.ownership)
    || (allowInitialOwnedCoalescing && coalescesInitialOwnedDefinition(target, declaration, state.ownership));
}

function mergeLayoutObject(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  state: ComponentMergeState,
): void {
  if (!canMergeLayoutObject(target, declaration, state.ownership)) {
    throw state.ownership.collisionError(declaration.planId, declaration.key, declaration.source);
  }

  const materializedByThisContribution = target.components[declaration.key] === undefined;
  if (materializedByThisContribution) {
    replaceComponent(target, declaration.key, declaration.component);
    state.ownership.claimRoot(declaration.planId, declaration.key);
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

function canMergeLayoutObject(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnershipLedger,
): boolean {
  const currentOwner = ownership.ownerOf(declaration.planId, declaration.key);
  return (currentOwner === undefined || currentOwner === rootOwnerId)
    && canMaterializeOrJoinReference(target.components[declaration.key], declaration.component, "layout-object");
}

function extendsRootValidationContainer(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnershipLedger,
): boolean {
  if (!targetsRootOwnedComponentFromPartial(declaration, ownership)) return false;
  if (declaration.component.contribution.kind !== "validation-container") return false;
  if (declaration.component.binding.kind !== "none") return false;
  if (declaration.component.container.kind !== "validation-container") return false;

  return sameRuntimeIdentity(target.components[declaration.key], declaration.component)
    && validationRulesOf(target.components[declaration.key]) !== undefined;
}

function referencesRootComponent(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnershipLedger,
): boolean {
  return targetsRootOwnedComponentFromPartial(declaration, ownership)
    && canMaterializeOrJoinReference(target.components[declaration.key], declaration.component, "object-target");
}

function targetsRootOwnedComponentFromPartial(
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnershipLedger,
): boolean {
  return declaration.source.kind === "partial"
    && ownership.ownerOf(declaration.planId, declaration.key) === rootOwnerId;
}

function referenceIntentCarriesOwnedState(component: Component): boolean {
  return isReferenceOnlyIntent(component)
    && (component.binding.kind !== "none" || component.container.kind !== "none");
}

function canMaterializeOrJoinReference(
  existing: Component | undefined,
  incoming: Component,
  kind: "object-target" | "layout-object",
): boolean {
  return incoming.contribution.kind === kind
    && !referenceIntentCarriesOwnedState(incoming)
    && sameRuntimeIdentity(existing, incoming);
}

function isReferenceOnlyIntent(component: Component): boolean {
  return component.contribution.kind === "object-target"
    || component.contribution.kind === "layout-object";
}

function isLayoutObject(component: Component): boolean {
  return component.contribution.kind === "layout-object";
}

function sameRuntimeIdentity(existing: Component | undefined, incoming: Component): boolean {
  if (existing === undefined) return true;

  return existing.id === incoming.id
    && existing.vendor === incoming.vendor
    && existing.type === incoming.type;
}

function coalescesInitialOwnedDefinition(
  target: Plan,
  declaration: ComponentContributionDeclaration,
  ownership: ComponentOwnershipLedger,
): boolean {
  const existing = target.components[declaration.key];
  if (existing === undefined) return false;

  return ownership.ownerOf(declaration.planId, declaration.key) === rootOwnerId
    && existing.contribution.kind === "owned-definition"
    && declaration.component.contribution.kind === "owned-definition"
    && sameRuntimeIdentity(existing, declaration.component)
    && stableJson(existing.binding) === stableJson(declaration.component.binding)
    && stableJson(existing.container) === stableJson(declaration.component.container);
}

function replaceComponent(target: Plan, key: string, incoming: Component): void {
  const existingRules = validationRulesOf(target.components[key]);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules !== undefined && incomingRules !== undefined) {
    incomingRules.replaceWith(existingRules.withIncomingReplacingByValidatedComponent(incomingRules.snapshot()));
  }

  target.components[key] = incoming;
}

function addNewValidationRules(existing: Component | undefined, incoming: Component): void {
  const existingRules = validationRulesOf(existing);
  const incomingRules = validationRulesOf(incoming);
  if (existingRules === undefined || incomingRules === undefined) return;

  existingRules.acceptNewValidatedComponentsFrom(incomingRules);
}

export function validationRulesOf(component: Component | undefined): ComponentValidationRules | undefined {
  if (component === undefined) return undefined;

  const container = component.container;
  if (container.kind === "none") return undefined;

  return new ComponentValidationRules(container);
}

export class ComponentValidationRules {
  constructor(private readonly container: ValidationContainerComponent) {}

  snapshot(): ComponentValidation[] {
    return [...this.container.validationRules];
  }

  replaceWith(validationRules: ComponentValidation[]): void {
    this.container.validationRules = validationRules;
  }

  withIncomingReplacingByValidatedComponent(incoming: ComponentValidation[]): ComponentValidation[] {
    const rulesByComponent = new Map(this.container.validationRules.map(rule => [rule.component, rule]));
    for (const rule of incoming) {
      rulesByComponent.set(rule.component, rule);
    }

    return [...rulesByComponent.values()];
  }

  acceptNewValidatedComponentsFrom(incoming: ComponentValidationRules): void {
    const existingComponents = new Set(this.container.validationRules.map(rule => rule.component));
    for (const rule of incoming.snapshot()) {
      if (existingComponents.has(rule.component)) continue;
      this.container.validationRules.push(rule);
      existingComponents.add(rule.component);
    }
  }

  removeExactRules(rules: ComponentValidation[]): void {
    const removedRules = new Set(rules);
    this.container.validationRules = this.container.validationRules
      .filter(rule => !removedRules.has(rule));
  }

  removeRulesForComponents(componentKeys: Set<string>): void {
    this.container.validationRules = this.container.validationRules
      .filter(rule => !componentKeys.has(rule.component));
  }
}

export class ValidationRuleContribution {
  private constructor(
    private readonly containerKey: string,
    private readonly rules: ComponentValidation[],
  ) {}

  static captureFrom(plan: Plan): ValidationRuleContribution[] {
    const contributions: ValidationRuleContribution[] = [];
    for (const [containerKey, component] of Object.entries(plan.components)) {
      const validationRules = validationRulesOf(component);
      if (validationRules === undefined) continue;
      contributions.push(new ValidationRuleContribution(containerKey, validationRules.snapshot()));
    }

    return contributions;
  }

  removeFrom(plan: Plan): void {
    const validationRules = validationRulesOf(plan.components[this.containerKey]);
    if (validationRules === undefined) return;

    validationRules.removeExactRules(this.rules);
  }
}

export function layoutObjectKeysFrom(plan: Plan): string[] {
  return Object.entries(plan.components)
    .filter(([, component]) => component.contribution.kind === "layout-object")
    .map(([key]) => key);
}
