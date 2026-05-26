import type { JsType, Shape } from "../types";
import { describePlanContribution } from "./plan-contribution-source";
import type { ContributionId, PlanContributionSource, PlanId } from "./plan-contribution-source";

export class BrowserObjectContracts {
  private readonly owners = new Map<string, BrowserObjectContractFragments>();

  record(planId: PlanId, key: string, type: JsType, source: PlanContributionSource): void {
    const fragment = BrowserObjectContractFragment.from(type);
    const ownershipKey = this.ownershipKey(planId, key);
    if (source.kind === "root") {
      this.owners.set(ownershipKey, BrowserObjectContractFragments.root(fragment));
      return;
    }

    const record = this.owners.get(ownershipKey);
    if (record === undefined) {
      this.owners.set(ownershipKey, BrowserObjectContractFragments.partial(source.contributionId, fragment));
      return;
    }

    if (!record.acceptsPartial(source.contributionId, fragment)) {
      throw new Error(
        `[alis] ${describePlanContribution(source)} cannot declare type "${key}"; ` +
        `type key is already owned by ${record.description} with an incompatible object contract fragment. ` +
        "Shared types may be declared by multiple sources only when their fragments can merge."
      );
    }

    record.addPartial(source.contributionId, fragment);
  }

  recordRoot(planId: PlanId, key: string, type: JsType): void {
    this.owners.set(
      this.ownershipKey(planId, key),
      BrowserObjectContractFragments.root(BrowserObjectContractFragment.from(type)),
    );
  }

