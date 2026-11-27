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

/* Stable ABI over libjxl 0.12.x. Built by native/build.sh into libchromajxl. */

#include <jxl/codestream_header.h>
#include <jxl/decode.h>
#include <jxl/encode.h>
#include <jxl/types.h>

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

CHROMA_API void chroma_jxl_free(void* ptr) { free(ptr); }

static int expand_rgba_into(
    const uint8_t* src,
    size_t stride,
    int width,
    int height,
    int channels,
    uint8_t* dest
)
{
    int y;
    int x;

    if (width <= 0 || height <= 0 || channels < 1 || channels > 4 || dest == NULL)
        return CHROMA_ERR;

    for (y = 0; y < height; y++)
    {
        const uint8_t* row = src + (size_t)y * stride;
        for (x = 0; x < width; x++)
        {
            const uint8_t* p = row + (size_t)x * (size_t)channels;
            uint8_t* d = dest + ((size_t)y * (size_t)width + (size_t)x) * 4u;
            if (channels == 1)
            {
                d[0] = d[1] = d[2] = p[0];
                d[3] = 255;
            }
            else if (channels == 2)
            {
                d[0] = d[1] = d[2] = p[0];
                d[3] = p[1];
            }
            else
            {
                d[0] = p[0];
                d[1] = p[1];
                d[2] = p[2];
                d[3] = channels == 4 ? p[3] : 255;
            }
        }
    }

    return CHROMA_OK;
}

static int expand_rgba(
    const uint8_t* src,
    size_t stride,
    int width,
    int height,
    int channels,
    uint8_t** out_rgba
)
{
    uint8_t* rgba;
    size_t count;

    if (out_rgba == NULL)
        return CHROMA_ERR;
    count = (size_t)width * (size_t)height;
    rgba = (uint8_t*)malloc(count * 4u);
    if (rgba == NULL)
        return CHROMA_ERR_NOMEM;
    if (expand_rgba_into(src, stride, width, height, channels, rgba) != CHROMA_OK)
    {
        free(rgba);
        return CHROMA_ERR;
    }
    *out_rgba = rgba;
    return CHROMA_OK;
}

static int ticks_to_centiseconds(uint32_t duration_ticks, uint32_t tps_num, uint32_t tps_den)
{
    double seconds;
    double cs;
    if (duration_ticks == 0 || tps_num == 0)
        return 0;
    if (tps_den == 0)
        tps_den = 1;
    seconds = ((double)duration_ticks * (double)tps_den) / (double)tps_num;
    cs = seconds * 100.0 + 0.5;
    if (cs > 32767.0)
        return 32767;
    return (int)cs;
}

