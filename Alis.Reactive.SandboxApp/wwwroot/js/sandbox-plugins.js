"use strict";
(() => {
  // Alis.Reactive.SandboxApp/Scripts/sandbox-plugins.ts
  (window.__alisPlugins ??= []).push({
    name: "array",
    instance: {
      count: (arr) => arr?.length ?? 0,
      pluck: (arr, index, key) => arr?.[index]?.[key],
      filter: (arr, key, val) => arr?.filter((i) => i[key] === val) ?? [],
      sum: (arr, key) => arr?.reduce((s, i) => s + (Number(i[key]) || 0), 0) ?? 0,
      some: (arr, key, val) => arr?.some((i) => i[key] === val) ?? false
    }
  });
  (window.__alisPlugins ??= []).push({
    name: "analytics",
    instance: {
      track: (event) => {
      }
    }
  });
})();
