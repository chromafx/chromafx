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
 *
 * Adapted from torcado's cleanEdge shader (Copyright 2022 torcado), MIT license:
 * https://gist.github.com/torcado194/e2794f5a4b22049ac0a41f972d14c329
 */

using System.Numerics;
using System.Runtime.CompilerServices;
using ChromaFx.Core.Colors;

namespace ChromaFx.PixelArt;

/// <summary>CPU port of torcado cleanEdge (slopes; cleanup optional).</summary>
internal static class CleanEdgeCore
{
    private static readonly Vector4 NoSlice = new(-1f, -1f, -1f, -1f);

    /// <summary>
    /// Upscales <paramref name="src"/> by <paramref name="scale"/> into <paramref name="dst"/>.
    /// Processes each source pixel’s 4 local quadrants once (shared neighborhood).
    /// </summary>
    public static void Upscale(
        Color[] src,
        int srcW,
        int srcH,
        Color[] dst,
        int scale,
        Vector3 highestColor,
        float similarThreshold,
        float lineWidth
    )
    {
        var dstW = srcW * scale;
        // First output index in the cell where local >= 0.5 (GLSL floor(local+0.5) → +1).
        var mid = (int)Math.Ceiling(scale / 2.0 - 0.5);

        Parallel.For(
            0,
            srcH,
            sy =>
            {
                Span<Vector4> n = stackalloc Vector4[21];
                for (var sx = 0; sx < srcW; ++sx)
                {
                    var baseOx = sx * scale;
                    var baseOy = sy * scale;

                    // Four quadrants: local x/y below or ≥ 0.5 → pointDir ±1.
                    for (var qy = 0; qy < 2; ++qy)
                    {
                        var dirY = qy == 0 ? -1f : 1f;
                        var i0 = qy == 0 ? 0 : mid;
                        var i1 = qy == 0 ? mid : scale;

                        for (var qx = 0; qx < 2; ++qx)
                        {
                            var dirX = qx == 0 ? -1f : 1f;
                            var j0 = qx == 0 ? 0 : mid;
                            var j1 = qx == 0 ? mid : scale;
                            var pointDir = new Vector2(dirX, dirY);

                            LoadNeighborhood(src, srcW, srcH, sx, sy, pointDir, n);

                            for (var i = i0; i < i1; ++i)
                            {
                                var localY = (i + 0.5f) / scale;
                                for (var j = j0; j < j1; ++j)
                                {
                                    var localX = (j + 0.5f) / scale;
                                    var local = new Vector2(localX, localY);
                                    dst[(baseOy + i) * dstW + (baseOx + j)] = Evaluate(
                                        local,
                                        pointDir,
                                        n,
                                        highestColor,
                                        similarThreshold,
                                        lineWidth,
                                        enableCleanup: true
                                    );
                                }
                            }
                        }
                    }
                }
            }
        );
    }

