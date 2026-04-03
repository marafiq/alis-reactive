import type {
  CapabilityContract,
  ContractMember,
  MethodMember,
  Plan,
  PropertyMember,
  RuntimeObject,
  ValueShape,
  ExecContext,
} from "../types";
import { resolveCallable, setSegments, walk, walkSegments } from "../core/walk";

function resolveVendorRoot(el: HTMLElement, resolver: CapabilityContract["resolver"]): unknown {
  switch (resolver) {
    case "native-element":
      return el;
    case "fusion-instance": {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any -- Syncfusion attaches runtime instances here.
      const root = (el as any).ej2_instances?.[0];
      if (root == null) {
        throw new Error(`[alis] no vendor root for "${el.id}" (resolver: ${resolver}) — is the component initialized?`);
      }
      return root;
    }
    default:
      return el;
  }
}

function resolveContextObject(plan: Plan, objectName: string, objectRef: RuntimeObject): unknown {
  const alis = (globalThis as any).alis;
  const alisObjects = alis?.objects as Record<string, unknown> | undefined;
  if (alisObjects && objectName in alisObjects) {
    return alisObjects[objectName];
  }

  if (objectRef.elementId) {
    const el = document.getElementById(objectRef.elementId);
    if (el) return el;
  }

  const fromWindowPath = walk(globalThis, objectName);
  if (fromWindowPath !== undefined) {
    return fromWindowPath;
  }

  const contractPath = walk(globalThis, objectRef.contract);
  if (contractPath !== undefined) {
    return contractPath;
  }

  throw new Error(`[alis] context object not found: ${objectName}`);
}

export function getObject(plan: Plan, objectName: string): RuntimeObject {
  const objectRef = plan.objects[objectName];
  if (!objectRef) {
    throw new Error(`[alis] object not found: ${objectName}`);
  }
  return objectRef;
}

export function getContract(plan: Plan, contractKey: string): CapabilityContract {
  const contract = plan.contracts[contractKey];
  if (!contract) {
    throw new Error(`[alis] contract not found: ${contractKey}`);
  }
  return contract;
}

export function getObjectContract(plan: Plan, objectName: string): CapabilityContract {
  if (objectName === "$eventObject") {
    if (!plan.contracts["$eventObject"]) {
      throw new Error("[alis] $eventObject contract is not available in this workflow context");
    }
    return plan.contracts["$eventObject"];
  }

  const objectRef = getObject(plan, objectName);
  return getContract(plan, objectRef.contract);
}

export function resolveObjectRoot(plan: Plan, objectName: string, ctx?: ExecContext): unknown {
  if (objectName === "$eventObject") {
    if (ctx?.eventObject == null) {
      throw new Error("[alis] $eventObject is only available inside object-event workflows");
    }
    return ctx.eventObject;
  }

  const objectRef = getObject(plan, objectName);
  const contract = getContract(plan, objectRef.contract);

  switch (contract.resolver) {
    case "event-object":
      if (ctx?.eventObject == null) {
        throw new Error("[alis] event-object contract requires eventObject context");
      }
      return ctx.eventObject;

    case "context-object":
      return resolveContextObject(plan, objectName, objectRef);

    case "native-element":
    case "fusion-instance": {
      if (!objectRef.elementId) {
        throw new Error(`[alis] object "${objectName}" is missing elementId`);
      }
      const el = document.getElementById(objectRef.elementId);
      if (!el) {
        throw new Error(`[alis] element not found: ${objectRef.elementId}`);
      }
      return resolveVendorRoot(el, contract.resolver);
    }

    default:
      throw new Error(`[alis] unsupported contract resolver: ${contract.resolver}`);
  }
}

export function getContractMember(plan: Plan, objectName: string, memberName: string): ContractMember {
  const contract = getObjectContract(plan, objectName);
  const member = contract.members[memberName];
  if (!member) {
    throw new Error(`[alis] member "${memberName}" not found on object "${objectName}"`);
  }
  return member;
}

export function getPropertyMember(plan: Plan, objectName: string, memberName: string): PropertyMember {
  const member = getContractMember(plan, objectName, memberName);
  if (member.kind !== "property") {
    throw new Error(`[alis] member "${memberName}" on "${objectName}" is not a property`);
  }
  return member;
}

export function getMethodMember(plan: Plan, objectName: string, memberName: string): MethodMember {
  const member = getContractMember(plan, objectName, memberName);
  if (member.kind !== "method") {
    throw new Error(`[alis] member "${memberName}" on "${objectName}" is not a method`);
  }
  return member;
}

export function readMemberValue(plan: Plan, objectName: string, memberName: string, ctx?: ExecContext): unknown {
  const root = resolveObjectRoot(plan, objectName, ctx);
  const member = getContractMember(plan, objectName, memberName);
  if (member.kind === "property") {
    return walkSegments(root, member.path);
  }

  const callable = resolveCallable(root, member.path);
  return callable.fn.call(callable.owner);
}

export function setMemberValue(plan: Plan, objectName: string, memberName: string, value: unknown, ctx?: ExecContext): void {
  const member = getPropertyMember(plan, objectName, memberName);
  if (member.access !== "write" && member.access !== "readwrite") {
    throw new Error(`[alis] member "${memberName}" on "${objectName}" is not writable`);
  }

  const root = resolveObjectRoot(plan, objectName, ctx);
  setSegments(root, member.path, value);
}

export function callMember(plan: Plan, objectName: string, memberName: string, args: unknown[], ctx?: ExecContext): unknown {
  const member = getMethodMember(plan, objectName, memberName);
  const root = resolveObjectRoot(plan, objectName, ctx);
  const callable = resolveCallable(root, member.path);
  return callable.fn.apply(callable.owner, args);
}

export function getBindingValue(plan: Plan, bindingName: string, ctx?: ExecContext): unknown {
  const binding = plan.bindings[bindingName];
  if (!binding) {
    throw new Error(`[alis] binding not found: ${bindingName}`);
  }
  return readMemberValue(plan, binding.object, binding.valueMember, ctx);
}

export function getBindingShape(plan: Plan, bindingName: string): ValueShape {
  const binding = plan.bindings[bindingName];
  if (!binding) {
    throw new Error(`[alis] binding not found: ${bindingName}`);
  }
  return binding.shape;
}

export function tryGetElementIdForBinding(plan: Plan, bindingName: string): string | undefined {
  const binding = plan.bindings[bindingName];
  if (!binding) return undefined;
  if (binding.object === "$eventObject") return undefined;
  return plan.objects[binding.object]?.elementId;
}
