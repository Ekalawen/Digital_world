using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AutoDisableIcone : MonoBehaviour {

    public float offsetBeforeStarting = 0.0f;
    public float dureeEnabled = 2.0f;
    public float dureeDisabled = 2.0f;
    public GameObject toDisableGameObject;

    public void Start() {
        StartCoroutine(CDisablePeriodically());
    }

    protected IEnumerator CDisablePeriodically() {
        yield return new WaitForSeconds(offsetBeforeStarting);
        while (true) {
            SetActive(true);
            yield return new WaitForSeconds(dureeEnabled);
            SetActive(false);
            yield return new WaitForSeconds(dureeDisabled);
        }
    }


    public void SetActive(bool value) {
        toDisableGameObject.SetActive(value);
    }
}
