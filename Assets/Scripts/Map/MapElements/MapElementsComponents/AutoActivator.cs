using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoActivator : MonoBehaviour {

    public GameObject go; // It's not really an "auto" as an object can re-activate himself ! :)
    public float activateIn = 3.0f;
    public List<MonoBehaviour> componentsToActivate;

    public IEnumerator Start() {
        go.SetActive(false);
        yield return new WaitForSeconds(activateIn);
        go.SetActive(true);
        foreach(MonoBehaviour component in componentsToActivate) {
            component.enabled = true;
        }
    }
}
