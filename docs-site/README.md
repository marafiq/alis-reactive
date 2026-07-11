# Alis.Reactive Docs Site

This is the public documentation site for Alis.Reactive.

It uses Astro Starlight and keeps the public docs compact. The site teaches the
Reactive Plan mental model instead of publishing one page per component.

## Commands

Run these from `docs-site/`:

| Command | Action |
| :-- | :-- |
| `npm ci` | Installs pinned dependencies |
| `npm run dev` | Starts the local site at `localhost:4321` |
| `npm run build` | Builds the production site to `./dist/` |
| `npm run preview` | Previews the built site locally |

## Content Rule

Every code sample must map to the current public C# DSL. Read the source before
documenting a primitive.
