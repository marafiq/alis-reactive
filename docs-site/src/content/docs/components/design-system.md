---
title: "Design System Tag Helpers"
description: "Semantic HTML tag helpers that enforce a consistent, token-driven design system — layout, typography, spacing, and cards without writing CSS classes."
---

Alis.Reactive ships 13 tag helpers that replace raw `<div>` soup with semantic layout primitives. Instead of memorizing Tailwind classes, you write `<native-vstack gap="Lg">` and the tag helper renders the correct responsive CSS.

**Why tag helpers instead of CSS classes?** Classes drift. One developer writes `gap-4`, another writes `gap-6`, a third writes `space-y-4`. Tag helpers enforce the spacing scale, color tokens, and layout patterns at compile time. The design system becomes a contract, not a suggestion.

**net10.0 only.** Tag helpers are an ASP.NET Core feature — they are not available on .NET Framework 4.8.

## Setup

Add the NuGet package and register the tag helpers:

```cshtml title="_ViewImports.cshtml"
@addTagHelper *, Alis.Reactive.NativeTagHelpers
```

## Layout

### How do you stack content vertically?

```html
<native-vstack gap="Lg">
    <native-heading level="H1">Page Title</native-heading>
    <native-text color="Secondary">Subtitle text here.</native-text>
    <native-card>...</native-card>
</native-vstack>
```

`native-vstack` renders a flex column with consistent spacing. The `gap` attribute uses the framework's spacing scale — not arbitrary pixel values.

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `gap` | SpacingScale | `Base` | `None`, `Xs`, `Sm`, `Base`, `Md`, `Lg`, `Xl`, `Xxl`, `Max` |
| `divide-y` | bool | `false` | Adds horizontal divider lines between items |

### How do you arrange items horizontally?

```html
<native-hstack gap="Sm" align="Center" justify="Between">
    <native-text bold>Total</native-text>
    <native-text color="Accent">$4,500.00</native-text>
</native-hstack>
```

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `gap` | SpacingScale | `Base` | `None` through `Max` |
| `align` | AlignItems | `Center` | `Start`, `Center`, `End`, `Stretch`, `Baseline` |
| `justify` | JustifyContent | `Start` | `Start`, `Center`, `End`, `Between`, `Around`, `Evenly` |
| `wrap` | bool | `false` | Allow items to wrap to the next line |

### How do you create a responsive grid?

```html
<native-grid cols="C3" gap="Md">
    <native-card>...</native-card>
    <native-card>...</native-card>
    <native-card>...</native-card>
</native-grid>
```

Responsive by default — 3 columns on desktop, 2 on tablet, 1 on mobile.

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `cols` | GridCols | `C2` | `C1`, `C2`, `C3`, `C4`, `C5`, `C6` |
| `gap` | SpacingScale | `Md` | `None` through `Max` |
| `responsive` | bool | `true` | Set `false` for fixed columns |

### How do you constrain page width?

```html
<native-container>
    <!-- content centered with max-width and responsive padding -->
</native-container>
```

Renders `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8` — a standard centered container. No attributes needed.

## Typography

### Headings

```html
<native-heading level="H1">Resident Intake</native-heading>
<native-heading level="H3">Personal Information</native-heading>
<native-heading level="H6">SECTION LABEL</native-heading>
```

Each level renders the correct semantic HTML (`<h1>` through `<h6>`) with design-system typography classes.

| Level | Font size | Weight | Tracking |
|-------|-----------|--------|----------|
| H1 | `text-3xl` | `font-extrabold` | `tracking-tight` |
| H2 | `text-xl` | `font-semibold` | `tracking-tight` |
| H3 | `text-lg` | `font-semibold` | `tracking-tight` |
| H4 | `text-lg` | `font-medium` | — |
| H5 | `text-base` | `font-medium` | — |
| H6 | `text-sm` | `font-medium` | `uppercase tracking-wide` |

Optional overline label:

```html
<native-heading level="H2" overline="Step 1 of 3">Personal Information</native-heading>
```

### Text

