// The vendors C# writes into every plan (ComponentVendor.Native, FusionComponent.FusionVendor),
// for fixtures that build plans by hand.
import type { VendorContract } from "../../types/index";

export const nativeVendor: VendorContract = {
  objectPath: [],
  subscribe: "addEventListener",
  unsubscribe: "removeEventListener",
  payload: "dom-event",
};

export const fusionVendor: VendorContract = {
  objectPath: [{ kind: "property", name: "ej2_instances" }, { kind: "index", index: 0 }],
  subscribe: "addEventListener",
  unsubscribe: "removeEventListener",
  payload: "argument",
};

export const testVendors: Record<string, VendorContract> = {
  native: nativeVendor,
  fusion: fusionVendor,
};
