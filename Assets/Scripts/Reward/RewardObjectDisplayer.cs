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
    protected Transform displayerFolder;
    protected float scaleFactor;

    public virtual void Initialize(GameObject prefab, ObjectHistory history, float duration, float delay, float acceleration, float scaleFactor) {
        this.prefab = prefab;
        this.history = history;
        this.duration = duration;
        this.delay = delay;
        this.acceleration = acceleration;
        this.curve = CreateCurveFromHistory(this.history, this.acceleration);
        this.delayBeforeSpawning = history.positions[0].time;// / acceleration;
        this.delayBeforeEnd = duration - history.LastTime();
        this.scaleFactor = scaleFactor;

        this.durationTimer = new Timer(duration - delayBeforeSpawning - delayBeforeEnd);
        this.delayTimer = new Timer(delay);

        this.displayerFolder = RewardManager.Instance.GetDisplayersFolder();

        StartCoroutine(UpdateObject());
    }

    public virtual void ResetObject() {
        if (obj != null)
            Destroy(obj.gameObject);
        obj = GameObject.Instantiate(prefab, curve[0], Quaternion.identity, displayerFolder);
    }

    public static Curve CreateCurveFromHistory(ObjectHistory history, float acceleration) {
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

    public Curve GetCurve() {
        return curve;
    }

    public Timer GetDurationTimer() {
        return durationTimer;
    }

    public GameObject GetObject() {
        return obj;
    }
}
