/*
 * Copyright 2026 Ho Tzin Mein
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* Stable ABI over libwebp 1.6.x (+ demux). Built by native/build.sh into libchromawebp. */

#include <webp/decode.h>
#include <webp/demux.h>
#include <webp/encode.h>

#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#if defined(_WIN32)
#define CHROMA_API __declspec(dllexport)
#else
#define CHROMA_API __attribute__((visibility("default")))
#endif

#define CHROMA_OK 0
#define CHROMA_ERR 1
#define CHROMA_ERR_ANIMATION 2
#define CHROMA_ERR_NOMEM 3
#define CHROMA_MAX_FRAMES 10000

CHROMA_API void chroma_webp_free(void* ptr) { free(ptr); }

static int ms_to_centiseconds(int duration_ms)
{
    int cs;
    if (duration_ms <= 0)
        return 0;
    cs = (duration_ms + 5) / 10;
    if (cs > 32767)
        return 32767;
    return cs;
}

CHROMA_API int chroma_webp_decode(
    const uint8_t* data,
    size_t size,
    uint8_t** out_rgba,
    int* out_width,
    int* out_height
)
{
    WebPBitstreamFeatures features;
    uint8_t* decoded;
    int width;
    int height;
    size_t bytes;
    uint8_t* copy;

    if (out_rgba == NULL || out_width == NULL || out_height == NULL)
        return CHROMA_ERR;
    *out_rgba = NULL;
    *out_width = 0;
    *out_height = 0;
    if (data == NULL || size == 0)
        return CHROMA_ERR;

    if (WebPGetFeatures(data, size, &features) != VP8_STATUS_OK)
        return CHROMA_ERR;

    if (features.has_animation)
    {
        WebPData webp_data;
        WebPAnimDecoderOptions opts;
        WebPAnimDecoder* decoder;
        WebPAnimInfo info;
        uint8_t* frame;
        int timestamp;

        webp_data.bytes = data;
        webp_data.size = size;
        if (!WebPAnimDecoderOptionsInit(&opts))
            return CHROMA_ERR;
        opts.color_mode = MODE_RGBA;
        opts.use_threads = 0;

        decoder = WebPAnimDecoderNew(&webp_data, &opts);
        if (decoder == NULL)
            return CHROMA_ERR;
        if (!WebPAnimDecoderGetInfo(decoder, &info) || info.canvas_width == 0 || info.canvas_height == 0)
        {
            WebPAnimDecoderDelete(decoder);
            return CHROMA_ERR;
        }
        if (!WebPAnimDecoderGetNext(decoder, &frame, &timestamp) || frame == NULL)
        {
            WebPAnimDecoderDelete(decoder);
            return CHROMA_ERR;
        }

        width = (int)info.canvas_width;
        height = (int)info.canvas_height;
        bytes = (size_t)width * (size_t)height * 4u;
        copy = (uint8_t*)malloc(bytes);
        if (copy == NULL)
        {
            WebPAnimDecoderDelete(decoder);
            return CHROMA_ERR_NOMEM;
        }
        memcpy(copy, frame, bytes);
        WebPAnimDecoderDelete(decoder);
        *out_rgba = copy;
        *out_width = width;
        *out_height = height;
        return CHROMA_OK;
    }

    decoded = WebPDecodeRGBA(data, size, &width, &height);
    if (decoded == NULL || width <= 0 || height <= 0)
        return CHROMA_ERR;

    bytes = (size_t)width * (size_t)height * 4u;
    copy = (uint8_t*)malloc(bytes);
    if (copy == NULL)
    {
        WebPFree(decoded);
        return CHROMA_ERR_NOMEM;
    }
    memcpy(copy, decoded, bytes);
    WebPFree(decoded);
    *out_rgba = copy;
    *out_width = width;
    *out_height = height;
    return CHROMA_OK;
}

