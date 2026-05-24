// merge-plan.ts — Plan registry and merge logic.
// Plans: types, components, behaviors. Merge logic for partial plan injection.

import type { Plan, Behavior, Component, ComponentValidation, JsType } from "../types";
import { unwireField } from "../validation/live-clear";

type PlanId = string;
type PartId = string;
type ValidationContainerComponent = Extract<Component["container"], { kind: "validation-container" }>;
const rootOwnerId = "$root";

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan, signal?: AbortSignal) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

export interface PartialSlotLoadResult {
  readonly loadedPlans: Plan[];
  readonly affectedPlanIds: PlanId[];
}

export interface PartialSlotUnloadResult {
  readonly affectedPlanIds: PlanId[];
}

type PlanMergeSource = RootPlanMergeSource | PartialPlanMergeSource;

class ComponentOwnershipLedger {
  private readonly owners = new Map<string, PartId>();

  request(planId: PlanId, section: OwnedPlanSection, key: string): OwnershipClaim {
    return new OwnershipClaim(planId, section, key, this.ownerOf(planId, section, key));
  }

  claim(planId: PlanId, section: OwnedPlanSection, key: string, partId: PartId): void {
    this.owners.set(section.ownershipKey(planId, key), partId);
  }

  claimRoot(planId: PlanId, section: OwnedPlanSection, key: string): void {
    this.owners.set(section.ownershipKey(planId, key), rootOwnerId);
  }

  isOwnedBy(planId: PlanId, section: OwnedPlanSection, key: string, partId: PartId): boolean {
    return this.ownerOf(planId, section, key) === partId;
  }

  ownerOf(planId: PlanId, section: OwnedPlanSection, key: string): PartId | undefined {
    return this.owners.get(section.ownershipKey(planId, key));
  }

  release(planId: PlanId, section: OwnedPlanSection, key: string): void {
    this.owners.delete(section.ownershipKey(planId, key));
  }

  reset(): void {
    this.owners.clear();
  }
}

class LayoutObjectReferenceLedger {
  private readonly references = new Map<string, LayoutObjectReferences>();

