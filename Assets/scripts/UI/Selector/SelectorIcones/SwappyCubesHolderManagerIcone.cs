using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderManagerIcone : MonoBehaviour {

    public float frequence = 1.5f;
    public float previsualisationDuration = 1.0f;
    public List<GameObject> cubes1;
    public List<GameObject> cubes2;

    protected bool currentState = false;
    protected Material mat1;
    protected Material mat2;

    public void Start() {
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
            foreach (GameObject cube in cubes1) {
                StartCoroutine(CSetEnableValueIn(true, previsualisationDuration, cube));
            }
            foreach (GameObject cube in cubes2) {
                StartCoroutine(CSetEnableValueIn(false, previsualisationDuration, cube));
            }
        } else {
            foreach (GameObject cube in cubes1) {
                StartCoroutine(CSetEnableValueIn(false, previsualisationDuration, cube));
            }
            foreach (GameObject cube in cubes2) {
                StartCoroutine(CSetEnableValueIn(true, previsualisationDuration, cube));
            }
        }
    }

    protected IEnumerator CSetEnableValueIn(bool value, float duration, GameObject go) {
        Vector3 impactPoint = go.transform.position;
        Material mat = go.GetComponent<Renderer>().material;
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
