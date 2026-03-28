---
name: descriptor-restructure-plan
description: SOLID+DDD restructuring plan for descriptor layer — file splits, polymorphic simplification, merge.ts extraction, validation pending UI, fail-fast
type: project
---

Plan file: `/Users/muhammadadnanrafiq/.claude/plans/lively-marinating-corbato.md`

Key decisions:
- AllGather and validation enrichment STAY runtime (partials require live map)
- WriteOnlyPolymorphicConverter replaces [JsonPolymorphic]+[JsonDerivedType] (25 classes)
- BindSource moves from Builders/Conditions to Descriptors/Sources
- CoercionTypes moves from Guards to Descriptors root (shared at 5 levels)
- merge.ts extracted from boot.ts — owns partial lifecycle with tracked contributions
- Validation missing component → pending UI summary div (not throw, not skip)
- trigger.ts readExpr ?? "value" → throw (no fallback ever)
- PipelineBuilder → 3 partial class files
- Command.cs (11 types) split into Commands/ + Mutations/ sub-domain

**Why:** Architecture must scale to 100+ component vertical slices
**How to apply:** Follow phases A-G in the plan file
