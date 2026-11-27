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

/* Stable ABI over libavif 1.3.x. Built by native/build.sh into libchromaavif. */

#include <avif/avif.h>

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

#define CHROMA_MAX_DIM 1000000u
#define CHROMA_MAX_FRAMES 10000

CHROMA_API void chroma_avif_free(void* ptr) { free(ptr); }

static int duration_to_centiseconds(double duration_seconds)
{
    double cs;
    if (duration_seconds <= 0.0)
        return 0;
    cs = duration_seconds * 100.0 + 0.5;
    if (cs > 32767.0)
        return 32767;
    return (int)cs;
}

/* AVIF repetitionCount: -1 infinite, 0 play once, N = N extra repeats.
 * Animation.RepeatCount: 0 infinite, N = play N times. */
static int map_repeat_count(int avif_repetition)
{
    if (avif_repetition < 0)
        return 0;
    if (avif_repetition >= 65534)
        return 65535;
    return avif_repetition + 1;
}

static int copy_frame_rgba(const avifImage* image, uint8_t* dest)
{
    avifRGBImage rgb;
    avifResult result;

    memset(&rgb, 0, sizeof(rgb));
    avifRGBImageSetDefaults(&rgb, image);
    rgb.format = AVIF_RGB_FORMAT_RGBA;
    rgb.depth = 8;
    rgb.pixels = dest;
    rgb.rowBytes = image->width * 4u;
    result = avifImageYUVToRGB(image, &rgb);
    return result == AVIF_RESULT_OK ? CHROMA_OK : CHROMA_ERR;
}

CHROMA_API int chroma_avif_decode(
    const uint8_t* data,
    size_t size,
    uint8_t** out_rgba,
    int* out_width,
    int* out_height
)
{
    avifDecoder* decoder;
    avifResult result;
    uint32_t width;
    uint32_t height;
    uint8_t* pixels;

    if (out_rgba == NULL || out_width == NULL || out_height == NULL)
        return CHROMA_ERR;
    *out_rgba = NULL;
    *out_width = 0;
    *out_height = 0;
    if (data == NULL || size == 0)
        return CHROMA_ERR;

    decoder = avifDecoderCreate();
    if (decoder == NULL)
        return CHROMA_ERR_NOMEM;

    result = avifDecoderSetIOMemory(decoder, data, size);
    if (result != AVIF_RESULT_OK)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    result = avifDecoderParse(decoder);
    if (result != AVIF_RESULT_OK)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    result = avifDecoderNextImage(decoder);
    if (result != AVIF_RESULT_OK || decoder->image == NULL)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    width = decoder->image->width;
    height = decoder->image->height;
    if (width == 0 || height == 0 || width > CHROMA_MAX_DIM || height > CHROMA_MAX_DIM)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    pixels = (uint8_t*)malloc((size_t)width * (size_t)height * 4u);
    if (pixels == NULL)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR_NOMEM;
    }

    if (copy_frame_rgba(decoder->image, pixels) != CHROMA_OK)
    {
        free(pixels);
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    avifDecoderDestroy(decoder);
    *out_rgba = pixels;
    *out_width = (int)width;
    *out_height = (int)height;
    return CHROMA_OK;
}

