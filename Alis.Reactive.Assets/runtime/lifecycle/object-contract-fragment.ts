import type { JsType } from "../types";
import type { PartId, PlanContributionSource, PlanId } from "./plan-contribution-source";

export class BrowserObjectContractLedger {
  private readonly owners = new Map<string, BrowserObjectContractOwnership>();

  request(planId: PlanId, key: string, type: JsType): BrowserObjectContractClaim {
    return new BrowserObjectContractClaim(
      key,
      BrowserObjectContractFragment.from(type),
      this.owners.get(this.ownershipKey(planId, key)),
    );
  }

  claim(planId: PlanId, key: string, type: JsType, source: PlanContributionSource): void {
    const claim = this.request(planId, key, type);
    const ownershipKey = this.ownershipKey(planId, key);
    if (source.kind === "root") {
      this.owners.set(ownershipKey, BrowserObjectContractOwnership.root(claim.contract));
      return;
    }

    const record = this.owners.get(ownershipKey);
    if (record === undefined) {
      this.owners.set(ownershipKey, BrowserObjectContractOwnership.partial(source.partId, claim.contract));
      return;
    }

    record.addPartial(source.partId, claim.contract);
  }

  claimRoot(planId: PlanId, key: string, type: JsType): void {
    this.owners.set(
      this.ownershipKey(planId, key),
      BrowserObjectContractOwnership.root(BrowserObjectContractFragment.from(type)),
    );
  }

  releasePartial(planId: PlanId, key: string, partId: PartId): BrowserObjectContractFragment | undefined {
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

export class BrowserObjectContractClaim {
  constructor(
    readonly key: string,
    readonly contract: BrowserObjectContractFragment,
    private readonly currentOwner: BrowserObjectContractOwnership | undefined,
  ) {}

  canBeHeldBy(source: PlanContributionSource): boolean {
    if (source.kind === "root") return true;
    if (this.currentOwner === undefined) return true;

    return this.currentOwner.acceptsPartial(source.partId, this.contract);
  }

  collisionError(source: PlanContributionSource): Error {
    return new Error(
      `[alis] ${source.description} cannot declare type "${this.key}"; ` +
      `type key is already owned by ${this.currentOwner?.description ?? "another source"} ` +
      "with an incompatible object contract fragment. Shared types may be declared by multiple " +
      "sources only when their fragments can merge."
    );
  }
}

class BrowserObjectContractOwnership {
  private constructor(
    private readonly rootContract: BrowserObjectContractFragment | undefined,
    private readonly partialContracts: Map<PartId, BrowserObjectContractFragment>,
  ) {}

  static root(contract: BrowserObjectContractFragment): BrowserObjectContractOwnership {
    return new BrowserObjectContractOwnership(contract, new Map<PartId, BrowserObjectContractFragment>());
  }

  static partial(partId: PartId, contract: BrowserObjectContractFragment): BrowserObjectContractOwnership {
    return new BrowserObjectContractOwnership(
      undefined,
      new Map<PartId, BrowserObjectContractFragment>([[partId, contract]]),
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

  acceptsPartial(partId: PartId, contract: BrowserObjectContractFragment): boolean {
    const current = this.contract;
    if (current === undefined) return true;

    return this.partialContracts.has(partId) || current.canMerge(contract);
  }

  addPartial(partId: PartId, contract: BrowserObjectContractFragment): void {
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
