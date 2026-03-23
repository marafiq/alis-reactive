// Validation — Public API
//
// All logic lives in Scripts/validation/ modules.
// This barrel re-export provides the public surface.

export { validate, revalidateField, showServerErrors, clearAll } from "./orchestrator";
export { wireLiveValidation, unwireFields, resetLiveClearForTests } from "./live-clear";
