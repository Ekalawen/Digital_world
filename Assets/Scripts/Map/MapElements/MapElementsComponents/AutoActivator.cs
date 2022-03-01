using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoActivator : MonoBehaviour {

    public GameObject go; // It's not really an "auto" as an object can re-activate himself ! :)
    public float activateIn = 3.0f;
    public float loopDuration = 7.0f;
    public List<MonoBehaviour> componentsToActivate;

    public void Start() {
        StartCoroutine(Loop());
    }

    protected IEnumerator Loop() {
        go.SetActive(false);
        foreach(MonoBehaviour component in componentsToActivate) {
            component.enabled = false;
        }
        yield return new WaitForSeconds(activateIn);
        go.SetActive(true);
        foreach(MonoBehaviour component in componentsToActivate) {
            component.enabled = true;
        }
        yield return new WaitForSeconds(loopDuration - activateIn);
        StartCoroutine(Loop());
    }
}
