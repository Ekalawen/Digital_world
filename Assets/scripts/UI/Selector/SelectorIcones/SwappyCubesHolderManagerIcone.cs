using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderManagerIcone : MonoBehaviour {

    public float frequence = 1.5f;
    public float previsualisationDuration = 1.0f;
    public GameObject cube1;
    public GameObject cube2;

    protected bool currentState = false;
    protected Material mat1;
    protected Material mat2;

    public void Start() {
        mat1 = cube1.GetComponent<Renderer>().material;
        mat2 = cube2.GetComponent<Renderer>().material;
        StartCoroutine(CStartSwapping());
    }

    protected IEnumerator CStartSwapping() {
        Timer timer = new Timer(frequence);
        SetCubesState(currentState);

        while(true) {
            if(timer.IsOver()) {
                currentState = !currentState;
                SetCubesState(currentState);
                timer.Reset();
            }
            yield return null;
        }
    }

    protected void SetCubesState(bool currentState) {
        if(!currentState) {
            StartCoroutine(CSetEnableValueIn(mat1, true, previsualisationDuration, cube1.transform.position));
            StartCoroutine(CSetEnableValueIn(mat2, false, previsualisationDuration, cube2.transform.position));
        } else {
            StartCoroutine(CSetEnableValueIn(mat1, false, previsualisationDuration, cube1.transform.position));
            StartCoroutine(CSetEnableValueIn(mat2, true, previsualisationDuration, cube2.transform.position));
        }
    }

    protected IEnumerator CSetEnableValueIn(Material mat, bool value, float duration, Vector3 impactPoint) {
        float impactRadius = Mathf.Sqrt(3) / 2;
        StartImpact(mat, impactPoint, impactRadius, duration);
        yield return new WaitForSeconds(duration);
        StopImpact(mat);
        mat.SetFloat("_IsDisabled", value ? 1.0f : 0.0f);
    }

    public void StartImpact(Material mat, Vector3 impactPoint, float impactRadius, float impactDuration) {
        mat.SetFloat("_IsImpacting", 1.0f);
        mat.SetVector("_ImpactPoint", impactPoint);
        mat.SetFloat("_ImpactTime", Time.time);
        float impactSpeed = impactRadius / impactDuration;
        mat.SetFloat("_ImpactPropagationSpeed", impactSpeed);
    }

    public void StopImpact(Material mat) {
        mat.SetFloat("_IsImpacting", 0.0f);
    }
}
