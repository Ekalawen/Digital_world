#ifndef CONVERTCUBEUVTOXYZ_INCLUDED
#define CONVERTCUBEUVTOXYZ_INCLUDED

#define PI 3.1415926535897932384626433832795

void ConvertCubeUvToXyz_float(float index, float2 uv, out float3 xyz) {
    float uc = 2.0f * uv.x - 1.0f;
    float vc = 2.0f * uv.y - 1.0f;
    switch (index) {
        case 0: xyz = float3(1.0f, vc, -uc); break; // POSITIVE X
        case 1: xyz = float3(-1.0f, vc, uc); break; // NEGATIVE X
        case 2: xyz = float3(uc, 1.0f, -vc); break; // POSITIVE Y
        case 3: xyz = float3(uc, -1.0f, vc); break; // NEGATIVE Y
        case 4: xyz = float3(uc, vc, 1.0f); break; // POSITIVE Z
        case 5: xyz = float3(-uc, vc, -1.0f); break; // NEGATIVE Z
    }
}

#endif // CONVERTCUBEUVTOXYZ_INCLUDED