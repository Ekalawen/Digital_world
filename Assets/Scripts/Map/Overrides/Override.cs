using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Localization;

public abstract class Override: MonoBehaviour {

    public LocalizedString overrideName;

    protected GameManager gm;
    protected MapManager map;

    public void Initialize() {
        gm = GameManager.Instance;
        map = gm.map;

        InitializeSpecific();
    }

    protected abstract void InitializeSpecific();

    public string GetName() {
        return overrideName.GetLocalizedString().Result;
    }
}