CHROMA_API int chroma_avif_decode_anim(
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
    avifDecoder* decoder;
    avifResult result;
    uint32_t width = 0;
    uint32_t height = 0;
    uint32_t frame_count;
    uint32_t i;
    size_t frame_bytes;
    uint8_t* pixels = NULL;
    int* delays = NULL;

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

    decoder = avifDecoderCreate();
    if (decoder == NULL)
        return CHROMA_ERR_NOMEM;

    result = avifDecoderSetIOMemory(decoder, data, size);
    if (result != AVIF_RESULT_OK)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    result = avifDecoderParse(decoder);
    if (result != AVIF_RESULT_OK)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    frame_count = decoder->imageCount;
    if (frame_count == 0 || frame_count > CHROMA_MAX_FRAMES)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    for (i = 0; i < frame_count; i++)
    {
        result = avifDecoderNextImage(decoder);
        if (result != AVIF_RESULT_OK || decoder->image == NULL)
        {
            free(pixels);
            free(delays);
            avifDecoderDestroy(decoder);
            return CHROMA_ERR;
        }

        if (i == 0)
        {
            width = decoder->image->width;
            height = decoder->image->height;
            if (width == 0 || height == 0 || width > CHROMA_MAX_DIM || height > CHROMA_MAX_DIM)
            {
                avifDecoderDestroy(decoder);
                return CHROMA_ERR;
            }
            frame_bytes = (size_t)width * (size_t)height * 4u;
            pixels = (uint8_t*)malloc(frame_bytes * (size_t)frame_count);
            delays = (int*)malloc(sizeof(int) * (size_t)frame_count);
            if (pixels == NULL || delays == NULL)
            {
                free(pixels);
                free(delays);
                avifDecoderDestroy(decoder);
                return CHROMA_ERR_NOMEM;
            }
        }
        else if (decoder->image->width != width || decoder->image->height != height)
        {
            free(pixels);
            free(delays);
            avifDecoderDestroy(decoder);
            return CHROMA_ERR;
        }

        if (copy_frame_rgba(decoder->image, pixels + frame_bytes * (size_t)i) != CHROMA_OK)
        {
            free(pixels);
            free(delays);
            avifDecoderDestroy(decoder);
            return CHROMA_ERR;
        }

        delays[i] = duration_to_centiseconds(decoder->imageTiming.duration);
    }

    *out_rgba = pixels;
    *out_delays_cs = delays;
    *out_width = (int)width;
    *out_height = (int)height;
    *out_frame_count = (int)frame_count;
    *out_repeat_count = map_repeat_count(decoder->repetitionCount);
    avifDecoderDestroy(decoder);
    return CHROMA_OK;
}

CHROMA_API int chroma_avif_encode(
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
    avifImage* image;
    avifRGBImage rgb;
    avifEncoder* encoder;
    avifRWData output = AVIF_DATA_EMPTY;
    avifResult result;
    uint8_t* copy;

    if (out_data == NULL || out_size == NULL)
        return CHROMA_ERR;
    *out_data = NULL;
    *out_size = 0;
    if (rgba == NULL || width <= 0 || height <= 0 || stride < width * 4)
        return CHROMA_ERR;

    if (quality < 0)
        quality = 60;
    if (quality > 100)
        quality = 100;

    image = avifImageCreate(
        (uint32_t)width,
        (uint32_t)height,
        8,
        lossless ? AVIF_PIXEL_FORMAT_YUV444 : AVIF_PIXEL_FORMAT_YUV420
    );
    if (image == NULL)
        return CHROMA_ERR_NOMEM;

    if (lossless)
    {
        image->matrixCoefficients = AVIF_MATRIX_COEFFICIENTS_IDENTITY;
        image->yuvRange = AVIF_RANGE_FULL;
        quality = AVIF_QUALITY_LOSSLESS;
    }

    memset(&rgb, 0, sizeof(rgb));
    avifRGBImageSetDefaults(&rgb, image);
    rgb.format = AVIF_RGB_FORMAT_RGBA;
    rgb.depth = 8;
    rgb.pixels = (uint8_t*)rgba;
    rgb.rowBytes = (uint32_t)stride;

    result = avifImageRGBToYUV(image, &rgb);
    if (result != AVIF_RESULT_OK)
    {
        avifImageDestroy(image);
        return CHROMA_ERR;
    }

    encoder = avifEncoderCreate();
    if (encoder == NULL)
    {
        avifImageDestroy(image);
        return CHROMA_ERR_NOMEM;
    }

    encoder->maxThreads = 1;
    encoder->speed = 6;
    encoder->quality = quality;
    encoder->qualityAlpha = quality;

    result = avifEncoderWrite(encoder, image, &output);
    avifEncoderDestroy(encoder);
    avifImageDestroy(image);
    if (result != AVIF_RESULT_OK || output.data == NULL || output.size == 0)
    {
        avifRWDataFree(&output);
        return CHROMA_ERR;
    }

    copy = (uint8_t*)malloc(output.size);
    if (copy == NULL)
    {
        avifRWDataFree(&output);
        return CHROMA_ERR_NOMEM;
    }

    memcpy(copy, output.data, output.size);
    *out_data = copy;
    *out_size = output.size;
    avifRWDataFree(&output);
    return CHROMA_OK;
}

