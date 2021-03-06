using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoExplodeIcone : MonoBehaviour {

    public float dureeExplosion = 2.0f;
    public float offsetFirstExplosion = 0.0f;
    public VisualEffect visualEffect;

    public void Start() {
        StartCoroutine(CExplodePeriodically());
    }

    protected IEnumerator CExplodePeriodically() {
        yield return new WaitForSeconds(offsetFirstExplosion);
        while (true) {
            Explode();
            yield return new WaitForSeconds(dureeExplosion);
        }
    }


    public void Explode() {
        visualEffect.SendEvent("Explode");
    }
}
