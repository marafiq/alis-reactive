---
title: FusionInputMask
description: Format-enforced text input for phone numbers, SSNs, zip codes.
sidebar:
  order: 12
---

A masked text input that enforces a format character by character. Use it for phone numbers, SSNs, zip codes, medical-record IDs -- any field where the shape of the data is constant and you want the field to guide the typist.

**Model type:** `string` &nbsp; **ReadExpr:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one?

The `Mask` pattern uses Syncfusion's mask literals: `0` for required digit, `9` for optional digit, `L` for required letter. See [Syncfusion's Input Mask docs](https://ej2.syncfusion.com/documentation/maskedtextbox/mask-configuration) for the full grammar.

```csharp
Html.InputField(plan, m => m.PhoneNumber, o => o.Label("Phone Number"))
    .FusionInputMask(b => b
        .Mask("(000) 000-0000"));
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string)` | Sets the unmasked value |
| `FocusIn()` | Moves focus into the input |
| `Value()` | Reads the current unmasked value as a typed source (`TypedComponentSource<string>`) for conditions and gather |