```html
<native-text>Default body text.</native-text>
<native-text color="Secondary" size="Sm">Muted helper text.</native-text>
<native-text color="Error" bold>Validation failed.</native-text>
<native-text as-span color="Accent">inline accent</native-text>
```

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `size` | TextSize | `Base` | `Xs`, `Sm`, `Base`, `Lg`, `Xl` |
| `color` | TextColor | `Primary` | `Primary`, `Secondary`, `Muted`, `Inverse`, `Accent`, `Success`, `Warning`, `Error`, `Inherit` |
| `bold` | bool | `false` | Applies `font-semibold` |
| `as-span` | bool | `false` | Renders `<span>` instead of `<p>` |

## Cards

```html
<native-card elevation="Medium" accent="Success">
    <native-card-header divider="Header">
        <native-heading level="H3">Intake Complete</native-heading>
    </native-card-header>
    <native-card-body>
        <native-text>All fields validated.</native-text>
    </native-card-body>
    <native-card-footer divider="Footer">
        <native-hstack justify="End" gap="Sm">
            <button>Cancel</button>
            <button>Save</button>
        </native-hstack>
    </native-card-footer>
</native-card>
```

### Card

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `elevation` | CardElevation | `Low` | `Flat`, `Low`, `Medium`, `High` |
| `accent` | AccentColor | — | `Primary`, `Secondary`, `Success`, `Warning`, `Error`, `Info`, `Muted` |

### Card sections

| Tag | Attribute | Options |
|-----|-----------|---------|
| `native-card-header` | `divider` | `None`, `Header`, `Footer`, `Both` |
| `native-card-body` | `padding` | `None`, `Compact`, `Standard` |
| `native-card-footer` | `divider` | `None`, `Header`, `Footer`, `Both` |

## Data display

### Key-value pairs

```html
<native-kv label="Facility" value="Sunrise Senior Living" />
<native-kv label="Monthly Rate" value="$4,500" layout="Inline" />
```

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `label` | string | — | The key text |
| `value` | string | — | The value text |
| `layout` | KvLayout | `Stacked` | `Stacked`, `Inline` |

### Dividers

```html
<native-divider />
<native-divider style="Dashed" />
<native-divider label="OR" />
```

| Attribute | Type | Default | Options |
|-----------|------|---------|---------|
| `style` | DividerStyle | `Plain` | `Plain`, `Dashed` |
| `label` | string | — | Optional centered text label |

## Spacing scale

All `gap` attributes share the same spacing scale:

| Token | Tailwind | Pixels |
|-------|----------|--------|
| `None` | `gap-0` | 0 |
| `Xs` | `gap-1` | 4px |
| `Sm` | `gap-2` | 8px |
| `Base` | `gap-4` | 16px |
| `Md` | `gap-6` | 24px |
| `Lg` | `gap-8` | 32px |
| `Xl` | `gap-10` | 40px |
| `Xxl` | `gap-12` | 48px |
| `Max` | `gap-16` | 64px |

## A complete page

```html
<native-container>
    <native-vstack gap="Lg">
        <native-heading level="H1">Resident Intake</native-heading>
        <native-text color="Secondary">
            Complete all sections below.
        </native-text>

        <native-card>
            <native-card-body>
                <native-vstack gap="Base">
                    <native-heading level="H3">Personal Information</native-heading>
                    <native-grid cols="C2" gap="Md">
                        @{ Html.InputField(plan, m => m.FirstName,
                            o => o.Required().Label("First Name"))
                            .NativeTextBox(b => b.Placeholder("Jane")); }
                        @{ Html.InputField(plan, m => m.LastName,
                            o => o.Required().Label("Last Name"))
                            .NativeTextBox(b => b.Placeholder("Doe")); }
                    </native-grid>
                </native-vstack>
            </native-card-body>
        </native-card>

        <native-card accent="Info">
            <native-card-body padding="Compact">
                <native-hstack gap="Sm" align="Center">
                    <native-text as-span color="Accent" bold>Tip:</native-text>
                    <native-text as-span color="Secondary">
                        All dates use the facility's local timezone.
                    </native-text>
                </native-hstack>
            </native-card-body>
        </native-card>
    </native-vstack>
</native-container>
```

No CSS classes in sight. The tag helpers enforce the spacing scale, typography, and card structure. The design system is the contract.
