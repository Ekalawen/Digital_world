#ifndef VORONOILOOP_INCLUDED
#define VORONOILOOP_INCLUDED

#define PI 3.1415926535897932384626433832795

float hash(float n) {
    //return frac(sin(n) * 43758.5453123); // Normalement c'est ça x)
    return frac(sin(n * 43758.5453123));
}

float segmentDistance(float3 a, float3 b) {
    float dx = a.x - b.x;
    float dy = a.y - b.y;
    float dz = a.z - b.z;
    return dx * dx + dy * dy + dz * dz;
}

float3x3 AngleAxis3x3(float angle, float3 axis) {
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
}

void VoronoiLoopOnSphere_float(float3 pos, float time, float nbCells, float cellSpeed, float cellRange, float centerSpeed, float cellMaxOffset, out float indice, out float distanceInCell) {
    float3 center = float3(sin(time * centerSpeed), cos(time  * centerSpeed * 0.5f), sin(time * centerSpeed * 0.3f)) / 2 + 0.5f;
    //float3 center = float3(0.5f, 0.5f, 0.5f);
    float3 pp = float3(0, 0, 0); // Temporary vector
    float minDist = 4.0f;

    pos = normalize(pos);

    float s = 3.6f / sqrt(nbCells);
    float dz = 2.0f / nbCells;
    float z = 1 - dz / 2.0f;
    float longitude = 0.0f;

    float angleRot = sin(time) * cos(time) * PI;
    float thetaRot = (sin(time) + 1) * PI;
    float phiRot = (cos(time) + 1) * PI;
    float3 axisRot = float3(cos(phiRot) * sin(thetaRot), sin(phiRot) * sin(thetaRot), cos(thetaRot));

    for (float i = 0.0f; i < nbCells; i += 1.0f) {
        //float angle1 = hash(i) * 2 * PI + sin(time * PI * cellSpeed / 100000);
        //float angle2 = hash(angle1) * 2 * PI + cos(time * PI * cellSpeed / 100000);
        //float radius = (sqrt(hash(angle2)) + 0.2f) * cellRange;
        //float2 p = float2(center.x + cos(angle) * radius, center.y + sin(angle) * radius);
        //float3 p = float3(center.x + cos(angle),
        //                  center.y + sin(angle),
        //                  center.z + 
        float radius = sqrt(1 - z * z);
        float3 p = float3(cos(longitude) * radius, sin(longitude) * radius, z);
        float thetaOffset = hash(i) * 2 * PI + sin(time * PI * cellSpeed / 100000);
        float phiOffset = hash(thetaOffset) * 2 * PI + cos(time * PI * cellSpeed / 100000);
        float rOffset = cellMaxOffset;
        float3 offset = float3(rOffset * cos(phiOffset) * sin(thetaOffset),
                               rOffset * sin(phiOffset) * sin(thetaOffset),
                               rOffset * cos(thetaOffset));
        p = mul(AngleAxis3x3(angleRot, axisRot), p) + offset;
        p = normalize(p);
        z = z - dz;
        longitude += s / radius;
        float dist = segmentDistance(pos, p);
        minDist = min(minDist, dist);
        if (minDist == dist) {
            distanceInCell = minDist;
            indice = hash(i);
        }
    }
}

#endif // VORONOILOOP_INCLUDED