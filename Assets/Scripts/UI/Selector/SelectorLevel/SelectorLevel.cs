using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;

    protected SelectorManager selectorManager;

    public void Initialize(MenuBackgroundBouncing background) {
        selectorManager = SelectorManager.Instance;
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
        objectLevel.Initialize();
        menuLevel.selectorManager = selectorManager;
        menuLevel.menuBouncingBackground = background;
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

    public bool IsAccessible() {
        return selectorManager.IsLevelAccessible(this);
    }
}
