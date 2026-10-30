// The shipped bundle (dist/scripts/alis-reactive.dev.js): the framework's own components first, then
// the runtime, which boots the page. The components reach the runtime only through alis:mount and
// window.__alisPlugins, as an app's components do.

import "./components/index";
import "./runtime/root";
