using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDissolveIcone : MonoBehaviour {

    public float dissolveTime = 1.0f;
    public float periode = 2.0f;
    public float offset = 0.0f;
    public GameObject go;

    protected Material material;

    public void Start() {
        material = go.GetComponent<Renderer>().material;
        StartCoroutine(CDissolvePeriodically());
    }

    protected IEnumerator CDissolvePeriodically() {
        yield return new WaitForSeconds(offset);
        while (true) {
            Dissolve();
            yield return new WaitForSeconds(periode);
        }
    }


    public void Dissolve() {
        material.SetFloat("_DissolveTime", dissolveTime);
        material.SetFloat("_PlayerProximityCoef", 0.0f);
        material.SetFloat("_DissolveStartingTime", Time.time);
        material.SetFloat("_DecomposeStartingTime", 999999f); // Reinitialise Décompose Effect
    }
}
