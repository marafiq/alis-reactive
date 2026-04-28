# Syncfusion Tailwind3 Theme Tokens

Reference for customizing Syncfusion EJ2 components via CSS variables.
Complements `syncfusion.overrides.css` and `syncfusion.entry.css` in this folder.

## Source of truth

The full token definition lives in the installed npm package:

```
node_modules/@syncfusion/ej2-base/styles/definition/_tailwind3.scss   (2231 lines, human-readable, grouped)
node_modules/@syncfusion/ej2/tailwind3.css                             (compiled, shipped to browser)
```

- 415 custom properties declared at `:root`
- 401 are `--color-sf-*` (semantic color roles)
- Remainder: `--e-font-*`, `--e-radius`, `--e-border`, a few animation locals

Read the SCSS when you need to understand intent; grep the compiled CSS when you need to know the
exact value or selector that consumes a token.

## Naming pattern

```
--color-sf-<scope>-<role>[-<state>]
```

- **Scope**: `content`, `flyout`, `overlay`, `text-input`, `table`, `treeview`, component names
  (`chip-*`, `appbar-*`, `breadcrumb-*`, `badge-*`), or globals (`primary`, `danger`, `border`)
- **Role**: `bg-color`, `text-color`, `border-color`, `icon-color`
- **State**: `hover`, `pressed`, `focus`, `selected`, `dragged`, `disabled`, `light`, `dark`

Examples:
```
--color-sf-border                        global default border
--color-sf-border-focus                  same, focus state
--color-sf-content-bg-color              main content surface
--color-sf-content-bg-color-alt1…alt5    5 surface shades
--color-sf-content-text-color-alt1…alt4  4 text shades
--color-sf-chip-danger-bg-color-hover    specific component state
```

## Value formats — two conventions coexist

Most tokens are **hex**:
```
--color-sf-border: #d1d5db;
--color-sf-danger: #dc2626;
```

A few are **raw RGB triplets** (no `rgb()` wrapper). These are consumed inside `rgba()` calls so
Syncfusion can apply an alpha channel: `color: rgba(var(--color-sf-primary), 0.5)`.

```
--color-sf-primary: 122, 46, 59;     correct — wrapped by SF internally
--color-sf-black:   0, 0, 0;
--color-sf-white:   255, 255, 255;
```

**Rule of thumb**: if Syncfusion wraps the token in `rgba(var(--<name>))`, use the raw triplet
format. Otherwise use hex. Confirmed tokens that require the triplet format:
`--color-sf-primary`, `--color-sf-black`, `--color-sf-white`. Every other high-leverage token is
hex.

Mixing formats silently fails — a hex value inside `rgba(...)` is an invalid CSS color and the
browser drops the rule.

## High-leverage tokens (the ~10% you'll reach for most)

These cover the majority of brand customization. Full defaults are populated in
`syncfusion.overrides.css` — edit there, not here. This list is for discoverability.

### Brand
| Token | Default (Tailwind3 indigo) |
|---|---|
| `--color-sf-primary` | `79, 70, 229` (triplet) |
| `--color-sf-primary-text-color` | `#fff` |
| `--color-sf-primary-light` | `#818cf8` |
| `--color-sf-primary-lighter` | `#e0e7ff` |
| `--color-sf-primary-dark` | `#4338ca` |
| `--color-sf-primary-darker` | `#3730a3` |

### Primary button (separate from brand — SF distinguishes them)
| Token | Default |
|---|---|
| `--color-sf-primary-bg-color` | `#4f46e5` |
| `--color-sf-primary-bg-color-hover` | `#4338ca` |
| `--color-sf-primary-bg-color-pressed` | `#3730a3` |
| `--color-sf-primary-bg-color-focus` | `#4338ca` |
| `--color-sf-primary-bg-color-disabled` | `#a5b4fc` |
| `--color-sf-primary-border-color` | `#4f46e5` |
| `--color-sf-primary-text` | `#fff` |

