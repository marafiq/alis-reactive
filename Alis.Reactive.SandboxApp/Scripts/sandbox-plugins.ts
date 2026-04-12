// sandbox-plugins.ts — Plugin instances for sandbox testing.
// Bundled separately. Loaded before alis-reactive.js.

((window as any).__alisPlugins ??= []).push({
  name: "array",
  instance: {
    count:  (arr: any[]) => arr?.length ?? 0,
    pluck:  (arr: any[], index: number, key: string) => arr?.[index]?.[key],
    filter: (arr: any[], key: string, val: any) => arr?.filter((i: any) => i[key] === val) ?? [],
    sum:    (arr: any[], key: string) => arr?.reduce((s: number, i: any) => s + (Number(i[key]) || 0), 0) ?? 0,
    some:   (arr: any[], key: string, val: any) => arr?.some((i: any) => i[key] === val) ?? false,
  }
});

((window as any).__alisPlugins ??= []).push({
  name: "analytics",
  instance: {
    track: (event: string) => { /* sandbox no-op */ },
  }
});
