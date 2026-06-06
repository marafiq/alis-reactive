// Disable all Syncfusion EJ2 script-level animations globally.
// TODO: Replace this flakiness workaround with component-ready popup signals.
// Playwright's "stable" actionability check fails when bounding boxes change during animation.
// See: https://ej2.syncfusion.com/documentation/common/animation
ej.base.setGlobalAnimation(ej.base.GlobalAnimationMode.Disable);
