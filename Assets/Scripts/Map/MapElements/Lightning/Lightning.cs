using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Lightning : MonoBehaviour {

    public float vitesse = 10.0f;
    public float durationAfterArriving = 0.5f;
    public VisualEffect vfx;

    protected Vector3 start;
    protected Vector3 end;

    public void Initialize(Vector3 start, Vector3 end) {
        this.start = start;
        this.end = end;

        transform.position = start;
        transform.forward = (end - start).normalized;

        float distance = (end - start).magnitude;
        vfx.SetFloat("Lenght", distance);
        float durationToArriving = distance / vitesse;
        float duration = durationToArriving + durationAfterArriving;
        vfx.SetFloat("Lifetime", duration);
        vfx.SetFloat("TimeAtMaxLength", durationToArriving / duration);
    }
}
