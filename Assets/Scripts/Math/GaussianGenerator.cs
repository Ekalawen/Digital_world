using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianGenerator {

    public static float Next() {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);

        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }

    public static float Next(float mean, float standardDeviation) {
        return mean + Next() * standardDeviation;
    }

    public static float Next(float mean, float standardDeviation, float min, float max) {
        float x;
        do
        {
            x = Next(mean, standardDeviation);
        } while (x < min || x > max);
        return x;
    }
}
