using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevelObjectCube : MonoBehaviour {

    public SelectorLevelObject objectLevel;
    public Material normalMaterial;
    public Material focusedMaterial;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Start() {
        selectorManager = SelectorManager.Instance;
    }

    public void OnMouseEnter() {
        if (!selectorManager.PopupIsEnabled()) {
            objectLevel.level.OnMouseEnter();
            GetComponent<Renderer>().material = focusedMaterial;
            objectLevel.title.SetFocused();
        }
    }

    public void OnMouseExit() {
        objectLevel.level.OnMouseExit();
        GetComponent<Renderer>().material = normalMaterial;
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
}
