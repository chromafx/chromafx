#!/usr/bin/env bash
# Build pinned libwebp, libavif (+dav1d/libaom), and libjxl for one RID.
# Output: src/ChromaFx.Native/runtimes/<rid>/native/
# Does not commit binaries. Run on the matching OS, except osx-x64 which can
# cross-compile from an Apple Silicon Mac.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LIBWEBP_TAG="v1.6.0"
LIBAVIF_TAG="v1.3.0"
LIBJXL_TAG="v0.12.0"

host_rid() {
  local kernel arch
  kernel="$(uname -s)"
  arch="$(uname -m)"
  case "$arch" in
    arm64 | aarch64) arch="arm64" ;;
    x86_64 | amd64) arch="x64" ;;
    *)
      echo "unsupported architecture: $arch" >&2
      exit 1
      ;;
  esac
  # Git Bash / MSYS on Windows ARM64 often runs as an x64 process, so uname -m
  # is x86_64. Prefer the OS arch from the kernel name or PROCESSOR_* vars.
  case "$kernel" in
    *-ARM64 | *-arm64) arch="arm64" ;;
  esac
  case "${PROCESSOR_ARCHITECTURE:-}_${PROCESSOR_ARCHITEW6432:-}" in
    *ARM64* | *arm64*) arch="arm64" ;;
  esac
  case "$kernel" in
    Darwin) echo "osx-$arch" ;;
    Linux) echo "linux-$arch" ;;
    MINGW* | MSYS* | CYGWIN* | Windows_NT) echo "win-$arch" ;;
    *)
      echo "unsupported kernel: $kernel" >&2
      exit 1
      ;;
  esac
}

if [[ "${1:-}" == "--print-rid" ]]; then
  host_rid
  exit 0
fi

HOST="$(host_rid)"
RID="${1:-$HOST}"
CACHE="$ROOT/native/.cache/$RID"
OUT="$ROOT/src/ChromaFx.Native/runtimes/$RID/native"
CMAKE_EXTRA=()

case "$RID" in
  linux-x64 | linux-arm64 | osx-arm64 | osx-x64 | win-x64 | win-arm64) ;;
  *)
    echo "unsupported rid: $RID" >&2
    exit 1
    ;;
esac

if [[ "$RID" != "$HOST" ]]; then
  if [[ "$HOST" == "osx-arm64" && "$RID" == "osx-x64" ]]; then
    # libavif writes these into dav1d's meson cross file. An empty sysroot becomes -isysroot ''.
    sdk="$(xcrun --sdk macosx --show-sdk-path)"
    CMAKE_EXTRA+=(
      -DCMAKE_SYSTEM_NAME=Darwin
      -DCMAKE_SYSTEM_PROCESSOR=x86_64
      -DCMAKE_OSX_ARCHITECTURES=x86_64
      -DCMAKE_OSX_SYSROOT="$sdk"
      -DCMAKE_OSX_DEPLOYMENT_TARGET=11.0
    )
  else
    echo "Build $RID on a $RID machine (this host is $HOST)." >&2
    exit 1
  fi
fi

need() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "missing tool: $1" >&2
    exit 1
  fi
}

