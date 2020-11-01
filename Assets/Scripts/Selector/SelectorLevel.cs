using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public GameObject objectTitle;

    protected SelectorManager selectorManager;

    void Start() {
        selectorManager = SelectorManager.Instance;
        objectTitle.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
    }

    void Update() {
    }
}
