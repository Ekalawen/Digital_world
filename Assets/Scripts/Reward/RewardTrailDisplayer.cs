using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardTrailDisplayer : RewardObjectDisplayer {

    protected Color color;
    protected TrailRenderer trail;
    protected float trailDuration;

    public void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration, float trailDuration, Color color) {
        this.color = color;
        this.trailDuration = trailDuration;
        base.Initialize(prefab, history, duration, delay, acceleration, 1.0f);
    }

    public override void ResetObject() {
        base.ResetObject();
        trail = obj.GetComponent<TrailRenderer>();
        trail.time = trailDuration;
        trail.startColor = color;
    }

    public override void Update() {
        base.Update();
        if (trail != null) {
            trail.time = Mathf.Min(trailDuration, durationTimer.GetRemainingTime());
        }
    }

    public void SetColor(Color color) {
        this.color = color;
    }
}
