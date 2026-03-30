// Disable all Syncfusion EJ2 script-level animations globally.
// Eliminates popup slide-in/out animations that cause Playwright test flakiness
// (Playwright's "stable" actionability check fails when bounding boxes change during animation).
// See: https://ej2.syncfusion.com/documentation/common/animation
ej.base.setGlobalAnimation(ej.base.GlobalAnimationMode.Disable);
