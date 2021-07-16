using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class SelectorLevelObjectCube : MonoBehaviour {

    public SelectorLevel selectorLevel;
    public SelectorLevelObject objectLevel;
    public Material normalMaterial;
    public Material focusedMaterial;
    public Material lockedMaterial;
    public TooltipActivator tooltipActivator;
    public LocalizedString cubeNiveauSmartString;
    public float iconeSize = 0.8f;

    protected SelectorManager selectorManager;
    protected bool hasClickedDown = false;

    public void Initialize(bool hightlighted, GameObject selectorIconePrefab) {
        selectorManager = SelectorManager.Instance;
        SetMaterial(focus: false);
        InitSelectorIcone(selectorIconePrefab);
        tooltipActivator.localizedMessage = cubeNiveauSmartString;
        tooltipActivator.localizedMessage.Arguments = new object[] { selectorLevel.GetVisibleName() };
        GetComponent<AutoBouncer>().enabled = hightlighted;
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
                tooltipActivator.Hide();
                SetMaterial(focus: false);
                objectLevel.title.SetUnfocused();
            }
        }
    }

    private bool IsCubeClickable() {
        return !selectorManager.PopupIsEnabled()
            && !selectorManager.HasSelectorLevelOpen()
            && !selectorManager.HasSelectorPathUnlockScreenOpen();
    }

    public void SetMaterial(bool focus) {
        GetComponent<Renderer>().material = focus ? focusedMaterial : (selectorLevel.IsAccessible() ? normalMaterial : lockedMaterial);
    }

    protected void InitSelectorIcone(GameObject selectorIconePrefab) {
        if (selectorIconePrefab != null) {
            GameObject go = Instantiate(selectorIconePrefab, transform.position, transform.rotation, parent: transform);
            go.transform.localScale = Vector3.one * iconeSize;
        }
    }
}
