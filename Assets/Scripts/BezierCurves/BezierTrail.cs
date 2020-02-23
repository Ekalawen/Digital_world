using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTrail : MonoBehaviour {

	public float vitesse; // La vitesse de déplacement du trail
    public BezierCurve curve;

    protected Timer timer;

    public void Start() {
        timer = new Timer(1.0f);
    }

    public void Update() {
        float t = timer.GetAvancement();
        transform.position = curve.GetPointAt(t);
    }

    public void AddPoints(List<Vector3> points) {
        foreach (Vector3 point in points)
            AddPoint(point);
    }

    public BezierPoint AddPoint(Vector3 point) {
        BezierPoint bezierPoint = new GameObject().AddComponent<BezierPoint>();
        bezierPoint.position = point;
        bezierPoint.curve = curve;
        //bezierPoint.handleStyle = BezierPoint.HandleStyle.Broken;
        //bezierPoint.handle1 = point;
        //bezierPoint.handle2 = point;
        if (curve.pointCount > 0) {
            BezierPoint lastpoint = curve[curve.pointCount - 1];
            //lastpoint.handle2 = point;
            //bezierPoint.handle1 = lastpoint.position;
        }
        curve.AddPoint(bezierPoint);
        return bezierPoint;
    }
}
