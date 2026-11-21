#!/usr/bin/env bash
# Build gen_anim_fixtures against native/.cache and write test assets.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
RID="$("$ROOT/native/build.sh" --print-rid)"
CACHE="$ROOT/native/.cache/$RID"
OUT_DIR="$ROOT/tests/ChromaFx.Tests/ExpectedResults/Formats/NativeAnim"
mkdir -p "$OUT_DIR"

need_file() {
  if [[ ! -f "$1" ]]; then
    echo "missing $1 — run native/build.sh first" >&2
    exit 1
  fi
}

AVIF_INC="$CACHE/libavif-install/include"
WEBP_INC="$CACHE/libwebp-install/include"
JXL_INC="$CACHE/libjxl-install/include"
AVIF_LIB="$(find "$CACHE/libavif-install" "$CACHE/libavif-build" \( -name 'libavif.a' -o -name 'avif.lib' \) -type f -print -quit)"
DAV1D_LIB="$(find "$CACHE/libavif-build" \( -name 'libdav1d.a' -o -name 'dav1d.lib' \) -type f -print -quit)"
AOM_LIB="$(find "$CACHE/libavif-build" \( -name 'libaom.a' -o -name 'aom.lib' \) -type f -print -quit)"
WEBP_LIB="$(find "$CACHE/libwebp-install" "$CACHE/libwebp-build" \( -name 'libwebp.a' -o -name 'webp.lib' \) -type f -print -quit)"
SHARPYUV_LIB="$(find "$CACHE/libwebp-install" "$CACHE/libwebp-build" \( -name 'libsharpyuv.a' -o -name 'sharpyuv.lib' \) -type f -print -quit || true)"
JXL_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libjxl.a' -o -name 'jxl.lib' \) -type f -print -quit)"
JXL_CMS_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libjxl_cms.a' -o -name 'jxl_cms.lib' \) -type f -print -quit)"
HWY_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libhwy.a' -o -name 'hwy.lib' \) -type f -print -quit)"
BROTLIENC_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libbrotlienc.a' -o -name 'brotlienc.lib' \) -type f -print -quit)"
BROTLIDEC_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libbrotlidec.a' -o -name 'brotlidec.lib' \) -type f -print -quit)"
BROTLICOMMON_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libbrotlicommon.a' -o -name 'brotlicommon.lib' \) -type f -print -quit)"
SKCMS_LIB="$(find "$CACHE/libjxl-install" "$CACHE/libjxl-build" \( -name 'libskcms.a' -o -name 'skcms.lib' \) -type f -print -quit || true)"

need_file "$AVIF_LIB"
need_file "$AOM_LIB"
need_file "$WEBP_LIB"
need_file "$JXL_LIB"

# Build static webpmux for the fixture tool only (not shipped).
rm -rf "$CACHE/libwebp-mux-build"
cmake -S "$CACHE/libwebp" -B "$CACHE/libwebp-mux-build" \
  -DCMAKE_BUILD_TYPE=Release \
  -DBUILD_SHARED_LIBS=OFF \
  -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
  -DWEBP_BUILD_ANIM_UTILS=OFF \
  -DWEBP_BUILD_CWEBP=OFF \
  -DWEBP_BUILD_DWEBP=OFF \
  -DWEBP_BUILD_GIF2WEBP=OFF \
  -DWEBP_BUILD_IMG2WEBP=OFF \
  -DWEBP_BUILD_VWEBP=OFF \
  -DWEBP_BUILD_WEBPINFO=OFF \
  -DWEBP_BUILD_LIBWEBPMUX=ON \
  -DWEBP_BUILD_WEBPMUX=OFF \
  -DWEBP_BUILD_EXTRAS=OFF \
  -DWEBP_BUILD_WEBP_JS=OFF \
  -DCMAKE_INSTALL_PREFIX="$CACHE/libwebp-mux-install"
cmake --build "$CACHE/libwebp-mux-build" --config Release --parallel 2
cmake --install "$CACHE/libwebp-mux-build" --config Release
WEBPMUX_LIB="$(find "$CACHE/libwebp-mux-install" "$CACHE/libwebp-mux-build" \( -name 'libwebpmux.a' -o -name 'webpmux.lib' \) -type f -print -quit)"
MUX_WEBP_LIB="$(find "$CACHE/libwebp-mux-install" "$CACHE/libwebp-mux-build" \( -name 'libwebp.a' -o -name 'webp.lib' \) -type f -print -quit)"
MUX_SHARPYUV_LIB="$(find "$CACHE/libwebp-mux-install" "$CACHE/libwebp-mux-build" \( -name 'libsharpyuv.a' -o -name 'sharpyuv.lib' \) -type f -print -quit || true)"
need_file "$WEBPMUX_LIB"
need_file "$MUX_WEBP_LIB"
WEBP_MUX_INC="$CACHE/libwebp-mux-install/include"

BIN="$CACHE/gen_anim_fixtures"
LIBS=(
  "$AVIF_LIB" "$AOM_LIB"
  "$WEBPMUX_LIB" "$MUX_WEBP_LIB"
  "$JXL_LIB" "$JXL_CMS_LIB" "$HWY_LIB"
  "$BROTLIENC_LIB" "$BROTLIDEC_LIB" "$BROTLICOMMON_LIB"
)
[[ -n "$DAV1D_LIB" ]] && LIBS+=("$DAV1D_LIB")
[[ -n "$MUX_SHARPYUV_LIB" ]] && LIBS+=("$MUX_SHARPYUV_LIB")
[[ -n "$SKCMS_LIB" ]] && LIBS+=("$SKCMS_LIB")

cc -O2 \
  -I"$AVIF_INC" -I"$WEBP_MUX_INC" -I"$JXL_INC" \
  "$ROOT/native/tools/gen_anim_fixtures.c" \
  -o "$BIN" \
  "${LIBS[@]}" \
  -lc++ -pthread -lm

"$BIN" "$OUT_DIR"
