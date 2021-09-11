#ifndef CONVERTXYZTOCUBEUV_INCLUDED
#define CONVERTXYZTOCUBEUV_INCLUDED


void ConvertXyzToCubeUv_float(float3 xyz, out float2 uv, out float index) {
    float absX = abs(xyz.x);
    float absY = abs(xyz.y);
    float absZ = abs(xyz.z);

    float isXPositive = xyz.x > 0 ? 1 : 0;
    float isYPositive = xyz.y > 0 ? 1 : 0;
    float isZPositive = xyz.z > 0 ? 1 : 0;

    float maxAxis, uc, vc;

    // POSITIVE X
    if (isXPositive && absX >= absY && absX >= absZ) {
        // u (0 to 1) goes from +z to -z
        // v (0 to 1) goes from -y to +y
        maxAxis = absX;
        uc = -xyz.z;
        vc = xyz.y;
        index = 0;
    }
    // NEGATIVE X
    if (!isXPositive && absX >= absY && absX >= absZ) {
        // u (0 to 1) goes from -z to +z
        // v (0 to 1) goes from -y to +y
        maxAxis = absX;
        uc = xyz.z;
        vc = xyz.y;
        index = 1;
    }
    // POSITIVE Y
    if (isYPositive && absY >= absX && absY >= absZ) {
        // u (0 to 1) goes from -x to +x
        // v (0 to 1) goes from +z to -z
        maxAxis = absY;
        uc = xyz.x;
        vc = -xyz.z;
        index = 2;
    }
    // NEGATIVE Y
    if (!isYPositive && absY >= absX && absY >= absZ) {
        // u (0 to 1) goes from -x to +x
        // v (0 to 1) goes from -z to +z
        maxAxis = absY;
        uc = xyz.x;
        vc = xyz.z;
        index = 3;
    }
    // POSITIVE Z
    if (isZPositive && absZ >= absX && absZ >= absY) {
        // u (0 to 1) goes from -x to +x
        // v (0 to 1) goes from -y to +y
        maxAxis = absZ;
        uc = xyz.x;
        vc = xyz.y;
        index = 4;
    }
    // NEGATIVE Z
    if (!isZPositive && absZ >= absX && absZ >= absY) {
        // u (0 to 1) goes from +x to -x
        // v (0 to 1) goes from -y to +y
        maxAxis = absZ;
        uc = -xyz.x;
        vc = xyz.y;
        index = 5;
    }

    // Convert range from -1 to 1 to 0 to 1
    uv = float2(0.5f * (uc / maxAxis + 1.0f), 0.5f * (vc / maxAxis + 1.0f));
}

#endif // CONVERTXYZTOCUBEUV_INCLUDED