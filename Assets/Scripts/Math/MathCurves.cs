using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathCurves {

    public static float Linear(float min, float max, float avancement) {
        return min + (max - min) * avancement;
    }

    public static float LinearReversed(float min, float max, float current) {
        return Mathf.Clamp01(LinearReversedUnclamped(min, max, current));
    }

    public static float LinearReversedUnclamped(float min, float max, float current) {
        return (current - min) / (max - min);
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
        return Linear(min, max, Mathf.Exp(avancement) / Mathf.Exp(1));
    }

    public static float ExpRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Exp(min, max, randNumber);
    }

    public static float Log(float min, float max, float avancement) {
        return Linear(min, max, Mathf.Log(avancement + 1) / Mathf.Log(2));
    }

    public static float LogRandom(float min, float max) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Log(min, max, randNumber);
    }

    public static float Norm(float min, float max, float avancement, float mean, float variance) {
        float norm = 1 / (variance * Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Exp(-Mathf.Pow(avancement - mean, 2) / (2 * Mathf.Pow(variance, 2)));
        return Linear(min, max, norm);
    }

    public static float NormRandom(float min, float max, float mean, float variance) {
        float randNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        return Norm(min, max, randNumber, mean, variance);
    }

    public static float Remap(this float value, Vector2 inRange, Vector2 outRange) {
        return (value - inRange.x) / (inRange.y - inRange.x) * (outRange.y - outRange.x) + outRange.x;
    }

    public static float Remap(this float value, float inX, float inY, float outX, float outY) {
        return Remap(value, new Vector2(inX, inY), new Vector2(outX, outY));
    }
}
