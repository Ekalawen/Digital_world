using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardObjectDisplayer : MonoBehaviour {

    protected GameObject prefab;
    protected ObjectHistory history;
    protected Curve curve;
    protected float duration;
    protected float delay;
    protected float acceleration;
    protected GameObject obj;
    protected Timer durationTimer;
    protected Timer delayTimer;

    protected virtual void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration) {
        this.prefab = prefab;
        this.history = history;
        this.duration = duration;
        this.delay = delay;
        this.acceleration = acceleration;
        this.curve = CreateCurveFromHistory();

        this.durationTimer = new Timer(duration);
        this.delayTimer = new Timer(delay);

        StartCoroutine(UpdateObject());
    }

    public virtual void ResetObject() {
        obj = GameObject.Instantiate(prefab, curve[0], Quaternion.identity);
    }

    protected Curve CreateCurveFromHistory() {
        Curve curve = new LinearCurve();
        for(int i = 0; i < history.positions.Count; i++) {
            TimedVector3 tpos = history.positions[i];
            tpos.time *= acceleration;
            history.positions[i] = tpos;
            curve.AddPoint(tpos.position);
        }
        return curve;
    }

    protected IEnumerator UpdateObject() {
        while(true) {
            ResetObject();

            durationTimer.Reset();
            yield return new WaitForSeconds(durationTimer.GetDuree());

            delayTimer.Reset();
            yield return new WaitForSeconds(delayTimer.GetDuree());
        }
    }

    public virtual void Update() {
        float avancement = durationTimer.GetAvancement();
        obj.transform.position = curve.GetAvancement(avancement);
    }
}
