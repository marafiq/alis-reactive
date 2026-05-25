# Alis.Reactive Agent Guidance

Use the root `../CLAUDE.md` and `../AGENTS.md` as the source of truth for this
project. This nested file intentionally contains no separate architecture model.

Primary rule: DSL -> Rich Plan Domain -> Generated Rich TS Contract -> Runtime
Executioner.

Do not resurrect old plan vocabulary, JSON schema authority, compatibility
layers, defensive fallback, or wrapper abstractions that do not directly name a
real DSL behavior.
