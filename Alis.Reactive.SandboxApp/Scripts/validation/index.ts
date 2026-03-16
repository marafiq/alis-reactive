// Validation — Public API barrel
//
// External consumers import from "./validation" (this folder).
// Internal modules: orchestrator, rule-engine, condition, error-display, live-clear.

export { validate, showServerErrors, clearAll } from "./orchestrator";
export { wireLiveClearing } from "./live-clear";
