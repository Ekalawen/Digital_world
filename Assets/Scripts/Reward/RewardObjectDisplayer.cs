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
    protected float delayBeforeSpawning;
    protected float delayBeforeEnd;
    protected GameObject obj;
    protected Timer durationTimer;
    protected Timer delayTimer;

    public virtual void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration) {
        this.prefab = prefab;
        this.history = history;
        this.duration = duration;
        this.delay = delay;
        this.acceleration = acceleration;
        this.curve = CreateCurveFromHistory();
        this.delayBeforeSpawning = history.positions[0].time;// / acceleration;
        this.delayBeforeEnd = duration - history.LastTime();

        this.durationTimer = new Timer(duration - delayBeforeSpawning - delayBeforeEnd);
        this.delayTimer = new Timer(delay);

        StartCoroutine(UpdateObject());
    }

    public virtual void ResetObject() {
        if (obj != null)
            Destroy(obj.gameObject);
        obj = GameObject.Instantiate(prefab, curve[0], Quaternion.identity);
        Debug.Log("Reset obj " + prefab.name);
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
            yield return new WaitForSeconds(delayBeforeSpawning);

            durationTimer.Reset();
            ResetObject();
            yield return new WaitForSeconds(durationTimer.GetDuree());

            yield return new WaitForSeconds(delayBeforeEnd);

            delayTimer.Reset();
            yield return new WaitForSeconds(delayTimer.GetDuree());
        }
    }

    public virtual void Update() {
        float avancement = durationTimer.GetAvancement();
        if(!durationTimer.IsOver() && obj != null)
            obj.transform.position = curve.GetAvancement(avancement);
    }
}