CHROMA_API int chroma_jxl_decode(
    const uint8_t* data,
    size_t size,
    uint8_t** out_rgba,
    int* out_width,
    int* out_height
)
{
    JxlDecoder* decoder;
    JxlBasicInfo info;
    JxlPixelFormat format;
    JxlDecoderStatus status;
    uint8_t* pixels = NULL;
    size_t pixel_bytes = 0;
    int channels = 0;
    int width = 0;
    int height = 0;
    int result = CHROMA_ERR;
    int saw_image = 0;

    if (out_rgba == NULL || out_width == NULL || out_height == NULL)
        return CHROMA_ERR;
    *out_rgba = NULL;
    *out_width = 0;
    *out_height = 0;
    if (data == NULL || size == 0)
        return CHROMA_ERR;

    decoder = JxlDecoderCreate(NULL);
    if (decoder == NULL)
        return CHROMA_ERR_NOMEM;

    if (
        JxlDecoderSetUnpremultiplyAlpha(decoder, JXL_TRUE) != JXL_DEC_SUCCESS
        || JxlDecoderSubscribeEvents(decoder, JXL_DEC_BASIC_INFO | JXL_DEC_FULL_IMAGE)
            != JXL_DEC_SUCCESS
        || JxlDecoderSetInput(decoder, data, size) != JXL_DEC_SUCCESS
    )
    {
        JxlDecoderDestroy(decoder);
        return CHROMA_ERR;
    }
    JxlDecoderCloseInput(decoder);

    memset(&format, 0, sizeof(format));
    format.data_type = JXL_TYPE_UINT8;
    format.endianness = JXL_NATIVE_ENDIAN;
    format.align = 0;

    for (;;)
    {
        status = JxlDecoderProcessInput(decoder);
        if (status == JXL_DEC_ERROR || status == JXL_DEC_NEED_MORE_INPUT)
            break;

        if (status == JXL_DEC_BASIC_INFO)
        {
            memset(&info, 0, sizeof(info));
            if (JxlDecoderGetBasicInfo(decoder, &info) != JXL_DEC_SUCCESS)
                break;
            if (info.xsize == 0 || info.ysize == 0 || info.xsize > 65535u || info.ysize > 65535u)
                break;
            if (info.num_color_channels != 1 && info.num_color_channels != 3)
                break;
            if (info.num_extra_channels > 1 || (info.num_extra_channels == 1 && info.alpha_bits == 0))
                break;
            width = (int)info.xsize;
            height = (int)info.ysize;
            channels = (int)info.num_color_channels + (info.alpha_bits > 0 ? 1 : 0);
            format.num_channels = (uint32_t)channels;
            continue;
        }

        if (status == JXL_DEC_NEED_IMAGE_OUT_BUFFER)
        {
            if (channels == 0)
                break;
            if (JxlDecoderImageOutBufferSize(decoder, &format, &pixel_bytes) != JXL_DEC_SUCCESS)
                break;
            free(pixels);
            pixels = (uint8_t*)malloc(pixel_bytes);
            if (pixels == NULL)
            {
                result = CHROMA_ERR_NOMEM;
                break;
            }
            if (JxlDecoderSetImageOutBuffer(decoder, &format, pixels, pixel_bytes) != JXL_DEC_SUCCESS)
                break;
            continue;
        }

        if (status == JXL_DEC_FULL_IMAGE)
        {
            size_t stride;
            if (pixels == NULL || height == 0 || pixel_bytes % (size_t)height != 0)
                break;
            stride = pixel_bytes / (size_t)height;
            if (stride < (size_t)width * (size_t)channels)
                break;
            result = expand_rgba(pixels, stride, width, height, channels, out_rgba);
            if (result == CHROMA_OK)
            {
                *out_width = width;
                *out_height = height;
            }
            saw_image = 1;
            break;
        }

        if (status == JXL_DEC_SUCCESS)
        {
            if (!saw_image)
                break;
            break;
        }

        break;
    }

    free(pixels);
    JxlDecoderDestroy(decoder);
    return result;
}

