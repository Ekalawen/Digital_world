using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDecomposeIcone : MonoBehaviour {

    public float dureeDecompose = 2.0f;

    protected Material material;

    void Start() {
        material = GetComponent<Renderer>().material;
        StartCoroutine(CDecomposePeriodically());
    }

    protected IEnumerator CDecomposePeriodically() {
        while (true) {
            Decompose();
            yield return new WaitForSeconds(dureeDecompose);
        }
    }


    public void Decompose() {
        material.SetFloat("_DecomposeTime", dureeDecompose);
        material.SetFloat("_DecomposeStartingTime", Time.time);
    }
}
