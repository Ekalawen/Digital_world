using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathCurves {

    public static float Linear(float min, float max, float avancement) {
        return min + (max - min) * avancement;
    }

    public static float LinearRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Linear(min, max, randNumber);
    }

    public static float Quadratic(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Pow(avancement, 2));
    }

    public static float QuadraticRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Quadratic(min, max, randNumber);
    }

    public static float Cubic(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Pow(avancement, 3));
    }

    public static float CubicRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Cubic(min, max, randNumber);
    }

    public static float QuadraticInverse(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Pow(avancement, 1f/2f));
    }

    public static float QuadraticInverseRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return QuadraticInverse(min, max, randNumber);
    }

    public static float CubicInverse(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Pow(avancement, 1f/3f));
    }

    public static float CubicInverseRandomc(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return CubicInverse(min, max, randNumber);
    }

    public static float Power(float min, float max, float avancement, float power) {
        return Linear(min, max, Mathf.Pow(avancement, power));
    }

    public static float PowerRandom(float min, float max, float power) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Power(min, max, randNumber, power);
    }

    public static float WithCurve(float min, float max, float avancement, AnimationCurve curve) {
        return Linear(min, max, curve.Evaluate(avancement));
    }

    public static float WithCurveRandom(float min, float max, float avancement, AnimationCurve curve) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return WithCurve(min, max, randNumber, curve);
    }

    public static float Exp(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Exp(avancement));
    }

    public static float ExpRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Exp(min, max, randNumber);
    }

    public static float Log(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Log(avancement));
    }

    public static float LogRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Log(min, max, randNumber);
    }
}