CHROMA_API int chroma_jxl_decode_anim(
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
    JxlDecoder* decoder;
    JxlBasicInfo info;
    JxlFrameHeader frame_header;
    JxlPixelFormat format;
    JxlDecoderStatus status;
    uint8_t* scratch = NULL;
    uint8_t* packed = NULL;
    int* delays = NULL;
    size_t pixel_bytes = 0;
    size_t frame_bytes = 0;
    size_t packed_cap = 0;
    int channels = 0;
    int width = 0;
    int height = 0;
    int frame_count = 0;
    int result = CHROMA_ERR;
    uint32_t tps_num = 0;
    uint32_t tps_den = 1;
    uint32_t pending_duration = 0;
    int have_animation = 0;

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

    decoder = JxlDecoderCreate(NULL);
    if (decoder == NULL)
        return CHROMA_ERR_NOMEM;

    if (
        JxlDecoderSetUnpremultiplyAlpha(decoder, JXL_TRUE) != JXL_DEC_SUCCESS
        || JxlDecoderSubscribeEvents(
               decoder,
               JXL_DEC_BASIC_INFO | JXL_DEC_FRAME | JXL_DEC_FULL_IMAGE
           )
            != JXL_DEC_SUCCESS
        || JxlDecoderSetInput(decoder, data, size) != JXL_DEC_SUCCESS
    )
    {
        JxlDecoderDestroy(decoder);
        return CHROMA_ERR;
    }
    JxlDecoderCloseInput(decoder);

    memset(&format, 0, sizeof(format));
    format.data_type = JXL_TYPE_UINT8;
    format.endianness = JXL_NATIVE_ENDIAN;
    format.align = 0;

    for (;;)
    {
        status = JxlDecoderProcessInput(decoder);
        if (status == JXL_DEC_ERROR || status == JXL_DEC_NEED_MORE_INPUT)
        {
            result = CHROMA_ERR;
            break;
        }

        if (status == JXL_DEC_BASIC_INFO)
        {
            memset(&info, 0, sizeof(info));
            if (JxlDecoderGetBasicInfo(decoder, &info) != JXL_DEC_SUCCESS)
                break;
            if (info.xsize == 0 || info.ysize == 0 || info.xsize > 65535u || info.ysize > 65535u)
                break;
            if (info.num_color_channels != 1 && info.num_color_channels != 3)
                break;
            if (info.num_extra_channels > 1 || (info.num_extra_channels == 1 && info.alpha_bits == 0))
                break;
            width = (int)info.xsize;
            height = (int)info.ysize;
            channels = (int)info.num_color_channels + (info.alpha_bits > 0 ? 1 : 0);
            format.num_channels = (uint32_t)channels;
            frame_bytes = (size_t)width * (size_t)height * 4u;
            have_animation = info.have_animation ? 1 : 0;
            if (have_animation)
            {
                tps_num = info.animation.tps_numerator;
                tps_den = info.animation.tps_denominator == 0 ? 1u : info.animation.tps_denominator;
                *out_repeat_count = (int)info.animation.num_loops;
            }
            continue;
        }

        if (status == JXL_DEC_FRAME)
        {
            memset(&frame_header, 0, sizeof(frame_header));
            if (JxlDecoderGetFrameHeader(decoder, &frame_header) != JXL_DEC_SUCCESS)
                break;
            pending_duration = have_animation ? frame_header.duration : 0;
            continue;
        }

        if (status == JXL_DEC_NEED_IMAGE_OUT_BUFFER)
        {
            if (channels == 0)
                break;
            if (JxlDecoderImageOutBufferSize(decoder, &format, &pixel_bytes) != JXL_DEC_SUCCESS)
                break;
            free(scratch);
            scratch = (uint8_t*)malloc(pixel_bytes);
            if (scratch == NULL)
            {
                result = CHROMA_ERR_NOMEM;
                break;
            }
            if (JxlDecoderSetImageOutBuffer(decoder, &format, scratch, pixel_bytes) != JXL_DEC_SUCCESS)
                break;
            continue;
        }

        if (status == JXL_DEC_FULL_IMAGE)
        {
            size_t stride;
            uint8_t* grown;
            int* grown_delays;
            if (scratch == NULL || height == 0 || pixel_bytes % (size_t)height != 0)
                break;
            stride = pixel_bytes / (size_t)height;
            if (stride < (size_t)width * (size_t)channels)
                break;
            if (frame_count >= CHROMA_MAX_FRAMES)
                break;

            if ((size_t)(frame_count + 1) * frame_bytes > packed_cap)
            {
                size_t new_cap = packed_cap == 0 ? frame_bytes * 4u : packed_cap * 2u;
                while (new_cap < (size_t)(frame_count + 1) * frame_bytes)
                    new_cap *= 2u;
                grown = (uint8_t*)realloc(packed, new_cap);
                if (grown == NULL)
                {
                    result = CHROMA_ERR_NOMEM;
                    break;
                }
                packed = grown;
                packed_cap = new_cap;
            }

            grown_delays = (int*)realloc(delays, sizeof(int) * (size_t)(frame_count + 1));
            if (grown_delays == NULL)
            {
                result = CHROMA_ERR_NOMEM;
                break;
            }
            delays = grown_delays;

            if (
                expand_rgba_into(
                    scratch,
                    stride,
                    width,
                    height,
                    channels,
                    packed + frame_bytes * (size_t)frame_count
                )
                != CHROMA_OK
            )
                break;

            delays[frame_count] = ticks_to_centiseconds(pending_duration, tps_num, tps_den);
            frame_count++;
            continue;
        }

        if (status == JXL_DEC_SUCCESS)
        {
            if (frame_count <= 0 || packed == NULL || delays == NULL)
                break;
            *out_rgba = packed;
            *out_delays_cs = delays;
            *out_width = width;
            *out_height = height;
            *out_frame_count = frame_count;
            packed = NULL;
            delays = NULL;
            result = CHROMA_OK;
            break;
        }

        break;
    }

    free(scratch);
    free(packed);
    free(delays);
    JxlDecoderDestroy(decoder);
    return result;
}

