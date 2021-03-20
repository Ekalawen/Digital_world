using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ResetTimeItem : Item {

    [Header("Reset Time")]
    public float settedTime = 35.0f;

    [Header("Destruction")]
    public float dureeReduction = 1.0f;
    public List<GameObject> toReduceElements;
    public AnimationCurve reduceCurve;
    public VisualEffect vfx;
    public GameObject lightObject;

    public override void OnTrigger(Collider hit) {
        gm.timerManager.SetTime(settedTime);
        gm.console.ResetTimeItemMessage(settedTime);
    }

    public override void Destroy() {
        foreach(GameObject toReduceElement in toReduceElements) {
            StartCoroutine(ReduceToZero(toReduceElement));
        }
        vfx.SetFloat("SpawnRate", 0);
        Destroy(lightObject);
        float dureeDestruction = Mathf.Max(vfx.GetVector2("Lifetime")[1], dureeReduction);
        Destroy(gameObject, dureeDestruction);
    }

    protected IEnumerator ReduceToZero(GameObject go) {
        Timer timer = new Timer(dureeReduction);
        Vector3 startingScale = go.transform.localScale;
        while(!timer.IsOver()) {
            go.transform.localScale = startingScale * reduceCurve.Evaluate(timer.GetAvancement());
            yield return null;
        }
        go.transform.localScale = Vector3.zero;
    }
}
