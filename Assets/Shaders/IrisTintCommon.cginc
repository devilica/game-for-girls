#ifndef DRESSUP_IRIS_TINT_COMMON_INCLUDED
#define DRESSUP_IRIS_TINT_COMMON_INCLUDED

// Reference iris brown from eyes_2.png (#b47842)
static const float3 IrisRefBrown = float3(0.705882, 0.470588, 0.258824);
static const float IrisRefLuma = dot(IrisRefBrown, float3(0.299, 0.587, 0.114));

inline float IrisUseTintMask(float3 irisTint)
{
    float useIrisTint = step(0.02, abs(irisTint.r - 1.0))
        + step(0.02, abs(irisTint.g - 1.0))
        + step(0.02, abs(irisTint.b - 1.0));
    return saturate(useIrisTint);
}

inline float BrownIrisMask(float3 rgb)
{
    float luma = dot(rgb, float3(0.299, 0.587, 0.114));
    float maxChannel = max(rgb.r, max(rgb.g, rgb.b));
    float minChannel = min(rgb.r, min(rgb.g, rgb.b));
    float saturation = maxChannel - minChannel;
    float warmness = rgb.r - rgb.b;

    // Exclude pupil and near-black pixels, but keep dark upper-iris shadow.
    float notBlack = smoothstep(0.038, 0.078, luma);

    // Exclude sclera: bright and mostly neutral.
    float isSclera = smoothstep(0.60, 0.76, luma)
        * (1.0 - smoothstep(0.05, 0.20, saturation));
    float notSclera = 1.0 - isSclera;

    // Exclude only tiny white catchlights.
    float isSpecular = smoothstep(0.86, 0.96, luma)
        * (1.0 - smoothstep(0.04, 0.18, saturation));
    float notSpecular = 1.0 - isSpecular;

    // Full iris ring around the pupil:
    // - bright lower/mid iris body (warm, colored)
    // - darker upper iris shadow under the eyelid (still warm, lower sat)
    float isWarm = smoothstep(-0.05, 0.04, warmness);
    float hasColor = smoothstep(0.02, 0.07, saturation);
    float inIrisLuma = smoothstep(0.07, 0.13, luma) * smoothstep(0.78, 0.58, luma);

    float mainIris = inIrisLuma * max(hasColor, isWarm);
    float darkIrisShadow = smoothstep(0.04, 0.16, luma)
        * smoothstep(0.34, 0.10, luma)
        * max(isWarm, hasColor * 0.65);

    float irisMask = notBlack * notSclera * notSpecular
        * saturate(max(mainIris, darkIrisShadow));

    return saturate(irisMask);
}

inline fixed3 RecolorBrownIris(fixed3 rgb, fixed3 irisTint)
{
    float luma = dot(rgb, float3(0.299, 0.587, 0.114));
    float irisMask = BrownIrisMask(rgb);
    float useIrisTint = IrisUseTintMask(irisTint);

    if (irisMask * useIrisTint < 0.001)
    {
        return rgb;
    }

    // Preserve shading but keep color visible even in dark upper-iris shadow.
    float rel = luma / max(IrisRefLuma, 0.001);
    fixed3 tinted = irisTint * saturate(max(rel, 0.38));

    // Keep only the small white catchlights untouched.
    float maxChannel = max(rgb.r, max(rgb.g, rgb.b));
    float minChannel = min(rgb.r, min(rgb.g, rgb.b));
    float saturation = maxChannel - minChannel;
    float specular = smoothstep(0.82, 0.95, luma)
        * (1.0 - smoothstep(0.08, 0.22, saturation));
    tinted = lerp(tinted, rgb, specular);

    return lerp(rgb, tinted, irisMask * useIrisTint);
}

#endif
