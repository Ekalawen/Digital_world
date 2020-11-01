using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPathCadenasCollider : MonoBehaviour {

    public SelectorPathCadenas cadenas;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Start() {
        selectorManager = SelectorManager.Instance;
    }

    public void OnMouseEnter() {
        if (!selectorManager.PopupIsEnabled())
            cadenas.SetFocused();
    }

    public void OnMouseExit() {
        cadenas.SetUnfocused();
        hasClickedDown = false;
    }

    public void OnMouseDown() {
        hasClickedDown = true;
    }

    public void OnMouseUp() {
        if (hasClickedDown) {
            hasClickedDown = false;
            if (!selectorManager.PopupIsEnabled())
                cadenas.selectorPath.OnCadenaClicked();
        }
    }
}
