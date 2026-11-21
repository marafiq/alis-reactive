playwright_project=poc

playwright_shards="poc"

playwright_shard_filter() {
  case "$1" in
    poc) printf '' ;;
    *) return 2 ;;
  esac
}

playwright_check_assets() {
  check_asset_output_is_fresh \
    "POC runtime bundle" \
    "tests/Alis.Reactive.Poc.App/wwwroot/js/alis-poc.js" \
    poc/runtime/src \
    poc/runtime/build.mjs \
    poc/runtime/tsconfig.json

  check_asset_output_is_fresh \
    "POC components bundle" \
    "tests/Alis.Reactive.Poc.App/wwwroot/js/poc-components.js" \
    tests/Alis.Reactive.Poc.App/Components \
    tests/Alis.Reactive.Poc.App/tsconfig.json \
    poc/runtime/build.mjs
}

playwright_build_assets() {
  node poc/runtime/build.mjs
}

playwright_sources() {
  printf '%s\n' poc/Alis.Reactive.Poc tests/Alis.Reactive.Poc.App
}
