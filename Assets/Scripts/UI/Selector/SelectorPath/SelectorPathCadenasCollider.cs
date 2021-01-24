using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class SelectorPathCadenasCollider : MonoBehaviour {

    public SelectorPathCadenas cadenas;
    public TooltipActivator tooltipActivator;
    public LocalizedString dataHackeesOfLevelString;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        //tooltipActivator.localizedMessage = $"Data Hackées() du niveau {cadenas.selectorPath.endLevel.GetName()}";
        tooltipActivator.localizedMessage = dataHackeesOfLevelString;
        tooltipActivator.localizedMessage.Arguments = new object[] { cadenas.selectorPath.endLevel.GetName() };
    }

    public void OnMouseEnter() {
        if (IsCadenasClickable()) {
            cadenas.SetFocused();
            tooltipActivator.Show();
        }
    }

    public void OnMouseExit() {
        if (IsCadenasClickable()) {
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
            if (IsCadenasClickable()) {
                cadenas.selectorPath.OnCadenaClicked();
                cadenas.SetUnfocused();
                tooltipActivator.Hide();
            }
        }
    }

    private bool IsCadenasClickable()
    {
        return !selectorManager.PopupIsEnabled()
            && !selectorManager.HasSelectorLevelOpen()
            && !selectorManager.HasSelectorPathUnlockScreenOpen();
    }
}
