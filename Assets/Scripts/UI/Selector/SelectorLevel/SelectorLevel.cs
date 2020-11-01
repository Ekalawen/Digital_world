using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;

    protected SelectorManager selectorManager;

    void Start() {
        selectorManager = SelectorManager.Instance;
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
    }

    public void OnMouseEnter() {
    }

    public void OnMouseExit() {
    }

    public string GetName() {
        return menuLevel.textLevelName.text;
    }

    public void OnMouseDown() {
        if(!selectorManager.HasSelectorLevelOpen())
            selectorManager.TryDisplayLevel(this);
    }

    public bool IsSucceeded() {
        return menuLevel.IsSucceeded();
    }
}
