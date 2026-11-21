/*
 * One-shot tool: write tiny 2-frame animated AVIF / JXL / WebP fixtures for tests.
 * Built against native/.cache static libs (see gen_anim_fixtures.sh).
 */
#include <avif/avif.h>
#include <jxl/encode.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>
#include <webp/mux.h>

static void fill_solid(uint8_t* rgba, int w, int h, uint8_t r, uint8_t g, uint8_t b, uint8_t a)
{
    int i;
    for (i = 0; i < w * h; i++)
    {
        rgba[i * 4 + 0] = r;
        rgba[i * 4 + 1] = g;
        rgba[i * 4 + 2] = b;
        rgba[i * 4 + 3] = a;
    }
}

static int write_file(const char* path, const uint8_t* data, size_t size)
{
    FILE* f = fopen(path, "wb");
    if (!f)
        return 1;
    if (fwrite(data, 1, size, f) != size)
    {
        fclose(f);
        return 1;
    }
    fclose(f);
    printf("wrote %s (%zu bytes)\n", path, size);
    return 0;
}

static int write_avif(const char* path, const uint8_t* f0, const uint8_t* f1, int w, int h)
{
    avifEncoder* enc = avifEncoderCreate();
    avifImage* img0 = NULL;
    avifImage* img1 = NULL;
    avifRGBImage rgb;
    avifRWData out = AVIF_DATA_EMPTY;
    int rc = 1;

    if (!enc)
        return 1;
    enc->maxThreads = 1;
    enc->speed = 6;
    enc->quality = AVIF_QUALITY_LOSSLESS;
    enc->qualityAlpha = AVIF_QUALITY_LOSSLESS;
    enc->timescale = 100;
    enc->repetitionCount = 0;

    img0 = avifImageCreate((uint32_t)w, (uint32_t)h, 8, AVIF_PIXEL_FORMAT_YUV444);
    img1 = avifImageCreate((uint32_t)w, (uint32_t)h, 8, AVIF_PIXEL_FORMAT_YUV444);
    if (!img0 || !img1)
        goto done;
    img0->matrixCoefficients = AVIF_MATRIX_COEFFICIENTS_IDENTITY;
    img0->yuvRange = AVIF_RANGE_FULL;
    img1->matrixCoefficients = AVIF_MATRIX_COEFFICIENTS_IDENTITY;
    img1->yuvRange = AVIF_RANGE_FULL;

    memset(&rgb, 0, sizeof(rgb));
    avifRGBImageSetDefaults(&rgb, img0);
    rgb.format = AVIF_RGB_FORMAT_RGBA;
    rgb.depth = 8;
    rgb.rowBytes = (uint32_t)(w * 4);
    rgb.pixels = (uint8_t*)f0;
    if (avifImageRGBToYUV(img0, &rgb) != AVIF_RESULT_OK)
        goto done;
    rgb.pixels = (uint8_t*)f1;
    if (avifImageRGBToYUV(img1, &rgb) != AVIF_RESULT_OK)
        goto done;

    if (avifEncoderAddImage(enc, img0, 10, AVIF_ADD_IMAGE_FLAG_NONE) != AVIF_RESULT_OK)
        goto done;
    if (avifEncoderAddImage(enc, img1, 10, AVIF_ADD_IMAGE_FLAG_NONE) != AVIF_RESULT_OK)
        goto done;
    if (avifEncoderFinish(enc, &out) != AVIF_RESULT_OK || !out.data)
        goto done;
    rc = write_file(path, out.data, out.size);

done:
    avifRWDataFree(&out);
    if (img0)
        avifImageDestroy(img0);
    if (img1)
        avifImageDestroy(img1);
    avifEncoderDestroy(enc);
    return rc;
}

