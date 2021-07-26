using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Coef {
    public float value;

    public Coef(float value) {
        this.value = value;
    }
}

public class RewardNewBestScoreButton : MonoBehaviour {

    public List<ParticleSystem> particleSystems;
    public float coefSize = 1.1f;
    public float dureeSize = 1.0f;
    public AnimationCurve sizeCurve;

    protected List<Coef> addedCoefs;
    protected float startSize;


    public void Initialize() {
        addedCoefs = new List<Coef>();
        startSize = GetSystemsSize();
        UISoundManager.Instance.PlayNewBestScoreClip();
    }

    public void Trigger() {
        foreach(ParticleSystem particleSystem in particleSystems) {
            particleSystem.Play();
        }
        UISoundManager.Instance.PlayNewBestScoreClickedClip();
        Coef coef = new Coef(1.0f);
        addedCoefs.Add(coef);
        StartCoroutine(CDecreaseCoef(coef));
    }

    protected IEnumerator CDecreaseCoef(Coef coef) {
        Timer timer = new Timer(dureeSize);
        while(!timer.IsOver()) {
            coef.value = sizeCurve.Evaluate(timer.GetAvancement());
            yield return null;
        }
        coef.value = 0.0f;
        addedCoefs.Remove(coef);
    }

    public void Update() {
        float size = startSize * Mathf.Pow(coefSize, addedCoefs.Select(c => c.value).Sum());
        SetSystemsSize(size);
    }

    public float GetSystemsSize() {
        return particleSystems[0].transform.localScale.x;
    }

    public void SetSystemsSize(float size) {
        foreach(ParticleSystem particleSystem in particleSystems) {
            particleSystem.transform.localScale = Vector3.one * size;
        }
    }
}