CHROMA_API int chroma_jxl_encode(
    const uint8_t* rgba,
    int width,
    int height,
    int stride,
    int quality,
    int lossless,
    uint8_t** out_bytes,
    size_t* out_size
)
{
    JxlEncoder* encoder;
    JxlEncoderFrameSettings* settings;
    JxlBasicInfo info;
    JxlColorEncoding color;
    JxlExtraChannelInfo extra;
    JxlPixelFormat format;
    JxlEncoderStatus status;
    uint8_t* compressed;
    uint8_t* next;
    size_t cap;
    size_t avail;
    int lossless_frame;
    float distance;
    size_t pixel_bytes;

    if (out_bytes == NULL || out_size == NULL)
        return CHROMA_ERR;
    *out_bytes = NULL;
    *out_size = 0;
    if (rgba == NULL || width <= 0 || height <= 0 || stride < width * 4)
        return CHROMA_ERR;
    if (quality < 0)
        quality = 0;
    if (quality > 100)
        quality = 100;

    lossless_frame = lossless || quality == 100;
    distance = lossless_frame ? 0.f : (float)(100 - quality) / 10.f;

    encoder = JxlEncoderCreate(NULL);
    if (encoder == NULL)
        return CHROMA_ERR_NOMEM;

    JxlEncoderInitBasicInfo(&info);
    info.xsize = (uint32_t)width;
    info.ysize = (uint32_t)height;
    info.bits_per_sample = 8;
    info.num_color_channels = 3;
    info.num_extra_channels = 1;
    info.alpha_bits = 8;
    info.alpha_premultiplied = JXL_FALSE;
    info.uses_original_profile = lossless_frame ? JXL_TRUE : JXL_FALSE;
    if (JxlEncoderSetBasicInfo(encoder, &info) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    memset(&color, 0, sizeof(color));
    JxlColorEncodingSetToSRGB(&color, JXL_FALSE);
    if (JxlEncoderSetColorEncoding(encoder, &color) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    JxlEncoderInitExtraChannelInfo(JXL_CHANNEL_ALPHA, &extra);
    extra.bits_per_sample = 8;
    if (JxlEncoderSetExtraChannelInfo(encoder, 0, &extra) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    settings = JxlEncoderFrameSettingsCreate(encoder, NULL);
    if (settings == NULL)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR_NOMEM;
    }
    if (
        JxlEncoderFrameSettingsSetOption(settings, JXL_ENC_FRAME_SETTING_EFFORT, 5) != JXL_ENC_SUCCESS
    )
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }
    if (lossless_frame)
    {
        if (JxlEncoderSetFrameLossless(settings, JXL_TRUE) != JXL_ENC_SUCCESS)
        {
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
    }
    else if (JxlEncoderSetFrameDistance(settings, distance) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    memset(&format, 0, sizeof(format));
    format.num_channels = 4;
    format.data_type = JXL_TYPE_UINT8;
    format.endianness = JXL_NATIVE_ENDIAN;
    format.align = 0;
    pixel_bytes = (size_t)height * (size_t)stride;
    if (JxlEncoderAddImageFrame(settings, &format, rgba, pixel_bytes) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }
    JxlEncoderCloseInput(encoder);

    cap = 65536;
    compressed = (uint8_t*)malloc(cap);
    if (compressed == NULL)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR_NOMEM;
    }
    next = compressed;
    avail = cap;
    for (;;)
    {
        size_t used;
        uint8_t* grown;
        size_t new_cap;
        status = JxlEncoderProcessOutput(encoder, &next, &avail);
        if (status == JXL_ENC_SUCCESS)
            break;
        if (status != JXL_ENC_NEED_MORE_OUTPUT)
        {
            free(compressed);
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
        used = (size_t)(next - compressed);
        new_cap = cap * 2u;
        grown = (uint8_t*)realloc(compressed, new_cap);
        if (grown == NULL)
        {
            free(compressed);
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR_NOMEM;
        }
        compressed = grown;
        next = compressed + used;
        avail = new_cap - used;
        cap = new_cap;
    }

    *out_size = (size_t)(next - compressed);
    if (*out_size == 0)
    {
        free(compressed);
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }
    *out_bytes = compressed;
    JxlEncoderDestroy(encoder);
    return CHROMA_OK;
}

CHROMA_API int chroma_jxl_encode_anim(
    const uint8_t* rgba,
    const int* delays_cs,
    int width,
    int height,
    int frame_count,
    int repeat_count,
    int quality,
    int lossless,
    uint8_t** out_bytes,
    size_t* out_size
)
{
    JxlEncoder* encoder;
    JxlEncoderFrameSettings* settings;
    JxlBasicInfo info;
    JxlColorEncoding color;
    JxlExtraChannelInfo extra;
    JxlPixelFormat format;
    JxlEncoderStatus status;
    uint8_t* compressed;
    uint8_t* next;
    size_t cap;
    size_t avail;
    int lossless_frame;
    float distance;
    size_t frame_bytes;
    size_t stride;
    int i;
    uint32_t num_loops;

    if (out_bytes == NULL || out_size == NULL)
        return CHROMA_ERR;
    *out_bytes = NULL;
    *out_size = 0;
    if (
        rgba == NULL
        || delays_cs == NULL
        || width <= 0
        || height <= 0
        || frame_count <= 0
        || frame_count > CHROMA_MAX_FRAMES
    )
        return CHROMA_ERR;
    if (quality < 0)
        quality = 0;
    if (quality > 100)
        quality = 100;

    lossless_frame = lossless || quality == 100;
    distance = lossless_frame ? 0.f : (float)(100 - quality) / 10.f;
    /* GIF RepeatCount matches JXL num_loops (0 = infinite). */
    num_loops = repeat_count <= 0 ? 0u : (uint32_t)repeat_count;

    encoder = JxlEncoderCreate(NULL);
    if (encoder == NULL)
        return CHROMA_ERR_NOMEM;

    JxlEncoderInitBasicInfo(&info);
    info.xsize = (uint32_t)width;
    info.ysize = (uint32_t)height;
    info.bits_per_sample = 8;
    info.num_color_channels = 3;
    info.num_extra_channels = 1;
    info.alpha_bits = 8;
    info.alpha_premultiplied = JXL_FALSE;
    info.uses_original_profile = lossless_frame ? JXL_TRUE : JXL_FALSE;
    info.have_animation = JXL_TRUE;
    info.animation.tps_numerator = 100;
    info.animation.tps_denominator = 1;
    info.animation.num_loops = num_loops;
    info.animation.have_timecodes = JXL_FALSE;
    if (JxlEncoderSetBasicInfo(encoder, &info) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    memset(&color, 0, sizeof(color));
    JxlColorEncodingSetToSRGB(&color, JXL_FALSE);
    if (JxlEncoderSetColorEncoding(encoder, &color) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    JxlEncoderInitExtraChannelInfo(JXL_CHANNEL_ALPHA, &extra);
    extra.bits_per_sample = 8;
    if (JxlEncoderSetExtraChannelInfo(encoder, 0, &extra) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    settings = JxlEncoderFrameSettingsCreate(encoder, NULL);
    if (settings == NULL)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR_NOMEM;
    }
    if (
        JxlEncoderFrameSettingsSetOption(settings, JXL_ENC_FRAME_SETTING_EFFORT, 5) != JXL_ENC_SUCCESS
    )
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }
    if (lossless_frame)
    {
        if (JxlEncoderSetFrameLossless(settings, JXL_TRUE) != JXL_ENC_SUCCESS)
        {
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
    }
    else if (JxlEncoderSetFrameDistance(settings, distance) != JXL_ENC_SUCCESS)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }

    memset(&format, 0, sizeof(format));
    format.num_channels = 4;
    format.data_type = JXL_TYPE_UINT8;
    format.endianness = JXL_NATIVE_ENDIAN;
    format.align = 0;

    stride = (size_t)width * 4u;
    frame_bytes = stride * (size_t)height;
    for (i = 0; i < frame_count; i++)
    {
        JxlFrameHeader header;
        uint32_t duration;

        JxlEncoderInitFrameHeader(&header);
        duration = delays_cs[i] < 0 ? 0u : (uint32_t)delays_cs[i];
        if (duration == 0)
            duration = 1;
        header.duration = duration;
        if (JxlEncoderSetFrameHeader(settings, &header) != JXL_ENC_SUCCESS)
        {
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
        if (
            JxlEncoderAddImageFrame(
                settings,
                &format,
                rgba + frame_bytes * (size_t)i,
                frame_bytes
            )
            != JXL_ENC_SUCCESS
        )
        {
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
    }
    JxlEncoderCloseInput(encoder);

    cap = 65536;
    compressed = (uint8_t*)malloc(cap);
    if (compressed == NULL)
    {
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR_NOMEM;
    }
    next = compressed;
    avail = cap;
    for (;;)
    {
        size_t used;
        uint8_t* grown;
        size_t new_cap;
        status = JxlEncoderProcessOutput(encoder, &next, &avail);
        if (status == JXL_ENC_SUCCESS)
            break;
        if (status != JXL_ENC_NEED_MORE_OUTPUT)
        {
            free(compressed);
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
        used = (size_t)(next - compressed);
        new_cap = cap * 2u;
        grown = (uint8_t*)realloc(compressed, new_cap);
        if (grown == NULL)
        {
            free(compressed);
            JxlEncoderDestroy(encoder);
            return CHROMA_ERR_NOMEM;
        }
        compressed = grown;
        next = compressed + used;
        avail = new_cap - used;
        cap = new_cap;
    }

    *out_size = (size_t)(next - compressed);
    if (*out_size == 0)
    {
        free(compressed);
        JxlEncoderDestroy(encoder);
        return CHROMA_ERR;
    }
    *out_bytes = compressed;
    JxlEncoderDestroy(encoder);
    return CHROMA_OK;
}
