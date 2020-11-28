using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPathCadenasCollider : MonoBehaviour {

    public SelectorPathCadenas cadenas;
    public TooltipActivator tooltipActivator;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        tooltipActivator.message = $"Déverouiller le niveau {cadenas.selectorPath.endLevel.GetName()}";
    }

    public void OnMouseEnter() {
        if (!selectorManager.PopupIsEnabled()) {
            cadenas.SetFocused();
            tooltipActivator.Show();
        }
    }

    public void OnMouseExit() {
        if (!selectorManager.PopupIsEnabled()) {
            cadenas.SetUnfocused();
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
            if (!selectorManager.PopupIsEnabled()
             && !selectorManager.HasSelectorLevelOpen()
             && !selectorManager.HasSelectorPathUnlockScreenOpen()) {
                cadenas.selectorPath.OnCadenaClicked();
            }
        }
    }
}
