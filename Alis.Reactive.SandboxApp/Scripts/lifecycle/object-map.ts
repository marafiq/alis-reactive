import type { CapabilityContract, FieldBinding, RuntimeObject } from "../types";
import { cloneShape, mergeContractIntoTarget, mergeShape } from "./contract-map";

export function cloneObjects(objects: Record<string, RuntimeObject>): Record<string, RuntimeObject> {
  return Object.fromEntries(
    Object.entries(objects).map(([name, objectRef]) => [name, cloneObject(objectRef)])
  );
}

export function cloneBindings(bindings: Record<string, FieldBinding>): Record<string, FieldBinding> {
  return Object.fromEntries(
    Object.entries(bindings).map(([name, binding]) => [name, cloneBinding(binding)])
  );
}

export function mergeObjectMaps(
  targetContracts: Record<string, CapabilityContract>,
  targetObjects: Record<string, RuntimeObject>,
  incomingContracts: Record<string, CapabilityContract>,
  incomingObjects: Record<string, RuntimeObject>
): void {
  for (const [name, incoming] of Object.entries(incomingObjects)) {
    const existing = targetObjects[name];
    if (!existing) {
      targetObjects[name] = cloneObject(incoming);
      continue;
    }

    const canonicalContract = chooseCanonicalContractKey(name, existing.contract, incoming.contract);
    if (canonicalContract === existing.contract && incoming.contract !== existing.contract) {
      mergeContractIntoTarget(targetContracts, canonicalContract, incomingContracts, incoming.contract);
    } else if (canonicalContract === incoming.contract && existing.contract !== incoming.contract) {
      mergeContractIntoTarget(targetContracts, canonicalContract, targetContracts, existing.contract);
      existing.contract = canonicalContract;
    }

    existing.elementId = mergeElementId(name, existing.elementId, incoming.elementId);
  }
}

export function mergeBindingMaps(
  target: Record<string, FieldBinding>,
  incoming: Record<string, FieldBinding>
): void {
  for (const [name, binding] of Object.entries(incoming)) {
    const existing = target[name];
    if (!existing) {
      target[name] = cloneBinding(binding);
      continue;
    }

    if (existing.object !== binding.object || existing.valueMember !== binding.valueMember) {
      throw new Error(
        `[alis] binding "${name}" changed target during merge (${existing.object}.${existing.valueMember} vs ${binding.object}.${binding.valueMember})`
      );
    }

    existing.shape = mergeShape(existing.shape, binding.shape);
  }
}

function cloneObject(objectRef: RuntimeObject): RuntimeObject {
  return {
    contract: objectRef.contract,
    ...(objectRef.elementId ? { elementId: objectRef.elementId } : {}),
  };
}

function cloneBinding(binding: FieldBinding): FieldBinding {
  return {
    object: binding.object,
    valueMember: binding.valueMember,
    shape: cloneShape(binding.shape),
  };
}

function mergeElementId(objectName: string, current?: string, incoming?: string): string | undefined {
  if (!current) {
    return incoming;
  }

  if (!incoming || current === incoming) {
    return current;
  }

  throw new Error(`[alis] object "${objectName}" changed elementId during merge (${current} vs ${incoming})`);
}

function chooseCanonicalContractKey(objectName: string, current: string, incoming: string): string {
  if (current === incoming) {
    return current;
  }

  if (isGenericComponentContract(current) && !isGenericComponentContract(incoming)) {
    return incoming;
  }

  if (!isGenericComponentContract(current) && isGenericComponentContract(incoming)) {
    return current;
  }

  throw new Error(`[alis] object "${objectName}" changed contract during merge (${current} vs ${incoming})`);
}

function isGenericComponentContract(contractKey: string): boolean {
  return contractKey.startsWith("native.component.") || contractKey.startsWith("fusion.component.");
}