  releasePartial(planId: PlanId, key: string, contributionId: ContributionId): BrowserObjectContractFragment | undefined {
    const ownershipKey = this.ownershipKey(planId, key);
    const record = this.owners.get(ownershipKey);
    if (record === undefined) return undefined;

    record.releasePartial(contributionId);
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

class BrowserObjectContractFragments {
  private constructor(
    private readonly rootContract: BrowserObjectContractFragment | undefined,
    private readonly partialContracts: Map<ContributionId, BrowserObjectContractFragment>,
  ) {}

  static root(contract: BrowserObjectContractFragment): BrowserObjectContractFragments {
    return new BrowserObjectContractFragments(contract, new Map<ContributionId, BrowserObjectContractFragment>());
  }

  static partial(contributionId: ContributionId, contract: BrowserObjectContractFragment): BrowserObjectContractFragments {
    return new BrowserObjectContractFragments(
      undefined,
      new Map<ContributionId, BrowserObjectContractFragment>([[contributionId, contract]]),
    );
  }

  get contract(): BrowserObjectContractFragment | undefined {
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

  acceptsPartial(contributionId: ContributionId, contract: BrowserObjectContractFragment): boolean {
    const current = this.contract;
    if (current === undefined) return true;

    return this.partialContracts.has(contributionId) || current.canMerge(contract);
  }

  addPartial(contributionId: ContributionId, contract: BrowserObjectContractFragment): void {
    if (!this.acceptsPartial(contributionId, contract)) {
      throw new Error("[alis] type ownership accepted an incompatible contract");
    }

    const existingFragment = this.partialContracts.get(contributionId);
    this.partialContracts.set(
      contributionId,
      existingFragment === undefined ? contract : existingFragment.merge(contract),
    );
  }

  releasePartial(contributionId: ContributionId): void {
    this.partialContracts.delete(contributionId);
  }
}

export class BrowserObjectContractFragment {
  private constructor(private readonly value: JsType) {}

  static from(type: JsType): BrowserObjectContractFragment {
    return new BrowserObjectContractFragment(cloneJsType(type));
  }

  canMerge(other: BrowserObjectContractFragment): boolean {
    return canMergeJsTypes(this.value, other.value);
  }

  merge(other: BrowserObjectContractFragment): BrowserObjectContractFragment {
    return new BrowserObjectContractFragment(mergeJsTypes(this.value, other.value));
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

export function mergeJsTypes(existing: JsType | undefined, incoming: JsType): JsType {
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
    && mergeObjectContractShapes(existing.shape, incoming.shape) !== undefined;
}

function mergeProperties(existing: JsProperty, incoming: JsProperty): JsProperty {
  if (stableJson(existing.path) !== stableJson(incoming.path)) {
    throw new Error("[alis] incompatible property contracts cannot be merged");
  }

  const shape = mergeObjectContractShapes(existing.shape, incoming.shape);
  if (shape === undefined) {
    throw new Error("[alis] incompatible property contracts cannot be merged");
  }

  return {
    ...existing,
    shape,
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
    && mergeMethodArguments(existing.arguments, incoming.arguments) !== undefined
    && mergeObjectContractShapes(existing.returns, incoming.returns) !== undefined;
}

function mergeMethods(existing: JsMethod, incoming: JsMethod): JsMethod {
  if (stableJson(existing.path) !== stableJson(incoming.path)) {
    throw new Error("[alis] incompatible method contracts cannot be merged");
  }

  const args = mergeMethodArguments(existing.arguments, incoming.arguments);
  const returns = mergeObjectContractShapes(existing.returns, incoming.returns);
  if (args === undefined || returns === undefined) {
    throw new Error("[alis] incompatible method contracts cannot be merged");
  }

  return {
    ...existing,
    arguments: args,
    returns,
  };
}

function mergeMethodArguments(
  existing: JsMethod["arguments"],
  incoming: JsMethod["arguments"],
): JsMethod["arguments"] | undefined {
  if (existing.kind === "open") return incoming;
  if (incoming.kind === "open") return existing;

  if (existing.shapes.length !== incoming.shapes.length) return undefined;

  const shapes: Shape[] = [];
  for (let index = 0; index < existing.shapes.length; index++) {
    const shape = mergeObjectContractShapes(existing.shapes[index]!, incoming.shapes[index]!);
    if (shape === undefined) return undefined;
    shapes.push(shape);
  }

  return { kind: "exact", shapes };
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

function mergeObjectContractShapes(existing: Shape, incoming: Shape): Shape | undefined {
  if (stableJson(existing) === stableJson(incoming)) return existing;
  if (existing.kind === "none" || incoming.kind === "none") return undefined;
  if (existing.kind === "any") return incoming;
  if (incoming.kind === "any") return existing;
  if (isNullableOf(existing, incoming)) return existing;
  if (isNullableOf(incoming, existing)) return incoming;
  if (existing.kind === "array" && incoming.kind === "array") {
    return mergeArrayShapes(existing, incoming);
  }
  if (existing.kind === "object" && incoming.kind === "object") {
    return mergeObjectShapes(existing, incoming);
  }

  return undefined;
}

function isNullableOf(shape: Shape, inner: Shape): boolean {
  return shape.kind === "nullable" && stableJson(shape.inner) === stableJson(inner);
}

function mergeArrayShapes(
  existing: Extract<Shape, { kind: "array" }>,
  incoming: Extract<Shape, { kind: "array" }>,
): Shape | undefined {
  const item = mergeObjectContractShapes(existing.item, incoming.item);
  if (item === undefined) return undefined;
  return { kind: "array", item };
}

function mergeObjectShapes(
  existing: Extract<Shape, { kind: "object" }>,
  incoming: Extract<Shape, { kind: "object" }>,
): Shape | undefined {
  const fields: Record<string, Shape> = { ...existing.fields };
  for (const [field, incomingShape] of Object.entries(incoming.fields)) {
    const existingShape = fields[field];
    if (existingShape === undefined) {
      fields[field] = incomingShape;
      continue;
    }

    const merged = mergeObjectContractShapes(existingShape, incomingShape);
    if (merged === undefined) return undefined;
    fields[field] = merged;
  }

  const bothAreOpenObjects = existing.additional && incoming.additional;
  const noDeclaredFields = Object.keys(fields).length === 0;
  return {
    kind: "object",
    fields,
    additional: bothAreOpenObjects && noDeclaredFields,
  };
}

export function stableJson(value: unknown): string {
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
