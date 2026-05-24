import type { Component, ComponentValidation, Plan } from "../types";
import { stableJson } from "./object-contract-fragment";
import { rootOwnerId, type PartId, type PlanContributionSource, type PlanId } from "./plan-contribution-source";

type ValidationContainerComponent = Extract<Component["container"], { kind: "validation-container" }>;

export class ComponentOwnershipLedger {
  private readonly owners = new Map<string, PartId>();

  request(planId: PlanId, key: string): ComponentOwnershipClaim {
    return new ComponentOwnershipClaim(planId, key, this.ownerOf(planId, key));
  }

  claim(planId: PlanId, key: string, partId: PartId): void {
    this.owners.set(this.ownershipKey(planId, key), partId);
  }

  claimRoot(planId: PlanId, key: string): void {
    this.owners.set(this.ownershipKey(planId, key), rootOwnerId);
  }

  isOwnedBy(planId: PlanId, key: string, partId: PartId): boolean {
    return this.ownerOf(planId, key) === partId;
  }

  ownerOf(planId: PlanId, key: string): PartId | undefined {
    return this.owners.get(this.ownershipKey(planId, key));
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

export class ComponentOwnershipClaim {
  constructor(
    readonly planId: PlanId,
    private readonly key: string,
    readonly currentOwner: PartId | undefined,
  ) {}

  canBeHeldBy(source: PlanContributionSource): boolean {
    if (source.kind === "root") return true;

    return this.currentOwner === undefined || this.currentOwner === source.partId;
  }

  assignTo(source: PlanContributionSource, ownership: ComponentOwnershipLedger): void {
    if (source.kind === "partial") {
      ownership.claim(this.planId, this.key, source.partId);
      return;
    }

    ownership.claimRoot(this.planId, this.key);
  }

  collisionError(source: PlanContributionSource): Error {
    return new Error(
      `[alis] ${source.description} cannot declare component "${this.key}"; ` +
      `that key is already owned by ${this.ownerDescription}. ` +
      "Partial plan keys are deterministic join keys and must have one owner."
    );
  }

  private get ownerDescription(): string {
    if (this.currentOwner === undefined) return "another source";
    if (this.currentOwner === rootOwnerId) return "root";
    return `"${this.currentOwner}"`;
  }
}

export class LayoutObjectReferenceLedger {
  private readonly references = new Map<string, LayoutObjectReferences>();

  track(planId: PlanId, key: string, source: PlanContributionSource, materializedByPartial: boolean): void {
    if (source.kind === "root") return;

    const referenceKey = this.referenceKey(planId, key);
    const record = this.references.get(referenceKey) ?? new LayoutObjectReferences();
    record.add(source.partId, materializedByPartial);
    this.references.set(referenceKey, record);
  }

  release(planId: PlanId, key: string, partId: PartId): LayoutObjectRelease {
    const referenceKey = this.referenceKey(planId, key);
    const record = this.references.get(referenceKey);
    if (record === undefined) return LayoutObjectRelease.keep();

    record.release(partId);
    if (record.hasReferences) return LayoutObjectRelease.keep();

    this.references.delete(referenceKey);
    return record.wasMaterializedByPartial
      ? LayoutObjectRelease.removeMaterializedObject()
      : LayoutObjectRelease.keep();
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

  markRootOwned(): void {
    this.materialized = false;
  }

  release(partId: PartId): void {
    this.partIds.delete(partId);
  }

  get hasReferences(): boolean {
    return this.partIds.size > 0;
  }

  get wasMaterializedByPartial(): boolean {
    return this.materialized;
  }
}

export class LayoutObjectRelease {
  private constructor(readonly shouldRemoveMaterializedObject: boolean) {}

  static keep(): LayoutObjectRelease {
    return new LayoutObjectRelease(false);
  }

  static removeMaterializedObject(): LayoutObjectRelease {
    return new LayoutObjectRelease(true);
  }
}

class ComponentMerge {
  static into(target: Record<string, Component>, key: string, incoming: Component): void {
    ComponentValidationContainerMerge.from(target[key], incoming).applyToIncoming();
    target[key] = incoming;
  }

  static extendValidationContainer(existing: Component | undefined, incoming: Component): boolean {
    return ComponentValidationContainerMerge.from(existing, incoming).appendMissingRulesToExisting();
  }
}

type ReferenceOnlyContributionKind = "object-target" | "layout-object";

class ReferenceOnlyComponentContribution {
  private constructor(
    private readonly existing: Component | undefined,
    private readonly incoming: Component,
    private readonly acceptedKinds: ReadonlySet<ReferenceOnlyContributionKind>,
  ) {}

  static acceptingAnyReferenceIntent(existing: Component | undefined, incoming: Component): ReferenceOnlyComponentContribution {
    return new ReferenceOnlyComponentContribution(
      existing,
      incoming,
      new Set<ReferenceOnlyContributionKind>(["object-target", "layout-object"]));
  }

  static objectTarget(existing: Component | undefined, incoming: Component): ReferenceOnlyComponentContribution {
    return new ReferenceOnlyComponentContribution(
      existing,
      incoming,
      new Set<ReferenceOnlyContributionKind>(["object-target"]));
  }

  static layoutObject(existing: Component | undefined, incoming: Component): ReferenceOnlyComponentContribution {
    return new ReferenceOnlyComponentContribution(
      existing,
      incoming,
      new Set<ReferenceOnlyContributionKind>(["layout-object"]));
  }

  get canJoinExistingComponentDefinition(): boolean {
    if (this.existing === undefined) return false;
    return this.canMaterializeOrJoinExistingIdentity;
  }

  get canMaterializeOrJoinExistingIdentity(): boolean {
    return this.declaresAcceptedReferenceIntent
      && !this.carriesOwnedState
      && this.matchesExistingRuntimeIdentity;
  }

  get carriesOwnedState(): boolean {
    return this.declaresAcceptedReferenceIntent
      && (this.incoming.binding.kind !== "none" || this.incoming.container.kind !== "none");
  }

  private get declaresAcceptedReferenceIntent(): boolean {
    const kind = this.incoming.contribution.kind;
    return (kind === "object-target" || kind === "layout-object")
      && this.acceptedKinds.has(kind);
  }

  private get matchesExistingRuntimeIdentity(): boolean {
    if (this.existing === undefined) return true;
    return this.existing.id === this.incoming.id
      && this.existing.vendor === this.incoming.vendor
      && this.existing.type === this.incoming.type;
  }
}

class LayoutObjectContribution {
  private constructor(
    private readonly existing: Component | undefined,
    private readonly incoming: Component,
    private readonly ownershipClaim: ComponentOwnershipClaim,
  ) {}

  static from(
    existing: Component | undefined,
    incoming: Component,
    ownershipClaim: ComponentOwnershipClaim,
  ): LayoutObjectContribution {
    return new LayoutObjectContribution(existing, incoming, ownershipClaim);
  }

  get canMaterializeOrJoinLayoutObject(): boolean {
    return this.currentOwnerAllowsLayoutObject
      && ReferenceOnlyComponentContribution
        .layoutObject(this.existing, this.incoming)
        .canMaterializeOrJoinExistingIdentity;
  }

  get materializedByThisContribution(): boolean {
    return this.existing === undefined;
  }

  private get currentOwnerAllowsLayoutObject(): boolean {
    return this.ownershipClaim.currentOwner === undefined
      || this.ownershipClaim.currentOwner === rootOwnerId;
  }
}

class ValidationContainerContribution {
  private constructor(
    private readonly existing: Component | undefined,
    private readonly incoming: Component,
  ) {}

  static from(existing: Component | undefined, incoming: Component): ValidationContainerContribution {
    return new ValidationContainerContribution(existing, incoming);
  }

  get canExtendExistingContainer(): boolean {
    return this.incoming.contribution.kind === "validation-container"
      && this.hasSameRuntimeIdentity
      && this.incomingCarriesValidationContainerState
      && ComponentValidationContainerMerge
        .from(this.existing, this.incoming)
        .canAppendMissingRules();
  }

  private get hasSameRuntimeIdentity(): boolean {
    if (this.existing === undefined) return false;
    return this.existing.id === this.incoming.id
      && this.existing.vendor === this.incoming.vendor
      && this.existing.type === this.incoming.type;
  }

  private get incomingCarriesValidationContainerState(): boolean {
    return this.incoming.binding.kind === "none"
      && this.incoming.container.kind === "validation-container";
  }
}

export class ComponentContribution {
  private constructor(
    private readonly target: Plan,
    private readonly key: string,
    private readonly incoming: Component,
    private readonly source: PlanContributionSource,
    private readonly ownershipClaim: ComponentOwnershipClaim,
  ) {}

  static from(
    target: Plan,
    key: string,
    incoming: Component,
    source: PlanContributionSource,
    ownershipClaim: ComponentOwnershipClaim,
  ): ComponentContribution {
    return new ComponentContribution(target, key, incoming, source, ownershipClaim);
  }

  assertMergeable(): void {
    if (this.canMerge) return;
    throw this.ownershipClaim.collisionError(this.source);
  }

  assertInitialComposable(): void {
    if (this.canComposeIntoInitialPlan) return;
    throw this.ownershipClaim.collisionError(this.source);
  }

  mergeInto(
    ownership: ComponentOwnershipLedger,
    layoutObjects: LayoutObjectReferenceLedger,
  ): void {
    if (this.referenceContributionCarriesOwnedState) {
      throw this.ownershipClaim.collisionError(this.source);
    }

    const layoutObject = this.layoutObjectContribution;
    if (this.declaresLayoutObject) {
      if (!layoutObject.canMaterializeOrJoinLayoutObject) {
        throw this.ownershipClaim.collisionError(this.source);
      }

      if (layoutObject.materializedByThisContribution) {
        ComponentMerge.into(this.target.components, this.key, this.incoming);
        ownership.claimRoot(this.ownershipClaim.planId, this.key);
      }

      if (this.source.kind === "root") {
        layoutObjects.markRootOwned(this.ownershipClaim.planId, this.key);
      }
      layoutObjects.track(
        this.ownershipClaim.planId,
        this.key,
        this.source,
        layoutObject.materializedByThisContribution);
      return;
    }

    if (this.ownershipClaim.canBeHeldBy(this.source)) {
      ComponentMerge.into(this.target.components, this.key, this.incoming);
      this.ownershipClaim.assignTo(this.source, ownership);
      if (this.source.kind === "root") {
        layoutObjects.markRootOwned(this.ownershipClaim.planId, this.key);
      }
      return;
    }

    if (this.extendsRootOwnedValidationContainer) {
      ComponentMerge.extendValidationContainer(this.target.components[this.key], this.incoming);
      return;
    }

    if (this.referencesRootOwnedComponent) return;

    throw this.ownershipClaim.collisionError(this.source);
  }

  composeIntoInitialPlan(
    ownership: ComponentOwnershipLedger,
    layoutObjects: LayoutObjectReferenceLedger,
  ): void {
    if (this.coalescesInitialOwnedDefinition || this.extendsRootOwnedValidationContainer) {
      ComponentMerge.into(this.target.components, this.key, this.incoming);
      return;
    }

    this.mergeInto(ownership, layoutObjects);
  }

  private get canMerge(): boolean {
    if (this.referenceContributionCarriesOwnedState) return false;
    if (this.declaresLayoutObject) {
      return this.layoutObjectContribution.canMaterializeOrJoinLayoutObject;
    }

    return this.ownershipClaim.canBeHeldBy(this.source)
      || this.extendsRootOwnedValidationContainer
      || this.referencesRootOwnedComponent;
  }

  private get canComposeIntoInitialPlan(): boolean {
    return this.canMerge || this.coalescesInitialOwnedDefinition;
  }

  private get declaresLayoutObject(): boolean {
    return this.incoming.contribution.kind === "layout-object";
  }

  private get referenceContributionCarriesOwnedState(): boolean {
    return ReferenceOnlyComponentContribution
      .acceptingAnyReferenceIntent(this.target.components[this.key], this.incoming)
      .carriesOwnedState;
  }

  private get layoutObjectContribution(): LayoutObjectContribution {
    return LayoutObjectContribution.from(
      this.target.components[this.key],
      this.incoming,
      this.ownershipClaim);
  }

  private get extendsRootOwnedValidationContainer(): boolean {
    if (!this.targetsRootOwnedComponentFromPartial) return false;

    return ValidationContainerContribution
      .from(this.target.components[this.key], this.incoming)
      .canExtendExistingContainer;
  }

  private get referencesRootOwnedComponent(): boolean {
    if (!this.targetsRootOwnedComponentFromPartial) return false;

    return ReferenceOnlyComponentContribution
      .objectTarget(this.target.components[this.key], this.incoming)
      .canJoinExistingComponentDefinition;
  }

  private get coalescesInitialOwnedDefinition(): boolean {
    const existing = this.target.components[this.key];
    if (existing === undefined) return false;

    const targetIsRootOwned = this.ownershipClaim.currentOwner === rootOwnerId;
    const bothAreOwnedDefinitions = existing.contribution.kind === "owned-definition"
      && this.incoming.contribution.kind === "owned-definition";
    const sameRuntimeIdentity = existing.id === this.incoming.id
      && existing.vendor === this.incoming.vendor
      && existing.type === this.incoming.type;
    const sameOwnedState = stableJson(existing.binding) === stableJson(this.incoming.binding)
      && stableJson(existing.container) === stableJson(this.incoming.container);

    return targetIsRootOwned && bothAreOwnedDefinitions && sameRuntimeIdentity && sameOwnedState;
  }

  private get targetsRootOwnedComponentFromPartial(): boolean {
    if (this.source.kind !== "partial") return false;

    return this.ownershipClaim.currentOwner === rootOwnerId;
  }
}

class ComponentValidationContainerMerge {
  private constructor(
    private readonly existing: Component | undefined,
    private readonly incoming: Component,
  ) {}

  static from(existing: Component | undefined, incoming: Component): ComponentValidationContainerMerge {
    return new ComponentValidationContainerMerge(existing, incoming);
  }

  applyToIncoming(): void {
    const existingRules = ComponentValidationRules.from(this.existing);
    const incomingRules = ComponentValidationRules.from(this.incoming);
    if (existingRules === undefined || incomingRules === undefined) return;

    incomingRules.replaceWith(existingRules.withIncomingReplacingByValidatedComponent(incomingRules.snapshot()));
  }

  appendMissingRulesToExisting(): boolean {
    const existingRules = ComponentValidationRules.from(this.existing);
    const incomingRules = ComponentValidationRules.from(this.incoming);
    if (existingRules === undefined || incomingRules === undefined) return false;

    existingRules.appendMissingValidatedComponentsFrom(incomingRules);
    return true;
  }

  canAppendMissingRules(): boolean {
    return ComponentValidationRules.from(this.existing) !== undefined
      && ComponentValidationRules.from(this.incoming) !== undefined;
  }
}

export class ComponentValidationRules {
  private constructor(private readonly container: ValidationContainerComponent) {}

  static from(component: Component | undefined): ComponentValidationRules | undefined {
    if (component === undefined) return undefined;

    const container = component.container;
    if (container.kind === "none") return undefined;

    return new ComponentValidationRules(container);
  }

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

  appendMissingValidatedComponentsFrom(incoming: ComponentValidationRules): void {
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
      const validationRules = ComponentValidationRules.from(component);
      if (validationRules === undefined) continue;
      contributions.push(new ValidationRuleContribution(containerKey, validationRules.snapshot()));
    }

    return contributions;
  }

  removeFrom(plan: Plan): void {
    const validationRules = ComponentValidationRules.from(plan.components[this.containerKey]);
    if (validationRules === undefined) return;

    validationRules.removeExactRules(this.rules);
  }
}

export function layoutObjectKeysFrom(plan: Plan): string[] {
  return Object.entries(plan.components)
    .filter(([, component]) => component.contribution.kind === "layout-object")
    .map(([key]) => key);
}

export function unionSets<T>(left: Set<T>, right: Set<T>): Set<T> {
  return new Set<T>([...left, ...right]);
}
