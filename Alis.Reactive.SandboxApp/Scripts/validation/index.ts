// Validation — Public API
// V3: validation rules live in Component.container.validationRules.
// ContainerScope defines which components belong to the form and their rules.

export { validateContainer, showServerErrors, clearContainerValidation, revalidateField } from "./orchestrator";
export { wireLiveValidation, resetLiveClearForTests } from "./live-clear";
