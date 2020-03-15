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

    // /!\ Attention cette fonction BOUSILLE PEUT-ETRE la liste !!!
    public static List<T> SelectSomeProportionOf<T>(List<T> l, float proportion) {
        List<T> res = new List<T>();
        int N = l.Count;
        float P = proportion;
        float mean = N * P;
        float variance = N * P * (1.0f - P);
        int nbSelected = (int)GaussianGenerator.Next(mean, variance, 0, N);
        nbSelected = (int)Mathf.Clamp(nbSelected, 0.0f, N);

        for(int i = 0; i < nbSelected; i++) {
            int ind = Random.Range(0, l.Count);
            res.Add(l[ind]);
            l.RemoveAt(ind);
        }

        return res;
    }

    public static List<T> SelectSomeProportionOfNaiveMethod<T>(List<T> l, float proportion) {
        List<T> res = new List<T>();

        for(int i = 0; i < l.Count; i++) {
            if(Random.Range(0.0f, 1.0f) < proportion)
                res.Add(l[i]);
        }

        return res;
    }
}
