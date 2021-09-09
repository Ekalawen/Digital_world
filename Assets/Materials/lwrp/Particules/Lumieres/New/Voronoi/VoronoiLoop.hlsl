#ifndef VORONOILOOP_INCLUDED
#define VORONOILOOP_INCLUDED

#define PI 3.1415926535897932384626433832795

float hash(float n) {
    return frac(sin(n) * 43758.5453123);
}

void VoronoiLoop_float(float2 pos, float time, float nbCells, float cellSpeed, float cellRadius, float3 center, out float3 color) {
    //float3 center = float3(sin(time), cos(time * 0.5f), 1.0f);
    //float3 center = float3(1, 1, 1) * 1.5f;
    float3 pp = float3(0, 0, 0); // Temporary vector
    float minDist = 4.0f;

    for (float i = 0.0f; i < nbCells; i += 1.0f) {
        float angle = hash(i) * 2 * PI + sin(time * PI * cellSpeed);
        float radius = sqrt(hash(angle)) * cellRadius;
        float2 p = float2(center.x * cos(angle) * radius, center.y + sin(angle) * radius);
        float dist = distance(pos, p);
        minDist = min(minDist, dist);
        if (minDist == dist) {
            pp.xz = p;
            pp.y = i / nbCells * pos.x * pos.y;
        }
    }

    //float3 shade = float3(1, 1, 1) * (1.0f - max(0.0f, dot(pp, center)));

    color = pp;// +shade;
}

#endif // VORONOILOOP_INCLUDED