#ifndef VORONOILOOP_INCLUDED
#define VORONOILOOP_INCLUDED

#define PI 3.1415926535897932384626433832795

float hash(float n) {
    //return frac(sin(n) * 43758.5453123); // Normalement c'est ça x)
    return frac(sin(n * 43758.5453123));
}

float wrappingDistanceSquared(float2 a, float2 b) {
    float dx = abs(a.x - b.x);
    float dy = abs(a.y - b.y);
    if (dx > 0.5f) {
        dx = 1.0f - dx;
    }
    if (dy > 0.5f) {
        dy = 1.0f - dy;
    }
    return dx * dx + dy * dy;
}

void VoronoiLoop_float(float2 pos, float time, float nbCells, float cellSpeed, float cellRange, float centerSpeed, out float indice, out float distanceInCell) {
    float3 center = float3(sin(time * centerSpeed), cos(time  * centerSpeed * 0.5f), 1.0f) / 2 + 0.5f;
    //float3 center = float3(0.5f, 0.5f, 0.5f);
    float3 pp = float3(0, 0, 0); // Temporary vector
    float minDist = 4.0f;

    for (float i = 0.0f; i < nbCells; i += 1.0f) {
        float angle = hash(i) * 2 * PI + sin(time * PI * cellSpeed / 100000);
        float radius = (sqrt(hash(angle)) + 0.2f) * cellRange;
        //float2 p = float2(center.x + cos(angle) * radius, center.y + sin(angle) * radius);
        float2 p = float2(frac(center.x + cos(angle) * radius), frac(center.y + sin(angle) * radius));
        float dist = wrappingDistanceSquared(pos, p);
        minDist = min(minDist, dist);
        if (minDist == dist) {
            distanceInCell = minDist;
            indice = hash(i);
            //pp.zy = p;
            //pp.x = i / nbCells * pos.x * pos.y;
            //pp.x = i / nbCells * abs(pos.x - 0.5f) * abs(pos.y - 0.5f) * 4;
        }
    }

    //centerDistance = wrappingDistanceSquared(pos, center);

    //float3 shade = float3(1, 1, 1) * (1.0f - max(0.0f, dot(pp, center)));

    //color = pp + shade;
    //color = pp + shade;
}

#endif // VORONOILOOP_INCLUDED