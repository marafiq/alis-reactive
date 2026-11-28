#!/usr/bin/env bash
alis_toolchain_root="${ALIS_TOOLCHAIN:-$HOME/.local/share/alis-toolchain}"
for alis_toolchain_folder in "${alis_toolchain_root}/dotnet11" "${alis_toolchain_root}/node26/bin" "${alis_toolchain_root}/python315/python/bin"; do
  if [ ! -d "${alis_toolchain_folder}" ]; then
    echo "toolchain environment not loaded: missing folder ${alis_toolchain_folder}" >&2
    unset alis_toolchain_root alis_toolchain_folder
    return 1
  fi
done
export DOTNET_ROOT="${alis_toolchain_root}/dotnet11" DOTNET_MULTILEVEL_LOOKUP=0 PATH="${alis_toolchain_root}/dotnet11:${alis_toolchain_root}/node26/bin:${alis_toolchain_root}/python315/python/bin:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 MSBUILDDISABLENODEREUSE=1 PYTHONDONTWRITEBYTECODE=1
unset alis_toolchain_root alis_toolchain_folder