/* GIF RepeatCount → AVIF: 0 infinite → -1; N plays → N-1 extra repeats. */
static int unmap_repeat_count(int gif_repeat)
{
    if (gif_repeat <= 0)
        return AVIF_REPETITION_COUNT_INFINITE;
    return gif_repeat - 1;
}

CHROMA_API int chroma_avif_encode_anim(
    const uint8_t* rgba,
    const int* delays_cs,
    int width,
    int height,
    int frame_count,
    int repeat_count,
    int quality,
    int lossless,
    uint8_t** out_data,
    size_t* out_size
)
{
    avifEncoder* encoder;
    avifRWData output = AVIF_DATA_EMPTY;
    avifResult result;
    uint8_t* copy;
    size_t frame_bytes;
    int i;
    int q;

    if (out_data == NULL || out_size == NULL)
        return CHROMA_ERR;
    *out_data = NULL;
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

    q = quality;
    if (q < 0)
        q = 60;
    if (q > 100)
        q = 100;

    encoder = avifEncoderCreate();
    if (encoder == NULL)
        return CHROMA_ERR_NOMEM;

    encoder->maxThreads = 1;
    encoder->speed = 6;
    encoder->timescale = 100; /* delays are GIF centiseconds */
    encoder->repetitionCount = unmap_repeat_count(repeat_count);
    if (lossless)
        q = AVIF_QUALITY_LOSSLESS;
    encoder->quality = q;
    encoder->qualityAlpha = q;

    frame_bytes = (size_t)width * (size_t)height * 4u;
    for (i = 0; i < frame_count; i++)
    {
        avifImage* image;
        avifRGBImage rgb;
        uint64_t duration;

        image = avifImageCreate(
            (uint32_t)width,
            (uint32_t)height,
            8,
            lossless ? AVIF_PIXEL_FORMAT_YUV444 : AVIF_PIXEL_FORMAT_YUV420
        );
        if (image == NULL)
        {
            avifEncoderDestroy(encoder);
            return CHROMA_ERR_NOMEM;
        }

        if (lossless)
        {
            image->matrixCoefficients = AVIF_MATRIX_COEFFICIENTS_IDENTITY;
            image->yuvRange = AVIF_RANGE_FULL;
        }

        memset(&rgb, 0, sizeof(rgb));
        avifRGBImageSetDefaults(&rgb, image);
        rgb.format = AVIF_RGB_FORMAT_RGBA;
        rgb.depth = 8;
        rgb.pixels = (uint8_t*)(rgba + frame_bytes * (size_t)i);
        rgb.rowBytes = (uint32_t)(width * 4);

        result = avifImageRGBToYUV(image, &rgb);
        if (result != AVIF_RESULT_OK)
        {
            avifImageDestroy(image);
            avifEncoderDestroy(encoder);
            return CHROMA_ERR;
        }

        duration = delays_cs[i] < 0 ? 0u : (uint64_t)delays_cs[i];
        if (duration == 0)
            duration = 1;
        result = avifEncoderAddImage(encoder, image, duration, AVIF_ADD_IMAGE_FLAG_NONE);
        avifImageDestroy(image);
        if (result != AVIF_RESULT_OK)
        {
            avifEncoderDestroy(encoder);
            return CHROMA_ERR;
        }
    }

    result = avifEncoderFinish(encoder, &output);
    avifEncoderDestroy(encoder);
    if (result != AVIF_RESULT_OK || output.data == NULL || output.size == 0)
    {
        avifRWDataFree(&output);
        return CHROMA_ERR;
    }

    copy = (uint8_t*)malloc(output.size);
    if (copy == NULL)
    {
        avifRWDataFree(&output);
        return CHROMA_ERR_NOMEM;
    }

    memcpy(copy, output.data, output.size);
    *out_data = copy;
    *out_size = output.size;
    avifRWDataFree(&output);
    return CHROMA_OK;
}
