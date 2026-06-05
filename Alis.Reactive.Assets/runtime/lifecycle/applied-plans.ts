// Applied plan state owns boot snapshots, loaded partial slots, and per-slot AbortControllers.
// Active Plan state recomposes from the boot snapshot plus currently loaded slots; the boot snapshot is immutable.

import type { PlanDocument, Behavior } from "../types/index";
import { MergePolicy, emptyPlan, snapshotPlan } from "./merge-policy";

type PlanId = string;
type SlotId = string;

type WireBehaviors = (behaviors: Behavior[], activePlan: PlanDocument, signal?: AbortSignal) => void;
type WireContainerValidation = (activePlan: PlanDocument, signal?: AbortSignal) => void;

export interface ActivePlanWiring {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface PartialSlotLoad {
  readonly abortController: AbortController;
  readonly slotPlans: PlanDocument[];
}

export class AppliedPlans {
  private readonly activePlans = new Map<PlanId, PlanDocument>();
  private readonly bootSnapshots = new Map<PlanId, PlanDocument>();
  private readonly partialSlotLoads = new Map<SlotId, PartialSlotLoad>();

  register(bootPlan: PlanDocument): void {
    this.activePlans.set(bootPlan.planId, bootPlan);
    this.bootSnapshots.set(bootPlan.planId, snapshotPlan(bootPlan));
  }

  loadPartialSlot(slotId: SlotId, incomingPlans: PlanDocument[], wiring: ActivePlanWiring): PlanId[] {
    const affectedPlanIds = new Set(this.unloadSlot(slotId));

    const abortController = new AbortController();
    const slotPlans = incomingPlans.map(snapshotPlan);
    const loadedPlanIds = planIdsIn(slotPlans);
    this.partialSlotLoads.set(slotId, { abortController, slotPlans });

    for (const planId of loadedPlanIds) affectedPlanIds.add(planId);

    this.recomposePlans(affectedPlanIds);
    for (const slotPlan of slotPlans) {
      const activePlan = this.activePlans.get(slotPlan.planId)!;
      wiring.wireBehaviors(slotPlan.behaviors, activePlan, abortController.signal);
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
    this.partialSlotLoads.clear();
  }

  private unloadSlot(slotId: SlotId): PlanId[] {
    const slotLoad = this.partialSlotLoads.get(slotId);
    if (slotLoad === undefined) return [];

    this.partialSlotLoads.delete(slotId);
    slotLoad.abortController.abort();
    return planIdsIn(slotLoad.slotPlans);
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

    const activePlan = this.activePlanDocument(planId);
    resetPlanDocument(activePlan, planId);

    if (bootPlan !== undefined) {
      MergePolicy.composeBootPlanInto(activePlan, bootPlan);
    }

    for (const slotPlan of slotPlans) {
      MergePolicy.composeSlotPlanInto(activePlan, slotPlan, bootPlan);
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
    const matchingSlotPlans: PlanDocument[] = [];
    for (const slotLoad of this.partialSlotLoads.values()) {
      for (const slotPlan of slotLoad.slotPlans) {
        if (slotPlan.planId === planId) matchingSlotPlans.push(slotPlan);
      }
    }

    return matchingSlotPlans;
  }

  private abortSlots(): void {
    for (const slotLoad of this.partialSlotLoads.values()) {
      slotLoad.abortController.abort();
    }
  }
}

export const appliedPlans = new AppliedPlans();

export function composeInitialPlans(bootPlans: PlanDocument[]): PlanDocument[] {
  const activePlans = new Map<PlanId, PlanDocument>();
  for (const bootPlan of bootPlans) {
    const activePlan = activePlans.get(bootPlan.planId) ?? emptyPlan(bootPlan.planId);
    activePlans.set(bootPlan.planId, activePlan);
    MergePolicy.composeBootPlanInto(activePlan, bootPlan);
  }

  return Array.from(activePlans.values());
}

function resetPlanDocument(planDocument: PlanDocument, planId: PlanId): void {
  planDocument.version = 3;
  planDocument.planId = planId;
  planDocument.scope = { kind: "root" };
  planDocument.types = {};
  planDocument.components = {};
  planDocument.behaviors = [];
}

function planIdsIn(planDocuments: PlanDocument[]): PlanId[] {
  return Array.from(new Set(planDocuments.map(planDocument => planDocument.planId)));
}
