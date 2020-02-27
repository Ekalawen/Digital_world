using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Curve {

    public List<Vector3> points;

    public Curve() {
        this.points = new List<Vector3>();
    }
    public Curve(List<Vector3> points) {
        this.points = points;
    }

    public void AddPoint(Vector3 point) {
        points.Add(point);
    }

    public abstract Vector3 GetAvancement(int indStartPoint, float t);

    public Vector3 GetAvancement(float t) {
        t = Mathf.Clamp01(t);

        float interval = 1.0f / points.Count;

        int indPoint = (int)Mathf.Floor(t / interval);
        indPoint = (int)Mathf.Min(indPoint, points.Count - 1);

        float pointT = (t - (indPoint * interval)) / interval;

        return GetAvancement(indPoint, pointT);
    }

    public Vector3 GetSegmentStartingPoint(float t) {
        t = Mathf.Clamp01(t);
        float interval = 1.0f / points.Count;
        int indPoint = (int)Mathf.Floor(t / interval);
        return points[indPoint];
    }

    public List<Vector3> GetSurroundingPoints(float t) {
        t = Mathf.Clamp01(t);
        float interval = 1.0f / points.Count;
        int ind1 = (int)Mathf.Floor(t / interval);
        Vector3 p1 = points[ind1];
        int ind2 = (ind1 != points.Count - 1) ? ind1 + 1 : ind1;
        Vector3 p2 = points[ind2];
        return new List<Vector3>() { p1, p2 };
    }

    public Vector3 this[int key] {
        get => points[key];
        set => points[key] = value;
    }

    public int GetCount() {
        return points.Count;
    }

    public Vector3 Last() {
        return points[GetCount() - 1];
    }
}
