import { afterEach, vi } from "vitest";
import { resetBootStateForTests } from "../boot";
import { resetNativeActionLinksForTests } from "../native-action-link";

afterEach(() => {
  resetNativeActionLinksForTests();
  resetBootStateForTests();
  vi.restoreAllMocks();
  document.body.innerHTML = "";
});
