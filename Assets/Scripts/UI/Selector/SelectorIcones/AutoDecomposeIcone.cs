using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDecomposeIcone : MonoBehaviour {

    public float dureeDecompose = 2.0f;
    public float dureeAfterDecompose = 0;
    public float dureeBeforeDecompose = 0;

    protected Material material;

    void Start() {
        material = GetComponent<Renderer>().material;
        StartCoroutine(CDecomposePeriodically());
    }

    protected IEnumerator CDecomposePeriodically() {
        while (true) {
            SetNotDecomposed();
            yield return new WaitForSeconds(dureeBeforeDecompose);
            Decompose();
            yield return new WaitForSeconds(dureeDecompose + dureeAfterDecompose);
        }
    }

    protected void SetNotDecomposed() {
        material.SetFloat("_DecomposeTime", dureeBeforeDecompose);
        material.SetFloat("_DecomposeStartingTime", Time.time + dureeBeforeDecompose);
    }

    protected void Decompose() {
        material.SetFloat("_DecomposeTime", dureeDecompose);
        material.SetFloat("_DecomposeStartingTime", Time.time);
    }
}