# bash 3.2 (macOS /bin/bash) errors on "${arr[@]}" when the array is empty and set -u.
run_cmake() {
  if [[ ${#CMAKE_EXTRA[@]} -gt 0 ]]; then
    cmake "$@" "${CMAKE_EXTRA[@]}"
  else
    cmake "$@"
  fi
}

# libjxl's C++ objects are large. Uncapped -j is killed with SIGTERM (exit 143) on CI.
build_jobs() {
  if [[ -n "${JOBS:-}" ]]; then
    echo "$JOBS"
    return 0
  fi
  local n
  n="$(getconf _NPROCESSORS_ONLN 2>/dev/null || echo 2)"
  if [[ "$n" -gt 2 ]]; then
    n=2
  fi
  echo "$n"
}

cmake_build() {
  cmake --build "$1" --config Release --parallel "$(build_jobs)"
}

cmake_install() {
  cmake --install "$1" --config Release
}

# libaom rejects NASM 3: `nasm -hf` no longer documents -Ox multipass.
ensure_nasm() {
  case "$RID" in
    *-x64) ;;
    *) return 0 ;;
  esac
  if command -v nasm >/dev/null 2>&1 && nasm -hf 2>/dev/null | grep -q -- '-Ox'; then
    echo "using $(nasm -v)"
    return 0
  fi

  local version="2.16.03"
  local prefix="$CACHE/nasm-$version"
  local bin="$prefix/nasm"
  if [[ "$RID" == win-* ]]; then
    bin="$prefix/nasm.exe"
  fi
  if [[ ! -x "$bin" ]]; then
    echo "building nasm $version (system nasm is missing or too new for libaom)"
    mkdir -p "$prefix"
    if [[ "$RID" == win-* ]]; then
      local archive="$CACHE/nasm-$version-win64.zip"
      curl -fsSL -o "$archive" "https://www.nasm.us/pub/nasm/releasebuilds/$version/win64/nasm-$version-win64.zip"
      rm -rf "$CACHE/nasm-unpack"
      python3 - "$archive" "$CACHE/nasm-unpack" <<'PY'
import pathlib, sys, zipfile
dest = pathlib.Path(sys.argv[2])
dest.mkdir(parents=True, exist_ok=True)
zipfile.ZipFile(sys.argv[1]).extractall(dest)
PY
      local exe
      exe="$(find "$CACHE/nasm-unpack" -name nasm.exe -type f | head -n 1)"
      if [[ -z "$exe" ]]; then
        echo "nasm.exe not found in $archive" >&2
        exit 1
      fi
      cp "$exe" "$bin"
    else
      need make
      need cc
      local archive="$CACHE/nasm-$version.tar.gz"
      local src="$CACHE/nasm-$version-src"
      curl -fsSL -o "$archive" "https://www.nasm.us/pub/nasm/releasebuilds/$version/nasm-$version.tar.gz"
      rm -rf "$src"
      mkdir -p "$src"
      tar -xzf "$archive" -C "$src" --strip-components 1
      local jobs
      jobs="$(getconf _NPROCESSORS_ONLN 2>/dev/null || echo 4)"
      (cd "$src" && ./configure --prefix="$prefix" && make -j"$jobs")
      cp "$src/nasm" "$bin"
      chmod +x "$bin"
    fi
  fi
  export PATH="$prefix:$PATH"
  hash -r 2>/dev/null || true
  echo "using $($bin -v)"
  if ! "$bin" -hf 2>/dev/null | grep -q -- '-Ox'; then
    echo "nasm $version still lacks -Ox; libaom cannot use it" >&2
    exit 1
  fi
}

# dav1d on MSVC aarch64 requires gas-preprocessor.pl (and armasm64) for ASM.
# Git Bash's uname may look like x64; we still need this whenever RID is win-arm64.
ensure_gas_preprocessor() {
  [[ "$RID" == win-arm64 ]] || return 0
  if command -v gas-preprocessor.pl >/dev/null 2>&1; then
    echo "using $(command -v gas-preprocessor.pl)"
  else
    if ! command -v perl >/dev/null 2>&1; then
      echo "perl is required for dav1d ASM on win-arm64 (gas-preprocessor.pl). Git Bash usually provides it." >&2
      exit 1
    fi
    local dest="$CACHE/bin"
    mkdir -p "$dest"
    curl -fsSL -o "$dest/gas-preprocessor.pl" \
      "https://raw.githubusercontent.com/FFmpeg/gas-preprocessor/master/gas-preprocessor.pl"
    chmod +x "$dest/gas-preprocessor.pl"
    # Meson on Windows often resolves the tool via a .bat shim.
    cat >"$dest/gas-preprocessor.pl.bat" <<'EOF'
@echo off
perl "%~dp0gas-preprocessor.pl" %*
EOF
    export PATH="$dest:$PATH"
    hash -r 2>/dev/null || true
    echo "installed gas-preprocessor.pl for win-arm64 dav1d ASM"
  fi
  if ! command -v armasm64 >/dev/null 2>&1; then
    echo "armasm64 not on PATH. Use an MSVC arm64 developer shell (ilammy/msvc-dev-cmd arch=arm64)." >&2
    exit 1
  fi
  echo "using $(command -v armasm64)"
}

# Git for Windows ships usr/bin/link.exe, which is not a linker. dav1d's meson
# setup rejects it. The MSVC developer shell sets VCToolsInstallDir.
prefer_msvc_link() {
  [[ "$RID" == win-* ]] || return 0
  local raw="${VCToolsInstallDir:-}"
  if [[ -z "$raw" ]]; then
    echo "VCToolsInstallDir is unset. Start from an MSVC developer shell so dav1d uses the MSVC linker." >&2
    exit 1
  fi
  raw="${raw//\\//}"
  [[ "$raw" == */ ]] || raw="${raw}/"
  local link_dir
  case "$RID" in
    win-arm64) link_dir="${raw}bin/Hostarm64/arm64" ;;
    *) link_dir="${raw}bin/Hostx64/x64" ;;
  esac
  if [[ ! -e "$link_dir/link.exe" ]]; then
    echo "MSVC link.exe not found at $link_dir" >&2
    exit 1
  fi
  if command -v cygpath >/dev/null 2>&1; then
    link_dir="$(cygpath -u "$link_dir")"
  fi
  export PATH="${link_dir}:$PATH"
  hash -r 2>/dev/null || true
  echo "preferring MSVC linker at $link_dir"
}

