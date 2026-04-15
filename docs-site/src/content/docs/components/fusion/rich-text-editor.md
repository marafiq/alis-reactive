---
title: FusionRichTextEditor
description: WYSIWYG rich text editor for long-form notes.
sidebar:
  order: 13
---

A WYSIWYG rich text editor for long-form notes -- care plan narratives, incident descriptions, admissions letters. The value is HTML, not plain text, so store it in a field that accepts markup.

**Model type:** `string` (HTML) &nbsp; **ReadExpr:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one?

```csharp
Html.InputField(plan, m => m.CarePlan, o => o.Label("Care Plan"))
    .FusionRichTextEditor(b => b);
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string)` | Sets the HTML content |
| `FocusIn()` | Moves focus into the editor |
