# The sandbox's Playwright project, as scripts/playwright.sh reads it: its name, its CI shards, the
# browser bundles the sandbox serves, and the sources its test binaries are built from beside the core.
# shellcheck shell=bash

playwright_project=sandbox

# The CI shard partition (docs/CI.md, "Sharding"). VSTest filters have no wildcards, so Fusion
# component namespaces are split by their first letter: `~.Components.Fusion.A` is a contains
# match on the namespace segment, and every letter is listed so a new component always lands in
# exactly one shard. The last shard is the complement of the others, so a new top-level namespace
# can never fall outside every shard.
playwright_shards="fusion-a-f fusion-grid fusion-h-r fusion-s-z http-core-and-patterns components validation conditions-and-rest"

fusion_letters() {
  local expr="" letter
  for letter in "$@"; do
    expr="${expr:+$expr|}FullyQualifiedName~.Components.Fusion.$letter"
  done
  printf '%s' "$expr"
}

playwright_shard_filter() {
  case "$1" in
    fusion-a-f)
      printf '(%s)' "$(fusion_letters A B C D E F)"
      ;;
    fusion-grid)
      # G holds Grid alone, the largest component suite.
      printf '(%s)' "$(fusion_letters G)"
      ;;
    fusion-h-r)
      printf '(%s)' "$(fusion_letters H I J K L M N O P Q R)"
      ;;
    fusion-s-z)
      printf '(%s)' "$(fusion_letters S T U V W X Y Z)"
      ;;
    http-core-and-patterns)
      printf 'FullyQualifiedName~.HttpPipeline.|FullyQualifiedName~.CoreBehaviors.|FullyQualifiedName~.Patterns.'
      ;;
    components)
      printf 'FullyQualifiedName~.Components.&FullyQualifiedName!~.Components.Fusion.&FullyQualifiedName!~.Validation.'
      ;;
    validation)
      printf 'FullyQualifiedName~.Validation.'
      ;;
    conditions-and-rest)
      printf 'FullyQualifiedName!~.Components.&FullyQualifiedName!~.Validation.&FullyQualifiedName!~.CoreBehaviors.&FullyQualifiedName!~.HttpPipeline.&FullyQualifiedName!~.Patterns.'
      ;;
    *)
      return 2
      ;;
  esac
}

playwright_check_assets() {
  check_asset_output_is_fresh \
    "design-system CSS" \
    "Alis.Reactive.Assets/dist/css/design-system.dev.css" \
    Alis.Reactive.Assets/design-system \
    Alis.Reactive.Assets/vite.design-system.config.ts \
    Alis.Reactive.Assets/package.json

  check_asset_output_is_fresh \
    "design-system Fusion CSS" \
    "Alis.Reactive.Assets/dist/css/design-system-fusion.dev.css" \
    Alis.Reactive.Assets/design-system \
    Alis.Reactive.Assets/vite.design-system.config.ts \
    Alis.Reactive.Assets/package.json

  check_asset_output_is_fresh \
    "Syncfusion CSS" \
    "Alis.Reactive.Assets/dist/css/syncfusion.dev.css" \
    Alis.Reactive.Assets/fusion \
    Alis.Reactive.Assets/vite.fusion.config.ts \
    Alis.Reactive.Assets/package.json

  check_asset_output_is_fresh \
    "sandbox plugin bundle" \
    "Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js" \
    Alis.Reactive.SandboxApp/Scripts \
    Alis.Reactive.SandboxApp/esbuild.config.mjs \
    Alis.Reactive.SandboxApp/package.json

  check_asset_output_is_fresh \
    "sandbox components bundle" \
    "Alis.Reactive.SandboxApp/wwwroot/js/sandbox-components.js" \
    Alis.Reactive.SandboxApp/Scripts \
    Alis.Reactive.SandboxApp/esbuild.config.mjs \
    Alis.Reactive.SandboxApp/package.json

  check_asset_output_is_fresh \
    "sandbox CSS" \
    "Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css" \
    Alis.Reactive.SandboxApp/Styles \
    Alis.Reactive.SandboxApp/build-css.mjs \
    Alis.Reactive.SandboxApp/vite.config.ts \
    Alis.Reactive.SandboxApp/package.json
}

playwright_sources() {
  printf '%s\n' \
    Alis.Reactive.DesignSystem \
    Alis.Reactive.Fusion \
    Alis.Reactive.Native \
    Alis.Reactive.NativeTagHelpers \
    Alis.Reactive.SandboxApp
}
