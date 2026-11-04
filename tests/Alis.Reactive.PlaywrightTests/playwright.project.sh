playwright_project=sandbox

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
    "Alis.Reactive.DesignSystem/dist/css/design-system.css" \
    Alis.Reactive.DesignSystem/css \
    Alis.Reactive.DesignSystem/package.json

  check_asset_output_is_fresh \
    "sandbox Syncfusion copy" \
    "Alis.Reactive.SandboxApp/wwwroot/vendor/syncfusion/ej2.min.js" \
    Alis.Reactive.SandboxApp/copy-vendor.mjs \
    Alis.Reactive.SandboxApp/package.json

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
