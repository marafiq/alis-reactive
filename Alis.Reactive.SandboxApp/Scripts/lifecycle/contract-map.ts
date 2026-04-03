import type {
  CapabilityContract,
  ContractMember,
  EventContract,
  MethodMember,
  PathSegment,
  PropertyMember,
  ValueExpr,
  ValueShape,
} from "../types";

export function cloneContracts(contracts: Record<string, CapabilityContract>): Record<string, CapabilityContract> {
  const cloned: Record<string, CapabilityContract> = {};

  for (const [key, contract] of Object.entries(contracts)) {
    cloned[key] = cloneContract(contract);
  }

  return cloned;
}

export function mergeContractMaps(
  target: Record<string, CapabilityContract>,
  incoming: Record<string, CapabilityContract>
): void {
  for (const [key, contract] of Object.entries(incoming)) {
    const existing = target[key];
    if (!existing) {
      target[key] = cloneContract(contract);
      continue;
    }

    assertCompatibleContract(key, existing, contract);
    mergeMembers(existing.members, contract.members);
    mergeEvents(existing, contract);
  }
}

export function mergeContractIntoTarget(
  target: Record<string, CapabilityContract>,
  targetKey: string,
  sourceContracts: Record<string, CapabilityContract>,
  sourceKey: string
): void {
  const source = sourceContracts[sourceKey];
  if (!source) {
    throw new Error(`[alis] contract "${sourceKey}" is missing during merge`);
  }

  const existing = target[targetKey];
  if (!existing) {
    target[targetKey] = cloneContract(source);
    return;
  }

  assertCompatibleContract(targetKey, existing, source);
  mergeMembers(existing.members, source.members);
  mergeEvents(existing, source);
}

export function pruneUnreferencedContracts(
  contracts: Record<string, CapabilityContract>,
  objects: Record<string, { contract: string }>
): void {
  const referenced = new Set<string>();

  for (const objectRef of Object.values(objects)) {
    collectReferencedContracts(objectRef.contract, contracts, referenced);
  }

  for (const key of Object.keys(contracts)) {
    if (!referenced.has(key)) {
      delete contracts[key];
    }
  }
}

export function cloneContract(contract: CapabilityContract): CapabilityContract {
  const members: CapabilityContract["members"] = {};
  for (const [name, member] of Object.entries(contract.members)) {
    members[name] = cloneMember(member);
  }

  const events = contract.events
    ? Object.fromEntries(
        Object.entries(contract.events).map(([name, event]) => [name, cloneEvent(event)])
      )
    : undefined;

  return {
    kind: contract.kind,
    resolver: contract.resolver,
    members,
    ...(events ? { events } : {}),
  };
}

function cloneMember(member: ContractMember): ContractMember {
  if (member.kind === "property") {
    return {
      kind: "property",
      path: clonePath(member.path),
      shape: cloneShape(member.shape),
      access: member.access,
    };
  }

  return {
    kind: "method",
    path: clonePath(member.path),
    ...(member.args ? { args: member.args.map(cloneShape) } : {}),
    ...(member.returns ? { returns: member.returns === "void" ? "void" : cloneShape(member.returns) } : {}),
  };
}

function cloneEvent(event: EventContract): EventContract {
  const data = event.data
    ? Object.fromEntries(
        Object.entries(event.data).map(([name, value]) => [name, cloneValueExpr(value)])
      )
    : undefined;

  return {
    channel: event.channel,
    ...(event.eventObject ? { eventObject: { contract: event.eventObject.contract } } : {}),
    ...(data ? { data } : {}),
  };
}

function clonePath(path: PathSegment[]): PathSegment[] {
  return path.map(segment => {
    if (Object.prototype.hasOwnProperty.call(segment, "prop")) {
      return { prop: segment.prop as string };
    }

    return { index: segment.index as number };
  });
}

export function cloneShape(shape: ValueShape): ValueShape {
  switch (shape.kind) {
    case "scalar":
      return { kind: "scalar", type: shape.type };
    case "array":
      return { kind: "array", item: cloneShape(shape.item) };
    case "object":
      return {
        kind: "object",
        ...(shape.fields
          ? {
              fields: Object.fromEntries(
                Object.entries(shape.fields).map(([name, value]) => [name, cloneShape(value)])
              ),
            }
          : {}),
        ...(shape.additional ? { additional: true } : {}),
      };
    case "any":
    default:
      return { kind: "any" };
  }
}

function cloneValueExpr(expr: ValueExpr): ValueExpr {
  switch (expr.kind) {
    case "literal":
      return Object.prototype.hasOwnProperty.call(expr, "value")
        ? { kind: "literal", value: expr.value }
        : { kind: "literal" };
    case "binding":
      return { kind: "binding", binding: expr.binding };
    case "member":
      return { kind: "member", object: expr.object, member: expr.member };
    case "context":
      return {
        kind: "context",
        scope: expr.scope,
        ...(expr.path ? { path: clonePath(expr.path) } : {}),
      };
    case "object":
      return {
        kind: "object",
        fields: Object.fromEntries(
          Object.entries(expr.fields).map(([name, value]) => [name, cloneValueExpr(value)])
        ),
      };
    case "array":
      return { kind: "array", items: expr.items.map(cloneValueExpr) };
    case "convert":
      return { kind: "convert", value: cloneValueExpr(expr.value), to: cloneShape(expr.to) };
    default:
      return expr;
  }
}

