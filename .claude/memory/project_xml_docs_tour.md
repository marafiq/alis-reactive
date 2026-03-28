---
name: XML Docs Tour — Framework API Documentation
description: Session tracking the guided XML documentation tour of Alis.Reactive. Covers skill creation, style learnings, and progress through framework files.
type: project
---

## Skill Created

`~/.claude/skills/dotnet-xml-docs/` — reviewed by 2 agents, iterated with user feedback.
- SKILL.md — core rules, "The Standard", property voice convention, Alis.Reactive patterns
- references/tag-reference.md — complete tag syntax from Microsoft Learn
- references/csharp-spec-annex-d.md — C# Language Spec Annex D formal grammar

## User's Style (Critical for Future Sessions)

1. **Dense single-sentence remarks** — pack multiple concepts naturally, no bullet-pointing
2. **Frame from user's mental model** — producer, event, intent — not framework internals (entries, descriptors, two-phase boot)
3. **Trigger/component names as inline nouns** — not "Supports: X, Y, Z" lists
4. **Every word earns its place** — no filler, no over-explaining what devs already know
5. **Two audiences** — summary+params speak dev language, remarks can speak internal language when prefixed with "Internally"
6. **Don't say "Unused"** for extension method `this` param — confuses devs
7. **Don't say "anywhere"** for placement — too liberal
8. **Include anti-patterns in remarks** — e.g., "avoid defining same event twice"

## Tour Progress

### DONE: HtmlExtensions.cs (Alis.Reactive.Native/Extensions/)
- Class-level summary added
- On() method: summary, remarks, typeparam, params — all rewritten
- User refined remarks in IDE — final version on branch refactor/api-surface-xml-docs
- NOTE: changes were reverted during ISP refactor experiments. Need to re-apply.

### NEXT: IReactivePlan.cs + ReactivePlan.cs
- Responsibility map completed (7 members, 5 audiences)
- ISP refactor parked — see project_isp_refactor_plan.md
- XML docs for these files should wait until after ISP refactor settles

### Tour Path (remaining)
1. TriggerBuilder.cs
2. PipelineBuilder.cs
3. ElementBuilder.cs
4. Descriptors: Entry.cs, Trigger.cs, Reaction.cs, Command.cs
5. BindSource, ExpressionPathHelper
6. IComponent, IInputComponent, ComponentRef<T>
7. RequestBuilder, GatherBuilder, ResponseBuilder
8. WhenBuilder, guards, operators
9. IValidationExtractor, rule types

## Branch: refactor/api-surface-xml-docs (created from main, up to date)
