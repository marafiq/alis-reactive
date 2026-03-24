# Two-Round Agent Review System — Prompt Architecture Design

> Designed against the actual `agents.mjs` on `feature/reactive-reader-v3`.
> Replaces the current single-round dispatch with an evidence-driven two-round protocol.

---

## Problem Statement

Current agents produce weak evidence. A BDD Tester writes `"reasoning": "Looks independent enough"`.
An Architect writes `"evidence": "Seems fine architecturally"`. The output schema asks for
`investScores` with per-criterion reasoning but there is no enforcement beyond a 20-character minimum.

Root causes:
1. **No rubric in the prompt.** Agents know their role but not what "good evidence" looks like for that role.
2. **Free-text evidence fields.** Nothing forces structured citations.
3. **No cross-pollination.** Round 1 reviews never see each other, so conflicts go unchallenged.
4. **No scoring feedback loop.** Agents never learn their evidence was weak.

---

## Deliverable 1: Evidence Rubric Schema

### Schema Definition

Each agent template carries an `evidence_rubric` object. The rubric defines evidence categories,
each with a `kind` (what structural element is expected), `min_count` (how many are needed), and
`examples` (in-prompt guidance for the agent).

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "AgentEvidenceRubric",
  "description": "Defines what constitutes substantive evidence for a specific agent role",
  "type": "object",
  "properties": {
    "role": {
      "type": "string",
      "enum": ["architect", "csharp", "bdd", "pm", "ui", "human-proxy"]
    },
    "evidence_categories": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string",
            "description": "Machine-readable category ID"
          },
          "label": {
            "type": "string",
            "description": "Human-readable label shown in prompt"
          },
          "kind": {
            "type": "string",
            "enum": [
              "file_reference",
              "dependency_direction",
              "principle_citation",
              "method_signature",
              "type_constraint",
              "test_name",
              "ac_reference",
              "edge_case",
              "scope_metric",
              "code_snippet",
              "command_sequence",
              "hard_rule_citation",
              "scaling_analysis"
            ],
            "description": "The structural type of evidence expected"
          },
          "min_count": {
            "type": "integer",
            "minimum": 1,
            "description": "Minimum number of instances required for this category"
          },
          "weight": {
            "type": "number",
            "minimum": 0,
            "maximum": 1,
            "description": "Relative weight in scoring (all weights for a role sum to 1.0)"
          },
          "examples": {
            "type": "array",
            "items": { "type": "string" },
            "description": "Concrete examples shown in the prompt — what good evidence looks like"
          },
          "anti_examples": {
            "type": "array",
            "items": { "type": "string" },
            "description": "What FAILS the rubric — shown in prompt as negative examples"
          }
        },
        "required": ["id", "label", "kind", "min_count", "weight", "examples", "anti_examples"]
      },
      "minItems": 3,
      "description": "At least 3 evidence categories per role"
    },
    "invest_evidence_overrides": {
      "type": "object",
      "description": "Per-INVEST-criterion, what evidence this role must provide (not all roles evaluate all criteria equally)",
      "properties": {
        "I": { "$ref": "#/$defs/invest_criterion_rubric" },
        "N": { "$ref": "#/$defs/invest_criterion_rubric" },
        "V": { "$ref": "#/$defs/invest_criterion_rubric" },
        "E": { "$ref": "#/$defs/invest_criterion_rubric" },
        "S": { "$ref": "#/$defs/invest_criterion_rubric" },
        "T": { "$ref": "#/$defs/invest_criterion_rubric" }
      }
    }
  },
  "required": ["role", "evidence_categories"],
  "$defs": {
    "invest_criterion_rubric": {
      "type": "object",
      "properties": {
        "required_evidence_kinds": {
          "type": "array",
          "items": { "type": "string" },
          "description": "Which evidence kinds must appear in reasoning for this criterion"
        },
        "min_reasoning_length": {
          "type": "integer",
          "minimum": 50,
          "description": "Minimum character count for reasoning (higher than the default 20)"
        },
        "skip": {
          "type": "boolean",
          "default": false,
          "description": "If true, this role is not expected to deeply evaluate this INVEST criterion"
        }
      }
    }
  }
}
```

### Rubric Instances Per Role

```javascript
const EVIDENCE_RUBRICS = {

  architect: {
    role: "architect",
    evidence_categories: [
      {
        id: "dep_direction",
        label: "Dependency Direction",
        kind: "dependency_direction",
        min_count: 2,
        weight: 0.30,
        examples: [
          "Core <- Native (correct: Native depends on Core)",
          "Alis.Reactive.Fusion -> Alis.Reactive (Core). Adding this story would create Fusion -> Native, violating layer boundaries.",
          "New descriptor in Core has zero external deps — verified by checking csproj references."
        ],
        anti_examples: [
          "Dependencies look fine.",
          "This follows proper architecture.",
          "No dependency issues."
        ]
      },
      {
        id: "solid_principle",
        label: "SOLID Principle Reference",
        kind: "principle_citation",
        min_count: 1,
        weight: 0.25,
        examples: [
          "OCP: New FusionColorPicker adds a new descriptor (ColorPickerComponent) and handler — existing switch in execute.ts is NOT modified.",
          "SRP: Story asks ElementBuilder to handle both DOM mutation AND component method dispatch. These are two distinct responsibilities.",
          "LSP: IInputComponent.ReadExpr returns 'value' for TextBox and 'checked' for CheckBox — both are valid substitutions."
        ],
        anti_examples: [
          "Follows SOLID.",
          "Good separation of concerns.",
          "This is properly designed."
        ]
      },
      {
        id: "file_path",
        label: "Codebase File Path",
        kind: "file_reference",
        min_count: 2,
        weight: 0.25,
        examples: [
          "Alis.Reactive/Descriptors/Commands/Command.cs — new JsonDerivedType attribute needed here.",
          "Scripts/execution/element.ts — resolveRoot() handles vendor='fusion' via ej2_instances[0].",
          "Alis.Reactive.Fusion/Components/NumericTextBox/ (7-file vertical slice to follow)."
        ],
        anti_examples: [
          "The relevant files.",
          "In the descriptors folder.",
          "See the codebase."
        ]
      },
      {
        id: "d2_artifact",
        label: "D2 Diagram Artifact",
        kind: "code_snippet",
        min_count: 1,
        weight: 0.20,
        examples: [
          "D2 showing: Core.Descriptors -> Plan.Schema -> Runtime.Execute with the new command flowing through all three."
        ],
        anti_examples: [
          "A diagram would be helpful here."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: {
        required_evidence_kinds: ["dependency_direction", "file_reference"],
        min_reasoning_length: 80
      },
      N: { skip: true },
      V: { skip: true },
      E: { skip: true },
      S: {
        required_evidence_kinds: ["file_reference"],
        min_reasoning_length: 60
      },
      T: { skip: true }
    }
  },

  csharp: {
    role: "csharp",
    evidence_categories: [
      {
        id: "method_sig",
        label: "Method Signature",
        kind: "method_signature",
        min_count: 2,
        weight: 0.30,
        examples: [
          "public static IInputComponent<TModel, string> FusionColorPickerFor<TModel>(this IHtmlHelper<TModel> html, Expression<Func<TModel, string>> expr)",
          "internal sealed class ColorPickerComponent : IComponent, IInputComponent { public string Vendor => \"fusion\"; public string ReadExpr => \"value\"; }",
          "public PipelineBuilder<TModel> SetValue(string value) — returns PipelineBuilder for fluent chaining."
        ],
        anti_examples: [
          "The method signature looks correct.",
          "Standard builder pattern.",
          "Type-safe API."
        ]
      },
      {
        id: "type_constraint",
        label: "Generic Constraint / Type Safety",
        kind: "type_constraint",
        min_count: 1,
        weight: 0.25,
        examples: [
          "where TComponent : IComponent, IInputComponent, new() — the new() constraint enables ComponentRef<TComponent> to cache a static instance.",
          "Expression<Func<TModel, TProp>> — ExpressionPathHelper will convert x => x.ColorCode to 'evt.colorCode'.",
          "IInputComponent.ReadExpr must return the JS property path — 'value' for TextBox, 'checked' for CheckBox."
        ],
        anti_examples: [
          "Types look fine.",
          "Generic constraints are correct.",
          "Properly typed."
        ]
      },
      {
        id: "builder_file",
        label: "Builder/Descriptor File Reference",
        kind: "file_reference",
        min_count: 2,
        weight: 0.25,
        examples: [
          "Alis.Reactive/Builders/ElementBuilder.cs — AddClass/RemoveClass/ToggleClass pattern to follow.",
          "Alis.Reactive.Native/Extensions/InputFieldExtensions.cs — Html.InputField<TModel>() factory, constructor internal.",
          "Alis.Reactive/Descriptors/Triggers/Trigger.cs — [JsonDerivedType(typeof(CustomEventTrigger), 'custom-event')]."
        ],
        anti_examples: [
          "In the builders folder.",
          "Follow existing patterns.",
          "See the extensions."
        ]
      },
      {
        id: "code_artifact",
        label: "Code Signature Artifact",
        kind: "code_snippet",
        min_count: 1,
        weight: 0.20,
        examples: [
          "Produce the actual extension method, builder class, and component sealed class — all three signatures."
        ],
        anti_examples: [
          "Code would look similar to existing components."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: { skip: true },
      N: {
        required_evidence_kinds: ["method_signature"],
        min_reasoning_length: 60
      },
      V: { skip: true },
      E: {
        required_evidence_kinds: ["file_reference"],
        min_reasoning_length: 60
      },
      S: {
        required_evidence_kinds: ["file_reference"],
        min_reasoning_length: 60
      },
      T: { skip: true }
    }
  },

  bdd: {
    role: "bdd",
    evidence_categories: [
      {
        id: "test_name",
        label: "Concrete Test Name",
        kind: "test_name",
        min_count: 3,
        weight: 0.30,
        examples: [
          "C#: WhenMutatingAnElement.Should_add_class_via_call_mutation()",
          "TS: when-using-unified-call-mutations.test.ts → 'should call void method on fusion component'",
          "Playwright: WhenEventChainFires.Should_resolve_nested_payload_path()"
        ],
        anti_examples: [
          "Tests should cover this.",
          "Add unit tests.",
          "All 3 layers need tests."
        ]
      },
      {
        id: "ac_ref",
        label: "Acceptance Criteria Reference",
        kind: "ac_reference",
        min_count: 2,
        weight: 0.25,
        examples: [
          "AC1 ('ColorPicker renders with correct default') — verified by Playwright: element has data-alis-plan, runtime boots without console errors.",
          "AC3 ('Value binding flows both directions') — C# test: snapshot shows source binding in plan JSON. TS test: resolveSource reads from component root.",
          "MISSING: No AC covers what happens when ColorPicker receives an invalid hex string — edge case needed."
        ],
        anti_examples: [
          "ACs are testable.",
          "Good acceptance criteria.",
          "All ACs covered."
        ]
      },
      {
        id: "edge_case",
        label: "Edge Case Identification",
        kind: "edge_case",
        min_count: 2,
        weight: 0.25,
        examples: [
          "What if the user clears the color picker and submits? ReadExpr 'value' returns '' — does the validator handle empty string vs null?",
          "What if Syncfusion component is not yet initialized when dom-ready fires? ej2_instances will be empty array — runtime should throw, not silently skip.",
          "Concurrent dispatch: two dom-ready reactions both dispatch to the same custom-event — execution order is non-deterministic."
        ],
        anti_examples: [
          "Edge cases should be considered.",
          "Some edge cases exist.",
          "Might need more testing."
        ]
      },
      {
        id: "test_artifact",
        label: "Test Case List Artifact",
        kind: "command_sequence",
        min_count: 1,
        weight: 0.20,
        examples: [
          "Produce a table: AC number | C# test file:method | TS test file:describe | Playwright file:method — one row per AC."
        ],
        anti_examples: [
          "Tests need to be written."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: { skip: true },
      N: {
        required_evidence_kinds: ["ac_reference"],
        min_reasoning_length: 60
      },
      V: { skip: true },
      E: { skip: true },
      S: { skip: true },
      T: {
        required_evidence_kinds: ["test_name", "ac_reference", "edge_case"],
        min_reasoning_length: 100
      }
    }
  },

  pm: {
    role: "pm",
    evidence_categories: [
      {
        id: "scope_metric",
        label: "Scope Metric (files x projects)",
        kind: "scope_metric",
        min_count: 2,
        weight: 0.30,
        examples: [
          "Touches 7 files in Alis.Reactive.Fusion (vertical slice) + 1 schema update in Core + 3 test files = 11 files across 3 projects.",
          "Story touches only Alis.Reactive.Native — single project. Estimated 4 new files (component, events, extensions, builder).",
          "AC5 requires changes to Scripts/execution/element.ts — crosses from C# work into TS runtime. Consider splitting."
        ],
        anti_examples: [
          "Reasonable scope.",
          "Seems about right sized.",
          "Should fit in one session."
        ]
      },
      {
        id: "dependency_ref",
        label: "Blocking Story Reference",
        kind: "file_reference",
        min_count: 1,
        weight: 0.25,
        examples: [
          "Depends on V-002 (Validation Rule Types) being done — new component needs Required() and Pattern() extractors.",
          "No blocking dependencies — component vertical slice is self-contained per architecture.",
          "Soft dependency on V-001 (INVEST gate): if that changes the review schema, this story's ACs may need updating."
        ],
        anti_examples: [
          "No dependencies.",
          "Independent enough.",
          "Should be fine."
        ]
      },
      {
        id: "value_statement",
        label: "Who Benefits and How",
        kind: "scope_metric",
        min_count: 1,
        weight: 0.25,
        examples: [
          "Framework developer building senior living intake forms — currently must hand-write color inputs. After this story, Html.FusionColorPickerFor() provides type-safe, validated color selection.",
          "Value is internal: reduces 45-line manual pattern to 3-line DSL call. Prevents 3 known error patterns (missing vendor, wrong readExpr, missing schema entry)."
        ],
        anti_examples: [
          "Adds value.",
          "Users will benefit.",
          "Good feature."
        ]
      },
      {
        id: "scope_artifact",
        label: "Scope Table Artifact",
        kind: "scope_metric",
        min_count: 1,
        weight: 0.20,
        examples: [
          "Table with columns: File | Project | New/Modify | Effort — one row per touched file."
        ],
        anti_examples: [
          "Scope seems manageable."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: {
        required_evidence_kinds: ["file_reference"],
        min_reasoning_length: 60
      },
      N: {
        required_evidence_kinds: ["scope_metric"],
        min_reasoning_length: 60
      },
      V: {
        required_evidence_kinds: ["scope_metric"],
        min_reasoning_length: 80
      },
      E: {
        required_evidence_kinds: ["scope_metric"],
        min_reasoning_length: 60
      },
      S: {
        required_evidence_kinds: ["scope_metric"],
        min_reasoning_length: 80
      },
      T: { skip: true }
    }
  },

  ui: {
    role: "ui",
    evidence_categories: [
      {
        id: "cshtml_snippet",
        label: ".cshtml Code Snippet",
        kind: "code_snippet",
        min_count: 2,
        weight: 0.30,
        examples: [
          "CORRECT: @{ Html.Field(plan, f => f.ColorCode, Html.FusionColorPickerFor(m => m.ColorCode).Render()); }",
          "WRONG: <input type='color' asp-for='ColorCode' /> — raw HTML, no framework builder, no validation slot.",
          "Plan element: <script type='application/json' data-alis-plan data-trace='trace'>@Html.Raw(plan.Render())</script>"
        ],
        anti_examples: [
          "Follow the builder pattern.",
          "Use Html.Field().",
          "Standard view code."
        ]
      },
      {
        id: "component_api",
        label: "Component API Reference",
        kind: "method_signature",
        min_count: 1,
        weight: 0.25,
        examples: [
          "Syncfusion EJ2 ColorPicker: el.ej2_instances[0].value returns hex string '#ff0000'. ReadExpr = 'value'.",
          "Telerik DatePicker: not applicable — we use Syncfusion exclusively.",
          "Native checkbox: el.checked returns boolean. ReadExpr = 'checked'."
        ],
        anti_examples: [
          "Standard component usage.",
          "Syncfusion handles this.",
          "Works as expected."
        ]
      },
      {
        id: "a11y",
        label: "Accessibility Check",
        kind: "code_snippet",
        min_count: 1,
        weight: 0.25,
        examples: [
          "Html.Field() wraps with <label> and aria-describedby for validation errors — ColorPicker needs a visible label for screen readers.",
          "Color-only indicator is not sufficient — must also show hex text value for colorblind users.",
          "Tab order: ColorPicker popup must be keyboard-navigable (Syncfusion EJ2 handles this natively)."
        ],
        anti_examples: [
          "Accessibility looks fine.",
          "Standard a11y.",
          "Should be accessible."
        ]
      },
      {
        id: "view_artifact",
        label: "Sandbox View Artifact",
        kind: "code_snippet",
        min_count: 1,
        weight: 0.20,
        examples: [
          "Produce the complete .cshtml sandbox page showing the component in context with plan, trigger, and reactive binding."
        ],
        anti_examples: [
          "Add a sandbox page."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: { skip: true },
      N: { skip: true },
      V: {
        required_evidence_kinds: ["code_snippet"],
        min_reasoning_length: 60
      },
      E: { skip: true },
      S: { skip: true },
      T: {
        required_evidence_kinds: ["code_snippet"],
        min_reasoning_length: 60
      }
    }
  },

  "human-proxy": {
    role: "human-proxy",
    evidence_categories: [
      {
        id: "hard_rule",
        label: "Hard Rule Citation",
        kind: "hard_rule_citation",
        min_count: 2,
        weight: 0.30,
        examples: [
          "Hard Rule #4: Builder constructors MUST be internal. Story's AC2 describes a public constructor — BLOCKED.",
          "Hard Rule #6: Vertical slice shape is inviolable — 7 files. Story lists only 5 files — missing events class and gather extension.",
          "Hard Rule #11: No tech debt. AC4 says 'temporarily use string lookup' — BLOCKED."
        ],
        anti_examples: [
          "Follows the rules.",
          "No rule violations.",
          "Looks compliant."
        ]
      },
      {
        id: "scaling_analysis",
        label: "Scales-to-100 Analysis",
        kind: "scaling_analysis",
        min_count: 1,
        weight: 0.25,
        examples: [
          "At 100 components, this pattern requires 100 case branches in execute.ts — violates O/C. Must use registry pattern instead.",
          "Pattern is additive: new component = new 7-file slice + zero changes to existing files. Scales correctly.",
          "This introduces a ComponentsMap lookup per render — at 100 components that's O(n) per page. Acceptable if map is a dictionary (O(1) amortized)."
        ],
        anti_examples: [
          "Should scale fine.",
          "No scaling concerns.",
          "Works for current needs."
        ]
      },
      {
        id: "quality_gate",
        label: "Quality Gate Commands",
        kind: "command_sequence",
        min_count: 1,
        weight: 0.25,
        examples: [
          "Pre-work: `npm test` (944 pass), `dotnet test UnitTests` (282 pass), `dotnet test PlaywrightTests` (483 pass). Post-work: all counts equal or higher.",
          "After implementing AC1-AC4: run `npm run build:all && dotnet build` then full suite. Zero regressions allowed."
        ],
        anti_examples: [
          "Run all tests.",
          "Tests should pass.",
          "Follow the test protocol."
        ]
      },
      {
        id: "override_artifact",
        label: "Pre-Work Verification Artifact",
        kind: "command_sequence",
        min_count: 1,
        weight: 0.20,
        examples: [
          "Produce the exact bash commands to verify the current test counts before starting work."
        ],
        anti_examples: [
          "Verify tests pass."
        ]
      }
    ],
    invest_evidence_overrides: {
      I: {
        required_evidence_kinds: ["hard_rule_citation"],
        min_reasoning_length: 60
      },
      N: {
        required_evidence_kinds: ["hard_rule_citation"],
        min_reasoning_length: 60
      },
      V: {
        required_evidence_kinds: ["scaling_analysis"],
        min_reasoning_length: 80
      },
      E: { skip: true },
      S: {
        required_evidence_kinds: ["hard_rule_citation", "scaling_analysis"],
        min_reasoning_length: 80
      },
      T: {
        required_evidence_kinds: ["command_sequence"],
        min_reasoning_length: 60
      }
    }
  }

};
```

---

## Deliverable 2: Round 1 Prompt Enhancement

### Enhanced Output Schema

Replace the current `OUTPUT_SCHEMA` with structured evidence fields.

```javascript
const ENHANCED_OUTPUT_SCHEMA = `OUTPUT FORMAT: You MUST respond with valid JSON matching this schema.
No markdown, no explanation outside the JSON object.

{
  "verdict": "approve" | "object" | "approve-with-notes",
  "confidence": "high" | "medium" | "low",
  "executive": "2-3 sentence summary of your review",

  "findings": [
    {
      "severity": "blocker" | "concern" | "observation",
      "title": "Short title",
      "text": "Detailed explanation (min 50 chars)",
      "evidence": {
        "citations": [
          {
            "kind": "<matches evidence_rubric kind>",
            "content": "The actual citation text — file path, method signature, test name, etc."
          }
        ]
      },
      "recommendation": "What to do about it (min 30 chars)"
    }
  ],

  "artifacts": [
    {
      "kind": "d2-diagram" | "csharp-signature" | "test-cases" | "scope-table" | "cshtml-snippet" | "command-sequence",
      "label": "Human-readable label",
      "content": "The artifact content"
    }
  ],

  "investScores": {
    "I": {
      "pass": true | false,
      "reasoning": "Why (min length from rubric, must contain required evidence kinds)",
      "citations": [
        { "kind": "<evidence kind>", "content": "specific citation" }
      ]
    },
    "N": { "pass": true | false, "reasoning": "...", "citations": [...] },
    "V": { "pass": true | false, "reasoning": "...", "citations": [...] },
    "E": { "pass": true | false, "reasoning": "...", "citations": [...] },
    "S": { "pass": true | false, "reasoning": "...", "citations": [...] },
    "T": { "pass": true | false, "reasoning": "...", "citations": [...] }
  },

  "selfAssessment": {
    "weakestFinding": "Which of my findings has the least evidence? Why?",
    "whatIMightBeMissing": "What would a different role see that I cannot?"
  }
}`;
```

### Enhanced Round 1 Prompt Assembly

```javascript
function assembleRound1Prompt(role, story, plan, relatedStories) {
  const roleConfig = ROLE_PROMPTS[role];
  const rubric = EVIDENCE_RUBRICS[role];
  if (!roleConfig) throw new Error(`Unknown role: ${role}`);
  if (!rubric) throw new Error(`No rubric for role: ${role}`);

  const goals = typeof plan.goals === 'string' ? JSON.parse(plan.goals) : plan.goals;
  const goalsList = goals.map((g, i) => `${i+1}. ${g.done ? '[DONE] ' : ''}${g.text}`).join('\n');

  const related = relatedStories
    .filter(s => s.id !== story.id)
    .map(s => `- ${s.id}: ${s.title} (${s.status})`)
    .join('\n');

  // Build the rubric section for the prompt
  const rubricSection = buildRubricPromptSection(rubric);

  // Build the INVEST evidence requirements section
  const investSection = buildInvestEvidenceSection(rubric);

  return `${PREAMBLE}

---

${roleConfig.prompt}

---

EVIDENCE RUBRIC — YOUR REVIEW WILL BE SCORED

Your review will be programmatically scored against this rubric. Low-evidence reviews
are flagged and your findings are discounted. Meet or exceed all minimums.

${rubricSection}

---

INVEST EVIDENCE REQUIREMENTS

For each INVEST criterion you evaluate, you must meet these evidence thresholds.
Criteria marked SKIP are outside your role — mark them with "skip": true.

${investSection}

---

REVIEW ASSIGNMENT

You are reviewing the following INVEST story from your role's perspective.

MASTER PLAN: ${plan.title}
GOALS:
${goalsList}

RELATED STORIES:
${related || '(none)'}

---

STORY TO REVIEW:

ID: ${story.id}
Title: ${story.title}
Size: ${story.size || 'not set'}
Status: ${story.status}

${story.body || '(no body)'}

---

${ENHANCED_OUTPUT_SCHEMA}

---

ANTI-RUBBER-STAMP PROTOCOL

Before writing your verdict, you MUST complete these steps IN ORDER:

1. LIST every file path or type name referenced in the story. If you cannot list at least 2, you have not read deeply enough.
2. For each AC, write ONE sentence about what would break if implemented wrong.
3. Identify ONE thing that no other agent role would catch — something only YOUR expertise reveals.
4. Check your evidence.citations arrays — if any finding has zero citations, DELETE that finding and re-examine.
5. Check your investScores.citations — if any non-skip criterion has zero citations, you are rubber-stamping.

Only THEN write your verdict.`;
}
```

### Rubric Prompt Section Builder

```javascript
function buildRubricPromptSection(rubric) {
  let section = '';
  for (const cat of rubric.evidence_categories) {
    section += `\n### ${cat.label} (min: ${cat.min_count}, weight: ${(cat.weight * 100).toFixed(0)}%)
GOOD evidence looks like:
${cat.examples.map(e => `  + ${e}`).join('\n')}
FAILS the rubric:
${cat.anti_examples.map(e => `  - ${e}`).join('\n')}
`;
  }
  return section;
}

function buildInvestEvidenceSection(rubric) {
  if (!rubric.invest_evidence_overrides) return '(Use default evidence standards for all INVEST criteria.)';

  let section = '';
  for (const [letter, override] of Object.entries(rubric.invest_evidence_overrides)) {
    if (override.skip) {
      section += `  ${letter}: SKIP (outside your role — mark "skip": true)\n`;
    } else {
      const kinds = override.required_evidence_kinds?.join(', ') || 'any';
      const minLen = override.min_reasoning_length || 50;
      section += `  ${letter}: Must include [${kinds}] citations. Min reasoning: ${minLen} chars.\n`;
    }
  }
  return section;
}
```

---

## Deliverable 3: Round 2 Challenge Prompt

### Design Principles

1. Each agent sees ALL round 1 reviews from other agents (not their own — they wrote it).
2. Conflicts are surfaced explicitly: "Agent X said PASS on I, you said FAIL — respond."
3. Each agent receives their round 1 evidence score and must improve it.
4. Agents can change their verdict — and must explain why if they do.

### Round 2 Prompt Template

```javascript
function assembleRound2Prompt(role, story, plan, relatedStories, round1Reviews, ownRound1Score) {
  const roleConfig = ROLE_PROMPTS[role];
  const rubric = EVIDENCE_RUBRICS[role];
  if (!roleConfig) throw new Error(`Unknown role: ${role}`);

  const goals = typeof plan.goals === 'string' ? JSON.parse(plan.goals) : plan.goals;
  const goalsList = goals.map((g, i) => `${i+1}. ${g.done ? '[DONE] ' : ''}${g.text}`).join('\n');

  const related = relatedStories
    .filter(s => s.id !== story.id)
    .map(s => `- ${s.id}: ${s.title} (${s.status})`)
    .join('\n');

  // Format other agents' reviews for cross-visibility
  const otherReviews = round1Reviews
    .filter(r => r.agent_role !== role)
    .map(r => formatReviewForChallenge(r));

  // Detect conflicts between this agent and others
  const conflicts = detectConflicts(role, round1Reviews);

  // Build rubric section (same as round 1 — standards don't lower)
  const rubricSection = buildRubricPromptSection(rubric);
  const investSection = buildInvestEvidenceSection(rubric);

  // Get this agent's own round 1 review
  const ownReview = round1Reviews.find(r => r.agent_role === role);
  const ownReviewJson = typeof ownReview?.review_json === 'string'
    ? JSON.parse(ownReview.review_json)
    : ownReview?.review_json;

  return `${PREAMBLE}

---

${roleConfig.prompt}

---

ROUND 2: CHALLENGE & STRENGTHEN

This is Round 2. You submitted a Round 1 review. You now see all other agents' reviews.
Your job is to:

1. RESPOND to every conflict where another agent disagrees with you.
2. STRENGTHEN your findings with better evidence (your Round 1 score: ${ownRound1Score}/100).
3. RETRACT any finding that another agent's evidence proves wrong.
4. ADOPT any finding from another agent that falls within your expertise and you missed.
5. You MAY change your verdict — but you must explain why with specific citations.

YOU MUST NOT:
- Repeat your Round 1 reasoning verbatim. If you cannot add new evidence, say so explicitly.
- Dismiss another agent's finding without citing counter-evidence.
- Rubber-stamp by saying "I agree with all other reviews."

---

YOUR ROUND 1 REVIEW (for reference):
Verdict: ${ownReviewJson?.verdict}
Confidence: ${ownReviewJson?.confidence}
Evidence Score: ${ownRound1Score}/100
${ownReviewJson?.findings?.map((f, i) => `  Finding ${i+1}: [${f.severity}] ${f.title}`).join('\n') || '(no findings)'}

---

OTHER AGENTS' ROUND 1 REVIEWS:

${otherReviews.join('\n\n---\n\n')}

---

CONFLICTS REQUIRING YOUR RESPONSE:

${conflicts.length > 0 ? conflicts.map(c => c.description).join('\n\n') : '(No direct conflicts detected — but check for subtle disagreements in INVEST scores.)'}

---

EVIDENCE RUBRIC (same as Round 1 — standards do not lower):

${rubricSection}

---

INVEST EVIDENCE REQUIREMENTS:

${investSection}

---

STORY UNDER REVIEW (for reference):

ID: ${story.id}
Title: ${story.title}
Size: ${story.size || 'not set'}

${story.body || '(no body)'}

---

MASTER PLAN: ${plan.title}
GOALS:
${goalsList}

RELATED STORIES:
${related || '(none)'}

---

ROUND 2 OUTPUT FORMAT:

{
  "verdict": "approve" | "object" | "approve-with-notes",
  "verdictChanged": true | false,
  "verdictChangeReason": "Why you changed (or null if unchanged)",
  "confidence": "high" | "medium" | "low",
  "executive": "2-3 sentence summary incorporating round 2 analysis",

  "conflictResponses": [
    {
      "conflictId": "<from conflicts list>",
      "otherAgent": "<role>",
      "otherPosition": "What they claimed",
      "myResponse": "agree" | "disagree" | "partially-agree",
      "evidence": {
        "citations": [
          { "kind": "<evidence kind>", "content": "counter-evidence or supporting evidence" }
        ]
      },
      "reasoning": "Why I agree/disagree, with specific citations (min 80 chars)"
    }
  ],

  "findings": [
    {
      "severity": "blocker" | "concern" | "observation",
      "title": "Short title",
      "text": "Detailed explanation",
      "source": "original" | "strengthened" | "retracted" | "adopted",
      "adoptedFrom": "<role, if adopted>",
      "evidence": {
        "citations": [
          { "kind": "<evidence kind>", "content": "citation" }
        ]
      },
      "recommendation": "What to do about it"
    }
  ],

  "retractions": [
    {
      "originalTitle": "Title of retracted finding",
      "reason": "Why this finding was wrong, citing other agent's evidence"
    }
  ],

  "artifacts": [
    {
      "kind": "d2-diagram" | "csharp-signature" | "test-cases" | "scope-table" | "cshtml-snippet" | "command-sequence",
      "label": "label",
      "content": "content"
    }
  ],

  "investScores": {
    "I": {
      "pass": true | false,
      "reasoning": "Updated reasoning incorporating round 2 evidence",
      "citations": [...],
      "changedFromRound1": true | false,
      "changeReason": "Why the score changed (if it did)"
    },
    "N": { ... },
    "V": { ... },
    "E": { ... },
    "S": { ... },
    "T": { ... }
  },

  "selfAssessment": {
    "evidenceImprovement": "What new evidence did I add in round 2?",
    "remainingWeakness": "What part of my review still has the weakest evidence?"
  }
}

---

ANTI-RUBBER-STAMP (Round 2 — STRICTER):

1. If you changed zero findings and adopted zero findings, justify why EVERY other agent's unique perspective added nothing to yours.
2. If your verdict is the same as round 1 with no new evidence, your confidence MUST be "low".
3. If another agent found a blocker you missed, you must either adopt it or provide counter-evidence — silence is not an option.
4. Your round 2 score must be >= your round 1 score (${ownRound1Score}). If you cannot improve, explain what you investigated and why it yielded no new evidence.`;
}
```

### Conflict Detection

```javascript
function detectConflicts(role, round1Reviews) {
  const ownReview = round1Reviews.find(r => r.agent_role === role);
  if (!ownReview) return [];

  const own = typeof ownReview.review_json === 'string'
    ? JSON.parse(ownReview.review_json) : ownReview.review_json;
  const conflicts = [];
  let conflictId = 0;

  for (const other of round1Reviews) {
    if (other.agent_role === role) continue;
    const otherJson = typeof other.review_json === 'string'
      ? JSON.parse(other.review_json) : other.review_json;

    // 1. Verdict conflict: one approves, other objects
    if (own.verdict === 'approve' && otherJson.verdict === 'object') {
      conflicts.push({
        id: `conflict-${++conflictId}`,
        type: 'verdict',
        description: `VERDICT CONFLICT [conflict-${conflictId}]: You (${role}) said APPROVE but ${other.agent_role} said OBJECT.\n` +
          `Their blockers: ${otherJson.findings?.filter(f => f.severity === 'blocker').map(f => f.title).join(', ') || '(none listed)'}\n` +
          `You must address each blocker: agree (adopt it) or disagree (cite counter-evidence).`
      });
    } else if (own.verdict === 'object' && otherJson.verdict === 'approve') {
      conflicts.push({
        id: `conflict-${++conflictId}`,
        type: 'verdict',
        description: `VERDICT CONFLICT [conflict-${conflictId}]: You (${role}) said OBJECT but ${other.agent_role} said APPROVE.\n` +
          `Your blockers: ${own.findings?.filter(f => f.severity === 'blocker').map(f => f.title).join(', ') || '(none listed)'}\n` +
          `Does ${other.agent_role}'s review provide evidence that your blockers are unfounded? Strengthen or retract.`
      });
    }

    // 2. INVEST score conflicts: same criterion, different pass/fail
    if (own.investScores && otherJson.investScores) {
      for (const letter of ['I', 'N', 'V', 'E', 'S', 'T']) {
        const ownScore = own.investScores[letter];
        const otherScore = otherJson.investScores[letter];
        if (!ownScore || !otherScore) continue;
        if (ownScore.skip || otherScore.skip) continue;
        if (ownScore.pass !== otherScore.pass) {
          conflicts.push({
            id: `conflict-${++conflictId}`,
            type: 'invest',
            description: `INVEST CONFLICT [conflict-${conflictId}]: ${letter} criterion — You said ${ownScore.pass ? 'PASS' : 'FAIL'}, ${other.agent_role} said ${otherScore.pass ? 'PASS' : 'FAIL'}.\n` +
              `Their reasoning: "${otherScore.reasoning?.slice(0, 200)}"\n` +
              `Your reasoning: "${ownScore.reasoning?.slice(0, 200)}"\n` +
              `Respond with citations supporting your position or change your score.`
          });
        }
      }
    }

    // 3. Blocker that no other agent mentions — isolation check
    const otherBlockers = otherJson.findings?.filter(f => f.severity === 'blocker') || [];
    for (const blocker of otherBlockers) {
      const ownFindings = own.findings || [];
      const ownMentions = ownFindings.some(f =>
        f.title?.toLowerCase().includes(blocker.title?.toLowerCase().split(' ')[0])
      );
      if (!ownMentions) {
        conflicts.push({
          id: `conflict-${++conflictId}`,
          type: 'unaddressed_blocker',
          description: `UNADDRESSED BLOCKER [conflict-${conflictId}]: ${other.agent_role} raised blocker "${blocker.title}" which you did not mention.\n` +
            `Their evidence: "${blocker.evidence?.citations?.[0]?.content || blocker.evidence || '(none)'}"\n` +
            `You must: (a) adopt this as your own finding, (b) explain why it's not relevant to your role, or (c) provide counter-evidence.`
        });
      }
    }
  }

  return conflicts;
}
```

### Review Formatter for Cross-Visibility

```javascript
function formatReviewForChallenge(reviewRow) {
  const json = typeof reviewRow.review_json === 'string'
    ? JSON.parse(reviewRow.review_json) : reviewRow.review_json;

  const findings = (json.findings || []).map((f, i) =>
    `  ${i+1}. [${f.severity}] ${f.title}\n     ${f.text?.slice(0, 200)}${f.text?.length > 200 ? '...' : ''}\n     Evidence: ${JSON.stringify(f.evidence?.citations?.[0] || f.evidence || '(none)')}`
  ).join('\n');

  const invest = Object.entries(json.investScores || {}).map(([k, v]) =>
    `  ${k}: ${v.skip ? 'SKIP' : (v.pass ? 'PASS' : 'FAIL')} — ${v.reasoning?.slice(0, 100) || '(no reasoning)'}`
  ).join('\n');

  return `AGENT: ${json.roleName || reviewRow.agent_role}
VERDICT: ${json.verdict} (confidence: ${json.confidence})
EXECUTIVE: ${json.executive || '(none)'}

FINDINGS:
${findings || '  (none)'}

INVEST SCORES:
${invest || '  (none)'}`;
}
```

---

## Deliverable 4: Evidence Scoring Algorithm

### Algorithm: 0-100 Score Computed From Review JSON + Rubric

The score has three components weighted 50/30/20:

```javascript
/**
 * Compute an evidence quality score (0-100) for a review against its role's rubric.
 *
 * Components:
 *   Category Evidence (50%): Did the agent provide enough evidence of each required kind?
 *   INVEST Evidence (30%):   Did INVEST reasoning meet the role-specific thresholds?
 *   Structural Quality (20%): Findings have citations, artifacts exist, no vague language.
 *
 * @param {object} reviewJson - The parsed review JSON from the agent
 * @param {object} rubric - The EVIDENCE_RUBRICS[role] object
 * @returns {{ score: number, breakdown: object, flags: string[] }}
 */
function scoreEvidence(reviewJson, rubric) {
  const flags = [];

  // ─── Component 1: Category Evidence (50%) ────────────────────
  let categoryScore = 0;
  const categoryBreakdown = {};

  for (const cat of rubric.evidence_categories) {
    // Count citations matching this category's kind across ALL findings + INVEST scores
    const matchingCitations = countCitationsOfKind(reviewJson, cat.kind);
    const ratio = Math.min(matchingCitations / cat.min_count, 1.0);
    const catScore = ratio * cat.weight;
    categoryScore += catScore;

    categoryBreakdown[cat.id] = {
      found: matchingCitations,
      required: cat.min_count,
      ratio,
      weightedScore: catScore,
      weight: cat.weight
    };

    if (matchingCitations === 0) {
      flags.push(`ZERO_EVIDENCE: No ${cat.label} citations found (required: ${cat.min_count})`);
    } else if (matchingCitations < cat.min_count) {
      flags.push(`LOW_EVIDENCE: Only ${matchingCitations}/${cat.min_count} ${cat.label} citations`);
    }
  }
  // categoryScore is 0..1, scale to 50
  const categoryPoints = categoryScore * 50;

  // ─── Component 2: INVEST Evidence (30%) ──────────────────────
  let investScore = 0;
  let investCriteria = 0;
  const investBreakdown = {};

  const investOverrides = rubric.invest_evidence_overrides || {};
  for (const letter of ['I', 'N', 'V', 'E', 'S', 'T']) {
    const override = investOverrides[letter];
    const agentScore = reviewJson.investScores?.[letter];

    // Skip if role doesn't evaluate this criterion
    if (override?.skip) {
      investBreakdown[letter] = { status: 'skip' };
      continue;
    }

    investCriteria++;
    let criterionScore = 0;

    if (!agentScore) {
      flags.push(`MISSING_INVEST: No score for ${letter}`);
      investBreakdown[letter] = { status: 'missing', score: 0 };
      continue;
    }

    // Sub-score 1: Reasoning length (40% of criterion)
    const minLen = override?.min_reasoning_length || 50;
    const reasoning = agentScore.reasoning || '';
    const lenRatio = Math.min(reasoning.length / minLen, 1.0);
    criterionScore += lenRatio * 0.4;

    if (reasoning.length < 20) {
      flags.push(`RUBBER_STAMP: ${letter} reasoning is ${reasoning.length} chars (min: ${minLen})`);
    }

    // Sub-score 2: Required evidence kinds present (40% of criterion)
    const requiredKinds = override?.required_evidence_kinds || [];
    if (requiredKinds.length > 0) {
      const citations = agentScore.citations || [];
      const kindsFound = new Set(citations.map(c => c.kind));
      const kindsPresent = requiredKinds.filter(k => kindsFound.has(k)).length;
      criterionScore += (kindsPresent / requiredKinds.length) * 0.4;

      if (kindsPresent < requiredKinds.length) {
        const missing = requiredKinds.filter(k => !kindsFound.has(k));
        flags.push(`MISSING_EVIDENCE_KIND: ${letter} missing [${missing.join(', ')}]`);
      }
    } else {
      // No specific kinds required — give full marks if reasoning exists
      criterionScore += (reasoning.length > 0 ? 1.0 : 0.0) * 0.4;
    }

    // Sub-score 3: Citations exist (20% of criterion)
    const citationCount = agentScore.citations?.length || 0;
    criterionScore += (citationCount > 0 ? 1.0 : 0.0) * 0.2;

    investBreakdown[letter] = {
      status: 'evaluated',
      score: criterionScore,
      reasoningLength: reasoning.length,
      minLength: minLen,
      citationCount
    };
    investScore += criterionScore;
  }

  const investPoints = investCriteria > 0
    ? (investScore / investCriteria) * 30
    : 30; // If all criteria are skipped, give full marks

  // ─── Component 3: Structural Quality (20%) ───────────────────
  let structuralScore = 0;
  const structuralBreakdown = {};

  // 3a. Findings with citations (8 points)
  const findings = reviewJson.findings || [];
  const findingsWithCitations = findings.filter(f =>
    f.evidence?.citations?.length > 0
  ).length;
  const findingCitationRatio = findings.length > 0
    ? findingsWithCitations / findings.length
    : 0;
  structuralScore += findingCitationRatio * 8;
  structuralBreakdown.findingCitations = {
    withCitations: findingsWithCitations,
    total: findings.length,
    points: findingCitationRatio * 8
  };

  if (findings.length > 0 && findingsWithCitations === 0) {
    flags.push('NO_FINDING_CITATIONS: Zero findings have structured citations');
  }

  // 3b. At least 1 artifact exists (4 points)
  const hasArtifact = (reviewJson.artifacts?.length || 0) > 0;
  structuralScore += hasArtifact ? 4 : 0;
  structuralBreakdown.hasArtifact = hasArtifact;
  if (!hasArtifact) {
    flags.push('NO_ARTIFACT: Review contains no artifacts');
  }

  // 3c. Self-assessment present and substantive (4 points)
  const selfAssess = reviewJson.selfAssessment;
  const hasSelfAssess = selfAssess
    && (selfAssess.weakestFinding?.length > 20 || selfAssess.evidenceImprovement?.length > 20)
    && (selfAssess.whatIMightBeMissing?.length > 20 || selfAssess.remainingWeakness?.length > 20);
  structuralScore += hasSelfAssess ? 4 : 0;
  structuralBreakdown.hasSelfAssessment = !!hasSelfAssess;

  // 3d. No vague language patterns (4 points — deduct for each vague phrase)
  const vaguePatterns = [
    /looks?\s+(fine|good|correct|ok)/i,
    /seems?\s+(fine|good|correct|reasonable)/i,
    /should\s+be\s+(fine|ok)/i,
    /no\s+(issues?|problems?|concerns?)\s*\.?$/i,
    /standard\s+(pattern|approach|usage)/i,
    /properly\s+(designed|typed|implemented)/i,
    /follows?\s+(best\s+)?practices/i,
  ];
  const allText = JSON.stringify(reviewJson);
  let vagueCount = 0;
  for (const pattern of vaguePatterns) {
    if (pattern.test(allText)) vagueCount++;
  }
  const vaguePenalty = Math.min(vagueCount * 1, 4);
  structuralScore += (4 - vaguePenalty);
  structuralBreakdown.vaguePatterns = { count: vagueCount, penalty: vaguePenalty };
  if (vagueCount > 0) {
    flags.push(`VAGUE_LANGUAGE: ${vagueCount} vague phrase(s) detected`);
  }

  const structuralPoints = structuralScore; // Already on 0-20 scale

  // ─── Final Score ─────────────────────────────────────────────
  const totalScore = Math.round(categoryPoints + investPoints + structuralPoints);

  return {
    score: Math.max(0, Math.min(100, totalScore)),
    breakdown: {
      categoryEvidence: { points: Math.round(categoryPoints), max: 50, detail: categoryBreakdown },
      investEvidence: { points: Math.round(investPoints), max: 30, detail: investBreakdown },
      structuralQuality: { points: Math.round(structuralPoints), max: 20, detail: structuralBreakdown },
    },
    flags
  };
}

/**
 * Count citations matching a specific kind across all findings and INVEST scores.
 */
function countCitationsOfKind(reviewJson, kind) {
  let count = 0;

  // Check findings citations
  for (const finding of (reviewJson.findings || [])) {
    for (const citation of (finding.evidence?.citations || [])) {
      if (citation.kind === kind) count++;
    }
  }

  // Check INVEST score citations
  for (const letter of ['I', 'N', 'V', 'E', 'S', 'T']) {
    const score = reviewJson.investScores?.[letter];
    for (const citation of (score?.citations || [])) {
      if (citation.kind === kind) count++;
    }
  }

  return count;
}
```

### Score Interpretation

| Score Range | Label | Action |
|-------------|-------|--------|
| 80-100 | Strong Evidence | Review is substantive. Findings carry full weight. |
| 60-79 | Adequate | Review is usable but has gaps. Findings carry weight with caveats. |
| 40-59 | Weak | Review lacks evidence. Findings are flagged. Agent MUST improve in Round 2. |
| 0-39 | Rubber Stamp | Review is discounted. Agent's verdict does not count toward consensus. |

### Consensus Algorithm Update

```javascript
/**
 * Compute consensus, weighting by evidence score.
 * Low-evidence reviews have reduced influence.
 */
function computeConsensus(reviews, scores) {
  let approveWeight = 0;
  let objectWeight = 0;
  let totalWeight = 0;
  const blockers = [];

  for (const review of reviews) {
    const score = scores[review.agent_role]?.score || 0;
    const json = typeof review.review_json === 'string'
      ? JSON.parse(review.review_json) : review.review_json;

    // Weight: score/100, but minimum 0.1 (even weak reviews get a voice)
    // Human-proxy always gets full weight (veto power)
    const weight = review.agent_role === 'human-proxy'
      ? 1.0
      : Math.max(score / 100, 0.1);

    totalWeight += weight;

    if (json.verdict === 'approve' || json.verdict === 'approve-with-notes') {
      approveWeight += weight;
    } else {
      objectWeight += weight;
      // Collect blockers from objecting reviews with adequate evidence
      if (score >= 40) {
        const reviewBlockers = (json.findings || [])
          .filter(f => f.severity === 'blocker')
          .map(f => ({ agent: review.agent_role, ...f }));
        blockers.push(...reviewBlockers);
      }
    }
  }

  return {
    approveRatio: totalWeight > 0 ? approveWeight / totalWeight : 0,
    objectRatio: totalWeight > 0 ? objectWeight / totalWeight : 0,
    weightedVerdicts: reviews.map(r => ({
      agent: r.agent_role,
      verdict: (typeof r.review_json === 'string' ? JSON.parse(r.review_json) : r.review_json).verdict,
      evidenceScore: scores[r.agent_role]?.score || 0,
      weight: r.agent_role === 'human-proxy' ? 1.0 : Math.max((scores[r.agent_role]?.score || 0) / 100, 0.1)
    })),
    blockers,
    recommendation: objectWeight > approveWeight ? 'revise-and-resubmit' : 'approve-with-conditions'
  };
}
```

---

## Integration: Updated Dispatch Orchestrator

### Two-Round Flow

```javascript
export async function dispatchTwoRoundReview(storyId, onProgress) {
  if (_reviewsInProgress.has(storyId)) {
    throw new Error(`Review already in progress for story ${storyId}`);
  }
  _reviewsInProgress.add(storyId);

  try {
    const story = getStory(storyId);
    if (!story) throw new Error(`Story not found: ${storyId}`);
    const plan = getPlan(story.plan_id);
    if (!plan) throw new Error(`Plan not found: ${story.plan_id}`);
    const relatedStories = getStoriesByPlan(story.plan_id);

    // ── Round 1: Independent Review ──────────────────────────
    onProgress?.('system', 'round1-started', { round: 1 });

    const round1Results = await dispatchRound(1, story, plan, relatedStories, onProgress);

    // Score all round 1 reviews
    const round1Scores = {};
    for (const role of ALL_ROLES) {
      const review = round1Results.find(r => r.role === role);
      if (review?.json) {
        round1Scores[role] = scoreEvidence(review.json, EVIDENCE_RUBRICS[role]);
        onProgress?.(role, 'scored', {
          round: 1,
          score: round1Scores[role].score,
          flags: round1Scores[role].flags
        });
      }
    }

    // ── Decision: Need Round 2? ──────────────────────────────
    const needsRound2 = shouldTriggerRound2(round1Results, round1Scores);
    if (!needsRound2.trigger) {
      onProgress?.('system', 'consensus-reached', { round: 1, reason: needsRound2.reason });
      return { rounds: 1, results: round1Results, scores: round1Scores };
    }

    onProgress?.('system', 'round2-triggered', { reason: needsRound2.reason });

    // ── Round 2: Challenge ───────────────────────────────────
    const round1ReviewRows = getReviews(storyId, 1);

    const round2Results = [];
    const promises = ALL_ROLES.map(async role => {
      onProgress?.(role, 'round2-started', { round: 2 });
      try {
        const prompt = assembleRound2Prompt(
          role, story, plan, relatedStories,
          round1ReviewRows,
          round1Scores[role]?.score || 0
        );
        const review = await dispatchAgent(role, prompt);

        createReview({
          storyId,
          agentRole: role,
          round: 2,
          verdict: review.verdict,
          confidence: review.confidence || 'medium',
          reviewJson: { roleName: ROLE_PROMPTS[role].roleName, ...review },
        });

        round2Results.push({ role, json: review });
        onProgress?.(role, 'round2-completed', { verdict: review.verdict });
      } catch (err) {
        round2Results.push({ role, error: err.message });
        onProgress?.(role, 'round2-failed', { error: err.message });
      }
    });

    await Promise.all(promises);

    // Score round 2
    const round2Scores = {};
    for (const result of round2Results) {
      if (result.json) {
        round2Scores[result.role] = scoreEvidence(result.json, EVIDENCE_RUBRICS[result.role]);
      }
    }

    return {
      rounds: 2,
      round1: { results: round1Results, scores: round1Scores },
      round2: { results: round2Results, scores: round2Scores },
    };
  } finally {
    _reviewsInProgress.delete(storyId);
  }
}

/**
 * Determine if round 2 is needed.
 * Triggers: verdict conflicts, low evidence scores, INVEST disagreements.
 */
function shouldTriggerRound2(round1Results, round1Scores) {
  const verdicts = round1Results
    .filter(r => r.json)
    .map(r => r.json.verdict);

  // Trigger 1: Any "object" verdict
  if (verdicts.includes('object')) {
    return { trigger: true, reason: 'One or more agents objected' };
  }

  // Trigger 2: Any evidence score below 40 (rubber stamp)
  const rubberStamps = Object.entries(round1Scores)
    .filter(([_, s]) => s.score < 40)
    .map(([role, _]) => role);
  if (rubberStamps.length > 0) {
    return { trigger: true, reason: `Rubber-stamp detected: ${rubberStamps.join(', ')}` };
  }

  // Trigger 3: INVEST score disagreements (any criterion where agents split)
  // (agents with skip are excluded)
  for (const letter of ['I', 'N', 'V', 'E', 'S', 'T']) {
    const scores = round1Results
      .filter(r => r.json?.investScores?.[letter] && !r.json.investScores[letter].skip)
      .map(r => r.json.investScores[letter].pass);
    if (scores.includes(true) && scores.includes(false)) {
      return { trigger: true, reason: `INVEST disagreement on ${letter}` };
    }
  }

  // Trigger 4: Average evidence score below 60
  const avgScore = Object.values(round1Scores).reduce((sum, s) => sum + s.score, 0) / Object.values(round1Scores).length;
  if (avgScore < 60) {
    return { trigger: true, reason: `Average evidence score ${avgScore.toFixed(0)} < 60` };
  }

  return { trigger: false, reason: 'Unanimous approval with adequate evidence' };
}

async function dispatchRound(round, story, plan, relatedStories, onProgress) {
  const results = [];
  const promises = ALL_ROLES.map(async role => {
    const roleConfig = ROLE_PROMPTS[role];
    onProgress?.(role, 'started', { round, roleName: roleConfig.roleName });

    try {
      const prompt = assembleRound1Prompt(role, story, plan, relatedStories);
      const review = await dispatchAgent(role, prompt);

      if (!review.verdict || !review.findings || !review.artifacts) {
        throw new Error('Missing required fields');
      }

      createReview({
        storyId: story.id,
        agentRole: role,
        round,
        verdict: review.verdict,
        confidence: review.confidence || 'medium',
        reviewJson: { roleName: roleConfig.roleName, ...review },
      });

      results.push({ role, json: review });
      onProgress?.(role, 'completed', { round, verdict: review.verdict, roleName: roleConfig.roleName });
    } catch (err) {
      results.push({ role, error: err.message });
      onProgress?.(role, 'failed', { round, error: err.message, roleName: roleConfig.roleName });
    }
  });

  await Promise.all(promises);
  return results;
}
```

---

## DB Schema Addition

Add an `evidence_scores` table to persist scores across rounds:

```sql
CREATE TABLE IF NOT EXISTS evidence_scores (
    id TEXT PRIMARY KEY,
    review_id TEXT NOT NULL REFERENCES reviews(id) ON DELETE CASCADE,
    score INTEGER NOT NULL CHECK (score BETWEEN 0 AND 100),
    category_points INTEGER NOT NULL,
    invest_points INTEGER NOT NULL,
    structural_points INTEGER NOT NULL,
    flags TEXT NOT NULL DEFAULT '[]',
    breakdown_json TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    UNIQUE (review_id)
);
```

---

## Summary of Changes

| File | What Changes |
|------|-------------|
| `tools/md-viewer/agents.mjs` | Add `EVIDENCE_RUBRICS`, replace `OUTPUT_SCHEMA` with `ENHANCED_OUTPUT_SCHEMA`, add `assembleRound1Prompt`, `assembleRound2Prompt`, `scoreEvidence`, `detectConflicts`, `dispatchTwoRoundReview`, `shouldTriggerRound2` |
| `tools/md-viewer/db.mjs` | Add `evidence_scores` table to schema, add `createEvidenceScore` / `getEvidenceScores` query helpers |
| `tools/md-viewer/server.mjs` | Update dispatch endpoint to use `dispatchTwoRoundReview`, add `/api/story/:id/scores` endpoint |
| `tools/md-viewer/public/app.js` | Update review panel to show evidence scores, flag badges, round 2 conflict responses |

### Token Budget Estimate

| Prompt Section | Round 1 | Round 2 |
|---------------|---------|---------|
| Preamble | ~350 | ~350 |
| Role prompt | ~200 | ~200 |
| Evidence rubric | ~600 | ~600 |
| INVEST evidence section | ~150 | ~150 |
| Story content | ~500-2000 | ~500-2000 |
| Other reviews | 0 | ~2000-4000 |
| Conflict descriptions | 0 | ~300-800 |
| Own round 1 summary | 0 | ~200 |
| Output schema | ~400 | ~600 |
| Anti-rubber-stamp | ~150 | ~200 |
| **Total** | **~2350-3850** | **~5100-9100** |

Round 2 prompts are 2-3x larger due to cross-visibility. This is intentional — evidence
depth requires context. At ~9000 tokens maximum, this is well within Claude's input capacity.