need git
need cmake
need meson
need ninja
need curl
need python3

mkdir -p "$CACHE" "$OUT"
ensure_nasm
prefer_msvc_link
ensure_gas_preprocessor

clone() {
  local url="$1" tag="$2" dest="$3"
  if [[ -d "$dest/.git" ]]; then
    return 0
  fi
  rm -rf "$dest"
  git init "$dest"
  git -C "$dest" remote add origin "$url"
  # Peel annotated tags. `git clone --branch vX` warns "is not a commit" on a shallow fetch.
  git -C "$dest" fetch --depth 1 origin "refs/tags/${tag}:refs/tags/${tag}"
  git -C "$dest" checkout --detach "${tag}^{}"
}

copy_shared() {
  local search="$1" stem="$2"
  local found=""
  found="$(find "$search" \( -name "lib${stem}.dylib" -o -name "lib${stem}.so" -o -name "lib${stem}.dll" -o -name "${stem}.dll" \) -type f ! -name "*.so.*" | head -n 1 || true)"
  if [[ -z "$found" ]]; then
    found="$(find "$search" \( -name "lib${stem}.so.*" -o -name "lib${stem}.*.dylib" \) -type f | head -n 1 || true)"
  fi
  if [[ -z "$found" ]]; then
    echo "could not find shared library $stem under $search" >&2
    exit 1
  fi
  local dest_name
  case "$RID" in
    win-*) dest_name="${stem}.dll" ;;
    osx-*) dest_name="lib${stem}.dylib" ;;
    *) dest_name="lib${stem}.so" ;;
  esac
  cp "$found" "$OUT/$dest_name"
  if [[ "$RID" == osx-* ]]; then
    install_name_tool -id "@rpath/$dest_name" "$OUT/$dest_name"
  fi
  echo "staged $OUT/$dest_name"
}

echo "building libwebp $LIBWEBP_TAG for $RID"
clone "https://github.com/webmproject/libwebp.git" "$LIBWEBP_TAG" "$CACHE/libwebp"
# Keep sharpyuv inside libwebp so the package does not need a second dylib.
if ! grep -q 'add_library(sharpyuv STATIC' "$CACHE/libwebp/CMakeLists.txt"; then
  python3 - "$CACHE/libwebp/CMakeLists.txt" <<'PY'
