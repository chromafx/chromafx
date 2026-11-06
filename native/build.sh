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
  linux-x64 | linux-arm64 | osx-arm64 | osx-x64 | win-x64) ;;
  *)
    echo "unsupported rid: $RID" >&2
    exit 1
    ;;
esac

if [[ "$RID" != "$HOST" ]]; then
  if [[ "$HOST" == "osx-arm64" && "$RID" == "osx-x64" ]]; then
    CMAKE_EXTRA+=(-DCMAKE_SYSTEM_NAME=Darwin -DCMAKE_SYSTEM_PROCESSOR=x86_64 -DCMAKE_OSX_ARCHITECTURES=x86_64)
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

need git
need cmake
need meson
need ninja
case "$RID" in
  *-x64) need nasm ;;
esac

mkdir -p "$CACHE" "$OUT"

clone() {
  local url="$1" tag="$2" dest="$3"
  if [[ ! -d "$dest/.git" ]]; then
    rm -rf "$dest"
    git clone --depth 1 --branch "$tag" "$url" "$dest"
  fi
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
cmake -S "$CACHE/libwebp" -B "$CACHE/libwebp-build" \
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
  -DCMAKE_INSTALL_PREFIX="$CACHE/libwebp-install" \
  "${CMAKE_EXTRA[@]}"
cmake --build "$CACHE/libwebp-build" --parallel
cmake --install "$CACHE/libwebp-build"
copy_shared "$CACHE/libwebp-install" webp

echo "building libavif $LIBAVIF_TAG (dav1d decode, libaom encode) for $RID"
clone "https://github.com/AOMediaCodec/libavif.git" "$LIBAVIF_TAG" "$CACHE/libavif"
cmake -S "$CACHE/libavif" -B "$CACHE/libavif-build" \
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
  -DCMAKE_INSTALL_PREFIX="$CACHE/libavif-install" \
  "${CMAKE_EXTRA[@]}"
cmake --build "$CACHE/libavif-build" --parallel
cmake --install "$CACHE/libavif-build"

AVIF_LIB="$(find "$CACHE/libavif-install" "$CACHE/libavif-build" \( -name 'libavif.a' -o -name 'avif.lib' \) -type f -print -quit)"
DAV1D_LIB="$(find "$CACHE/libavif-build" \( -name 'libdav1d.a' -o -name 'dav1d.lib' \) -type f -print -quit)"
AOM_LIB="$(find "$CACHE/libavif-build" \( -name 'libaom.a' -o -name 'aom.lib' \) -type f -print -quit)"
if [[ -z "$AVIF_LIB" || -z "$DAV1D_LIB" || -z "$AOM_LIB" ]]; then
  echo "missing static codec library (avif=$AVIF_LIB dav1d=$DAV1D_LIB aom=$AOM_LIB)" >&2
  exit 1
fi

cmake -S "$ROOT/native/chroma_avif" -B "$CACHE/chromaavif-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DAVIF_INCLUDE_DIR="$CACHE/libavif-install/include" \
  -DAVIF_LIBRARY="$AVIF_LIB" \
  -DDAV1D_LIBRARY="$DAV1D_LIB" \
  -DAOM_LIBRARY="$AOM_LIB" \
  "${CMAKE_EXTRA[@]}"
cmake --build "$CACHE/chromaavif-build" --parallel
copy_shared "$CACHE/chromaavif-build" chromaavif

echo "building libjxl $LIBJXL_TAG for $RID"
if [[ ! -d "$CACHE/libjxl/.git" ]]; then
  rm -rf "$CACHE/libjxl"
  git clone --depth 1 --branch "$LIBJXL_TAG" "https://github.com/libjxl/libjxl.git" "$CACHE/libjxl"
fi
git -C "$CACHE/libjxl" submodule update --init --depth 1 -- third_party/highway third_party/brotli third_party/skcms
cmake -S "$CACHE/libjxl" -B "$CACHE/libjxl-build" \
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
  -DCMAKE_INSTALL_PREFIX="$CACHE/libjxl-install" \
  "${CMAKE_EXTRA[@]}"
cmake --build "$CACHE/libjxl-build" --parallel
cmake --install "$CACHE/libjxl-build"

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

cmake -S "$ROOT/native/chroma_jxl" -B "$CACHE/chromajxl-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DJXL_INCLUDE_DIR="$CACHE/libjxl-install/include" \
  -DJXL_LIBRARY="$JXL_LIB" \
  -DJXL_CMS_LIBRARY="$JXL_CMS_LIB" \
  -DHWY_LIBRARY="$HWY_LIB" \
  -DBROTLIENC_LIBRARY="$BROTLIENC_LIB" \
  -DBROTLIDEC_LIBRARY="$BROTLIDEC_LIB" \
  -DBROTLICOMMON_LIBRARY="$BROTLICOMMON_LIB" \
  -DSKCMS_LIBRARY="$SKCMS_LIB" \
  "${CMAKE_EXTRA[@]}"
cmake --build "$CACHE/chromajxl-build" --parallel
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
