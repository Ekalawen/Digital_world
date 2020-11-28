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
    public TooltipActivator tooltipActivator;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Initialize() {
        selectorManager = SelectorManager.Instance;
        SetMaterial(focus: false);
        tooltipActivator.message = $"Niveau {selectorLevel.GetName()}";
    }

    public void OnMouseEnter() {
        if (IsCubeClickable()) {
            objectLevel.level.OnMouseEnter();
            SetMaterial(focus: true);
            objectLevel.title.SetFocused();
            tooltipActivator.Show();
        }
    }

    public void OnMouseExit() {
        if (IsCubeClickable()) {
            objectLevel.level.OnMouseExit();
            SetMaterial(focus: false);
            objectLevel.title.SetUnfocused();
            hasClickedDown = false;
            tooltipActivator.Hide();
        }
    }

    public void OnMouseDown() {
        hasClickedDown = true;
    }

    public void OnMouseUp() {
        if (hasClickedDown) {
            hasClickedDown = false;
            if (IsCubeClickable()) {
                objectLevel.level.OnMouseDown();
            }
        }
    }

    private bool IsCubeClickable() {
        return !selectorManager.PopupIsEnabled()
            && !selectorManager.HasSelectorLevelOpen()
            && !selectorManager.HasSelectorPathUnlockScreenOpen();
    }

    public void SetMaterial(bool focus) {
        if (!selectorLevel.IsAccessible())
            GetComponent<Renderer>().material = lockedMaterial;
        else
            GetComponent<Renderer>().material = focus ? focusedMaterial : normalMaterial;
    }
}