### Status
| Token | Default |
|---|---|
| `--color-sf-success` / `-light` / `-lighter` / `-dark` | `#15803d` / `#dcfce7` / `#f0fdf4` / `#166534` |
| `--color-sf-info` / `-light` / `-lighter` / `-dark` | `#0e7490` / `#cffafe` / `#ecfeff` / `#155e75` |
| `--color-sf-warning` / `-light` / `-lighter` / `-dark` | `#c2410c` / `#ffedd5` / `#fff7fd` / `#9a3412` |
| `--color-sf-danger` / `-light` / `-lighter` / `-dark` | `#dc2626` / `#fee2e2` / `#fef2f2` / `#b91c1c` |

### Content surfaces (page/card backgrounds, 5 levels)
| Token | Default |
|---|---|
| `--color-sf-content-bg-color` | `rgba(255, 255, 255)` |
| `--color-sf-content-bg-color-alt1` | `#f9fafb` |
| `--color-sf-content-bg-color-alt2` | `#f3f4f6` |
| `--color-sf-content-bg-color-alt3` | `#e5e7eb` |
| `--color-sf-content-bg-color-alt4` | `#9ca3af` |
| `--color-sf-content-bg-color-alt5` | `#6b7280` |
| `--color-sf-content-bg-color-disabled` | `#ffffff` |

### Content text (4 hierarchy levels)
| Token | Default |
|---|---|
| `--color-sf-content-text-color` | `#111827` |
| `--color-sf-content-text-color-alt1` | `#374151` |
| `--color-sf-content-text-color-alt2` | `#4b5563` |
| `--color-sf-content-text-color-alt3` | `#6b7280` |
| `--color-sf-content-text-color-alt4` | `#9ca3af` |
| `--color-sf-content-text-color-disabled` | `#9ca3af` |
| `--color-sf-placeholder-text-color` | `#6b7280` |

### Borders
| Token | Default |
|---|---|
| `--color-sf-border-light` | `#e5e7eb` |
| `--color-sf-border` | `#d1d5db` |
| `--color-sf-border-dark` | `#9ca3af` |
| `--color-sf-border-hover` | `#d1d5db` |
| `--color-sf-border-focus` | `#d1d5db` |
| `--color-sf-border-disabled` | `#e5e7eb` |
| `--color-sf-border-error` | `#dc2626` |
| `--color-sf-border-warning` | `#c2410c` |
| `--color-sf-border-success` | `#15803d` |

### Input surfaces
| Token | Default |
|---|---|
| `--color-sf-text-input-bg-color` | `#ffffff` |
| `--color-sf-flyout-bg-color` | `#ffffff` |
| `--color-sf-overlay-bg-color` | `rgba(107, 114, 128, .75)` |

### Typography & geometry
| Token | Default |
|---|---|
| `--e-font-name` | `'Inter', system-ui, -apple-system, 'Segoe UI', Roboto, …` |
| `--e-font-family` | `var(--e-font-name), var(--e-font-sans)` |
| `--e-radius` | `1rem` |
| `--e-border` | `1rem` |

## Workflow

1. Open `_tailwind3.scss` in your editor. Search for the visual element (e.g. "chip", "grid",
   "scheduler").
2. Note the token name + default value.
3. Redeclare it at `:root` in `syncfusion.overrides.css`. Match the value format of the default
   (hex → hex, triplet → triplet).
4. Only drop to selector-level `.e-*` overrides (which `syncfusion.overrides.css` already has many
   of) when no token covers the property you need to change (geometry, specific states).

## Finding what uses a token

```bash
grep -oE '[^;{]*var\(--color-sf-<name>\)[^;}]*' node_modules/@syncfusion/ej2/tailwind3.css
```

Shows every CSS property + selector context that reads the token. Use this before overriding to
verify which visual elements your change will affect.