  track(planId: PlanId, key: string, source: PlanMergeSource, materializedByPartial: boolean): void {
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

class LayoutObjectRelease {
  private constructor(readonly shouldRemoveMaterializedObject: boolean) {}

  static keep(): LayoutObjectRelease {
    return new LayoutObjectRelease(false);
  }

  static removeMaterializedObject(): LayoutObjectRelease {
    return new LayoutObjectRelease(true);
  }
}

class ObjectContractFragmentLedger {
  private readonly owners = new Map<string, ObjectContractFragmentOwnership>();

  request(planId: PlanId, key: string, type: JsType): ObjectContractFragmentClaim {
    return new ObjectContractFragmentClaim(planId, key, ObjectContractFragment.from(type), this.owners.get(this.ownershipKey(planId, key)));
  }

  claim(planId: PlanId, key: string, type: JsType, source: PlanMergeSource): void {
    const claim = this.request(planId, key, type);
    const ownershipKey = this.ownershipKey(planId, key);
    if (source.kind === "root") {
      this.owners.set(ownershipKey, ObjectContractFragmentOwnership.root(claim.contract));
      return;
    }

    const record = this.owners.get(ownershipKey);
    if (record === undefined) {
      this.owners.set(ownershipKey, ObjectContractFragmentOwnership.partial(source.partId, claim.contract));
      return;
    }

    record.addPartial(source.partId, claim.contract);
  }

  claimRoot(planId: PlanId, key: string, type: JsType): void {
    this.owners.set(this.ownershipKey(planId, key), ObjectContractFragmentOwnership.root(ObjectContractFragment.from(type)));
  }

  releasePartial(planId: PlanId, key: string, partId: PartId): ObjectContractFragment | undefined {
    const ownershipKey = this.ownershipKey(planId, key);
    const record = this.owners.get(ownershipKey);
    if (record === undefined) return undefined;

    record.releasePartial(partId);
    const remaining = record.contract;
    if (remaining !== undefined) return remaining;

    this.owners.delete(ownershipKey);
    return undefined;
  }

  reset(): void {
    this.owners.clear();
  }

  private ownershipKey(planId: PlanId, key: string): string {
    return `${planId}:${key}`;
  }
}

class ObjectContractFragmentClaim {
  constructor(
    readonly planId: PlanId,
    readonly key: string,
    readonly contract: ObjectContractFragment,
    private readonly currentOwner: ObjectContractFragmentOwnership | undefined,
  ) {}

  canBeHeldBy(source: PlanMergeSource): boolean {
    if (source.kind === "root") return true;
    if (this.currentOwner === undefined) return true;

    return this.currentOwner.acceptsPartial(source.partId, this.contract);
  }

  collisionError(source: PlanMergeSource): Error {
    return new Error(
      `[alis] ${source.description} cannot declare type "${this.key}"; ` +
      `type key is already owned by ${this.currentOwner?.description ?? "another source"} ` +
      "with an incompatible object contract fragment. Shared types may be declared by multiple " +
      "sources only when their fragments can merge."
    );
  }
}

class ObjectContractFragmentOwnership {
  private constructor(
    private readonly rootContract: ObjectContractFragment | undefined,
    private readonly partialContracts: Map<PartId, ObjectContractFragment>,
  ) {}

  static root(contract: ObjectContractFragment): ObjectContractFragmentOwnership {
    return new ObjectContractFragmentOwnership(contract, new Map<PartId, ObjectContractFragment>());
  }

  static partial(partId: PartId, contract: ObjectContractFragment): ObjectContractFragmentOwnership {
    return new ObjectContractFragmentOwnership(
      undefined,
      new Map<PartId, ObjectContractFragment>([[partId, contract]]),
    );
  }

  get hasOwners(): boolean {
    return this.rootContract !== undefined || this.partialContracts.size > 0;
  }

  get contract(): ObjectContractFragment | undefined {
    let merged = this.rootContract;
    for (const fragment of this.partialContracts.values()) {
      merged = merged === undefined ? fragment : merged.merge(fragment);
    }

    return merged;
  }

  get description(): string {
    if (this.rootContract !== undefined) return "root";
    return [...this.partialContracts.keys()].map(owner => `"${owner}"`).join(", ");
  }

  acceptsPartial(partId: PartId, contract: ObjectContractFragment): boolean {
    const current = this.contract;
    if (current === undefined) return true;

    return this.partialContracts.has(partId) || current.canMerge(contract);
  }

  addPartial(partId: PartId, contract: ObjectContractFragment): void {
    if (!this.acceptsPartial(partId, contract)) {
      throw new Error("[alis] type ownership accepted an incompatible contract");
    }

    const existingFragment = this.partialContracts.get(partId);
    this.partialContracts.set(
      partId,
      existingFragment === undefined ? contract : existingFragment.merge(contract),
    );
  }

  releasePartial(partId: PartId): void {
    this.partialContracts.delete(partId);
  }
}

class ObjectContractFragment {
  private constructor(private readonly value: JsType) {}

  static from(type: JsType): ObjectContractFragment {
    return new ObjectContractFragment(cloneJsType(type));
  }

  canMerge(other: ObjectContractFragment): boolean {
    return canMergeJsTypes(this.value, other.value);
  }

  merge(other: ObjectContractFragment): ObjectContractFragment {
    return new ObjectContractFragment(mergeJsTypes(this.value, other.value));
  }

  toJsType(): JsType {
    return cloneJsType(this.value);
  }
}

function cloneJsType(type: JsType): JsType {
  return mergeJsTypes(emptyJsType(), type);
}

function emptyJsType(): JsType {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function canMergeJsTypes(existing: JsType, incoming: JsType): boolean {
  return canMergeMemberContracts(existing.properties, incoming.properties, canMergeProperties)
    && canMergeMemberContracts(existing.methods, incoming.methods, canMergeMethods)
    && canMergeMemberContracts(existing.events, incoming.events, canMergeEvents);
}

function mergeJsTypes(existing: JsType | undefined, incoming: JsType): JsType {
  if (existing === undefined) return cloneJsType(incoming);

  return {
    properties: mergeMemberContracts(existing.properties, incoming.properties, mergeProperties),
    methods: mergeMemberContracts(existing.methods, incoming.methods, mergeMethods),
    events: mergeMemberContracts(existing.events, incoming.events, mergeEvents),
  };
}

function canMergeMemberContracts<T>(
  existing: Record<string, T>,
  incoming: Record<string, T>,
  canMerge: (left: T, right: T) => boolean,
): boolean {
  for (const [member, incomingContract] of Object.entries(incoming)) {
    const existingContract = existing[member];
    if (existingContract === undefined) continue;
    if (!canMerge(existingContract, incomingContract)) return false;
  }

  return true;
}

function mergeMemberContracts<T>(
  existing: Record<string, T>,
  incoming: Record<string, T>,
  merge: (left: T, right: T) => T,
): Record<string, T> {
  const merged = { ...existing };
  for (const [member, incomingContract] of Object.entries(incoming)) {
    const existingContract = merged[member];
    merged[member] = existingContract === undefined
      ? incomingContract
      : merge(existingContract, incomingContract);
  }

  return merged;
}

type JsProperty = JsType["properties"][string];
type JsMethod = JsType["methods"][string];
type JsEvent = JsType["events"][string];

function canMergeProperties(existing: JsProperty, incoming: JsProperty): boolean {
  return stableJson(existing.path) === stableJson(incoming.path)
    && stableJson(existing.shape) === stableJson(incoming.shape);
}

function mergeProperties(existing: JsProperty, incoming: JsProperty): JsProperty {
  if (!canMergeProperties(existing, incoming)) {
    throw new Error("[alis] incompatible property contracts cannot be merged");
  }

  return {
    ...existing,
    access: mergeMemberAccess(existing.access, incoming.access),
  };
}

function mergeMemberAccess(
  existing: JsProperty["access"],
  incoming: JsProperty["access"],
): JsProperty["access"] {
  if (existing === incoming) return existing;
  return "readwrite";
}

function canMergeMethods(existing: JsMethod, incoming: JsMethod): boolean {
  return stableJson(existing.path) === stableJson(incoming.path)
    && canMergeMethodArguments(existing.arguments, incoming.arguments)
    && stableJson(existing.returns) === stableJson(incoming.returns);
}

function mergeMethods(existing: JsMethod, incoming: JsMethod): JsMethod {
  if (!canMergeMethods(existing, incoming)) {
    throw new Error("[alis] incompatible method contracts cannot be merged");
  }

  return {
    ...existing,
    arguments: mergeMethodArguments(existing.arguments, incoming.arguments),
  };
}

function canMergeMethodArguments(
  existing: JsMethod["arguments"],
  incoming: JsMethod["arguments"],
): boolean {
  return existing.kind === "open"
    || incoming.kind === "open"
    || stableJson(existing.shapes) === stableJson(incoming.shapes);
}

function mergeMethodArguments(
  existing: JsMethod["arguments"],
  incoming: JsMethod["arguments"],
): JsMethod["arguments"] {
  if (existing.kind === "open") return incoming;
  if (incoming.kind === "open") return existing;
  return existing;
}

function canMergeEvents(existing: JsEvent, incoming: JsEvent): boolean {
  return stableJson(existing) === stableJson(incoming);
}

function mergeEvents(existing: JsEvent, incoming: JsEvent): JsEvent {
  if (!canMergeEvents(existing, incoming)) {
    throw new Error("[alis] incompatible event contracts cannot be merged");
  }

  return existing;
}

function stableJson(value: unknown): string {
  if (Array.isArray(value)) return `[${value.map(stableJson).join(",")}]`;

  if (value !== null && typeof value === "object") {
    const entries = Object.entries(value as Record<string, unknown>)
      .sort(([left], [right]) => comparePropertyNames(left, right));
    return `{${entries.map(([key, item]) => `${JSON.stringify(key)}:${stableJson(item)}`).join(",")}}`;
  }

  return JSON.stringify(value);
}

function comparePropertyNames(left: string, right: string): number {
  if (left < right) return -1;
  if (left > right) return 1;
  return 0;
}

class OwnedPlanSection {
  static readonly Component = new OwnedPlanSection("component");

  private constructor(readonly label: string) {}

  ownershipKey(planId: PlanId, key: string): string {
    return `${planId}:${this.label}:${key}`;
  }
}

class OwnershipClaim {
  constructor(
    readonly planId: PlanId,
    private readonly section: OwnedPlanSection,
    private readonly key: string,
    readonly currentOwner: PartId | undefined,
  ) {}

  canBeHeldBy(source: PlanMergeSource): boolean {
    if (source.kind === "root") return true;

    return this.currentOwner === undefined || this.currentOwner === source.partId;
  }

  assignTo(source: PlanMergeSource, ownership: ComponentOwnershipLedger): void {
    if (source.kind === "partial") {
      ownership.claim(this.planId, this.section, this.key, source.partId);
      return;
    }

    ownership.claimRoot(this.planId, this.section, this.key);
  }

  collisionError(source: PlanMergeSource): Error {
    return new Error(
      `[alis] ${source.description} cannot declare ${this.section.label} "${this.key}"; ` +
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

  get declaresReferenceIntent(): boolean {
    return this.declaresAcceptedReferenceIntent;
  }

  private get declaresAcceptedReferenceIntent(): boolean {
    return this.acceptedKinds.has(this.incoming.contribution.kind as ReferenceOnlyContributionKind);
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
    private readonly ownershipClaim: OwnershipClaim,
  ) {}

  static from(
    existing: Component | undefined,
    incoming: Component,
    ownershipClaim: OwnershipClaim,
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

class ComponentContribution {
  private constructor(
    private readonly target: Plan,
    private readonly key: string,
    private readonly incoming: Component,
    private readonly source: PlanMergeSource,
    private readonly ownershipClaim: OwnershipClaim,
  ) {}

  static from(
    target: Plan,
    key: string,
    incoming: Component,
    source: PlanMergeSource,
    ownershipClaim: OwnershipClaim,
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
        ownership.claimRoot(this.ownershipClaim.planId, OwnedPlanSection.Component, this.key);
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

class ComponentValidationRules {
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

class InitialPlanComposition {
  private readonly assemblies = new Map<PlanId, BootPlanAssembly>();

  static from(plans: Plan[]): InitialPlanComposition {
    const composition = new InitialPlanComposition();
    for (const plan of plans) composition.accept(plan);
    return composition;
  }

  bootPlans(): Plan[] {
    return Array.from(this.assemblies.values()).map(assembly => assembly.toPlan());
  }

  private accept(plan: Plan): void {
    const existing = this.assemblies.get(plan.planId);
    if (existing) {
      existing.accept(plan);
      return;
    }

    this.assemblies.set(plan.planId, BootPlanAssembly.seed(plan));
  }
}

class BootPlanAssembly {
  private readonly componentOwnership = new ComponentOwnershipLedger();
  private readonly layoutObjects = new LayoutObjectReferenceLedger();

  private constructor(private readonly plan: Plan) {}

  static seed(plan: Plan): BootPlanAssembly {
    const assembly = new BootPlanAssembly({
      version: 3,
      planId: plan.planId,
      scope: { kind: "root" },
      types: { ...plan.types },
      components: { ...plan.components },
      behaviors: [...plan.behaviors],
    });
    assembly.claimBootRootComponents();
    return assembly;
  }

  accept(contribution: Plan): void {
    const source = planMergeSourceFrom(contribution);
    this.assertComponentsCanCompose(contribution, source);

    for (const [key, type] of Object.entries(contribution.types)) {
      this.plan.types[key] = mergeJsTypes(this.plan.types[key], type);
    }

    this.mergeComponents(contribution, source);
    this.plan.behaviors.push(...contribution.behaviors);
  }

  private assertComponentsCanCompose(contribution: Plan, source: PlanMergeSource): void {
    for (const [key, component] of Object.entries(contribution.components)) {
      const claim = this.componentOwnership.request(contribution.planId, OwnedPlanSection.Component, key);
      ComponentContribution.from(this.plan, key, component, source, claim).assertInitialComposable();
    }
  }

  private mergeComponents(contribution: Plan, source: PlanMergeSource): void {
    for (const [key, component] of Object.entries(contribution.components)) {
      const claim = this.componentOwnership.request(contribution.planId, OwnedPlanSection.Component, key);
      ComponentContribution.from(this.plan, key, component, source, claim)
        .composeIntoInitialPlan(this.componentOwnership, this.layoutObjects);
    }
  }

  private claimBootRootComponents(): void {
    for (const key of Object.keys(this.plan.components)) {
      this.componentOwnership.claimRoot(this.plan.planId, OwnedPlanSection.Component, key);
    }
  }

  toPlan(): Plan {
    return this.plan;
  }
}

class PartialSlotLoad {
  private readonly lifetime = new PartialSlotLifetime();

  private constructor(
    readonly partId: PartId,
    private readonly plans: Plan[],
  ) {}

  static containing(partId: PartId, plans: Plan[]): PartialSlotLoad {
    return new PartialSlotLoad(partId, plans);
  }

  contributions(): PartialSlotContribution[] {
    return this.plans.map(plan => new PartialSlotContribution(
      this.scopedPlan(plan),
      this.lifetime.sourceFor(this.partId),
    ));
  }

  private scopedPlan(plan: Plan): Plan {
    return {
      ...plan,
      scope: { kind: "partial", partId: this.partId },
    };
  }
}

class PartialSlotLifetime {
  private readonly abort = new AbortController();

  sourceFor(partId: PartId): PartialPlanMergeSource {
    return new PartialPlanMergeSource(partId, this.abort);
  }
}

class PartialSlotContribution {
  constructor(
    readonly plan: Plan,
    readonly source: PartialPlanMergeSource,
  ) {}
}

function planMergeSourceFrom(plan: Plan): PlanMergeSource {
  const scope = plan.scope;
  const incomingPlanIsPartial = scope.kind === "partial";
  if (incomingPlanIsPartial) return new PartialPlanMergeSource(scope.partId);

  return RootPlanMergeSource.instance;
}

class RootPlanMergeSource {
  static readonly instance = new RootPlanMergeSource();

  readonly kind: "root" = "root";
  readonly label = "root";
  readonly description = "root plan contribution";
  readonly behaviorSignal = undefined;

  private constructor() {}
}

class PartialPlanMergeSource {
  readonly kind: "partial" = "partial";

  constructor(
    readonly partId: PartId,
    private readonly abort = new AbortController(),
  ) {}

  get label(): string {
    return this.partId;
  }

  get description(): string {
    return `partial plan contribution "${this.partId}"`;
  }

  get behaviorSignal(): AbortSignal {
    return this.abort.signal;
  }

  get abortController(): AbortController {
    return this.abort;
  }
}

interface TrackedPartialPlanSnapshot {
  readonly partId: PartId;
  readonly planId: PlanId;
  readonly abort: AbortController;
  readonly behaviors: Behavior[];
  readonly componentKeys: string[];
  readonly layoutObjectKeys: string[];
  readonly typeKeys: string[];
  readonly validationRuleContributions: ValidationRuleContribution[];
}

class ValidationRuleContribution {
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

class TrackedPartialPlan {
  private constructor(private readonly snapshot: TrackedPartialPlanSnapshot) {}

  static capture(source: PartialPlanMergeSource, incoming: Plan): TrackedPartialPlan {
    return new TrackedPartialPlan({
      partId: source.partId,
      planId: incoming.planId,
      abort: source.abortController,
      behaviors: [...incoming.behaviors],
      componentKeys: Object.keys(incoming.components),
      layoutObjectKeys: layoutObjectKeysFrom(incoming),
      typeKeys: Object.keys(incoming.types),
      validationRuleContributions: validationRuleContributionsFrom(incoming),
    });
  }

  get partId(): PartId {
    return this.snapshot.partId;
  }

  get planId(): PlanId {
    return this.snapshot.planId;
  }

  get behaviors(): Behavior[] {
    return this.snapshot.behaviors;
  }

  get componentKeys(): string[] {
    return this.snapshot.componentKeys;
  }

  get layoutObjectKeys(): string[] {
    return this.snapshot.layoutObjectKeys;
  }

  get typeKeys(): string[] {
    return this.snapshot.typeKeys;
  }

  get validationRuleContributions(): ValidationRuleContribution[] {
    return this.snapshot.validationRuleContributions;
  }

  abortWiredBehaviors(): void {
    this.snapshot.abort.abort();
  }
}

function validationRuleContributionsFrom(plan: Plan): ValidationRuleContribution[] {
  return ValidationRuleContribution.captureFrom(plan);
}

function layoutObjectKeysFrom(plan: Plan): string[] {
  return Object.entries(plan.components)
    .filter(([, component]) => component.contribution.kind === "layout-object")
    .map(([key]) => key);
}

function unionSets<T>(left: Set<T>, right: Set<T>): Set<T> {
  return new Set<T>([...left, ...right]);
}

class PartialSlotRegistry {
  private readonly slots = new Map<PartId, TrackedPartialSlot>();

  contributions(partId: PartId): TrackedPartialPlan[] {
    return this.slots.get(partId)?.contributions() ?? [];
  }

  track(source: PartialPlanMergeSource, incoming: Plan): void {
    this.slotFor(source.partId).track(TrackedPartialPlan.capture(source, incoming));
  }

  clear(partId: PartId): void {
    this.slots.delete(partId);
  }

  reset(): void {
    for (const slot of this.slots.values()) slot.abortWiredBehaviors();
    this.slots.clear();
  }

  private slotFor(partId: PartId): TrackedPartialSlot {
    let slot = this.slots.get(partId);
    if (slot === undefined) {
      slot = new TrackedPartialSlot();
      this.slots.set(partId, slot);
    }

    return slot;
  }
}

class TrackedPartialSlot {
  private readonly trackedContributions: TrackedPartialPlan[] = [];

  track(contribution: TrackedPartialPlan): void {
    this.trackedContributions.push(contribution);
  }

  contributions(): TrackedPartialPlan[] {
    return [...this.trackedContributions];
  }

  abortWiredBehaviors(): void {
    for (const contribution of this.trackedContributions) contribution.abortWiredBehaviors();
  }
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly slots = new PartialSlotRegistry();
  private readonly componentOwnership = new ComponentOwnershipLedger();
  private readonly layoutObjects = new LayoutObjectReferenceLedger();
  private readonly typeOwnership = new ObjectContractFragmentLedger();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.claimRootKeys(plan);
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const source = planMergeSourceFrom(incoming);
    if (source.kind === "partial") this.removePartialSlot(source.partId);

    return this.mergeContribution(incoming, hooks, source);
  }

  loadPartialSlot(partId: PartId, plans: Plan[], hooks: MergeHooks): PartialSlotLoadResult {
    if (plans.length === 0) {
      throw new Error("[alis] partial slot load requires at least one plan; unload the slot explicitly instead");
    }

    const affectedPlanIds = new Set(this.removePartialSlot(partId));
    const loadedPlans: Plan[] = [];

    const load = PartialSlotLoad.containing(partId, plans);
    for (const contribution of load.contributions()) {
      const merged = this.mergeContribution(contribution.plan, hooks, contribution.source);
      loadedPlans.push(merged);
      affectedPlanIds.add(merged.planId);
    }

    return {
      loadedPlans,
      affectedPlanIds: [...affectedPlanIds],
    };
  }

  unloadPartialSlot(partId: PartId): PartialSlotUnloadResult {
    return {
      affectedPlanIds: this.removePartialSlot(partId),
    };
  }

  private removePartialSlot(partId: PartId): PlanId[] {
    const contributions = this.slots.contributions(partId);
    const affectedPlanIds = new Set<PlanId>();

    for (const contribution of contributions) {
      affectedPlanIds.add(contribution.planId);
      this.removeContribution(contribution);
    }

    this.slots.clear(partId);
    return [...affectedPlanIds];
  }

  private mergeContribution(incoming: Plan, hooks: MergeHooks, source: PlanMergeSource): Plan {
    const target = this.ensureTarget(incoming.planId);
    this.assertTypesCanMerge(incoming, source);
    this.assertComponentsCanMerge(incoming, target, source);
    this.mergeTypes(incoming, target, source);
    this.mergeComponents(incoming, target, source);

    hooks.wireBehaviors(incoming.behaviors, target, source.behaviorSignal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, source.behaviorSignal);
    this.trackMergedContribution(source, incoming);

    return target;
  }

  private assertTypesCanMerge(incoming: Plan, source: PlanMergeSource): void {
    for (const [key, type] of Object.entries(incoming.types)) {
      const claim = this.typeOwnership.request(incoming.planId, key, type);
      if (!claim.canBeHeldBy(source)) throw claim.collisionError(source);
    }
  }

  private assertComponentsCanMerge(incoming: Plan, target: Plan, source: PlanMergeSource): void {
    for (const [key, comp] of Object.entries(incoming.components)) {
      const claim = this.componentOwnership.request(incoming.planId, OwnedPlanSection.Component, key);
      ComponentContribution.from(target, key, comp, source, claim).assertMergeable();
    }
  }

  /** Get or create the target plan for merging. */
  private ensureTarget(planId: string): Plan {
    let target = this.plans.get(planId);
    if (!target) {
      target = { version: 3, planId, scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
      this.plans.set(planId, target);
    }
    return target;
  }

  /** Merge types — ownership-aware: block cross-source collisions. */
  private mergeTypes(incoming: Plan, target: Plan, source: PlanMergeSource): void {
    for (const [key, type] of Object.entries(incoming.types)) {
      const claim = this.typeOwnership.request(incoming.planId, key, type);
      if (!claim.canBeHeldBy(source)) throw claim.collisionError(source);
      target.types[key] = mergeJsTypes(target.types[key], type);
      this.typeOwnership.claim(incoming.planId, key, type, source);
    }
  }

  /** Merge components — ownership-aware with deep-merge for validation rules. */
  private mergeComponents(incoming: Plan, target: Plan, source: PlanMergeSource): void {
    for (const [key, comp] of Object.entries(incoming.components)) {
      const claim = this.componentOwnership.request(incoming.planId, OwnedPlanSection.Component, key);
      ComponentContribution.from(target, key, comp, source, claim)
        .mergeInto(this.componentOwnership, this.layoutObjects);
    }
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    this.slots.reset();
    this.componentOwnership.reset();
    this.layoutObjects.reset();
    this.typeOwnership.reset();
  }

  private removeContribution(source: TrackedPartialPlan): void {
    const plan = this.plans.get(source.planId);
    if (!plan) {
      source.abortWiredBehaviors();
      return;
    }

    source.abortWiredBehaviors();
    this.removeSourceBehaviors(plan, source);
    const removedLayoutObjectKeys = this.removeSourceLayoutObjects(plan, source);
    const removedComponentKeys = this.removeSourceComponents(plan, source);
    this.removeSourceValidationRules(plan, source);
    this.pruneOrphanedValidationRules(
      plan,
      source,
      unionSets(removedComponentKeys, removedLayoutObjectKeys));
    this.removeSourceTypes(plan, source);

    const mergedPlanNoLongerCarriesBehavior = this.canPruneMergedPlan(source.planId, plan);
    if (mergedPlanNoLongerCarriesBehavior) {
      this.plans.delete(source.planId);
    }
  }

  /** Remove behaviors that were merged from this source. */
  private removeSourceBehaviors(plan: Plan, source: TrackedPartialPlan): void {
    for (const behavior of source.behaviors) {
      const idx = plan.behaviors.indexOf(behavior);
      if (idx >= 0) plan.behaviors.splice(idx, 1);
    }
  }

  private removeSourceLayoutObjects(plan: Plan, source: TrackedPartialPlan): Set<string> {
    const removed = new Set<string>();
    for (const key of source.layoutObjectKeys) {
      const release = this.layoutObjects.release(source.planId, key, source.partId);
      if (!release.shouldRemoveMaterializedObject) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(source.planId, OwnedPlanSection.Component, key);
      removed.add(key);
    }

    return removed;
  }

  /**
   * Remove components owned by this source. Ownership check prevents
   * deleting keys that were taken over by a different source.
   */
  private removeSourceComponents(plan: Plan, source: TrackedPartialPlan): Set<string> {
    const removed = new Set<string>();
    for (const key of source.componentKeys) {
      if (!this.componentOwnership.isOwnedBy(source.planId, OwnedPlanSection.Component, key, source.partId)) continue;
      const comp = plan.components[key];
      if (comp) unwireField(comp.id);
      delete plan.components[key];
      this.componentOwnership.release(source.planId, OwnedPlanSection.Component, key);
      removed.add(key);
    }
    return removed;
  }

  /**
   * Remove orphaned validation rules for deleted components — but ONLY
   * from containers that this source also owns. If the container belongs
   * to the root plan, its rules were set at C# build time and the partial
   * doesn't re-supply them on re-merge.
   */
  private pruneOrphanedValidationRules(
    plan: Plan,
    source: TrackedPartialPlan,
    removedKeys: Set<string>,
  ): void {
    if (removedKeys.size === 0) return;
    for (const [compKey, comp] of Object.entries(plan.components)) {
      const validationRules = ComponentValidationRules.from(comp);
      if (validationRules === undefined) continue;
      if (!this.componentOwnership.isOwnedBy(source.planId, OwnedPlanSection.Component, compKey, source.partId)) continue;
      validationRules.removeRulesForComponents(removedKeys);
    }
  }

  /** Remove types owned by this source. */
  private removeSourceTypes(plan: Plan, source: TrackedPartialPlan): void {
    for (const key of source.typeKeys) {
      const remainingContract = this.typeOwnership.releasePartial(source.planId, key, source.partId);
      if (remainingContract === undefined) {
        delete plan.types[key];
        continue;
      }

      plan.types[key] = remainingContract.toJsType();
    }
  }

  private removeSourceValidationRules(plan: Plan, source: TrackedPartialPlan): void {
    for (const contribution of source.validationRuleContributions) {
      contribution.removeFrom(plan);
    }
  }

  private canPruneMergedPlan(planId: PlanId, plan: Plan): boolean {
    const planWasNotBootedAsRoot = !this.rootPlanIds.has(planId);
    const planHasNoBehaviors = plan.behaviors.length === 0;
    const planHasNoComponents = Object.keys(plan.components).length === 0;
    const planHasNoTypes = Object.keys(plan.types).length === 0;

    return planWasNotBootedAsRoot && planHasNoBehaviors && planHasNoComponents && planHasNoTypes;
  }

  private trackMergedContribution(source: PlanMergeSource, incoming: Plan): void {
    if (source.kind === "root") return;

    this.slots.track(source, incoming);
  }

  private claimRootKeys(plan: Plan): void {
    for (const [key, type] of Object.entries(plan.types))
      this.typeOwnership.claimRoot(plan.planId, key, type);
    for (const key of Object.keys(plan.components))
      this.componentOwnership.claimRoot(plan.planId, OwnedPlanSection.Component, key);
  }
}

// -- Singleton + delegating exports --

const registry = new PlanRegistry();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  return InitialPlanComposition.from(plans).bootPlans();
}

export function registerBootedPlan(plan: Plan): void { registry.register(plan); }
export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan { return registry.add(incoming, hooks); }
export function applyPartialSlotLoad(partId: PartId, plans: Plan[], hooks: MergeHooks): PartialSlotLoadResult {
  return registry.loadPartialSlot(partId, plans, hooks);
}
export function applyPartialSlotUnload(partId: PartId): PartialSlotUnloadResult {
  return registry.unloadPartialSlot(partId);
}
export function getBootedPlan(planId: string): Plan | undefined { return registry.get(planId); }
export function resetMergePlanState(): void { registry.reset(); }
