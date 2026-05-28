// browser-plans.ts - boot plan composition and partial slot lifetimes.

import type { PlanDocument, Behavior } from "../types";
import {
  mergeBootComponent,
  mergeSlotComponent,
} from "./component-merge";
import { mergeObjectContracts } from "./object-contracts";

type PlanId = string;
type SlotId = string;

type WireBehaviors = (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: PlanDocument, signal?: AbortSignal) => void;

export interface BrowserPlanWiring {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface LoadedPartialSlot {
  readonly abortController: AbortController;
  readonly plans: PlanDocument[];
}

export class AppliedBrowserPlans {
  private readonly activePlans = new Map<PlanId, PlanDocument>();
  private readonly bootSnapshots = new Map<PlanId, PlanDocument>();
  private readonly loadedSlots = new Map<SlotId, LoadedPartialSlot>();

  register(plan: PlanDocument): void {
    this.activePlans.set(plan.planId, plan);
    this.bootSnapshots.set(plan.planId, snapshotPlan(plan));
  }

  loadPartialSlot(slotId: SlotId, plans: PlanDocument[], wiring: BrowserPlanWiring): PlanId[] {
    const affectedPlanIds = new Set(this.unloadSlot(slotId));

    const abortController = new AbortController();
    const slotPlans = plans.map(snapshotPlan);
    const loadedPlanIds = planIdsIn(slotPlans);
    this.loadedSlots.set(slotId, { abortController, plans: slotPlans });

    for (const planId of loadedPlanIds) affectedPlanIds.add(planId);

    this.recomposePlans(affectedPlanIds);
    for (const plan of slotPlans) {
      const activePlan = this.activePlans.get(plan.planId)!;
      wiring.wireBehaviors(plan.behaviors, activePlan, abortController.signal);
    }
    for (const planId of loadedPlanIds) {
      const activePlan = this.activePlans.get(planId)!;
      wiring.wireContainerValidation(activePlan, abortController.signal);
    }

    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: SlotId): PlanId[] {
    const affectedPlanIds = new Set(this.unloadSlot(slotId));
    this.recomposePlans(affectedPlanIds);
    return [...affectedPlanIds];
  }

  get(planId: string): PlanDocument | undefined {
    return this.activePlans.get(planId);
  }

  reset(): void {
    this.abortSlots();
    this.activePlans.clear();
    this.bootSnapshots.clear();
    this.loadedSlots.clear();
  }

  private unloadSlot(slotId: SlotId): PlanId[] {
    const loadedSlot = this.loadedSlots.get(slotId);
    if (loadedSlot === undefined) return [];

    this.loadedSlots.delete(slotId);
    loadedSlot.abortController.abort();
    return planIdsIn(loadedSlot.plans);
  }

  private recomposePlans(planIds: Iterable<PlanId>): void {
    for (const planId of planIds) {
      this.recomposePlan(planId);
    }
  }

  private recomposePlan(planId: PlanId): void {
    const bootPlan = this.bootSnapshots.get(planId);
    const slotPlans = this.slotPlansFor(planId);

    if (bootPlan === undefined && slotPlans.length === 0) {
      this.activePlans.delete(planId);
      return;
    }

    const target = this.activePlanDocument(planId);
    resetPlanDocument(target, planId);

    if (bootPlan !== undefined) {
      composeBootPlanInto(target, bootPlan);
    }

    for (const slotPlan of slotPlans) {
      composeSlotPlanInto(target, slotPlan, bootPlan);
    }
  }

  private activePlanDocument(planId: PlanId): PlanDocument {
    let activePlan = this.activePlans.get(planId);
    if (activePlan === undefined) {
      activePlan = emptyPlan(planId);
      this.activePlans.set(planId, activePlan);
    }

    return activePlan;
  }

  private slotPlansFor(planId: PlanId): PlanDocument[] {
    const plans: PlanDocument[] = [];
    for (const loadedSlot of this.loadedSlots.values()) {
      for (const plan of loadedSlot.plans) {
        if (plan.planId === planId) plans.push(plan);
      }
    }

    return plans;
  }

  private abortSlots(): void {
    for (const loadedSlot of this.loadedSlots.values()) {
      loadedSlot.abortController.abort();
    }
  }
}

export const appliedBrowserPlans = new AppliedBrowserPlans();

export function composeInitialPlans(plans: PlanDocument[]): PlanDocument[] {
  const assembledPlans = new Map<PlanId, PlanDocument>();
  for (const plan of plans) {
    const assembled = assembledPlans.get(plan.planId) ?? emptyPlan(plan.planId);
    assembledPlans.set(plan.planId, assembled);
    composeBootPlanInto(assembled, plan);
  }

  return Array.from(assembledPlans.values());
}

function composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void {
  mergeTypeContracts(target, incoming);

  for (const [componentKey, component] of Object.entries(incoming.components)) {
    mergeBootComponent(target, componentKey, component);
  }

  target.behaviors.push(...incoming.behaviors);
}

function composeSlotPlanInto(
  target: PlanDocument,
  incoming: PlanDocument,
  bootPlan: PlanDocument | undefined,
): void {
  mergeTypeContracts(target, incoming);

  for (const [componentKey, component] of Object.entries(incoming.components)) {
    mergeSlotComponent(
      target,
      componentKey,
      component,
      bootPlan?.components[componentKey] !== undefined,
    );
  }

  target.behaviors.push(...incoming.behaviors);
}

function mergeTypeContracts(target: PlanDocument, incoming: PlanDocument): void {
  for (const [typeKey, contract] of Object.entries(incoming.types)) {
    target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
  }
}

function resetPlanDocument(plan: PlanDocument, planId: PlanId): void {
  plan.version = 3;
  plan.planId = planId;
  plan.scope = { kind: "root" };
  plan.types = {};
  plan.components = {};
  plan.behaviors = [];
}

function emptyPlan(planId: PlanId): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function snapshotPlan(plan: PlanDocument): PlanDocument {
  return {
    version: plan.version,
    planId: plan.planId,
    scope: { ...plan.scope },
    types: { ...plan.types },
    components: { ...plan.components },
    behaviors: [...plan.behaviors],
  };
}

function planIdsIn(plans: PlanDocument[]): PlanId[] {
  return Array.from(new Set(plans.map(plan => plan.planId)));
}
