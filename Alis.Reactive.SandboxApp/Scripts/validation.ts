// Validation — Public API
//
// All logic lives in Scripts/validation/ modules.
// This file re-exports the public API so existing imports continue to work.
// Root file required because TypeScript/esbuild resolves "validation.ts" before "validation/index.ts".

export { validate, showServerErrors, clearAll } from "./validation/orchestrator";
export { wireLiveClearing } from "./validation/live-clear";