CHROMA_API int chroma_webp_decode_anim(
    const uint8_t* data,
    size_t size,
    uint8_t** out_rgba,
    int** out_delays_cs,
    int* out_width,
    int* out_height,
    int* out_frame_count,
    int* out_repeat_count
)
{
    WebPData webp_data;
    WebPAnimDecoderOptions opts;
    WebPAnimDecoder* decoder;
    WebPAnimInfo info;
    uint8_t* packed = NULL;
    int* delays = NULL;
    size_t frame_bytes;
    uint32_t i;
    int prev_timestamp = 0;

    if (
        out_rgba == NULL
        || out_delays_cs == NULL
        || out_width == NULL
        || out_height == NULL
        || out_frame_count == NULL
        || out_repeat_count == NULL
    )
        return CHROMA_ERR;

    *out_rgba = NULL;
    *out_delays_cs = NULL;
    *out_width = 0;
    *out_height = 0;
    *out_frame_count = 0;
    *out_repeat_count = 1;
    if (data == NULL || size == 0)
        return CHROMA_ERR;

    webp_data.bytes = data;
    webp_data.size = size;
    if (!WebPAnimDecoderOptionsInit(&opts))
        return CHROMA_ERR;
    opts.color_mode = MODE_RGBA;
    opts.use_threads = 0;

    decoder = WebPAnimDecoderNew(&webp_data, &opts);
    if (decoder == NULL)
    {
        /* Still WebP: treat as one frame with zero delay. */
        uint8_t* still = NULL;
        int width;
        int height;
        int status = chroma_webp_decode(data, size, &still, &width, &height);
        if (status != CHROMA_OK)
            return status;
        delays = (int*)malloc(sizeof(int));
        if (delays == NULL)
        {
            free(still);
            return CHROMA_ERR_NOMEM;
        }
        delays[0] = 0;
        *out_rgba = still;
        *out_delays_cs = delays;
        *out_width = width;
        *out_height = height;
        *out_frame_count = 1;
        *out_repeat_count = 1;
        return CHROMA_OK;
    }

    if (!WebPAnimDecoderGetInfo(decoder, &info))
    {
        WebPAnimDecoderDelete(decoder);
        return CHROMA_ERR;
    }
    if (
        info.canvas_width == 0
        || info.canvas_height == 0
        || info.frame_count == 0
        || info.frame_count > CHROMA_MAX_FRAMES
    )
    {
        WebPAnimDecoderDelete(decoder);
        return CHROMA_ERR;
    }

    frame_bytes = (size_t)info.canvas_width * (size_t)info.canvas_height * 4u;
    packed = (uint8_t*)malloc(frame_bytes * (size_t)info.frame_count);
    delays = (int*)malloc(sizeof(int) * (size_t)info.frame_count);
    if (packed == NULL || delays == NULL)
    {
        free(packed);
        free(delays);
        WebPAnimDecoderDelete(decoder);
        return CHROMA_ERR_NOMEM;
    }

    for (i = 0; i < info.frame_count; i++)
    {
        uint8_t* frame;
        int timestamp;
        if (!WebPAnimDecoderGetNext(decoder, &frame, &timestamp) || frame == NULL)
        {
            free(packed);
            free(delays);
            WebPAnimDecoderDelete(decoder);
            return CHROMA_ERR;
        }
        memcpy(packed + frame_bytes * (size_t)i, frame, frame_bytes);
        delays[i] = ms_to_centiseconds(timestamp - prev_timestamp);
        prev_timestamp = timestamp;
    }

    *out_rgba = packed;
    *out_delays_cs = delays;
    *out_width = (int)info.canvas_width;
    *out_height = (int)info.canvas_height;
    *out_frame_count = (int)info.frame_count;
    /* WebP loop_count matches GIF: 0 = infinite. */
    if (info.loop_count > 65535u)
        *out_repeat_count = 65535;
    else
        *out_repeat_count = (int)info.loop_count;
    WebPAnimDecoderDelete(decoder);
    return CHROMA_OK;
}

CHROMA_API int chroma_webp_encode(
    const uint8_t* rgba,
    int width,
    int height,
    int stride,
    int quality,
    int lossless,
    uint8_t** out_data,
    size_t* out_size
)
{
    uint8_t* encoded = NULL;
    size_t size = 0;
    uint8_t* copy;

    if (out_data == NULL || out_size == NULL)
        return CHROMA_ERR;
    *out_data = NULL;
    *out_size = 0;
    if (rgba == NULL || width <= 0 || height <= 0 || stride < width * 4)
        return CHROMA_ERR;

    if (quality < 0)
        quality = 0;
    if (quality > 100)
        quality = 100;

    if (lossless)
        size = WebPEncodeLosslessRGBA(rgba, width, height, stride, &encoded);
    else
        size = WebPEncodeRGBA(rgba, width, height, stride, (float)quality, &encoded);

    if (size == 0 || encoded == NULL)
        return CHROMA_ERR;

    copy = (uint8_t*)malloc(size);
    if (copy == NULL)
    {
        WebPFree(encoded);
        return CHROMA_ERR_NOMEM;
    }
    memcpy(copy, encoded, size);
    WebPFree(encoded);
    *out_data = copy;
    *out_size = size;
    return CHROMA_OK;
}
