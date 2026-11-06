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

CHROMA_API void chroma_avif_free(void* ptr) { free(ptr); }

CHROMA_API int chroma_avif_decode(
    const uint8_t* data,
    size_t size,
    uint8_t** out_rgba,
    int* out_width,
    int* out_height
)
{
    avifDecoder* decoder;
    avifRGBImage rgb;
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

    if (decoder->imageSequenceTrackPresent)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR_ANIMATION;
    }

    result = avifDecoderNextImage(decoder);
    if (result != AVIF_RESULT_OK || decoder->image == NULL)
    {
        avifDecoderDestroy(decoder);
        return CHROMA_ERR;
    }

    width = decoder->image->width;
    height = decoder->image->height;
    if (width == 0 || height == 0 || width > 1000000u || height > 1000000u)
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

    memset(&rgb, 0, sizeof(rgb));
    avifRGBImageSetDefaults(&rgb, decoder->image);
    rgb.format = AVIF_RGB_FORMAT_RGBA;
    rgb.depth = 8;
    rgb.pixels = pixels;
    rgb.rowBytes = width * 4u;

    result = avifImageYUVToRGB(decoder->image, &rgb);
    avifDecoderDestroy(decoder);
    if (result != AVIF_RESULT_OK)
    {
        free(pixels);
        return CHROMA_ERR;
    }

    *out_rgba = pixels;
    *out_width = (int)width;
    *out_height = (int)height;
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