    /// <summary>
    /// Samples cleanEdge at continuous source coords (pixel edges at integers), matching
    /// GLSL <c>fract</c> / <c>ceil(px)-0.5</c> / <c>pointDir</c>.
    /// </summary>
    public static Color SampleAt(
        Color[] src,
        int srcW,
        int srcH,
        float fx,
        float fy,
        Vector3 highestColor,
        float similarThreshold,
        float lineWidth,
        bool enableCleanup
    )
    {
        var local = new Vector2(Fract(fx), Fract(fy));
        var sx = (int)MathF.Ceiling(fx) - 1;
        var sy = (int)MathF.Ceiling(fy) - 1;
        var pointDir = new Vector2(MathF.Round(local.X) * 2f - 1f, MathF.Round(local.Y) * 2f - 1f);

        Span<Vector4> n = stackalloc Vector4[21];
        LoadNeighborhood(src, srcW, srcH, sx, sy, pointDir, n);
        return Evaluate(
            local,
            pointDir,
            n,
            highestColor,
            similarThreshold,
            lineWidth,
            enableCleanup
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fract(float v) => v - MathF.Floor(v);

    private static void LoadNeighborhood(
        Color[] src,
        int srcW,
        int srcH,
        int sx,
        int sy,
        Vector2 pointDir,
        Span<Vector4> n
    )
    {
        // Layout matches Sample()'s former locals: uub,uu,uuf, ubb,ub,u,uf,uff, bb,b,c,f,ff, dbb,db,d,df,dff, ddb,dd,ddf
        n[0] = ToVec(At(src, srcW, srcH, sx, sy, -1, -2, pointDir)); // uub
        n[1] = ToVec(At(src, srcW, srcH, sx, sy, 0, -2, pointDir)); // uu
        n[2] = ToVec(At(src, srcW, srcH, sx, sy, 1, -2, pointDir)); // uuf
        n[3] = ToVec(At(src, srcW, srcH, sx, sy, -2, -2, pointDir)); // ubb
        n[4] = ToVec(At(src, srcW, srcH, sx, sy, -1, -1, pointDir)); // ub
        n[5] = ToVec(At(src, srcW, srcH, sx, sy, 0, -1, pointDir)); // u
        n[6] = ToVec(At(src, srcW, srcH, sx, sy, 1, -1, pointDir)); // uf
        n[7] = ToVec(At(src, srcW, srcH, sx, sy, 2, -1, pointDir)); // uff
        n[8] = ToVec(At(src, srcW, srcH, sx, sy, -2, 0, pointDir)); // bb
        n[9] = ToVec(At(src, srcW, srcH, sx, sy, -1, 0, pointDir)); // b
        n[10] = ToVec(At(src, srcW, srcH, sx, sy, 0, 0, pointDir)); // c
        n[11] = ToVec(At(src, srcW, srcH, sx, sy, 1, 0, pointDir)); // f
        n[12] = ToVec(At(src, srcW, srcH, sx, sy, 2, 0, pointDir)); // ff
        n[13] = ToVec(At(src, srcW, srcH, sx, sy, -2, 1, pointDir)); // dbb
        n[14] = ToVec(At(src, srcW, srcH, sx, sy, -1, 1, pointDir)); // db
        n[15] = ToVec(At(src, srcW, srcH, sx, sy, 0, 1, pointDir)); // d
        n[16] = ToVec(At(src, srcW, srcH, sx, sy, 1, 1, pointDir)); // df
        n[17] = ToVec(At(src, srcW, srcH, sx, sy, 2, 1, pointDir)); // dff
        n[18] = ToVec(At(src, srcW, srcH, sx, sy, -1, 2, pointDir)); // ddb
        n[19] = ToVec(At(src, srcW, srcH, sx, sy, 0, 2, pointDir)); // dd
        n[20] = ToVec(At(src, srcW, srcH, sx, sy, 1, 2, pointDir)); // ddf
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color At(
        Color[] src,
        int srcW,
        int srcH,
        int sx,
        int sy,
        int dx,
        int dy,
        Vector2 pointDir
    )
    {
        // Matches Floor((sx+0.5) + dx*pointDir) for integer dx/dy and ±1 pointDir.
        var x = sx + (int)(dx * pointDir.X);
        var y = sy + (int)(dy * pointDir.Y);
        if (x < 0)
            x = 0;
        else if (x >= srcW)
            x = srcW - 1;
        if (y < 0)
            y = 0;
        else if (y >= srcH)
            y = srcH - 1;
        return src[y * srcW + x];
    }

    private static Color Evaluate(
        Vector2 local,
        Vector2 pointDir,
        ReadOnlySpan<Vector4> n,
        Vector3 highestColor,
        float similarThreshold,
        float lineWidth,
        bool enableCleanup
    )
    {
        var uub = n[0];
        var uu = n[1];
        var uuf = n[2];
        var ubb = n[3];
        var ub = n[4];
        var u = n[5];
        var uf = n[6];
        var uff = n[7];
        var bb = n[8];
        var b = n[9];
        var c = n[10];
        var f = n[11];
        var ff = n[12];
        var dbb = n[13];
        var db = n[14];
        var d = n[15];
        var df = n[16];
        var dff = n[17];
        var ddb = n[18];
        var dd = n[19];
        var ddf = n[20];

        var col = c;
        var cCol = SliceDist(
            local,
            new Vector2(1, 1),
            pointDir,
            ub,
            u,
            uf,
            uff,
            b,
            c,
            f,
            ff,
            db,
            d,
            df,
            dff,
            ddb,
            dd,
            ddf,
            highestColor,
            similarThreshold,
            lineWidth,
            enableCleanup
        );
        var bCol = SliceDist(
            local,
            new Vector2(-1, 1),
            pointDir,
            uf,
            u,
            ub,
            ubb,
            f,
            c,
            b,
            bb,
            df,
            d,
            db,
            dbb,
            ddf,
            dd,
            ddb,
            highestColor,
            similarThreshold,
            lineWidth,
            enableCleanup
        );
        var uCol = SliceDist(
            local,
            new Vector2(1, -1),
            pointDir,
            db,
            d,
            df,
            dff,
            b,
            c,
            f,
            ff,
            ub,
            u,
            uf,
            uff,
            uub,
            uu,
            uuf,
            highestColor,
            similarThreshold,
            lineWidth,
            enableCleanup
        );

        if (cCol.X >= 0f)
            col = cCol;
        if (bCol.X >= 0f)
            col = bCol;
        if (uCol.X >= 0f)
            col = uCol;

        return FromVec(col);
    }

    private static Vector4 SliceDist(
        Vector2 point,
        Vector2 mainDir,
        Vector2 pointDir,
        Vector4 ub,
        Vector4 u,
        Vector4 uf,
        Vector4 uff,
        Vector4 b,
        Vector4 c,
        Vector4 f,
        Vector4 ff,
        Vector4 db,
        Vector4 d,
        Vector4 df,
        Vector4 dff,
        Vector4 ddb,
        Vector4 dd,
        Vector4 ddf,
        Vector3 highestColor,
        float similarThreshold,
        float lineWidth,
        bool enableCleanup
    )
    {
        const float minWidth = 0.45f;
        const float maxWidth = 1.142f;
        var lw = Math.Clamp(lineWidth, minWidth, maxWidth);
        point = mainDir * (point - new Vector2(0.5f)) + new Vector2(0.5f);

        var distAgainst = 4f * Cd(f, d) + Cd(uf, c) + Cd(c, db) + Cd(ff, df) + Cd(df, dd);
        var distTowards = 4f * Cd(c, df) + Cd(u, f) + Cd(f, dff) + Cd(b, d) + Cd(d, ddf);
        var shouldSlice =
            distAgainst < distTowards
            || (distAgainst < distTowards + 0.001f && !Higher(c, f, highestColor, similarThreshold));
        if (
            Similar4(f, d, b, u, similarThreshold)
            && Similar4(uf, df, db, ub, similarThreshold)
            && !Similar(c, f, similarThreshold)
        )
            shouldSlice = false;
        if (!shouldSlice)
            return NoSlice;

        var dist = 1f;
        var flip = false;
        var center = new Vector2(0.5f, 0.5f);

        if (
            Similar3(f, d, db, similarThreshold)
            && !Similar3(f, d, b, similarThreshold)
            && !Similar(uf, db, similarThreshold)
        )
        {
            if (!(Similar(c, df, similarThreshold) && Higher(c, f, highestColor, similarThreshold)))
            {
                if (Higher(c, f, highestColor, similarThreshold))
                    flip = true;
                if (
                    Similar(u, f, similarThreshold)
                    && !Similar(c, df, similarThreshold)
                    && !Higher(c, u, highestColor, similarThreshold)
                )
                    flip = true;
            }

            dist = flip
                ? lw
                    - DistToLine(
                        point,
                        center + new Vector2(1.5f, -1f) * pointDir,
                        center + new Vector2(-0.5f, 0f) * pointDir,
                        -pointDir
                    )
                : DistToLine(
                    point,
                    center + new Vector2(1.5f, 0f) * pointDir,
                    center + new Vector2(-0.5f, 1f) * pointDir,
                    pointDir
                );

            if (
                enableCleanup
                && !flip
                && Similar(c, uf, similarThreshold)
                && !(
                    Similar3(c, uf, uff, similarThreshold)
                    && !Similar3(c, uf, ff, similarThreshold)
                    && !Similar(d, uff, similarThreshold)
                )
            )
            {
                var dist2 = DistToLine(
                    point,
                    center + new Vector2(2f, -1f) * pointDir,
                    center + new Vector2(0f, 1f) * pointDir,
                    pointDir
                );
                dist = MathF.Min(dist, dist2);
            }

            dist -= lw / 2f;
            return dist <= 0f ? (Cd(c, f) <= Cd(c, d) ? f : d) : NoSlice;
        }

        if (
            Similar3(uf, f, d, similarThreshold)
            && !Similar3(u, f, d, similarThreshold)
            && !Similar(uf, db, similarThreshold)
        )
        {
            if (!(Similar(c, df, similarThreshold) && Higher(c, d, highestColor, similarThreshold)))
            {
                if (Higher(c, d, highestColor, similarThreshold))
                    flip = true;
                if (
                    Similar(b, d, similarThreshold)
                    && !Similar(c, df, similarThreshold)
                    && !Higher(c, d, highestColor, similarThreshold)
                )
                    flip = true;
            }

            dist = flip
                ? lw
                    - DistToLine(
                        point,
                        center + new Vector2(0f, -0.5f) * pointDir,
                        center + new Vector2(-1f, 1.5f) * pointDir,
                        -pointDir
                    )
                : DistToLine(
                    point,
                    center + new Vector2(1f, -0.5f) * pointDir,
                    center + new Vector2(0f, 1.5f) * pointDir,
                    pointDir
                );

            if (
                enableCleanup
                && !flip
                && Similar(c, db, similarThreshold)
                && !(
                    Similar3(c, db, ddb, similarThreshold)
                    && !Similar3(c, db, dd, similarThreshold)
                    && !Similar(f, ddb, similarThreshold)
                )
            )
            {
                var dist2 = DistToLine(
                    point,
                    center + new Vector2(1f, 0f) * pointDir,
                    center + new Vector2(-1f, 2f) * pointDir,
                    pointDir
                );
                dist = MathF.Min(dist, dist2);
            }

            dist -= lw / 2f;
            return dist <= 0f ? (Cd(c, f) <= Cd(c, d) ? f : d) : NoSlice;
        }

        if (Similar(f, d, similarThreshold))
        {
            if (Similar(c, df, similarThreshold) && Higher(c, f, highestColor, similarThreshold))
            {
                if (!Similar(c, dd, similarThreshold) && !Similar(c, ff, similarThreshold))
                    flip = true;
            }
            else
            {
                if (Higher(c, f, highestColor, similarThreshold))
                    flip = true;
                if (!Similar(c, b, similarThreshold) && Similar4(b, f, d, u, similarThreshold))
                    flip = true;
            }

            if (
                (
                    (Similar(f, db, similarThreshold) && Similar3(u, f, df, similarThreshold))
                    || (Similar(uf, d, similarThreshold) && Similar3(b, d, df, similarThreshold))
                ) && !Similar(c, df, similarThreshold)
            )
                flip = true;

            dist = flip
                ? lw
                    - DistToLine(
                        point,
                        center + new Vector2(1f, -1f) * pointDir,
                        center + new Vector2(-1f, 1f) * pointDir,
                        -pointDir
                    )
                : DistToLine(
                    point,
                    center + new Vector2(1f, 0f) * pointDir,
                    center + new Vector2(0f, 1f) * pointDir,
                    pointDir
                );

            if (
                enableCleanup
                && !flip
                && Similar3(c, uf, uff, similarThreshold)
                && !Similar3(c, uf, ff, similarThreshold)
                && !Similar(d, uff, similarThreshold)
            )
            {
                var dist2 = DistToLine(
                    point,
                    center + new Vector2(1.5f, 0f) * pointDir,
                    center + new Vector2(-0.5f, 1f) * pointDir,
                    pointDir
                );
                dist = MathF.Max(dist, dist2);
            }

            if (
                enableCleanup
                && !flip
                && Similar3(ddb, db, c, similarThreshold)
                && !Similar3(dd, db, c, similarThreshold)
                && !Similar(ddb, f, similarThreshold)
            )
            {
                var dist2 = DistToLine(
                    point,
                    center + new Vector2(1f, -0.5f) * pointDir,
                    center + new Vector2(0f, 1.5f) * pointDir,
                    pointDir
                );
                dist = MathF.Max(dist, dist2);
            }

            dist -= lw / 2f;
            return dist <= 0f ? (Cd(c, f) <= Cd(c, d) ? f : d) : NoSlice;
        }

        if (
            Similar3(ff, df, d, similarThreshold)
            && !Similar3(ff, df, c, similarThreshold)
            && !Similar(uff, d, similarThreshold)
        )
        {
            if (!(Similar(f, dff, similarThreshold) && Higher(f, ff, highestColor, similarThreshold)))
            {
                if (Higher(f, ff, highestColor, similarThreshold))
                    flip = true;
                if (
                    Similar(uf, ff, similarThreshold)
                    && !Similar(f, dff, similarThreshold)
                    && !Higher(f, uf, highestColor, similarThreshold)
                )
                    flip = true;
            }

            dist = flip
                ? lw
                    - DistToLine(
                        point,
                        center + new Vector2(2.5f, -1f) * pointDir,
                        center + new Vector2(0.5f, 0f) * pointDir,
                        -pointDir
                    )
                : DistToLine(
                    point,
                    center + new Vector2(2.5f, 0f) * pointDir,
                    center + new Vector2(0.5f, 1f) * pointDir,
                    pointDir
                );

            dist -= lw / 2f;
            return dist <= 0f ? (Cd(f, ff) <= Cd(f, df) ? ff : df) : NoSlice;
        }

        if (
            Similar3(f, df, dd, similarThreshold)
            && !Similar3(c, df, dd, similarThreshold)
            && !Similar(f, ddb, similarThreshold)
        )
        {
            if (!(Similar(d, ddf, similarThreshold) && Higher(d, dd, highestColor, similarThreshold)))
            {
                if (Higher(d, dd, highestColor, similarThreshold))
                    flip = true;
                if (
                    Similar(db, dd, similarThreshold)
                    && !Similar(d, ddf, similarThreshold)
                    && !Higher(d, dd, highestColor, similarThreshold)
                )
                    flip = true;
            }

            dist = flip
                ? lw
                    - DistToLine(
                        point,
                        center + new Vector2(0f, 0.5f) * pointDir,
                        center + new Vector2(-1f, 2.5f) * pointDir,
                        -pointDir
                    )
                : DistToLine(
                    point,
                    center + new Vector2(1f, 0.5f) * pointDir,
                    center + new Vector2(0f, 2.5f) * pointDir,
                    pointDir
                );

            dist -= lw / 2f;
            return dist <= 0f ? (Cd(d, df) <= Cd(d, dd) ? df : dd) : NoSlice;
        }

        return NoSlice;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Similar(Vector4 a, Vector4 b, float threshold) =>
        threshold <= 0f
            ? a == b || (a.W == 0f && b.W == 0f)
            : (a.W == 0f && b.W == 0f) || Vector4.Distance(a, b) <= threshold;

    private static bool Similar3(Vector4 a, Vector4 b, Vector4 c, float t) =>
        Similar(a, b, t) && Similar(b, c, t);

    private static bool Similar4(Vector4 a, Vector4 b, Vector4 c, Vector4 d, float t) =>
        Similar(a, b, t) && Similar(b, c, t) && Similar(c, d, t);

    private static bool Higher(Vector4 a, Vector4 b, Vector3 highest, float t)
    {
        if (Similar(a, b, t))
            return false;
        if (a.W == b.W)
        {
            var ha = new Vector3(a.X, a.Y, a.Z);
            var hb = new Vector3(b.X, b.Y, b.Z);
            return Vector3.Distance(ha, highest) < Vector3.Distance(hb, highest);
        }

        return a.W > b.W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Cd(Vector4 a, Vector4 b) => Vector4.Distance(a, b);

    private static float DistToLine(Vector2 testPt, Vector2 pt1, Vector2 pt2, Vector2 dir)
    {
        var lineDir = pt2 - pt1;
        var perpDir = new Vector2(lineDir.Y, -lineDir.X);
        var dirToPt1 = pt1 - testPt;
        var sign = Vector2.Dot(perpDir, dir) > 0f ? 1f : -1f;
        return sign * Vector2.Dot(Vector2.Normalize(perpDir), dirToPt1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector4 ToVec(Color c) =>
        new(c.Red / 255f, c.Green / 255f, c.Blue / 255f, c.Alpha / 255f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color FromVec(Vector4 v) =>
        new(
            (byte)Math.Clamp((int)MathF.Round(v.X * 255f), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(v.Y * 255f), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(v.Z * 255f), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(v.W * 255f), 0, 255)
        );
}
