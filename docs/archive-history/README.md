# archive-history

Historical documentation kept out of the active tree. Full git history is preserved.

## `rc1-docs/` — RC1 / 1.0-era documentation

The design and status docs that described the shipped RC1 system: domain language and coverage
matrices, `adr/`, `reviews/`, `superpowers/` plans + specs, `test-coverage/`, validation guides,
and the RC1 status docs.

These are **historical reference only**. They predate later cleanup and may contain retired
vocabulary (for example the old `WriteOnlyPolymorphicConverter` name — the real type is
`PlanNodeDiscriminator<T>`). The authoritative contract is the C# plan domain plus the generated
`runtime/types/plan.ts`; see the root `CLAUDE.md`.
