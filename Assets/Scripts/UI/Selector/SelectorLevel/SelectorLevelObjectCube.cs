using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevelObjectCube : MonoBehaviour {

    public SelectorLevel selectorLevel;
    public SelectorLevelObject objectLevel;
    public Material normalMaterial;
    public Material focusedMaterial;
    public Material lockedMaterial;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Initialize() {
        selectorManager = SelectorManager.Instance;
        SetMaterial(focus: false);
    }

    public void OnMouseEnter() {
        if (!selectorManager.PopupIsEnabled()) {
            objectLevel.level.OnMouseEnter();
            SetMaterial(focus: true);
            objectLevel.title.SetFocused();
        }
    }

    public void OnMouseExit() {
        objectLevel.level.OnMouseExit();
        SetMaterial(focus: false);
        objectLevel.title.SetUnfocused();
        hasClickedDown = false;
    }

    public void OnMouseDown() {
        hasClickedDown = true;
    }

    public void OnMouseUp() {
        if (hasClickedDown) {
            hasClickedDown = false;
            if(!selectorManager.PopupIsEnabled())
                objectLevel.level.OnMouseDown();
        }
    }

    protected void SetMaterial(bool focus) {
        if (!selectorLevel.IsAccessible())
            GetComponent<Renderer>().material = lockedMaterial;
        else
            GetComponent<Renderer>().material = focus ? focusedMaterial : normalMaterial;
    }
}