function assertCompatibleContract(key: string, current: CapabilityContract, incoming: CapabilityContract): void {
  if (current.kind !== incoming.kind || current.resolver !== incoming.resolver) {
    throw new Error(
      `[alis] contract "${key}" changed shape during merge (${current.kind}/${current.resolver} vs ${incoming.kind}/${incoming.resolver})`
    );
  }
}

function mergeMembers(
  target: Record<string, ContractMember>,
  incoming: Record<string, ContractMember>
): void {
  for (const [name, member] of Object.entries(incoming)) {
    const existing = target[name];
    if (!existing) {
      target[name] = cloneMember(member);
      continue;
    }

    if (existing.kind !== member.kind) {
      throw new Error(`[alis] member "${name}" changed kind during merge`);
    }

    if (!samePath(existing.path, member.path)) {
      throw new Error(`[alis] member "${name}" changed path during merge`);
    }

    if (existing.kind === "property" && member.kind === "property") {
      existing.shape = mergeShape(existing.shape, member.shape);
      existing.access = mergeAccess(existing.access, member.access);
      continue;
    }

    const targetMethod = existing as MethodMember;
    const incomingMethod = member as MethodMember;
    if (incomingMethod.args && incomingMethod.args.length > 0) {
      targetMethod.args = mergeArgShapes(targetMethod.args, incomingMethod.args);
    }

    if (!targetMethod.returns && incomingMethod.returns) {
      targetMethod.returns = incomingMethod.returns === "void"
        ? "void"
        : cloneShape(incomingMethod.returns);
    }
  }
}

function mergeEvents(target: CapabilityContract, incoming: CapabilityContract): void {
  if (!incoming.events || Object.keys(incoming.events).length === 0) {
    return;
  }

  target.events ??= {};

  for (const [name, event] of Object.entries(incoming.events)) {
    const existing = target.events[name];
    if (!existing) {
      target.events[name] = cloneEvent(event);
      continue;
    }

    existing.channel = event.channel;

    if (!existing.eventObject && event.eventObject) {
      existing.eventObject = { contract: event.eventObject.contract };
    }

    if (!event.data || Object.keys(event.data).length === 0) {
      continue;
    }

    existing.data ??= {};
    for (const [field, value] of Object.entries(event.data)) {
      existing.data[field] = cloneValueExpr(value);
    }
  }
}

function samePath(left: PathSegment[], right: PathSegment[]): boolean {
  if (left.length !== right.length) {
    return false;
  }

  for (let i = 0; i < left.length; i++) {
    const a = left[i];
    const b = right[i];
    if ("prop" in a && "prop" in b) {
      if (a.prop !== b.prop) {
        return false;
      }

      continue;
    }

    if ("index" in a && "index" in b) {
      if (a.index !== b.index) {
        return false;
      }

      continue;
    }

    return false;
  }

  return true;
}

function mergeArgShapes(current: ValueShape[] | undefined, next: ValueShape[]): ValueShape[] {
  if (!current || current.length === 0) {
    return next.map(cloneShape);
  }

  if (current.length !== next.length) {
    return current.map(cloneShape);
  }

  return current.map((shape, index) => mergeShape(shape, next[index]));
}

function mergeAccess(left: PropertyMember["access"], right: PropertyMember["access"]): PropertyMember["access"] {
  return left === right ? left : "readwrite";
}

export function mergeShape(left: ValueShape, right: ValueShape): ValueShape {
  if (left.kind === "any") {
    return cloneShape(right);
  }

  if (right.kind === "any") {
    return cloneShape(left);
  }

  if (left.kind !== right.kind) {
    return { kind: "any" };
  }

  if (left.kind === "scalar" && right.kind === "scalar") {
    return left.type === right.type ? cloneShape(left) : { kind: "any" };
  }

  if (left.kind === "array" && right.kind === "array") {
    return { kind: "array", item: mergeShape(left.item, right.item) };
  }

  if (left.kind === "object" && right.kind === "object") {
    const fields: Record<string, ValueShape> = {};
    const allKeys = new Set([
      ...Object.keys(left.fields ?? {}),
      ...Object.keys(right.fields ?? {}),
    ]);

    for (const key of allKeys) {
      const current = left.fields?.[key];
      const incoming = right.fields?.[key];
      if (current && incoming) {
        fields[key] = mergeShape(current, incoming);
      } else if (current) {
        fields[key] = cloneShape(current);
      } else if (incoming) {
        fields[key] = cloneShape(incoming);
      }
    }

    return {
      kind: "object",
      ...(Object.keys(fields).length > 0 ? { fields } : {}),
      ...(left.additional || right.additional ? { additional: true } : {}),
    };
  }

  return cloneShape(left);
}

function collectReferencedContracts(
  contractKey: string,
  contracts: Record<string, CapabilityContract>,
  referenced: Set<string>
): void {
  if (referenced.has(contractKey)) {
    return;
  }

  referenced.add(contractKey);

  const contract = contracts[contractKey];
  if (!contract?.events) {
    return;
  }

  for (const event of Object.values(contract.events)) {
    if (event.eventObject?.contract) {
      collectReferencedContracts(event.eventObject.contract, contracts, referenced);
    }
  }
}