import pathlib, sys
path = pathlib.Path(sys.argv[1])
text = path.read_text()
old = "add_library(sharpyuv ${WEBP_SHARPYUV_SRCS})"
new = "add_library(sharpyuv STATIC ${WEBP_SHARPYUV_SRCS})"
if old not in text:
    raise SystemExit("libwebp CMakeLists has no sharpyuv library line to patch")
path.write_text(text.replace(old, new, 1))
PY
fi
rm -rf "$CACHE/libwebp-build"
run_cmake -S "$CACHE/libwebp" -B "$CACHE/libwebp-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DBUILD_SHARED_LIBS=ON \
  -DWEBP_LINK_STATIC=ON \
  -DWEBP_BUILD_ANIM_UTILS=OFF \
  -DWEBP_BUILD_CWEBP=OFF \
  -DWEBP_BUILD_DWEBP=OFF \
  -DWEBP_BUILD_GIF2WEBP=OFF \
  -DWEBP_BUILD_IMG2WEBP=OFF \
  -DWEBP_BUILD_VWEBP=OFF \
  -DWEBP_BUILD_WEBPINFO=OFF \
  -DWEBP_BUILD_LIBWEBPMUX=OFF \
  -DWEBP_BUILD_WEBPMUX=OFF \
  -DWEBP_BUILD_EXTRAS=OFF \
  -DWEBP_BUILD_WEBP_JS=OFF \
  -DCMAKE_INSTALL_PREFIX="$CACHE/libwebp-install"
cmake_build "$CACHE/libwebp-build"
cmake_install "$CACHE/libwebp-build"
copy_shared "$CACHE/libwebp-install" webp

echo "building libavif $LIBAVIF_TAG (dav1d decode, libaom encode) for $RID"
clone "https://github.com/AOMediaCodec/libavif.git" "$LIBAVIF_TAG" "$CACHE/libavif"
run_cmake -S "$CACHE/libavif" -B "$CACHE/libavif-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DBUILD_SHARED_LIBS=OFF \
  -DAVIF_CODEC_AOM=LOCAL \
  -DAVIF_CODEC_AOM_DECODE=OFF \
  -DAVIF_CODEC_DAV1D=LOCAL \
  -DAVIF_BUILD_APPS=OFF \
  -DAVIF_BUILD_TESTS=OFF \
  -DAVIF_BUILD_EXAMPLES=OFF \
  -DAVIF_LIBYUV=OFF \
  -DAVIF_LIBSHARPYUV=OFF \
  -DCMAKE_INSTALL_PREFIX="$CACHE/libavif-install"
cmake_build "$CACHE/libavif-build"
cmake_install "$CACHE/libavif-build"

AVIF_LIB="$(find "$CACHE/libavif-install" "$CACHE/libavif-build" \( -name 'libavif.a' -o -name 'avif.lib' \) -type f -print -quit)"
DAV1D_LIB="$(find "$CACHE/libavif-build" \( -name 'libdav1d.a' -o -name 'dav1d.lib' \) -type f -print -quit)"
AOM_LIB="$(find "$CACHE/libavif-build" \( -name 'libaom.a' -o -name 'aom.lib' \) -type f -print -quit)"
if [[ -z "$AVIF_LIB" || -z "$DAV1D_LIB" || -z "$AOM_LIB" ]]; then
  echo "missing static codec library (avif=$AVIF_LIB dav1d=$DAV1D_LIB aom=$AOM_LIB)" >&2
  exit 1
fi

run_cmake -S "$ROOT/native/chroma_avif" -B "$CACHE/chromaavif-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DAVIF_INCLUDE_DIR="$CACHE/libavif-install/include" \
  -DAVIF_LIBRARY="$AVIF_LIB" \
  -DDAV1D_LIBRARY="$DAV1D_LIB" \
  -DAOM_LIBRARY="$AOM_LIB"
cmake_build "$CACHE/chromaavif-build"
copy_shared "$CACHE/chromaavif-build" chromaavif