static int write_jxl(const char* path, const uint8_t* f0, const uint8_t* f1, int w, int h)
{
    JxlEncoder* enc = JxlEncoderCreate(NULL);
    JxlEncoderFrameSettings* settings;
    JxlBasicInfo info;
    JxlColorEncoding color;
    JxlExtraChannelInfo extra;
    JxlFrameHeader header;
    JxlPixelFormat format;
    uint8_t* compressed = NULL;
    uint8_t* next;
    size_t cap = 65536;
    size_t avail;
    JxlEncoderStatus st;
    int rc = 1;
    size_t stride = (size_t)w * 4u;

    if (!enc)
        return 1;
    JxlEncoderInitBasicInfo(&info);
    info.xsize = (uint32_t)w;
    info.ysize = (uint32_t)h;
    info.bits_per_sample = 8;
    info.num_color_channels = 3;
    info.num_extra_channels = 1;
    info.alpha_bits = 8;
    info.uses_original_profile = JXL_TRUE;
    info.have_animation = JXL_TRUE;
    info.animation.tps_numerator = 100;
    info.animation.tps_denominator = 1;
    info.animation.num_loops = 0;
    if (JxlEncoderSetBasicInfo(enc, &info) != JXL_ENC_SUCCESS)
        goto done;
    memset(&color, 0, sizeof(color));
    JxlColorEncodingSetToSRGB(&color, JXL_FALSE);
    if (JxlEncoderSetColorEncoding(enc, &color) != JXL_ENC_SUCCESS)
        goto done;
    JxlEncoderInitExtraChannelInfo(JXL_CHANNEL_ALPHA, &extra);
    extra.bits_per_sample = 8;
    if (JxlEncoderSetExtraChannelInfo(enc, 0, &extra) != JXL_ENC_SUCCESS)
        goto done;

    settings = JxlEncoderFrameSettingsCreate(enc, NULL);
    if (!settings)
        goto done;
    if (JxlEncoderSetFrameLossless(settings, JXL_TRUE) != JXL_ENC_SUCCESS)
        goto done;

    memset(&format, 0, sizeof(format));
    format.num_channels = 4;
    format.data_type = JXL_TYPE_UINT8;
    format.endianness = JXL_NATIVE_ENDIAN;

    JxlEncoderInitFrameHeader(&header);
    header.duration = 10;
    if (JxlEncoderSetFrameHeader(settings, &header) != JXL_ENC_SUCCESS)
        goto done;
    if (JxlEncoderAddImageFrame(settings, &format, f0, stride * (size_t)h) != JXL_ENC_SUCCESS)
        goto done;
    if (JxlEncoderSetFrameHeader(settings, &header) != JXL_ENC_SUCCESS)
        goto done;
    if (JxlEncoderAddImageFrame(settings, &format, f1, stride * (size_t)h) != JXL_ENC_SUCCESS)
        goto done;
    JxlEncoderCloseInput(enc);

    compressed = (uint8_t*)malloc(cap);
    if (!compressed)
        goto done;
    next = compressed;
    avail = cap;
    for (;;)
    {
        st = JxlEncoderProcessOutput(enc, &next, &avail);
        if (st == JXL_ENC_SUCCESS)
            break;
        if (st != JXL_ENC_NEED_MORE_OUTPUT)
            goto done;
        {
            size_t used = (size_t)(next - compressed);
            size_t new_cap = cap * 2;
            uint8_t* grown = (uint8_t*)realloc(compressed, new_cap);
            if (!grown)
                goto done;
            compressed = grown;
            next = compressed + used;
            avail = new_cap - used;
            cap = new_cap;
        }
    }
    rc = write_file(path, compressed, (size_t)(next - compressed));

done:
    free(compressed);
    JxlEncoderDestroy(enc);
    return rc;
}

static int write_webp(const char* path, const uint8_t* f0, const uint8_t* f1, int w, int h)
{
    WebPAnimEncoderOptions opts;
    WebPAnimEncoder* enc;
    WebPConfig config;
    WebPPicture pic;
    WebPData webp;
    int rc = 1;
    int stride = w * 4;

    memset(&pic, 0, sizeof(pic));
    memset(&webp, 0, sizeof(webp));
    if (!WebPAnimEncoderOptionsInit(&opts))
        return 1;
    opts.anim_params.loop_count = 0;
    enc = WebPAnimEncoderNew(w, h, &opts);
    if (!enc)
        return 1;
    if (!WebPConfigInit(&config) || !WebPConfigLosslessPreset(&config, 0))
        goto done;

    if (!WebPPictureInit(&pic))
        goto done;
    pic.width = w;
    pic.height = h;
    pic.use_argb = 1;
    if (!WebPPictureImportRGBA(&pic, f0, stride))
        goto done;
    if (!WebPAnimEncoderAdd(enc, &pic, 0, &config))
        goto done;
    WebPPictureFree(&pic);

    if (!WebPPictureInit(&pic))
        goto done;
    pic.width = w;
    pic.height = h;
    pic.use_argb = 1;
    if (!WebPPictureImportRGBA(&pic, f1, stride))
        goto done;
    if (!WebPAnimEncoderAdd(enc, &pic, 100, &config))
        goto done;
    WebPPictureFree(&pic);
    memset(&pic, 0, sizeof(pic));

    if (!WebPAnimEncoderAdd(enc, NULL, 200, &config))
        goto done;
    if (!WebPAnimEncoderAssemble(enc, &webp))
        goto done;
    rc = write_file(path, webp.bytes, webp.size);

done:
    WebPDataClear(&webp);
    if (pic.argb)
        WebPPictureFree(&pic);
    WebPAnimEncoderDelete(enc);
    return rc;
}

int main(int argc, char** argv)
{
    const char* out_dir;
    char path[1024];
    uint8_t f0[2 * 2 * 4];
    uint8_t f1[2 * 2 * 4];
    int failed = 0;

    if (argc != 2)
    {
        fprintf(stderr, "usage: %s <out-dir>\n", argv[0]);
        return 1;
    }
    out_dir = argv[1];
    fill_solid(f0, 2, 2, 255, 0, 0, 255);
    fill_solid(f1, 2, 2, 0, 0, 255, 255);

    snprintf(path, sizeof(path), "%s/anim-2x2.avif", out_dir);
    failed |= write_avif(path, f0, f1, 2, 2);
    snprintf(path, sizeof(path), "%s/anim-2x2.jxl", out_dir);
    failed |= write_jxl(path, f0, f1, 2, 2);
    snprintf(path, sizeof(path), "%s/anim-2x2.webp", out_dir);
    failed |= write_webp(path, f0, f1, 2, 2);
    return failed ? 1 : 0;
}
