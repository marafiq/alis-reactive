playwright_project=conformance

playwright_shards="conformance"

playwright_shard_filter() {
  case "$1" in
    conformance) printf '' ;;
    *) return 2 ;;
  esac
}

playwright_check_assets() {
  :
}

playwright_build_assets() {
  :
}

playwright_sources() {
  printf '%s\n' tests/Alis.Reactive.Conformance.App tests/Alis.Reactive.Conformance.World
}