echo "building libjxl $LIBJXL_TAG for $RID"
clone "https://github.com/libjxl/libjxl.git" "$LIBJXL_TAG" "$CACHE/libjxl"
git -C "$CACHE/libjxl" submodule update --init --depth 1 -- third_party/highway third_party/brotli third_party/skcms
run_cmake -S "$CACHE/libjxl" -B "$CACHE/libjxl-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DBUILD_TESTING=OFF \
  -DBUILD_SHARED_LIBS=OFF \
  -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
  -DJPEGXL_ENABLE_TOOLS=OFF \
  -DJPEGXL_ENABLE_BENCHMARK=OFF \
  -DJPEGXL_ENABLE_EXAMPLES=OFF \
  -DJPEGXL_ENABLE_MANPAGES=OFF \
  -DJPEGXL_ENABLE_JNI=OFF \
  -DJPEGXL_ENABLE_SJPEG=OFF \
  -DJPEGXL_ENABLE_OPENEXR=OFF \
  -DJPEGXL_ENABLE_DOXYGEN=OFF \
  -DJPEGXL_ENABLE_VIEWERS=OFF \
  -DJPEGXL_ENABLE_PLUGINS=OFF \
  -DJPEGXL_ENABLE_DEVTOOLS=OFF \
  -DJPEGXL_ENABLE_TRANSCODE_JPEG=OFF \
  -DJPEGXL_ENABLE_SKCMS=ON \
  -DJPEGXL_FORCE_SYSTEM_BROTLI=OFF \
  -DJPEGXL_FORCE_SYSTEM_HWY=OFF \
  -DCMAKE_INSTALL_PREFIX="$CACHE/libjxl-install"
cmake_build "$CACHE/libjxl-build"
cmake_install "$CACHE/libjxl-build"

find_static() {
  local name="$1"
  local found=""
  found="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name "lib${name}.a" -o -name "${name}.lib" \) -type f -print -quit)"
  if [[ -z "$found" ]]; then
    echo "missing static library $name" >&2
    find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name '*.a' -o -name '*.lib' \) -type f >&2 || true
    exit 1
  fi
  echo "$found"
}

JXL_LIB="$(find_static jxl)"
JXL_CMS_LIB="$(find_static jxl_cms)"
HWY_LIB="$(find_static hwy)"
BROTLIENC_LIB="$(find_static brotlienc)"
BROTLIDEC_LIB="$(find_static brotlidec)"
BROTLICOMMON_LIB="$(find_static brotlicommon)"
SKCMS_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libskcms.a' -o -name 'skcms.lib' \) -type f -print -quit || true)"

run_cmake -S "$ROOT/native/chroma_jxl" -B "$CACHE/chromajxl-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DJXL_INCLUDE_DIR="$CACHE/libjxl-install/include" \
  -DJXL_LIBRARY="$JXL_LIB" \
  -DJXL_CMS_LIBRARY="$JXL_CMS_LIB" \
  -DHWY_LIBRARY="$HWY_LIB" \
  -DBROTLIENC_LIBRARY="$BROTLIENC_LIB" \
  -DBROTLIDEC_LIBRARY="$BROTLIDEC_LIB" \
  -DBROTLICOMMON_LIBRARY="$BROTLICOMMON_LIB" \
  -DSKCMS_LIBRARY="$SKCMS_LIB"
cmake_build "$CACHE/chromajxl-build"
copy_shared "$CACHE/chromajxl-build" chromajxl

if [[ "$RID" == osx-* ]]; then
  if otool -L "$OUT/libwebp.dylib" "$OUT/libchromaavif.dylib" "$OUT/libchromajxl.dylib" | grep -E 'sharpyuv|libavif|libdav1d|libaom|libjxl|libhwy|brotli|skcms'; then
    echo "shared libraries still depend on another codec dylib" >&2
    exit 1
  fi
elif [[ "$RID" == linux-* ]]; then
  if ldd "$OUT/libwebp.so" "$OUT/libchromaavif.so" "$OUT/libchromajxl.so" | grep -E 'sharpyuv|libavif|libdav1d|libaom|libjxl|libhwy|brotli|skcms'; then
    echo "shared libraries still depend on another codec so" >&2
    exit 1
  fi
fi

echo "native codecs ready in $OUT"
