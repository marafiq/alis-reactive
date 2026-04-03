// Disable Syncfusion EJ2 script-level animations when the vendor runtime is present.
// Some sandbox pages do not initialize Syncfusion before this script executes during
// navigation churn, so we guard the global instead of throwing before the reactive
// runtime can boot.
if (globalThis.ej?.base?.setGlobalAnimation && globalThis.ej?.base?.GlobalAnimationMode) {
    globalThis.ej.base.setGlobalAnimation(globalThis.ej.base.GlobalAnimationMode.Disable);
}
