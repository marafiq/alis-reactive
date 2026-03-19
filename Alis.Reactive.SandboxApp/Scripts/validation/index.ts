// Validation — Public API
//
// All logic lives in Scripts/validation/ modules.
// This barrel re-export provides the public surface.

export { validate, showServerErrors, clearAll } from "./orchestrator";
export { wireLiveClearing, resetLiveClearForTests } from "./live-clear";
