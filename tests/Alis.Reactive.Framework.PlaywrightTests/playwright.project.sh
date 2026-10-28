# The framework's Playwright project, as scripts/playwright.sh reads it: one shard, the whole project,
# its components' bundle, and the app it is written against.
# shellcheck shell=bash

playwright_project=framework

playwright_shards="framework"

playwright_shard_filter() {
  case "$1" in
    framework) printf '' ;;
    *) return 2 ;;
  esac
}

playwright_check_assets() {
  check_asset_output_is_fresh \
    "framework components bundle" \
    "tests/Alis.Reactive.Framework.App/wwwroot/js/framework-components.js" \
    tests/Alis.Reactive.Framework.App/Components \
    tests/Alis.Reactive.Framework.App/components.ts \
    tests/Alis.Reactive.Framework.App/esbuild.config.mjs \
    tests/Alis.Reactive.Framework.App/tsconfig.json \
    tests/Alis.Reactive.Framework.App/package.json
}

playwright_sources() {
  printf '%s\n' tests/Alis.Reactive.Framework.App
}
